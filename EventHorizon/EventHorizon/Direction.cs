using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EventHorizon
{
	public class Direction
	{
		private Direction(Size s, string friendlyName, string codeName)
		{
			Offset = s;
			FriendlyName = friendlyName;
			CodeName = codeName;
		}

		public static Direction None = new Direction(Size.Empty, "nowhere", "0");
		public static Direction North = new Direction(new Size(0, 1), "north", "+Y");
		public static Direction Northeast = new Direction(new Size(1, 0), "northeast", "+X");
		public static Direction Northwest = new Direction(new Size(-1, 1), "northwest", "+Z");
		public static Direction South = new Direction(new Size(0, -1), "south", "-Y");
		public static Direction Southeast = new Direction(new Size(1, -1), "southeast", "-Z");
		public static Direction Southwest = new Direction(new Size(-1, 0), "southwest", "-X");


		public Size Offset {get; private set;}

		public string FriendlyName { get; private set; }

		public string CodeName { get; private set; }
	}
}
