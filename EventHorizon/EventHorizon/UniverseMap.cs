using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EventHorizon
{
	public partial class UniverseMap : Control
	{
		private PointF origin;
		private PointF lastMousePos;
		private float zoom;
		private IEnumerable<Ship> highlightedShips;
		private Universe universe;
		private Sector selectedSector;

		public PointF Origin
		{
			get { return origin; }
			set
			{
				if (Universe != null)
					origin = Universe.NormalizePixel(value, Zoom);
				else
					origin = value;
				Invalidate();
			}
		}

		public float Zoom
		{
			get { return zoom; }
			set
			{
				zoom = value;
				if (zoom < 64)
					zoom = 64;
				if (zoom > 512)
					zoom = 512;
				Invalidate();
			}
		}

		public Universe Universe
		{
			get
			{
				return universe;
			}
			set
			{
				universe = value;
				Invalidate();
			}
		}

		public IEnumerable<Ship> HighlightedShips
		{
			get { return highlightedShips; }
			set
			{
				highlightedShips = value;
				Invalidate();
			}
		}

		public Sector SelectedSector
		{
			get { return selectedSector; }
			set
			{
				selectedSector = value;
				Invalidate();
			}
		}

		public UniverseMap()
		{
			InitializeComponent();

			// set default origin and zoom
			Origin = new PointF();
			Zoom = 64f;

			// set default colors
			BackColor = Color.Black;
			ForeColor = Color.White;

			// enable double buffering
			DoubleBuffered = true;

			// default ui mode
			AllowZoomingAndPanning = true;
			HighlightedShips = new Ship[] { };
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);

			var g = pe.Graphics;

			if (Universe != null)
			{
				// TODO - cache renderable stuff so it's not all recomputed every render

				// size of universe
				var bound = (new PointF((Universe.Width + 1) * Zoom, (Universe.Height + 1) * Zoom)).HexToScreen();

				// draw hexes
				var sf = new StringFormat();
				sf.Alignment = StringAlignment.Center;
				sf.LineAlignment = StringAlignment.Center;
				var hexBorder = Zoom / 16f; // width of single hexborder
				var hexBorders = hexBorder * 2f; // width of both hexborders (mothership and selection)
				var halfBorder = hexBorder / 2f; // width of half a hex border
				var halfHex = Zoom / 2f - hexBorder; // width of half hex (excluding border)
				var fullHex = Zoom - hexBorders; // with of full hex (excluding borders)
				for (var xHex = MinGhostCoords.X; xHex <= MaxGhostCoords.X; xHex++)
				{
					for (var yHex = MinGhostCoords.Y; yHex <= MaxGhostCoords.Y; yHex++)
					{
						var hexCoords = new Point(xHex, yHex);
						var realHexCoords = Universe.NormalizeHex(hexCoords);
						var pos = new PointF(xHex * Zoom, yHex * Zoom).HexToScreen();
						pos += new SizeF(Size.Width / 2f, Size.Height / 2f);
						pos = new PointF(pos.X + Origin.X, pos.Y + Origin.Y);
						var sector = Universe.Sectors[realHexCoords.X, realHexCoords.Y];
						Color fill;
						bool makePlanet = false;
						var maxStrength = Math.Max(Universe.MaximumEnemyFleetStrength, Universe.OurMaximumFleetStrength);
						if (maxStrength == 0)
							maxStrength = 1; // prevent divide by zero
						var blue = (Math.Sqrt(sector.OurFleetStrength / maxStrength) * 255);
						var red = (Math.Sqrt(sector.EnemyFleetStrength / maxStrength) * 255);
						blue = blue * Math.Min(1.0, 255.0 / (Universe.MaximumEnemyFleetStrength + Universe.OurMaximumFleetStrength));
						red = red * Math.Min(1.0, 255.0 / (Universe.MaximumEnemyFleetStrength + Universe.OurMaximumFleetStrength));
						if (sector != null)
						{
							fill = Color.FromArgb((int)red, (int)((red + blue) / 2.0), (int)blue);
							if (sector.Value != null)
								makePlanet = true;
						}
						else
							fill = Color.Gray;
						g.FillPolygon(new SolidBrush(fill), MakeHexagon(halfHex, pos));
						if (makePlanet)
						{
							var brush = new SolidBrush(Color.FromArgb(ToColorComponent(sector.Value.Weapons, universe.MaxValue.Weapons), ToColorComponent(sector.Value.Thrusters, universe.MaxValue.Thrusters), ToColorComponent(sector.Value.Shields, universe.MaxValue.Shields)));
							var pen = new Pen(Color.FromArgb(ToColorComponent(sector.Value.Hull, universe.MaxValue.Hull), Color.White), hexBorder);
							g.DrawEllipse(pen, pos.X - halfHex / 1.25f, pos.Y - halfHex / 1.75f, fullHex / 1.2f, fullHex / 1.75f);
							g.FillEllipse(brush, pos.X - halfHex / 2f, pos.Y - halfHex / 2f, fullHex / 2f, fullHex / 2f);
							g.DrawEllipse(Pens.White, pos.X - halfHex / 2f, pos.Y - halfHex / 2f, fullHex / 2f, fullHex / 2f);
						}
						// highlight mothership hexes
						if (sector.Ships.Any(ship => ship.IsMothership))
						{
							Color msColor;
							if (sector.Ships.Any(ship => ship.IsMothership && ship.IsOurs))
							{
								if (sector.Ships.Any(ship => ship.IsMothership && !ship.IsOurs))
									msColor = Color.White;
								else
									msColor = Color.Blue;
							}
							else
								msColor = Color.Red;
							var msPen = new Pen(msColor, hexBorder);
							g.DrawPolygon(msPen, MakeHexagon(halfHex, pos));
						}
						// highlight selected hex
						if (sector == SelectedSector)
						{
							var selPen = new Pen(Color.White, hexBorder);
							g.DrawPolygon(selPen, MakeHexagon(halfHex + hexBorder, pos));
						}
					}
				}
				// draw movement arrows for highlighted ships
				// TODO - draw arrows in ghosted hexes too!
				foreach (var ship in HighlightedShips)
				{
					var pos = new PointF(ship.Sector.Coordinates.X * Zoom, ship.Sector.Coordinates.Y * Zoom).HexToScreen();
					pos += new SizeF(Size.Width / 2f, Size.Height / 2f);
					pos = new PointF(pos.X + Origin.X, pos.Y + Origin.Y);

					if (ship.PreviousSector != null)
					{
						var fleet = HighlightedShips.Where(s => s.Sector == ship.Sector && s.PreviousSector == ship.PreviousSector && s.IsOurs == ship.IsOurs);
						if (fleet == null || fleet.Count() == 0)
							continue;
						var color = ship.IsOurs ? (fleet.Any(s => s.IsMothership) ? Color.Cyan : Color.Blue) : (fleet.Any(s => s.IsMothership) ? Color.Salmon : Color.Red);
						var moveDir = ship.PreviousDirection;
						if (moveDir != null && moveDir != Direction.None)
						{
							var pen = new Pen(new HatchBrush(HatchStyle.Percent20, color, Color.Transparent));
							pen.Width = (float)Math.Sqrt(fleet.Sum(fleetShip => fleetShip.Size)) * Zoom / 32;
							if (pen.Width > Zoom / 4)
								pen.Width = Zoom / 4;
							pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
							var offset = new PointF(moveDir.Offset.Width * Zoom / 2f, moveDir.Offset.Height * Zoom / 2f).HexToScreen();
							var oldPos = new PointF(pos.X - offset.X, pos.Y - offset.Y);
							var bgPen = (Pen)pen.Clone();
							bgPen.Width += 2;
							bgPen.Brush = new SolidBrush(Color.Black);
							g.DrawLine(bgPen, oldPos, pos);
							g.DrawLine(pen, oldPos, pos);
						}
					}
					if (ship.TargetSector != null)
					{
						var fleet = HighlightedShips.Where(s => s.Sector == ship.Sector && s.TargetSector == ship.TargetSector && s.IsOurs == ship.IsOurs);
						if (fleet == null || fleet.Count() == 0)
							continue;
						var color = ship.IsOurs ? (fleet.Any(s => s.IsMothership) ? Color.Cyan : Color.Blue) : (fleet.Any(s => s.IsMothership) ? Color.Salmon : Color.Red);
						var moveDir = ship.TargetDirection;
						if (moveDir != null && moveDir != Direction.None)
						{
							var pen = new Pen(color);
							pen.Width = (float)Math.Sqrt(fleet.Sum(fleetShip => fleetShip.Size)) * Zoom / 32;
							if (pen.Width > Zoom / 4)
								pen.Width = Zoom / 4;
							pen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
							var offset = new PointF(moveDir.Offset.Width * Zoom / 2f, moveDir.Offset.Height * Zoom / 2f).HexToScreen();
							var newPos = new PointF(pos.X + offset.X, pos.Y + offset.Y);
							var bgPen = (Pen)pen.Clone();
							bgPen.Width += 2;
							bgPen.Brush = new SolidBrush(Color.Black);
							g.DrawLine(bgPen, pos, newPos);
							g.DrawLine(pen, pos, newPos);
						}
					}
				}
				for (var xHex = MinGhostCoords.X; xHex <= MaxGhostCoords.X; xHex++)
				{
					for (var yHex = MinGhostCoords.Y; yHex <= MaxGhostCoords.Y; yHex++)
					{
						var hexCoords = new Point(xHex, yHex);
						var realHexCoords = Universe.NormalizeHex(hexCoords);
						var sector = Universe.Sectors[realHexCoords];
						var pos = new PointF(xHex * Zoom, yHex * Zoom).HexToScreen();
						pos += new SizeF(Size.Width / 2f + Origin.X, Size.Height / 2f + Origin.Y);
						g.DrawString(sector.DisplayName, new Font(Font.FontFamily, Font.Size * Zoom / 64), Brushes.Black, pos - new SizeF(-Zoom / 64, -Zoom / 64), sf);
						g.DrawString(sector.DisplayName, new Font(Font.FontFamily, Font.Size * Zoom / 64), Brushes.White, pos, sf);
					}
				}
			}
		}

		private static PointF[] MakeHexagon(float radius, PointF center)
		{
			float alot = radius / (float)Math.Cos(Math.PI / 6f);
			float abit = alot * (float)Math.Sin(Math.PI / 6f);
			float some = radius;
			return new PointF[] {
					new PointF(center.X - abit, center.Y - some),
					new PointF(center.X + abit, center.Y - some),
					new PointF(center.X + alot, center.Y),
					new PointF(center.X + abit, center.Y + some),
					new PointF(center.X - abit, center.Y + some),
					new PointF(center.X - alot, center.Y),
			};
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (Enabled && AllowZoomingAndPanning)
			{
				var factor = (float)Math.Pow(1.001, e.Delta);
				var oldzoom = Zoom;
				Zoom *= factor;
				var realfactor = Zoom / oldzoom;
				var dfactor = realfactor - 1;
				var dx = (e.X - Width / 2) * dfactor;
				var dy = (e.Y - Height / 2) * dfactor;
				var rx = Origin.X * realfactor - dx;
				var ry = Origin.Y * realfactor - dy;
				Origin = new PointF(rx, ry);
				Invalidate();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button == MouseButtons.Middle && Enabled && AllowZoomingAndPanning)
			{
				Origin = new PointF(Origin.X + e.X - lastMousePos.X, Origin.Y + e.Y - lastMousePos.Y);
				Invalidate();
			}

			lastMousePos = e.Location;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Up)
			{
				Origin = new PointF(Origin.X, Origin.Y + Zoom);
				Invalidate();
			}
			else if (e.KeyCode == Keys.Down)
			{
				Origin = new PointF(Origin.X, Origin.Y - Zoom);
				Invalidate();
			}
			else if (e.KeyCode == Keys.Left)
			{
				Origin = new PointF(Origin.X + Zoom, Origin.Y);
				Invalidate();
			}
			else if (e.KeyCode == Keys.Right)
			{
				Origin = new PointF(Origin.X - Zoom, Origin.Y);
				Invalidate();
			}
			else if (e.KeyCode == Keys.PageUp)
			{
				var factor = (float)Math.Pow(1.001, 120); // 120 is the standard mouse wheel tick delta
				Zoom *= factor;
				Origin = new PointF(Origin.X * factor, Origin.Y * factor); // HACK - why should the origin change when zooming? maybe the drawing code is wrong
				Invalidate();
			}
			else if (e.KeyCode == Keys.PageDown)
			{
				var factor = (float)Math.Pow(1.001, -120); // 120 is the standard mouse wheel tick delta
				Zoom *= factor;
				Origin = new PointF(Origin.X * factor, Origin.Y * factor); // HACK - why should the origin change when zooming? maybe the drawing code is wrong
				Invalidate();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// http://stackoverflow.com/a/1616965/1159763
			if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
			{
				OnKeyDown(new KeyEventArgs(keyData));
				return true;
			}
			else
				return base.ProcessCmdKey(ref msg, keyData);

		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			Invalidate();
		}

		private int ToColorComponent(double value, double max)
		{
			return (int)(value * value / max / max * 255);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (Universe != null)
			{
				var screen = new PointF((e.X - Origin.X - Width / 2f) / Zoom, (e.Y - Origin.Y - Height / 2f) / Zoom);
				var hex = screen.ScreenToHex();
				// don't forget the ghosty hexes!
				var nearby = new PointF[]
						{
							new PointF((float)Math.Floor(hex.X), (float)Math.Floor(hex.Y)),
							new PointF((float)Math.Floor(hex.X), (float)Math.Ceiling(hex.Y)),
							new PointF((float)Math.Ceiling(hex.X), (float)Math.Floor(hex.Y)),
							new PointF((float)Math.Ceiling(hex.X), (float)Math.Ceiling(hex.Y)),
						};
				List<PointF> withGhosts = new List<PointF>();
				for (var x = -1; x <= 1; x++)
				{
					for (var y = -1; y <= 1; y++)
					{
						foreach (var p in nearby)
							withGhosts.Add(new PointF(p.X + x * Universe.Width, p.Y + y * Universe.Height + x * Universe.Height / 2));
					}
				}
				var nearest = withGhosts.OrderBy(h =>
				{
					var s = h.HexToScreen();
					return Math.Sqrt((s.X - screen.X) * (s.X - screen.X) + (s.Y - screen.Y) * (s.Y - screen.Y));
				}).First();
				/*if (e.Button == MouseButtons.Left)
				{
					// TODO - make these properties of universe
					var xHexStart = -Universe.Width / 2;
					var yHexStart = -Universe.Height / 2;
					var xHexEnd = Universe.Width / 2 - 1;
					var yHexEnd = (Universe.Height - 1) / 2;
					while (nearest.X < xHexStart)
					{
						nearest.X += universe.Width;
						nearest.Y += universe.Width / 2;
					}
					while (nearest.X > xHexEnd)
					{
						nearest.X -= universe.Width;
						nearest.Y -= universe.Width / 2;
					}
					while (nearest.Y < yHexStart)
						nearest.Y += universe.Height;
					while (nearest.Y > yHexEnd)
						nearest.Y -= universe.Height;
				}*/
				var sector = Universe.Sectors[Universe.NormalizeHex(Point.Round(nearest))];
				if (sector != null)
				{
					if (SectorMouseDown != null)
						SectorMouseDown(this, new MouseEventArgs(e.Button, e.Clicks, sector.Coordinates.X, sector.Coordinates.Y, e.Delta));
				}
			}
		}

		public event MouseEventHandler SectorMouseDown;

		public void CenterOn(Point p)
		{
			Origin = new PointF(-p.X * Zoom, -p.Y * Zoom).HexToScreen();
		}

		public bool AllowZoomingAndPanning
		{
			get;
			set;
		}

		private PointF NormalizePan(PointF pan)
		{
			Universe u = universe;
			if (u == null)
				return new PointF();
			PointF upperLeft = new PointF((float)u.MinX / 2f, (float)u.MaxY / 2f + 0.5f).HexToScreen();
			PointF lowerRight = new PointF((float)u.MaxX / 2f + 0.5f, (float)u.MinY / 2f - 0.5f).HexToScreen();
			pan.X = pan.X.Normalize(upperLeft.X * zoom, lowerRight.Y * zoom);
			pan.Y = pan.Y.Normalize(upperLeft.Y * zoom, lowerRight.Y * zoom);
			return pan;
		}

		/// <summary>
		/// The minimum hex coordinates that could be displayed on the screen.
		/// </summary>
		private Point MinGhostCoords
		{
			get
			{
				float x = (Origin.X - Width) / Zoom;
				float y = (Origin.Y + Height) / Zoom;
				float hx = new PointF(x, (float)-Height / Zoom).ScreenToHex().X;
				float hy = new PointF((float)Width / Zoom, y).ScreenToHex().Y;
				return new Point((int)Math.Floor(hx), (int)Math.Ceiling(hy));
			}
		}

		/// <summary>
		/// The maximum hex coordinates that could be displayed on the screen.
		/// </summary>
		private Point MaxGhostCoords
		{
			get
			{
				float x = (Origin.X + Width) / Zoom;
				float y = (Origin.Y - Height) / Zoom;
				float hx = new PointF(x, (float)Height / Zoom).ScreenToHex().X;
				float hy = new PointF((float)-Width / Zoom, y).ScreenToHex().Y;
				return new Point((int)Math.Ceiling(hx), (int)Math.Floor(hy));
			}
		}
	}
}