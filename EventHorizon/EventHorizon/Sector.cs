using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Eggs;

namespace EventHorizon
{
	/// <summary>
	/// A sector in the universe.
	/// </summary>
	public class Sector
	{
		/// <summary>
		/// The ID of the sector.
		/// </summary>
		public string ID { get; internal set; }

		/// <summary>
		/// The universe containing this sector.
		/// </summary>
		public Universe Universe { get; private set; }

		/// <summary>
		/// The ships in this sector.
		/// </summary>
		public IEnumerable<Ship> Ships { get; internal set; }

		/// <summary>
		/// The coordinates of this sector.
		/// </summary>
		public Point Coordinates { get; internal set; }

		/// <summary>
		/// The name of this sector in the GAM/PLR file.
		/// </summary>
		public string RealName { get; internal set; }

		/// <summary>
		/// The name assigned by the user to the sector. Defaults to the real name.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The resource mining value of this sector.
		/// </summary>
		public Resources Value { get; internal set; }

		internal Sector(Universe universe)
		{
			Universe = universe;
		}

		public ILookup<Player, Ship> ShipsByPlayer
		{
			get
			{
				return Ships.ToLookup(ship => ship.Owner);
			}
		}

		/// <summary>
		/// Tests sector adjacency, including the universe wraparound effect.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsAdjacentTo(Sector other)
		{
			if (Universe != other.Universe)
				return false;
			var offset = Universe.GetHexOffset(Coordinates, other.Coordinates, true);
			return Universe.GetDirectionFromOffset(offset).Count() > 0;
		}

		/// <summary>
		/// An estimate of the strength of our ships in this sector.
		/// Formula: strength = sum(sizeclass)
		/// </summary>
		public double OurFleetStrength
		{
			get
			{
				return GetFleetStrength(Universe.Us);
			}
		}

		/// <summary>
		/// An estimate of the strength of all enemy ships in this sector.
		/// Formula: strength = sum(sizeclass)
		/// </summary>
		public double EnemyFleetStrength
		{
			get
			{
				return Ships.Where(ship => ship.Owner != Universe.Us).Sum(ship => ship.Size);
			}
		}

		/// <summary>
		/// Estimates the strength of a player's ships in this sector.
		/// Formula: strength = sum(sizeclass)
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public double GetFleetStrength(Player p)
		{
			return ShipsByPlayer[p].Sum(ship => ship.Size);
		}

		/// <summary>
		/// Our resource income from this sector.
		/// </summary>
		public Resources Income
		{
			get;
			internal set;
		}

		#region data binding stuff
		public int HullIncome
		{
			get { return (int)Income.Hull; }
		}

		public int WeaponIncome
		{
			get { return (int)Income.Weapons; }
		}

		public int ThrusterIncome
		{
			get { return (int)Income.Thrusters; }
		}

		public int ShieldIncome
		{
			get { return (int)Income.Shields; }
		}

		public int X
		{
			get { return Coordinates.X; }
		}

		public int Y
		{
			get { return Coordinates.Y; }
		}
		#endregion

		/// <summary>
		/// Has this sector been renamed this turn? If so, it will need a note created in the PLR file.
		/// </summary>
		public bool IsRenamed { get; set; }

		public override string ToString()
		{
			return DisplayName;
		}
	}
}