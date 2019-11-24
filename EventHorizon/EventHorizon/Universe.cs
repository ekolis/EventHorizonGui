using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Eggs;

namespace EventHorizon
{
	/// <summary>
	/// The game universe, as viewed by the player.
	/// </summary>
	public class Universe
	{
		/// <summary>
		/// The width of the universe (along the X-axis) in hexes. Should be an even number so the toroidal map is topologically consistent.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// The height of the universe (along the Y-axis) in hexes.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// The turn number.
		/// </summary>
		public int Turn { get; private set; }

		/// <summary>
		/// The ID of the player.
		/// </summary>
		public string PlayerID { get; private set; }

		/// <summary>
		/// The sectors in the universe.
		/// </summary>
		public SectorCollection Sectors { get; private set; }

		/// <summary>
		/// The players in the universe.
		/// </summary>
		public IEnumerable<Player> Players
		{
			get
			{
				return players;
			}
		}

		private List<Player> players;

		/// <summary>
		/// Turn event log.
		/// </summary>
		public IEnumerable<string> Events { get; private set; }

		private Universe(int width, int height, int turn, string playerID)
		{
			Width = width;
			Height = height;
			Turn = turn;
			PlayerID = playerID;
			Sectors = new SectorCollection();
			players = new List<Player>();
		}

