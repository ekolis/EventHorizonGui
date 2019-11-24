using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventHorizon
{
	public class ConstructionItem
	{
		public ConstructionItem()
		{
			Quantity = 1;
		}

		public string Name { get; set; }

		private int weapons;
		public int Weapons
		{
			get { return weapons; }
			set { weapons = Math.Max(0, value); }
		}

	
		private int thrusters;
		public int Thrusters
		{
			get { return thrusters; }
			set { thrusters = Math.Max(0, value); }
		}


		private int shields;
		public int Shields
		{
			get { return shields; }
			set { shields = Math.Max(0, value); }
		}

		private int armor;
		public int Armor
		{
			get { return armor; }
			set { armor = Math.Max(0, value); }
		}

		public int Hull { get { return (int)Math.Ceiling((Math.Pow(Weapons + Thrusters + Shields, 2.0) - 1.0) / 4.0) + Armor; } }

		private int quantity;
		public int Quantity
		{
			get
			{
				return quantity;
			}
			set
			{
				quantity = Math.Max(1, value);
			}
		}
		public string Warnings
		{
			get
			{
				return "TODO";	
			}
		}
	}
}
