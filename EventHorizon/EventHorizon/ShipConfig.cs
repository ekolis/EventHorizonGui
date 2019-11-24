using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventHorizon
{
	/// <summary>
	/// A ship configuration. All the data needed to determine a ship's image, apart from the shipset and image size!
	/// </summary>
	public struct ShipConfig
	{
		/// <summary>
		/// The size class of the ship divided by the size class of the largest known ship.
		/// </summary>
		public float ProportionalSize { get; set; }

		public bool IsMothership { get; set; }

		public bool IsMobile { get; set; }

		public bool IsArmed { get; set; }
	}
}
