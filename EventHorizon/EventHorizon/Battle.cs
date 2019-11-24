using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventHorizon.Properties;

namespace EventHorizon
{
	/// <summary>
	/// A battle! IN SPACE!
	/// </summary>
	public class Battle
	{
		public string ID { get; internal set; }

		public string Name { get; internal set; }

		public Sector Location { get; internal set; }

		public IEnumerable<Ship> Ships { get; internal set; }

		public IEnumerable<Ship> OurShips
		{
			get
			{
				return Ships.Where(ship => ship.IsOurs);
			}
		}

		public IEnumerable<Ship> EnemyShips
		{
			get
			{
				return Ships.Where(ship => !ship.IsOurs);
			}
		}

		public IEnumerable<BattleEvent> Events { get; internal set; }

		public int X { get { return Location.X; } }

		public int Y { get { return Location.Y; } }

		public string LocationName { get { return Location.DisplayName; } }

		public int OurStrengthBefore
		{
			get { return GetOurStrengthAfterRound(0); }
		}

		public int EnemyStrengthBefore
		{
			get { return GetEnemyStrengthAfterRound(0); }
		}

		public int GetOurStrengthAfterRound(int round)
		{
			// NOTE - may not be entirely accurate as ships shrink when they take damage!
			var salvos = Salvos.Take(round).ToArray();
			int str = 0;
			foreach (var ship in OurShips)
				if (DidShipSurviveRound(ship, round))
					str += ship.Size;
			return str;
		}

		public int GetEnemyStrengthAfterRound(int round)
		{
			// NOTE - may not be entirely accurate as ships shrink when they take damage!
			var salvos = Salvos.Take(round).ToArray();
			int str = 0;
			foreach (var ship in EnemyShips)
				if (DidShipSurviveRound(ship, round))
					str += ship.Size;
			return str;
		}

		public bool DidShipSurviveRound(Ship ship, int round)
		{
			foreach (BattleEvent salvo in Salvos.Take(round))
				if (salvo.Target == ship && salvo.TargetDestroyed)
					return false;
			return true;

		}

		/// <summary>
		/// Battle events aggregated by consecutive shots from the same attacker on the same target.
		/// </summary>
		public IEnumerable<BattleEvent> Salvos
		{
			get
			{
				BattleEvent old = null;
				foreach (var evt in Events)
				{
					if (old == null || old.Attacker != evt.Attacker || old.Target != evt.Target)
					{
						if (old != null)
							yield return old;
						old = new BattleEvent
						{
							Attacker = evt.Attacker,
							Target = evt.Target,
							ShotsFired = 1,
							TargetDestroyed = evt.TargetDestroyed,
							DamageBlocked = evt.DamageBlocked,
							DamageInflicted = new Resources
							{
								Hull = evt.DamageInflicted.Hull,
								Weapons = evt.DamageInflicted.Weapons,
								Thrusters = evt.DamageInflicted.Thrusters,
								Shields = evt.DamageInflicted.Shields,
							}
						};
					}
					else
					{
						old.ShotsFired++;
						old.TargetDestroyed = old.TargetDestroyed || evt.TargetDestroyed;
						old.DamageBlocked += evt.DamageBlocked;
						old.DamageInflicted.Hull += evt.DamageInflicted.Hull;
						old.DamageInflicted.Weapons += evt.DamageInflicted.Weapons;
						old.DamageInflicted.Thrusters += evt.DamageInflicted.Thrusters;
						old.DamageInflicted.Shields += evt.DamageInflicted.Shields;
					}
				}
				if (old != null)
					yield return old;
			}
		}

		public BattleOutcome Outcome
		{
			get
			{
				if (Ships.Where(ship => ship.IsOurs).All(ship => Salvos.Any(salvo => salvo.Target == ship && salvo.TargetDestroyed)))
					return BattleOutcome.Defeat;
				if (Ships.Where(ship => !ship.IsOurs).All(ship => Salvos.Any(salvo => salvo.Target == ship && salvo.TargetDestroyed)))
					return BattleOutcome.Victory;
				return BattleOutcome.Stalemate;
			}
		}

		public Mood GetMoodAfter(int round)
		{
			if (round >= Salvos.Count())
			{
				switch (Outcome)
				{
					case BattleOutcome.Stalemate:
						return Mood.BattleEndStalemate;
					case BattleOutcome.Victory:
						return Mood.BattleEndVictory;
					case BattleOutcome.Defeat:
						return Mood.BattleEndDefeat;
				}
			}
			int us = GetOurStrengthAfterRound(round);
			int them = GetEnemyStrengthAfterRound(round);
			if (us / them >= Settings.Default.BattleFavorableThreshold)
				return Mood.BattleFavorable;
			if (us / them <= Settings.Default.BattleUnfavorableThreshold)
				return Mood.BattleUnfavorable;
			return Mood.BattleEqual;
		}
	}
}
