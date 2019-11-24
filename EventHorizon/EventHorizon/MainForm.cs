using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Eggs;
using EventHorizon.Properties;
using NAudio;
using NAudio.Wave;
using NAudio.Midi;

namespace EventHorizon
{
	public partial class MainForm : Form
	{
		// game stuff
		private Universe universe;
		private Sector sector;
		private IEnumerable<Ship> shipsInSector;
		private IEnumerable<Ship> allShips;
		private List<Ship> selectedShips;
		private string gameName; // e.g. edsj
		private int turnNumber; // e.g. 30
		private string playerName; // e.g. Nick
		private string plrFileName; // e.g. edsj_30_Nick.plr

		// audio stuff
		private IWavePlayer waveOut;
		private WaveChannel32 volume;

		public string GameFileName { get; private set; }

		public MainForm()
		{
			InitializeComponent();
		}

		public MainForm(string gameFileName)
			: this()
		{
			GameFileName = gameFileName;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			if (GameFileName == null)
			{
				var dlg = new OpenFileDialog();
				dlg.Filter = "Savegames (*.gam)|*.gam|All files|*.*";
				dlg.Title = "Please select a savegame to load";
				var result = dlg.ShowDialog();
				if (result != DialogResult.OK)
				{
					Application.Exit();
					return;
				}
				GameFileName = dlg.FileName;
			}

			var fname = Path.GetFileNameWithoutExtension(GameFileName);
			plrFileName = Path.Combine(Path.GetDirectoryName(GameFileName), fname) + ".plr";
			try
			{
				var splitFileName = fname.Split('_');
				gameName = splitFileName[0];
				turnNumber = int.Parse(splitFileName[1]);
				playerName = splitFileName[2];
				Text = string.Format("Event Horizon - {0} turn {1}", gameName, turnNumber, playerName);
			}
			catch
			{
				Text = "Event Horizon";
			}
			try
			{
				var doc = XDocument.Load(GameFileName);
				var root = doc.Root;
				var shelf = NounCollection.FromXml(root);
				Library.NounLibrary.Add(shelf);


				// TODO - try to load commands from existing plr file?

				universe = Universe.Load(shelf["Vroot"]);
				map.Universe = universe;
				LoadEvents();
				dgvShips.AutoGenerateColumns = false;
				selectedShips = new List<Ship>();


				foreach (var sector in universe.Sectors)
				{
					foreach (var ship in sector.Ships)
					{
						var bmp = new Bitmap(128, 128);
						var g = Graphics.FromImage(bmp);
						g.Clear(Color.Black);
						ilShipsBig.Images.Add(ship.ID, bmp);

						ilShipsSmall.Images.Add(ship.ID, bmp);
					}
				}

				map.HighlightedShips = universe.Sectors.SelectMany(s => s.Ships);

				dgvIncome.AutoGenerateColumns = false;
				dgvIncome.DataSource = universe.IncomeSectors.ToArray();

				dgvConstruction.AutoGenerateColumns = false;
				foreach (var design in universe.Designs)
					colDesign.Items.Add(design.Name);
				bsConstruction.DataSource = universe.Construction;
				dgvConstruction.DataSource = bsConstruction;

				UpdateTreasury();

				dgvBattles.AutoGenerateColumns = false;
				dgvBattles.DataSource = universe.Battles;

				dgvEmpires.AutoGenerateColumns = false;
				foreach (var dir in Directory.GetDirectories(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Shipsets")).Select(dir => Path.GetFileName(dir)).Union(universe.Players.Select(p => p.ShipsetName)))
					colShipset.Items.Add(dir);
				dgvEmpires.DataSource = universe.Players.ToArray();
				allShips = SortShipsDefault(universe.Ships).ToArray();
			}
			catch (System.Xml.XmlException ex)
			{
				MessageBox.Show(GameFileName + "\nis not a valid Event Horizon savegame.");
				Application.Exit();
			}
			catch (FileNotFoundException ex)
			{
				MessageBox.Show($"{GameFileName} does not exist.");
				Application.Exit();
			}
			catch (DirectoryNotFoundException ex)
			{
				MessageBox.Show($"{GameFileName} does not exist.");
				Application.Exit();
			}
			// load settings
			Settings.Default.Reload();

			// start music if we have any audio devices to play on
			Music.CurrentMood = Mood.Strategic;

			// TODO - make a settings dialog!
			Settings.Default.Save();
		}

		private void splMain_SplitterMoved(object sender, SplitterEventArgs e)
		{
			// TODO - focus what used to have focus (not necessarily the map)
			map.Focus();
		}

		private void map_SectorMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				SetSector(universe.Sectors[e.Location]);
			}
			else if (e.Button == MouseButtons.Right)
			{
				TryMoveShips(universe.Sectors[universe.NormalizeHex(e.Location)]);
			}
		}

