using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EventHorizon
{
	public partial class BattleReplayForm : Form
	{
		public BattleReplayForm()
		{
			InitializeComponent();
		}

		private Battle battle;
		private int round;

		public Battle Battle
		{
			get
			{
				return battle;
			}
			set
			{
				battle = value;
				round = 0;
				Text = battle.Name.Replace(battle.Location.RealName, battle.Location.DisplayName);
				RebuildGui();
				timer.Interval = sldDelay.Value * (int)Math.Ceiling(3.0 / Math.Sqrt(Battle.Salvos.Count()));
			}
		}

		private void btnPlayPause_Click(object sender, EventArgs e)
		{
			timer.Enabled = !timer.Enabled;
			if (round >= Battle.Salvos.Count())
			{
				round = 0;
				RebuildGui();
			}
			if (timer.Enabled)
			{
				timer_Tick(null, null);
				btnPlayPause.Text = "Pause";
			}
			else
				btnPlayPause.Text = "Play";
		}

		private void sldDelay_Scroll(object sender, EventArgs e)
		{
			timer.Interval = sldDelay.Value * (int)Math.Ceiling(3.0 / Math.Sqrt(Battle.Salvos.Count()));
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (round < Battle.Salvos.Count())
				round++;
			else
			{
				timer.Enabled = false;
				btnPlayPause.Text = "Play";
			}
			RebuildGui();
		}

		public void RebuildGui()
		{
			if (round == 0 || Battle == null)
			{
				lblAttacker.Text = "";
				lblTarget.Text = "";
				picAttacker.Image = null;
				picTarget.Image = null;
				picBeam.Image = null;
				lblBlocked.Visible = false;
				lblHull.Visible = false;
				lblWeapon.Visible = false;
				lblThruster.Visible = false;
				lblShield.Visible = false;
				lblCritical.Visible = false;
				lblDestroyed.Visible = false;
				Music.CurrentMood = Battle == null ? Mood.BattleEqual : Battle.GetMoodAfter(round);
			}
			else
			{
				var evt = battle.Salvos.ElementAt(round - 1);
				lblAttacker.Text = evt.Attacker.DisplayName;
				lblTarget.Text = evt.Target.DisplayName;
				var atkImg = new Bitmap(evt.Attacker.Image);
				atkImg.RotateFlip(RotateFlipType.Rotate90FlipNone);
				picAttacker.Image = atkImg;
				var tgtImg = new Bitmap(evt.Target.Image);
				tgtImg.RotateFlip(RotateFlipType.Rotate270FlipNone);
				picBeam.Image = evt.Attacker.Owner.Shipset.BeamImage;
				var dmg = evt.DamageBlocked + evt.DamageInflicted.Hull + evt.DamageInflicted.Shields + evt.DamageInflicted.Thrusters + evt.DamageInflicted.Weapons;
				var physicalDmg = dmg - evt.DamageBlocked;
				picBeam.Height = evt.ShotsFired * 8;
				var dmgSize = (int)(16 * Math.Sqrt((double)physicalDmg));
				var dmgPos = 64 - dmgSize / 2;
				picBeam.Top = picAttacker.Top + (128 - picBeam.Height) / 2;
				var shieldSize = 128;
				var shieldPos = 64 - shieldSize / 2;
				var g = Graphics.FromImage(tgtImg);
				if (evt.DamageBlocked > 0)
					g.DrawImage(evt.Target.Owner.Shipset.ShieldFlareImage, shieldPos, shieldPos, shieldSize, shieldSize);
				if (evt.TargetDestroyed)
					g.DrawImage(evt.Target.Owner.Shipset.ExplosionImage, 0, 0, 128, 128);
				else if (physicalDmg > 0)
					g.DrawImage(evt.Target.Owner.Shipset.DamageImage, dmgPos, dmgPos, dmgSize, dmgSize);
				picTarget.Image = tgtImg;
				lblBlocked.Visible = evt.DamageBlocked > 0;
				lblBlocked.Text = "Blocked Dmg: " + evt.DamageBlocked;
				lblHull.Visible = evt.DamageInflicted.Hull > 0;
				lblHull.Text = "Hull Dmg: " + evt.DamageInflicted.Hull;
				lblWeapon.Visible = evt.DamageInflicted.Weapons > 0;
				lblWeapon.Text = "Weapon Dmg: " + evt.DamageInflicted.Weapons;
				lblThruster.Visible = evt.DamageInflicted.Thrusters > 0;
				lblThruster.Text = "Thruster Dmg: " + evt.DamageInflicted.Thrusters;
				lblShield.Visible = evt.DamageInflicted.Shields > 0;
				lblShield.Text = "Shield Dmg: " + evt.DamageInflicted.Shields;
				lblCritical.Visible = evt.IsCriticalHit;
				lblDestroyed.Visible = evt.TargetDestroyed;
				Music.CurrentMood = Battle.GetMoodAfter(round);
				
			}
			lblRound.Text = "Round: " + round + " of " + (Battle == null ? 0 : Battle.Salvos.Count());
		}

		private void BattleReplayForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Music.CurrentMood = Mood.Strategic;
		}

		private void BattleReplayForm_Load(object sender, EventArgs e)
		{
			btnPlayPause_Click(sender, e);
		}
	}
}