		/// <summary>
		/// Loads a universe from a root noun which has been loaded from a savegame.
		/// The root noun can be identified by its ID of "Vroot".
		/// Note that the root noun needs to belong to a noun collection in the library.
		/// </summary>
		/// <param name="root"></param>
		/// <returns></returns>
		public static Universe Load(Noun root)
		{
			// check library
			var shelf = Library.NounLibrary.Where(s => s.Contains(root)).FirstOrDefault();
			if (shelf == null)
				throw new Exception("Cannot load this universe; its root noun is not in the library.");

			// let there be light!
			var u = new Universe(root["mapwidth"][0], root["mapheight"][0], root["turn"][0], root["playerObject"][0]);

			// load players
			u.players.AddRange(shelf.Where(noun => noun.Contains("teamName")).Select(noun => Player.Load(noun, u)));
			u.Us = u.Players.Single(p => p.ID == u.PlayerID);
			foreach (var p in u.Players)
				p.Shipset = Shipset.DefaultEnemy;
			u.Us.Shipset = Shipset.DefaultFriendly;
			if (shelf.Contains("Vnotes"))
			{
				// HACK - hide self shipsets from old versions; don't name your empire Pme or an emptystring :)
				foreach (var note in shelf["Vnotes"].Where(note => note.Key.StartsWith("Shipset:") && note.Key != "Shipset:Pme" && note.Key != "Shipset:"))
				{
					var teamName = note.Key.Substring("Shipset:".Length);
					var noteplr = u.Players.SingleOrDefault(p => p.TeamName == teamName);
					if (noteplr == null)
						noteplr = u.AddPlayer(teamName);
					noteplr.ShipsetName = note[0];
				}
			}

			// load sectors
			u.Sectors = new SectorCollection(shelf.Where(noun => noun.Contains("X") && noun.Contains("Y")).Select(noun =>
				{
					var s = new Sector(u);

					// coordinates, name, etc.
					s.ID = noun.Key;
					s.Coordinates = new Point(noun["X"][0], noun["Y"][0]);
					s.RealName = noun["name"][0];
					if (shelf.Contains("Vnotes"))
					{
						if (shelf["Vnotes"].Contains("Name:" + s.ID))
							s.DisplayName = shelf["Vnotes"]["Name:" + s.ID][0];
						else
							s.DisplayName = s.RealName;
					}
					else
						s.DisplayName = s.RealName;

					// resource value
					if (noun.Contains("hullValue"))
					{
						s.Value = new Resources
						{
							Hull = noun["hullValue"][0],
							Weapons = noun["weaponValue"][0],
							Thrusters = noun["thrusterValue"][0],
							Shields = noun["shieldValue"][0],
						};
					}

					// resource income
					if (noun.Contains("minedHull"))
					{
						s.Income = new Resources
						{
							Hull = noun["minedHull"][0],
							Weapons = noun["minedWeapon"][0],
							Thrusters = noun["minedThruster"][0],
							Shields = noun["minedShield"][0],
						};
					}

					return s;
				}));
			
			u.MaxValue = new Resources()
			{
				Hull = u.Sectors.Max(s => s.Value == null ? double.Epsilon : s.Value.Hull),
				Weapons = u.Sectors.Max(s => s.Value == null ? double.Epsilon : s.Value.Weapons),
				Thrusters = u.Sectors.Max(s => s.Value == null ? double.Epsilon : s.Value.Thrusters),
				Shields = u.Sectors.Max(s => s.Value == null ? double.Epsilon : s.Value.Shields),
			};

			// load ships
			u.Ships = shelf.Where(noun => noun.Contains("location") && (noun.Contains("sizeClass") || noun.Contains("hull"))).Select(noun => Ship.Load(shelf, noun.Key, u.Sectors.Single(sector => sector.ID == noun["location"][0]), u)).ToArray();
			foreach (var sector in u.Sectors)
			{
				sector.Ships = u.Ships.Where(ship => ship.Sector == sector && (ship.IsMobile || ship.IsArmed)).ToArray();
			}

			// load treasury
			var usNoun = shelf[u.Us.ID];
			u.Treasury = new Resources
			{
				Hull = usNoun["availableHull"][0],
				Weapons = usNoun["availableWeapon"][0],
				Thrusters = usNoun["availableThruster"][0],
				Shields = usNoun["availableShield"][0],
			};

			// cache stuff
			u.OurMaximumFleetStrength = u.Sectors.Max(sector => sector.OurFleetStrength);
			u.MaximumEnemyFleetStrength = u.Sectors.Max(sector => sector.EnemyFleetStrength);
			u.MaximumShipSize = u.Ships.Max(ship => ship.Size);
			foreach (var ship in u.Ships.ToArray())
			{
				var s = ship; // needed to make the ship "real" or something for the linqy stuff
				ship.Image = new Cache<Image>(() => s.Owner.Shipset.BuildShipImage(128, s.Config));
				ship.SmallImage = new Cache<Image>(() => s.Owner.Shipset.BuildShipImage(24, s.Config));
			}

			// load events
			u.Events = shelf[u.PlayerID]["eventList"].Select(adj => (string)adj);

			// load battles
			u.Battles = shelf.Where(noun => noun.Contains("eventSequence")).Select(noun => new Battle
			{
				ID = noun.Key,
				Location = u.Sectors.Single(sector => sector.ID == noun["location"][0]),
				Name = noun["name"][0],
				Ships = u.Ships.Join(noun["shipsPresent"], ship => ship.ID, adjPhrase => (string)adjPhrase, (ship, adjPhrase) => ship).ToArray(),
				Events = noun["eventSequence"].Select(adjPhrase =>
				{
					var evtID = (string)adjPhrase;
					var evtNoun = shelf[evtID];
					return new BattleEvent
					{
						ID = evtID,
						Attacker = u.Ships.Single(ship => ship.ID == evtNoun["attacker"][0]),
						Target = u.Ships.Single(ship => ship.ID == evtNoun["target"][0]),
						DamageInflicted = new Resources
						{
							Hull = evtNoun["hull"][0],
							Weapons = evtNoun["weapon"][0],
							Thrusters = evtNoun["thruster"][0],
							Shields = evtNoun["shield"][0],
						},
						DamageBlocked = evtNoun["block"][0],
						TargetDestroyed = evtNoun["dead"][0] != "0" && evtNoun["dead"][0] != "" && evtNoun["dead"][0] != null,
					};
				}).ToArray()
			}).ToArray();

			// init construction
			u.Construction = new List<ConstructionItem>();
			
			// load ship designs
			var designs = new List<ConstructionItem>();
			if (shelf.Contains("Vnotes"))
			{
				foreach (var dn in shelf["Vnotes"].Where(dn => dn.Key.StartsWith("Design:")))
				{
					var d = new ConstructionItem();
					d.Quantity = 1;
					d.Name = dn.Key.Substring("Design:".Length);
					var split = ((string)dn[0]).Split(' ');
					d.Weapons = int.Parse(split[0]);
					d.Thrusters = int.Parse(split[1]);
					d.Shields = int.Parse(split[2]);
					d.Armor = int.Parse(split[3]);
					designs.Add(d);
				}
			}
			u.Designs = designs;

			return u;
		}

		/// <summary>
		/// The maximum resource values in any known sector.
		/// </summary>
		public Resources MaxValue
		{
			get;
			private set;
		}

		internal Player AddPlayer(string teamName)
		{
			var p = new Player(this);
			p.Shipset = Shipset.DefaultEnemy;
			p.TeamName = teamName;
			p.Name = teamName;
			players.Add(p);
			return p;
		}

		public Point OffsetHex(Point h, Direction dir, int distance, bool normalize)
		{
			return OffsetHex(h, new Size(dir.Offset.Width * distance, dir.Offset.Height * distance), normalize);
		}

