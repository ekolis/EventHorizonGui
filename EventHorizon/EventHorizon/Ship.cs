using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Reflection;
using Eggs;

namespace EventHorizon
{
	/// <summary>
	/// A ship! In SPAAAAACE!
	/// </summary>
	public class Ship
	{
		public static readonly Bitmap EmptyBitmap = CreatePixel(Color.Transparent);
		private static readonly string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static readonly Bitmap ArmedBitmap = File.Exists(Path.Combine(root, "fight.png")) ? new Bitmap(Path.Combine(root, "fight.png")) : CreatePixel(Color.Red);
		public static readonly Bitmap MobileBitmap = File.Exists(Path.Combine(root, "move.png")) ? new Bitmap(Path.Combine(root, "move.png")) : CreatePixel(Color.Green);
		public static readonly Bitmap MothershipBitmap = File.Exists(Path.Combine(root, "crown.png")) ? new Bitmap(Path.Combine(root, "crown.png")) : CreatePixel(Color.Blue);

		private static Bitmap CreatePixel(Color color)
		{
			var bmp = new Bitmap(1, 1);
			Graphics.FromImage(bmp).Clear(color);
			return bmp;
		}

		/// <summary>
		/// The ship ID.
		/// </summary>
		public string ID { get; private set; }

		/// <summary>
		/// The sector in which this ship is located, or null if the ship has been destroyed.
		/// </summary>
		public Sector Sector { get; private set; }

		/// <summary>
		/// The game universe.
		/// </summary>
		public Universe Universe { get; private set; }

		/// <summary>
		/// The sector in which this ship was previously located (if that data is known).
		/// </summary>
		public Sector PreviousSector { get; private set; }

		/// <summary>
		/// The sector which this ship is ordered to move to (only relevant if it's a friendly ship).
		/// </summary>
		public Sector TargetSector { get; set; }

		/// <summary>
		/// The name of the ship in the GAM/PLR files.
		/// </summary>
		public string RealName { get; private set; }

		/// <summary>
		/// The name of the ship as customized by the player. Defaults to the real name.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The owner of the ship.
		/// </summary>
		public Player Owner { get; private set; }

		/// <summary>
		/// Is this a mothership?
		/// </summary>
		public bool IsMothership { get; private set; }

		public string OwnerTeamName
		{
			get { return Owner.TeamName; }
		}

		/// <summary>
		/// Does this ship have any weapons?
		/// </summary>
		public bool IsArmed { get; private set; }

		/// <summary>
		/// Does this ship have thrusters?
		/// </summary>
		public bool IsMobile { get; private set; }

		/// <summary>
		/// Is this our ship, or an enemy ship?
		/// </summary>
		public bool IsOurs
		{
			get { return Owner == Universe.Us; }
		}

		/// <summary>
		/// The size class of the ship (proportional to square root of mass).
		/// This is a good rough estimate of a ship's "usefulness" or "strength", since mass is roughly proportional to non-hull components squared.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		private Ship(Sector sector, Universe universe)
		{
			Sector = sector;
			Universe = universe;
		}