		private void txtSectorName_TextChanged(object sender, EventArgs e)
		{
			if (sector != null && txtSectorName.Text != sector.DisplayName)
			{
				sector.DisplayName = txtSectorName.Text;
				LoadEvents();
				sector.IsRenamed = true;
			}
		}

		private void dgvShips_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.ColumnIndex == 0)
				{
					// revert to default sort
					shipsInSector = SortShipsDefault(shipsInSector).ToArray();
					allShips = SortShipsDefault(allShips).ToArray();
				}
				else if (e.ColumnIndex == 1)
				{
					// sort by owner, put ours on top
					shipsInSector = sector == null ? null : sector.Ships.OrderByDescending(ship => ship.IsOurs).ThenBy(ship => ship.OwnerTeamName).ToArray();
					allShips = universe.Ships.OrderByDescending(ship => ship.IsOurs).ThenBy(ship => ship.OwnerTeamName).ToArray();
				}
				else if (e.ColumnIndex == 2)
				{
					// sort by location
					shipsInSector = sector == null ? null : sector.Ships.OrderBy(ship => ship.Sector.DisplayName).ToArray();
					allShips = universe.Ships.OrderBy(ship => ship.Sector.DisplayName).ToArray();
				}
				else if (e.ColumnIndex == 3)
				{
					// sort by name
					shipsInSector = sector == null ? null : sector.Ships.OrderBy(ship => ship.DisplayName).ToArray();
					allShips = universe.Ships.OrderBy(ship => ship.DisplayName).ToArray();
				}
				else if (e.ColumnIndex == 4)
				{
					// put motherships on top
					shipsInSector = sector == null ? null : sector.Ships.OrderByDescending(ship => ship.IsMothership).ToArray();
					allShips = universe.Ships.OrderByDescending(ship => ship.IsMothership).ToArray();
				}
				else if (e.ColumnIndex == 5)
				{
					// put armed ships on top
					shipsInSector = sector == null ? null : sector.Ships.OrderByDescending(ship => ship.IsArmed).ToArray();
					allShips = universe.Ships.OrderByDescending(ship => ship.IsArmed).ToArray();
				}
				else if (e.ColumnIndex == 6)
				{
					// put mobile ships on top
					shipsInSector = sector == null ? null : sector.Ships.OrderByDescending(ship => ship.IsMobile).ToArray();
					allShips = universe.Ships.OrderByDescending(ship => ship.IsMobile).ToArray();
				}
				else if (e.ColumnIndex == 7)
				{
					// put biggest ships on top
					shipsInSector = sector == null ? null : sector.Ships.OrderByDescending(ship => ship.Size).ToArray();
					allShips = universe.Ships.OrderByDescending(ship => ship.Size).ToArray();
				}
				dgvShips.DataSource = chkGlobalShipList.Checked ? allShips : shipsInSector;
				selectedShips.Clear();
			}
		}

		private void map_MouseDown(object sender, MouseEventArgs e)
		{
			map.Focus();
		}

		private void map_DoubleClick(object sender, EventArgs e)
		{
			splUniverse.Panel2Collapsed = !splUniverse.Panel2Collapsed;
			splStatus.Panel2Collapsed = !splStatus.Panel2Collapsed;
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			// to get the tabs to resize properly
			Refresh();
		}

		private void map_Click(object sender, EventArgs e)
		{
			map.Focus();
		}

		private void dgvShips_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
		{

		}

		private void dgvShips_SelectionChanged(object sender, EventArgs e)
		{
			selectedShips.Clear();
			selectedShips.AddRange(dgvShips.SelectedRows.Cast<DataGridViewRow>().Select(row => (Ship)row.DataBoundItem));
			if (selectedShips.Count > 0)
			{
				moveMap.Universe = universe;
				if (!chkGlobalShipList.Checked)
				{
					moveMap.CenterOn(sector.Coordinates);
					moveMap.Visible = true;
				}
				else
					moveMap.Visible = false;
				moveMap.HighlightedShips = selectedShips;
				if (selectedShips.Count == 1)
				{
					var selShip = selectedShips.First();
					picShip.Image = selShip.Image;
					txtShipName.Text = selShip.DisplayName;
					txtShipName.Enabled = true;
					if (selShip.IsArmed)
						lblShipStatus.Text = "armed";
					else
						lblShipStatus.Text = "unarmed";
					if (selShip.IsMobile)
						lblShipStatus.Text += ", mobile";
					else
						lblShipStatus.Text += ", immobile";
					if (selShip.IsMothership)
						lblShipStatus.Text += ", mothership";
					lblShipSizeClass.Text = selShip.Size.ToString();
					if (selShip.Components != null)
					{
						lblShipHull.Text = selShip.Components.Hull.ToString();
						if (selShip.Armor > 0)
							lblShipArmor.Text = string.Format("(Armor: {0})", selShip.Armor);
						else
							lblShipArmor.Text = "(No Armor)";
						lblShipWeapons.Text = selShip.Components.Weapons.ToString();
						if (selShip.Components.Weapons > 1) // target lock is relevant only for ships with multiple weapons!
							lblShipTargetLock.Text = string.Format("(Target Lock: {0:p0})", 1.0 - Math.Pow(0.85, selShip.Components.Weapons));
						else
							lblShipTargetLock.Text = "";
						lblShipThrusters.Text = selShip.Components.Thrusters.ToString();
						if (selShip.Components.Thrusters > 0)
							lblShipMiningRate.Text = string.Format("(Mining Rate: {0:p0})", Math.Sqrt(selShip.Components.Thrusters));
						else
							lblShipMiningRate.Text = "";
						lblShipShields.Text = selShip.Components.Shields.ToString();
						if (selShip.Components.Shields > 0)
							lblShipShieldCharge.Text = string.Format("(Charge: {0} of {1})", selShip.ShieldCharge, selShip.MaximumShieldCharge);
						else
							lblShipShieldCharge.Text = "";
					}
					else
					{
						lblShipHull.Text = "Unknown";
						lblShipArmor.Text = "";
						lblShipWeapons.Text = "Unknown";
						lblShipTargetLock.Text = "";
						lblShipThrusters.Text = "Unknown";
						lblShipMiningRate.Text = "";
						lblShipShields.Text = "Unknown";
						lblShipShieldCharge.Text = "";
					}
				}
				else
				{
					picShip.Image = null; // TODO - fleet image?
					txtShipName.Text = selectedShips.Count + " ships";
					txtShipName.Enabled = false;
					lblShipSizeClass.Text = selectedShips.Sum(ship => ship.Size).ToString();
					// show summation of all comps in fleet if all ships are friendly
					if (selectedShips.All(ship => ship.IsOurs))
					{
						lblShipStatus.Text = "Various";
						lblShipHull.Text = selectedShips.Sum(ship => ship.Components.Hull).ToString();
						lblShipArmor.Text = "(Total Armor: " + selectedShips.Sum(ship => ship.Armor) + ")";
						lblShipWeapons.Text = selectedShips.Sum(ship => ship.Components.Weapons).ToString();
						lblShipTargetLock.Text = "";
						lblShipThrusters.Text = selectedShips.Sum(ship => ship.Components.Thrusters).ToString();
						lblShipMiningRate.Text = string.Format("(Mining Rate: {0:p0})", Math.Sqrt(selectedShips.Max(ship => ship.Components.Thrusters)));
						lblShipShields.Text = selectedShips.Sum(ship => ship.Components.Shields).ToString();
						lblShipShieldCharge.Text = string.Format("(Total Charge: {0} of {1})", selectedShips.Sum(ship => ship.ShieldCharge), selectedShips.Sum(ship => ship.MaximumShieldCharge));
					}
					else
					{
						lblShipStatus.Text = "Various";
						lblShipHull.Text = "Various";
						lblShipArmor.Text = "";
						lblShipWeapons.Text = "Various";
						lblShipTargetLock.Text = "";
						lblShipThrusters.Text = "Various";
						lblShipMiningRate.Text = "";
						lblShipShields.Text = "Various";
						lblShipShieldCharge.Text = "";
					}
				}
			}
			else
			{
				moveMap.Universe = null;
				picShip.Image = null;
				txtShipName.Text = "";
				txtShipName.Enabled = false;
				lblShipStatus.Text = "--";
				lblShipHull.Text = "--";
				lblShipArmor.Text = "";
				lblShipWeapons.Text = "--";
				lblShipTargetLock.Text = "";
				lblShipThrusters.Text = "--";
				lblShipMiningRate.Text = "";
				lblShipShields.Text = "--";
				lblShipShieldCharge.Text = "";
			}
		}

		private void moveMap_SectorMouseDown(object sender, MouseEventArgs e)
		{
			var target = universe.Sectors[e.Location];
			TryMoveShips(target);
		}

		private void TryMoveShips(Sector target)
		{
			foreach (var ship in selectedShips.Where(ship => ship.IsMobile && ship.IsOurs))
			{
				if (ship.Sector.IsAdjacentTo(target))
					ship.TargetSector = target; // move command
				else if (ship.Sector == target)
					ship.TargetSector = null; // halt command
			}
			map.Invalidate();
			moveMap.Invalidate();
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			var done = false;
			if (universe == null)
				done = true;
			while (!done)
			{
				var result = MessageBox.Show("Save your commands before quitting?", "Event Horizon", MessageBoxButtons.YesNoCancel);
				if (result == DialogResult.Yes)
					done = SaveCommands();
				else if (result == DialogResult.No)
					done = true;
				else if (result == DialogResult.Cancel)
				{
					e.Cancel = true;
					done = true;
				}
			}
			if (e.Cancel)
				Music.CurrentMood = Mood.Strategic;
		}

		private void txtShipName_TextChanged(object sender, EventArgs e)
		{
			if (selectedShips.Count == 1 && selectedShips[0].DisplayName != txtShipName.Text)
			{
				selectedShips[0].DisplayName = txtShipName.Text;
				selectedShips[0].IsRenamed = true;
			}
		}

		private void UpdateTreasury()
		{
			var mined = universe.IncomeSectors.Select(sector => sector.Income).Aggregate((r1, r2) => r1 + r2);
			var spent = universe.Expenditures;
			var stored = universe.Treasury - mined;
			var left = universe.Treasury - spent;

			SetBudgetLabel(lblHullLeft, left.Hull);
			SetBudgetLabel(lblWeaponsLeft, left.Weapons);
			SetBudgetLabel(lblThrustersLeft, left.Thrusters);
			SetBudgetLabel(lblShieldsLeft, left.Shields);

			SetBudgetLabel(lblHullMined, mined.Hull);
			SetBudgetLabel(lblWeaponsMined, mined.Weapons);
			SetBudgetLabel(lblThrustersMined, mined.Thrusters);
			SetBudgetLabel(lblShieldsMined, mined.Shields);

			SetBudgetLabel(lblHullSpent, spent.Hull);
			SetBudgetLabel(lblWeaponsSpent, spent.Weapons);
			SetBudgetLabel(lblThrustersSpent, spent.Thrusters);
			SetBudgetLabel(lblShieldsSpent, spent.Shields);

			SetBudgetLabel(lblHullStored, stored.Hull);
			SetBudgetLabel(lblWeaponsStored, stored.Weapons);
			SetBudgetLabel(lblThrustersStored, stored.Thrusters);
			SetBudgetLabel(lblShieldsStored, stored.Shields);

		}

		private void SetLabel(Label lbl, object value)
		{
			lbl.Text = value.ToString();
		}

		private void SetBudgetLabel(Label label, double value)
		{
			if (value < 0)
			{
				SetLabel(label, "(" + -(int)value + ")");
				label.ForeColor = Color.Red;
			}
			else
			{
				SetLabel(label, (int)value);
				label.ForeColor = Color.Black;
			}
		}

		private void dgvConstruction_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			if (e.ColumnIndex > 1)
				dgvConstruction.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
		}

		private void dgvConstruction_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			var row = dgvConstruction.Rows[e.RowIndex];
			var item = (ConstructionItem)row.DataBoundItem;
			var design = universe.Designs.FirstOrDefault(d => d.Name == (item == null ? null : item.Name));
			row.Cells[1].ReadOnly = false;
			row.Cells[2].ReadOnly = false;
			row.Cells[3].ReadOnly = false;
			row.Cells[4].ReadOnly = false;
			row.Cells[5].ReadOnly = false;
			if (e.ColumnIndex == 0 && design != null)
			{
				// load existing design
				item.Weapons = design.Weapons;
				item.Thrusters = design.Thrusters;
				item.Shields = design.Shields;
				item.Armor = design.Armor;
				row.Cells[1].ReadOnly = true;
				row.Cells[2].ReadOnly = true;
				row.Cells[3].ReadOnly = true;
				row.Cells[4].ReadOnly = true;
				row.Cells[5].ReadOnly = true;
			}
			else if (item != null && item.Name != null && !colDesign.Items.Contains(item.Name))
			{
				colDesign.Items.Add(item.Name);
			}
			UpdateTreasury();
		}

		private void dgvBattles_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (dgvBattles.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
			{
				var battle = (Battle)dgvBattles.Rows[e.RowIndex].DataBoundItem;
				if (battle.Salvos.Count() > 0)
				{
					var form = new BattleReplayForm();
					form.Battle = battle;
					form.ShowDialog();
				}
				else
				{
					MessageBox.Show("No shots were fired in this encounter.");
				}
			}
		}

		private void MainForm_HelpButtonClicked(object sender, CancelEventArgs e)
		{
			try
			{
				Process.Start("Rules.html");
			}
			catch
			{
				MessageBox.Show("Could not load Rules.html.");
			}
		}

		private void mHelpHelp_Click(object sender, EventArgs e)
		{
			try
			{
				Process.Start("Rules.html");
			}
			catch
			{
				MessageBox.Show("Could not load Rules.html.");
			}
		}

		private void mHelpAbout_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Event Horizon GUI Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
		}

		private void mFileSave_Click(object sender, EventArgs e)
		{
			if (universe != null)
				SaveCommands();
		}

		private bool SaveCommands()
		{
			try
			{
				var sc = universe.CommandSubjects;
				if (!Library.SubjectLibrary.Contains(sc))
					Library.SubjectLibrary.Add(sc);
				var xml = sc.ToXml();
				var sw = new StreamWriter(plrFileName);
				sw.Write(xml.ToString());
				sw.Close();
				MessageBox.Show("Your commands have been saved to:\n" + plrFileName);
				return true;
			}
			catch (Exception ex)
			{
				// TODO - log exception details in errorlog.txt
				MessageBox.Show("Could not save your commands!\n" + ex.GetType() + "\n" + ex.Message);
				return false;
			}
		}

		private void mFileExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void dgvEmpires_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			var plr = (Player)dgvEmpires.Rows[e.RowIndex].DataBoundItem;
			foreach (var ship in universe.Ships.Where(ship => ship.Owner == plr))
			{
				var s = ship; // needed to make the ship "real" or something for the linqy stuff
				ship.Image = new Cache<Image>(() => s.Owner.Shipset.BuildShipImage(128, s.Config));
				ship.SmallImage = new Cache<Image>(() => s.Owner.Shipset.BuildShipImage(24, s.Config));
			}
		}

		private void LoadEvents()
		{
			lstEvents.Items.Clear();
			foreach (var evt in universe.Events)
			{
				// hide boring events
				if (evt.StartsWith("Our ship, ") && evt.Contains(" has moved to "))
					continue;

				// TODO - condense construction events for multibuild

				// show custom sector names
				var evt2 = evt;
				foreach (var sector in universe.Sectors)
					evt2 = evt2.Replace(sector.RealName, sector.DisplayName);
				lstEvents.Items.Add(evt2);
			}
		}

		private void dgvBattles_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				SetSector(((Battle)dgvBattles.Rows[e.RowIndex].DataBoundItem).Location);
			}
		}

		private void dgvIncome_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				SetSector((Sector)dgvIncome.Rows[e.RowIndex].DataBoundItem);
			}
		}

		private void SetSector(Sector sector)
		{
			miniMap.Universe = map.Universe;
			miniMap.CenterOn(sector.Coordinates);
			this.sector = sector;
			map.SelectedSector = sector;
			txtSectorName.Text = sector.DisplayName;
			if (sector.Value != null)
			{
				lblHullValue.Text = sector.Value.Hull.ToString("f2");
				lblWeaponValue.Text = sector.Value.Weapons.ToString("f2");
				lblThrusterValue.Text = sector.Value.Thrusters.ToString("f2");
				lblShieldValue.Text = sector.Value.Shields.ToString("f2");
			}
			else
			{
				lblHullValue.Text = "Unknown";
				lblWeaponValue.Text = "Unknown";
				lblThrusterValue.Text = "Unknown";
				lblShieldValue.Text = "Unknown";
			}
			if (sector.Income != null)
			{
				lblSectorHullMined.Text = sector.Income.Hull.ToString();
				lblSectorWeaponsMined.Text = sector.Income.Weapons.ToString();
				lblSectorThrustersMined.Text = sector.Income.Thrusters.ToString();
				lblSectorShieldsMined.Text = sector.Income.Shields.ToString();
			}
			else
			{
				lblSectorHullMined.Text = "None";
				lblSectorWeaponsMined.Text = "None";
				lblSectorThrustersMined.Text = "None";
				lblSectorShieldsMined.Text = "None";
			}
			lblSectorCoordinates.Text = sector.Coordinates.X.ToString() + ", " + sector.Coordinates.Y.ToString();
			if (!chkGlobalShipList.Checked)
			{
				dgvShips.DataSource = shipsInSector = SortShipsDefault(sector.Ships).ToArray();
				dgvShips.ClearSelection();
			}
		}

		private void chkGlobalShipList_CheckedChanged(object sender, EventArgs e)
		{
			dgvShips.DataSource = chkGlobalShipList.Checked ? allShips : shipsInSector;
			selectedShips.Clear();
		}

		private IEnumerable<Ship> SortShipsDefault(IEnumerable<Ship> ships)
		{
			return ships.OrderByDescending(s => s.IsMothership).ThenByDescending(s => s.IsMobile).ThenByDescending(s => s.IsArmed).ThenByDescending(s => s.Size);
		}

		private void dgvBattles_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var battles = (IEnumerable<Battle>)dgvBattles.DataSource;
			if (e.Button == MouseButtons.Left)
			{
				if (e.ColumnIndex == 0)
				{
					// sort by X coordinate
					battles = battles.OrderBy(b => b.X);
				}
				else if (e.ColumnIndex == 1)
				{
					// sort by Y coordinate
					battles = battles.OrderBy(b => b.Y);
				}
				else if (e.ColumnIndex == 2)
				{
					// sort by location
					battles = battles.OrderBy(b => b.LocationName);
				}
				else if (e.ColumnIndex == 3)
				{
					// sort by our fleet strength
					battles = battles.OrderByDescending(b => b.OurStrengthBefore);
				}
				else if (e.ColumnIndex == 4)
				{
					// sort by enemy fleet strength
					battles = battles.OrderByDescending(b => b.EnemyStrengthBefore);
				}
				else if (e.ColumnIndex == 5)
				{
					// sort by outcome
					battles = battles.OrderByDescending(b => b.Outcome);
				}
			}
			dgvBattles.DataSource = battles.ToArray();
		}

		private void dgvIncome_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var income = (IEnumerable<Sector>)dgvIncome.DataSource;
			if (e.Button == MouseButtons.Left)
			{
				if (e.ColumnIndex == 0)
				{
					// sort by X coordinate
					income = income.OrderBy(s => s.X);
				}
				else if (e.ColumnIndex == 1)
				{
					// sort by Y coordinate
					income = income.OrderBy(s => s.Y);
				}
				else if (e.ColumnIndex == 2)
				{
					// sort by location name
					income = income.OrderBy(s => s.DisplayName);
				}
				else if (e.ColumnIndex == 3)
				{
					// sort by hull income
					income = income.OrderByDescending(s => s.HullIncome);
				}
				else if (e.ColumnIndex == 4)
				{
					// sort by weapon income
					income = income.OrderByDescending(s => s.WeaponIncome);
				}
				else if (e.ColumnIndex == 5)
				{
					// sort by thruster income
					income = income.OrderByDescending(s => s.ThrusterIncome);
				}
				else if (e.ColumnIndex == 6)
				{
					// sort by shield income
					income = income.OrderByDescending(s => s.ShieldIncome);
				}
			}
			dgvIncome.DataSource = income.ToArray();
		}
	}
}
