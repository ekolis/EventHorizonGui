using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventHorizon
{
	/// <summary>
	/// Resources used in the game.
	/// </summary>
	public class Resources
	{
		public double Hull { get; set; }
		public double Weapons { get; set; }
		public double Thrusters { get; set; }
		public double Shields { get; set; }

		public static Resources operator +(Resources r1, Resources r2)
		{
			return new Resources
			{
				Hull = r1.Hull + r2.Hull,
				Weapons = r1.Weapons + r2.Weapons,
				Thrusters = r1.Thrusters + r2.Thrusters,
				Shields = r1.Shields + r2.Shields,
			};
		}

		public static Resources operator -(Resources r)
		{
			return new Resources
			{
				Hull = -r.Hull,
				Weapons = -r.Weapons,
				Thrusters = -r.Thrusters,
				Shields = -r.Shields,
			};
		}

		public static Resources operator -(Resources r1, Resources r2)
		{
			return r1 + -r2;
		}
	}
}