		internal static Ship Load(NounCollection shelf, string id, Sector sector, Universe u)
		{
			var s = new Ship(sector, u);
			var noun = shelf[id];
			s.ID = id;
			s.RealName = noun["name"][0];
			s.Owner = u.Players.SingleOrDefault(p => p.TeamName == noun["owner"][0]);
			if (s.Owner == null)
				s.Owner = u.AddPlayer(noun["owner"][0]);
			if (shelf.Contains("Vnotes"))
			{
				if (shelf["Vnotes"].Contains("Name:" + s.ID))
					s.DisplayName = shelf["Vnotes"]["Name:" + s.ID][0];
				else if (s.RealName.StartsWith(s.Owner.TeamName + " "))
					s.DisplayName = s.RealName.Substring(s.Owner.TeamName.Length + 1);
				else
					s.DisplayName = s.RealName;
			}
			else if (s.RealName.StartsWith(s.Owner.TeamName + " "))
				s.DisplayName = s.RealName.Substring(s.Owner.TeamName.Length + 1);
			else
				s.DisplayName = s.RealName;
			s.IsMothership = noun["isMothership"][0] == 1;
			s.PreviousSector = u.Sectors.SingleOrDefault(sec =>
				{
					if (noun.Contains("previousLocation") && noun["previousLocation"].Count > 0)
						return noun["previousLocation"][0] == sec.ID;
					return false;
				});
			if (s.IsOurs)
			{
				s.Components = new Resources
				{
					Hull = noun["hull"][0],
					Weapons = noun["weapon"][0],
					Thrusters = noun["thruster"][0],
					Shields = noun["shield"][0],
				};
				s.Size = (int)(Math.Sqrt((int)noun["hull"][0] + (int)noun["weapon"][0] + (int)noun["thruster"][0] + (int)noun["shield"][0]));
				s.IsArmed = noun["weapon"][0] > 0;
				s.IsMobile = noun["thruster"][0] > 0;
				s.ShieldCharge = noun["shieldcharge"][0];
			}
			else
			{
				s.Size = noun["sizeClass"][0];
				var status = noun["status"][0];
				s.IsArmed = status == "normal" || status == "immobile";
				s.IsMobile = status == "normal" || status == "unarmed";
			}

			return s;
		}

		public Direction PreviousDirection
		{
			get
			{
				if (PreviousSector == null || Sector == null)
					return Direction.None;
				var offset = Universe.GetHexOffset(PreviousSector.Coordinates, Sector.Coordinates, true);
				try
				{
					return Universe.GetDirectionFromOffset(offset).First();
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("Couldn't get a direction from the offset between " + PreviousSector.Coordinates + " and " + Sector.Coordinates + " - computed offset: " + offset);
					System.Diagnostics.Debug.WriteLine("The ship that tried to move in a funky direction was: " + this.DisplayName);
					throw;
				}
			}
		}

		public Direction TargetDirection
		{
			get
			{
				if (TargetSector == null || Sector == null)
					return Direction.None;
				var offset = Universe.GetHexOffset(Sector.Coordinates, TargetSector.Coordinates, true);
				try
				{
					return Universe.GetDirectionFromOffset(offset).First();
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("Couldn't get a direction from the offset between " + Sector.Coordinates + " and " + TargetSector.Coordinates + " - computed offset: " + offset);
					System.Diagnostics.Debug.WriteLine("The ship that tried to move in a funky direction was: " + this.DisplayName);
					throw;
				}
			}
		}

		public Cache<Image> Image
		{
			get;
			internal set;
		}

		public Image SmallImage
		{
			get;
			internal set;
		}

		public Resources Components
		{
			get;
			private set;
		}

		public int ShieldCharge
		{
			get;
			private set;
		}

		public int MaximumShieldCharge
		{
			get { return (int)(Components == null ? 0 : Math.Max(0, Components.Shields * 2 - 1)); }
		}

		public int Armor
		{
			get { return (int)(Components == null ? 0 : (Components.Hull - RequiredHull)); }
		}

		public int Internals
		{
			get { return (int)(Components == null ? 0 : (Components.Weapons + Components.Thrusters + Components.Shields)); }
		}

		public int RequiredHull
		{
			get { return Components == null ? 0 : (int)Math.Ceiling((Math.Pow(Internals, 2.0) - 1.0) / 4.0); }
		}

		/// <summary>
		/// Has this ship been renamed this turn? If so, it will need a note created in the PLR file.
		/// </summary>
		public bool IsRenamed { get; set; }

		public Image ArmedStatusImage
		{
			get
			{
				if (IsArmed)
					return ArmedBitmap;
				else
					return EmptyBitmap;
			}
		}

		public Image MobileStatusImage
		{
			get
			{
				if (IsMobile)
					return MobileBitmap;
				else
					return EmptyBitmap;
			}
		}

		public Image MothershipStatusImage
		{
			get
			{
				if (IsMothership)
					return MothershipBitmap;
				else
					return EmptyBitmap;
			}
		}

		public ShipConfig Config
		{
			get
			{
				return new ShipConfig { IsArmed = IsArmed, IsMobile = IsMobile, IsMothership = IsMothership, ProportionalSize = (float)Size / (float)Universe.MaximumShipSize };
			}
		}
	}
}