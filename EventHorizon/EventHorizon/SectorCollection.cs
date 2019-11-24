using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;

namespace EventHorizon
{
	public class SectorCollection : KeyedCollection<Point, Sector>
	{
		public SectorCollection()
		{
		}

		public SectorCollection(IEnumerable<Sector> sectors)
		{
			foreach (var sector in sectors)
				Add(sector);
		}

		protected override Point GetKeyForItem(Sector item)
		{
			return item.Coordinates;
		}

		public new Sector this[Point p]
		{
			get
			{
				if (Contains(p))
					return base[p];
				return null;
			}
		}

		public Sector this[int x, int y]
		{
			get
			{
				return this[new Point(x, y)];
			}
		}
	}
}
