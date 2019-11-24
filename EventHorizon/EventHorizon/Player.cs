using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eggs;

namespace EventHorizon
{
	/// <summary>
	/// A game player.
	/// </summary>
	public class Player
	{
		/// <summary>
		/// The universe in which this player's playing.
		/// </summary>
		public Universe Universe { get; private set; }

		/// <summary>
		/// The ID of this player.
		/// </summary>
		public string ID { get; private set; }

		/// <summary>
		/// The name of this player's empire or team.
		/// </summary>
		public string TeamName { get; internal set; }

		/// <summary>
		/// The name of this player. The team name will be used if the player object isn't visible.
		/// </summary>
		public string Name { get; internal set; }

		internal Player(Universe universe)
		{
			Universe = universe;
		}

		internal static Player Load(Noun noun, Universe universe)
		{
			var p = new Player(universe);

			p.ID = noun.Key;
			p.TeamName = noun["teamName"][0];
			p.Name = noun["name"][0];

			return p;
		}

		/// <summary>
		/// The shipset used for this player's ships.
		/// </summary>
		public Shipset Shipset
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the player's shipset.
		/// </summary>
		public string ShipsetName
		{
			get
			{
				return Shipset.Name;
			}
			set
			{
				Shipset = new Shipset(value, this == Universe.Us);
			}
		}
	}
}
