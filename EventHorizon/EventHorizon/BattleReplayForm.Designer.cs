namespace EventHorizon
{
	partial class BattleReplayForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BattleReplayForm));
			this.picAttacker = new System.Windows.Forms.PictureBox();
			this.picTarget = new System.Windows.Forms.PictureBox();
			this.picBeam = new System.Windows.Forms.PictureBox();
			this.lblAttacker = new System.Windows.Forms.Label();
			this.lblTarget = new System.Windows.Forms.Label();
			this.lblBlocked = new System.Windows.Forms.Label();
			this.lblHull = new System.Windows.Forms.Label();
			this.lblWeapon = new System.Windows.Forms.Label();
			this.lblThruster = new System.Windows.Forms.Label();
			this.lblShield = new System.Windows.Forms.Label();
			this.lblRound = new System.Windows.Forms.Label();
			this.btnPlayPause = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.sldDelay = new System.Windows.Forms.TrackBar();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.label5 = new System.Windows.Forms.Label();
			this.lblCritical = new System.Windows.Forms.Label();
			this.lblDestroyed = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.picAttacker)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picTarget)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picBeam)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldDelay)).BeginInit();
			this.SuspendLayout();
			// 
			// picAttacker
			// 
			this.picAttacker.BackColor = System.Drawing.Color.Transparent;
			this.picAttacker.Location = new System.Drawing.Point(12, 12);
			this.picAttacker.Name = "picAttacker";
			this.picAttacker.Size = new System.Drawing.Size(128, 128);
			this.picAttacker.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picAttacker.TabIndex = 0;
			this.picAttacker.TabStop = false;
			// 
			// picTarget
			// 
			this.picTarget.BackColor = System.Drawing.Color.Transparent;
			this.picTarget.Location = new System.Drawing.Point(280, 12);
			this.picTarget.Name = "picTarget";
			this.picTarget.Size = new System.Drawing.Size(128, 128);
			this.picTarget.TabIndex = 1;
			this.picTarget.TabStop = false;
			// 
			// picBeam
			// 
			this.picBeam.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.picBeam.BackColor = System.Drawing.Color.Transparent;
			this.picBeam.Location = new System.Drawing.Point(146, 12);
			this.picBeam.Name = "picBeam";
			this.picBeam.Size = new System.Drawing.Size(128, 128);
			this.picBeam.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picBeam.TabIndex = 2;
			this.picBeam.TabStop = false;
			// 
			// lblAttacker
			// 
			this.lblAttacker.AutoEllipsis = true;
			this.lblAttacker.ForeColor = System.Drawing.Color.White;
			this.lblAttacker.Location = new System.Drawing.Point(12, 143);
			this.lblAttacker.Name = "lblAttacker";
			this.lblAttacker.Size = new System.Drawing.Size(128, 82);
			this.lblAttacker.TabIndex = 3;
			this.lblAttacker.Text = "Attacker";
			this.lblAttacker.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblTarget
			// 
			this.lblTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTarget.AutoEllipsis = true;
			this.lblTarget.ForeColor = System.Drawing.Color.White;
			this.lblTarget.Location = new System.Drawing.Point(280, 143);
			this.lblTarget.Name = "lblTarget";
			this.lblTarget.Size = new System.Drawing.Size(123, 82);
			this.lblTarget.TabIndex = 4;
			this.lblTarget.Text = "Target";
			this.lblTarget.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblBlocked
			// 
			this.lblBlocked.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblBlocked.AutoEllipsis = true;
			this.lblBlocked.ForeColor = System.Drawing.Color.Gray;
			this.lblBlocked.Location = new System.Drawing.Point(146, 144);
			this.lblBlocked.Name = "lblBlocked";
			this.lblBlocked.Size = new System.Drawing.Size(128, 16);
			this.lblBlocked.TabIndex = 6;
			this.lblBlocked.Text = "Blocked Dmg: 0";
			this.lblBlocked.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblHull
			// 
			this.lblHull.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblHull.AutoEllipsis = true;
			this.lblHull.ForeColor = System.Drawing.Color.White;
			this.lblHull.Location = new System.Drawing.Point(146, 160);
			this.lblHull.Name = "lblHull";
			this.lblHull.Size = new System.Drawing.Size(128, 16);
			this.lblHull.TabIndex = 7;
			this.lblHull.Text = "Hull Dmg: 0";
			this.lblHull.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblWeapon
			// 
			this.lblWeapon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblWeapon.AutoEllipsis = true;
			this.lblWeapon.ForeColor = System.Drawing.Color.Red;
			this.lblWeapon.Location = new System.Drawing.Point(146, 178);
			this.lblWeapon.Name = "lblWeapon";
			this.lblWeapon.Size = new System.Drawing.Size(128, 16);
			this.lblWeapon.TabIndex = 8;
			this.lblWeapon.Text = "Weapon Dmg: 0";
			this.lblWeapon.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblThruster
			// 
			this.lblThruster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblThruster.AutoEllipsis = true;
			this.lblThruster.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.lblThruster.Location = new System.Drawing.Point(146, 194);
			this.lblThruster.Name = "lblThruster";
			this.lblThruster.Size = new System.Drawing.Size(128, 16);
			this.lblThruster.TabIndex = 9;
			this.lblThruster.Text = "Thruster Dmg: 0";
			this.lblThruster.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblShield
			// 
			this.lblShield.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblShield.AutoEllipsis = true;
			this.lblShield.ForeColor = System.Drawing.Color.DodgerBlue;
			this.lblShield.Location = new System.Drawing.Point(146, 210);
			this.lblShield.Name = "lblShield";
			this.lblShield.Size = new System.Drawing.Size(128, 16);
			this.lblShield.TabIndex = 10;
			this.lblShield.Text = "Shield Dmg: 0";
			this.lblShield.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblRound
			// 
			this.lblRound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblRound.AutoEllipsis = true;
			this.lblRound.ForeColor = System.Drawing.Color.White;
			this.lblRound.Location = new System.Drawing.Point(315, 294);
			this.lblRound.Name = "lblRound";
			this.lblRound.Size = new System.Drawing.Size(88, 19);
			this.lblRound.TabIndex = 11;
			this.lblRound.Text = "Round: 0 of 0";
			this.lblRound.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnPlayPause
			// 
			this.btnPlayPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPlayPause.BackColor = System.Drawing.SystemColors.Control;
			this.btnPlayPause.Location = new System.Drawing.Point(146, 316);
			this.btnPlayPause.Name = "btnPlayPause";
			this.btnPlayPause.Size = new System.Drawing.Size(121, 23);
			this.btnPlayPause.TabIndex = 12;
			this.btnPlayPause.Text = "Play";
			this.btnPlayPause.UseVisualStyleBackColor = false;
			this.btnPlayPause.Click += new System.EventHandler(this.btnPlayPause_Click);
			// 
			// btnClose
			// 
			this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClose.BackColor = System.Drawing.SystemColors.Control;
			this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnClose.Location = new System.Drawing.Point(333, 316);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = new System.Drawing.Size(75, 23);
			this.btnClose.TabIndex = 13;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = false;
			// 
			// sldDelay
			// 
			this.sldDelay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.sldDelay.LargeChange = 250;
			this.sldDelay.Location = new System.Drawing.Point(12, 294);
			this.sldDelay.Maximum = 1000;
			this.sldDelay.Minimum = 250;
			this.sldDelay.Name = "sldDelay";
			this.sldDelay.Size = new System.Drawing.Size(104, 45);
			this.sldDelay.SmallChange = 50;
			this.sldDelay.TabIndex = 14;
			this.sldDelay.TickFrequency = 50;
			this.sldDelay.Value = 500;
			this.sldDelay.Scroll += new System.EventHandler(this.sldDelay_Scroll);
			// 
			// timer
			// 
			this.timer.Interval = 500;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoEllipsis = true;
			this.label5.ForeColor = System.Drawing.Color.White;
			this.label5.Location = new System.Drawing.Point(143, 270);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(121, 43);
			this.label5.TabIndex = 15;
			this.label5.Text = "Use the slider to adjust the delay between salvos.";
			this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblCritical
			// 
			this.lblCritical.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblCritical.AutoEllipsis = true;
			this.lblCritical.ForeColor = System.Drawing.Color.Yellow;
			this.lblCritical.Location = new System.Drawing.Point(143, 226);
			this.lblCritical.Name = "lblCritical";
			this.lblCritical.Size = new System.Drawing.Size(128, 16);
			this.lblCritical.TabIndex = 16;
			this.lblCritical.Text = "CRITICAL HIT!";
			this.lblCritical.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblCritical.Visible = false;
			// 
			// lblDestroyed
			// 
			this.lblDestroyed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblDestroyed.AutoEllipsis = true;
			this.lblDestroyed.ForeColor = System.Drawing.Color.Yellow;
			this.lblDestroyed.Location = new System.Drawing.Point(143, 242);
			this.lblDestroyed.Name = "lblDestroyed";
			this.lblDestroyed.Size = new System.Drawing.Size(128, 16);
			this.lblDestroyed.TabIndex = 17;
			this.lblDestroyed.Text = "TARGET DESTROYED!";
			this.lblDestroyed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblDestroyed.Visible = false;
			// 
			// BattleReplayForm
			// 
			this.AcceptButton = this.btnPlayPause;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.CancelButton = this.btnClose;
			this.ClientSize = new System.Drawing.Size(415, 348);
			this.Controls.Add(this.lblDestroyed);
			this.Controls.Add(this.lblCritical);
			this.Controls.Add(this.picBeam);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.sldDelay);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnPlayPause);
			this.Controls.Add(this.lblRound);
			this.Controls.Add(this.lblShield);
			this.Controls.Add(this.lblThruster);
			this.Controls.Add(this.lblWeapon);
			this.Controls.Add(this.lblHull);
			this.Controls.Add(this.lblBlocked);
			this.Controls.Add(this.lblTarget);
			this.Controls.Add(this.lblAttacker);
			this.Controls.Add(this.picTarget);
			this.Controls.Add(this.picAttacker);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "BattleReplayForm";
			this.Text = "Battle Replay";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BattleReplayForm_FormClosed);
			this.Load += new System.EventHandler(this.BattleReplayForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.picAttacker)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picTarget)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picBeam)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldDelay)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picAttacker;
		private System.Windows.Forms.PictureBox picTarget;
		private System.Windows.Forms.PictureBox picBeam;
		private System.Windows.Forms.Label lblAttacker;
		private System.Windows.Forms.Label lblTarget;
		private System.Windows.Forms.Label lblBlocked;
		private System.Windows.Forms.Label lblHull;
		private System.Windows.Forms.Label lblWeapon;
		private System.Windows.Forms.Label lblThruster;
		private System.Windows.Forms.Label lblShield;
		private System.Windows.Forms.Label lblRound;
		private System.Windows.Forms.Button btnPlayPause;
		private System.Windows.Forms.Button btnClose;
		private System.Windows.Forms.TrackBar sldDelay;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label lblCritical;
		private System.Windows.Forms.Label lblDestroyed;
	}
}