		public Point OffsetHex(Point h, Size offset, bool normalize)
		{
			var h2 = h + offset;
			if (normalize)
				h2 = NormalizeHex(h2);
			return h2;
		}

		public Point NormalizeHex(Point h)
		{
			while (h.X < -Width / 2)
			{
				h.X += Width;
				h.Y -= Width / 2;
			}
			while (h.X >= Width / 2)
			{
				h.X -= Width;
				h.Y += Width / 2;
			}
			while (h.Y < -Height / 2)
			{
				h.Y += Height;
			}
			while (h.Y >= Height / 2)
			{
				h.Y -= Height;
			}
			return h;
		}

		public PointF NormalizePixel(PointF p, float zoom)
		{
			PointF upperLeft = new PointF((float)MinX / 2f, (float)MaxY / 2f + 0.5f).HexToScreen();
			PointF lowerRight = new PointF((float)MaxX / 2f + 0.5f, (float)MinY / 2f - 0.5f).HexToScreen();
			p.X = p.X.Normalize(upperLeft.X * zoom, lowerRight.Y * zoom);
			p.Y = p.Y.Normalize(upperLeft.Y * zoom, lowerRight.Y * zoom);
			return p;
		}

		public Size GetHexOffset(Point from, Point to, bool normalize)
		{
			var offset = new Size(to.X - from.X, to.Y - from.Y);
			if (normalize)
				return new Size(NormalizeHex(new Point(offset)));
			return offset;
		}

		public IEnumerable<Direction> GetDirectionFromOffset(Size offset)
		{
			var p = NormalizeHex(new Point(offset));
			var dirs = new Direction[]
			{
				Direction.None,
				Direction.North,
				Direction.Northeast,
				Direction.Northwest,
				Direction.South,
				Direction.Southeast,
				Direction.Southwest
			};
			foreach (var dir in dirs)
			{
				if (dir.Offset == offset)
					return new Direction[] { dir };
			}
			// TODO - multiple directions
			return new Direction[] { };
		}

		//public Direction WrapDirection(Point h, Size offset)
		//{
		//    var normal = NormalizeHex(h);
		//    var pos = h + offset;
		//    if (pos.X >= -Width / 2 && pos.Y >= -Height / 2 && pos.X < Width / 2 && pos.Y < Height / 2)
		//        return Direction.None; // no wrapping occurs
		//    return GetDirectionFromOffset(offset).First();
		//}


		public SubjectCollection CommandSubjects
		{
			get
			{
				var sc = new SubjectCollection();
				sc.Tag = "plr";

				// me commands for notes
				var me = new Subject();
				me.Key = "me";
				var meq = new VerbQueue();
				meq.Key = "immediate";
				me.Add(meq);

				// iterate through friendly ships for commands
				foreach (var ship in Ships.Where(ship => ship.IsOurs))
				{
					// move commands
					var subject = new Subject();
					subject.Key = ship.RealName;
					if (ship.TargetDirection != null && ship.TargetDirection != Direction.None)
					{
						var vq = new VerbQueue();
						vq.Key = "orders";
						var v = new Verb();
						v.Key = "move";
						var av = new Adverb();
						av.ScalarValue = ship.TargetDirection.CodeName;
						v.Add(av);
						vq.Add(v);
						subject.Add(vq);
					}

					// build commands
					if (ship.IsMothership)
					{
						var vq = new VerbQueue();
						vq.Key = "immediate";
						foreach (var item in Construction.Where(item => item.Weapons > 0 || item.Thrusters > 0))
						{
							// HACK - nothing in the eggs library allows for repeats yet
							for (var i = 0; i < item.Quantity; i++)
							{
								var v = new Verb();
								v.Key = "build";

								var av1 = new Adverb();
								av1.ScalarValue = item.Hull.ToString();
								v.Add(av1);

								var av2 = new Adverb();
								av2.ScalarValue = item.Weapons.ToString();
								v.Add(av2);

								var av3 = new Adverb();
								av3.ScalarValue = item.Thrusters.ToString();
								v.Add(av3);

								var av4 = new Adverb();
								av4.ScalarValue = item.Shields.ToString();
								v.Add(av4);

								var av5 = new Adverb();
								av5.ScalarValue = item.Name;
								v.Add(av5);

								vq.Add(v);
							}

							// ship design notes (NOTE: they're Weapons/Thrusters/Shields/Armor, not Hull/Weapons/Thrusters/Shields!)
							var dv = new Verb();
							dv.Key = "note";

							var dav1 = new Adverb();
							dav1.ScalarValue = "Design:" + item.Name;
							dv.Add(dav1);

							var dav2 = new Adverb();
							dav2.ScalarValue = item.Weapons + " " + item.Thrusters + " " + item.Shields + " " + item.Armor;
							dv.Add(dav2);

							meq.Add(dv);
						}
						subject.Add(vq);
					}

					if (subject.Count > 0)
						sc.Add(subject);
				}

				// sector rename notes
				foreach (var sector in Sectors.Where(sector => sector.IsRenamed))
				{
					var v = new Verb();
					v.Key = "note";

					var av1 = new Adverb();
					av1.ScalarValue = "Name:" + sector.ID;
					v.Add(av1);

					var av2 = new Adverb();
					av2.ScalarValue = sector.DisplayName;
					v.Add(av2);

					meq.Add(v);
				}
				// ship rename notes
				foreach (var ship in Ships.Where(ship => ship.IsRenamed))
				{
					var v = new Verb();
					v.Key = "note";

					var av1 = new Adverb();
					av1.ScalarValue = "Name:" + ship.ID;
					v.Add(av1);

					var av2 = new Adverb();
					av2.ScalarValue = ship.DisplayName;
					v.Add(av2);

					meq.Add(v);
				}
				// shipset notes
				foreach (var plr in Players)
				{
					var v = new Verb();
					v.Key = "note";

					var av1 = new Adverb();
					av1.ScalarValue = "Shipset:" + plr.TeamName;
					v.Add(av1);

					var av2 = new Adverb();
					av2.ScalarValue = plr.ShipsetName;
					v.Add(av2);

					meq.Add(v);
				}
				sc.Add(me);

				return sc;
			}
		}

