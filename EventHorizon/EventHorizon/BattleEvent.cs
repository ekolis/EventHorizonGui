using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventHorizon
{
	/// <summary>
	/// An event in battle.
	/// </summary>
	public class BattleEvent
	{
		public string ID { get; internal set; }

		public Ship Attacker { get; internal set; }

		public Ship Target { get; internal set; }

		public Resources DamageInflicted { get; internal set; }

		public int DamageBlocked { get; internal set; }

		public bool TargetDestroyed { get; internal set; }

		public int ShotsFired { get; internal set; }

		public bool IsCriticalHit
		{
			get
			{
				return (DamageBlocked + DamageInflicted.Weapons + DamageInflicted.Hull + DamageInflicted.Shields + DamageInflicted.Thrusters) > ShotsFired;
			}
		}
	}
}
