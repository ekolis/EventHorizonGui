using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EventHorizon
{
	static class Extensions
	{
		/// <summary>
		/// Converts hexagonal coordinates into screen coordinates.
		/// </summary>
		/// <param name="h">The hex coordinates.</param>
		/// <returns>The screen coordinates.</returns>
		public static PointF HexToScreen(this PointF h)
		{
			return new PointF(h.X * (float)Math.Cos(Math.PI / 6), -h.Y - h.X * (float)Math.Sin(Math.PI / 6));
		}

		/// <summary>
		/// Converts screen coordinates into hexagonal coordinates.
		/// </summary>
		/// <param name="s">The screen coordinates.</param>
		/// <returns>The hexagonal coordinates.</returns>
		public static PointF ScreenToHex(this PointF s)
		{
			var x = s.X / (float)Math.Cos(Math.PI / 6);
			return new PointF(x, -s.Y - x * (float)Math.Sin(Math.PI / 6));
		}

		public static float Normalize(this float value, float min, float max)
		{
			float stepSize = max - min;
			float need = 0;
			if (value < min)
				need = min - value;
			else if (value > max)
				need = max - value;
			int stepCount = (int)Math.Ceiling(Math.Abs(need) / stepSize);
			return value + stepSize * stepCount * (int)Math.Sign(need);
		}

		public static int Normalize(this int value, int min, int max)
		{
			int stepSize = max - min;
			int need = 0;
			if (value < min)
				need = min - value;
			else if (value > max)
				need = max - value;
			int stepCount = (int)Math.Ceiling((double)(Math.Abs(need) / stepSize));
			return value + stepSize * stepCount * Math.Sign(need);
		}

		public static T PickRandom<T>(this IEnumerable<T> list)
		{
			var rand = new Random();
			var mapping = list.Select(x => new { Item = x, Number = rand.Next() }).OrderBy(x => x.Number).Select(x => x.Item);
			return mapping.FirstOrDefault();
		}
	}
}