		/// <summary>
		/// The player who's viewing the universe.
		/// </summary>
		public Player Us
		{
			get;
			private set;
		}

		/// <summary>
		/// The maximum our-fleet strength of any sector.
		/// </summary>
		public double OurMaximumFleetStrength
		{
			get;
			private set;
		}

		/// <summary>
		/// The maximum enemy-fleet strength of any sector.
		/// </summary>
		public double MaximumEnemyFleetStrength
		{
			get;
			private set;
		}

		/// <summary>
		/// The maximum size of any known ship.
		/// </summary>
		public int MaximumShipSize
		{
			get;
			private set;
		}

		public IEnumerable<Sector> IncomeSectors
		{
			get
			{
				return Sectors.Where(sector => sector.Income != null && (sector.Income.Hull > 0 || sector.Income.Weapons > 0 || sector.Income.Thrusters > 0 || sector.Income.Shields > 0));
			}
		}

		/// <summary>
		/// Construction expenses for this turn.
		/// </summary>
		public Resources Expenditures
		{
			get
			{
				return new Resources
				{
					Hull = Construction.Sum(ci => ci.Hull * ci.Quantity),
					Weapons = Construction.Sum(ci => ci.Weapons * ci.Quantity),
					Thrusters = Construction.Sum(ci => ci.Thrusters * ci.Quantity),
					Shields = Construction.Sum(ci => ci.Shields * ci.Quantity),
				};
			}
		}

		/// <summary>
		/// Resources in storage.
		/// </summary>
		public Resources Treasury
		{
			get;
			private set;
		}

		/// <summary>
		/// Surplus resources.
		/// </summary>
		public Resources Surplus
		{
			get
			{
				return new Resources
				{
					Hull = Treasury.Hull - Expenditures.Hull,
					Weapons = Treasury.Weapons - Expenditures.Weapons,
					Thrusters = Treasury.Thrusters - Expenditures.Thrusters,
					Shields = Treasury.Shields - Expenditures.Shields,
				};
			}
		}

		public IList<ConstructionItem> Construction
		{
			get;
			private set;
		}

		public IEnumerable<Battle> Battles
		{
			get;
			private set;
		}

		/// <summary>
		/// All known ships.
		/// </summary>
		public IEnumerable<Ship> Ships
		{
			get;
			private set;
		}

		/// <summary>
		/// Ship designs that have been used previously.
		/// </summary>
		public IEnumerable<ConstructionItem> Designs
		{
			get;
			private set;
		}

		public int MinX
		{
			get { return -Width / 2; }
		}

		public int MaxX
		{
			get { return Width / 2 - 1; }
		}

		public int MinY
		{
			get
			{
				if (Height % 2 == 0)
					return -Height / 2;
				else
					return -(Height - 1) / 2;
			}
		}

		public int MaxY
		{
			get
			{
				if (Height % 2 == 0)
					return Height / 2 - 1;
				else
					return (Height - 1) / 2;
			}
		}
	}
}
