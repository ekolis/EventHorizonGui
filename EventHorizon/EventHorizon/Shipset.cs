using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EventHorizon
{
	/// <summary>
	/// A set of ship graphics used by a player.
	/// </summary>
	public class Shipset
	{
		private IDictionary<int, IDictionary<ShipConfig, Image>> configs = new Dictionary<int, IDictionary<ShipConfig, Image>>();

		private static readonly Bitmap EmptyBitmap = new Bitmap(1, 1);

		/// <summary>
		/// The default shipset for our ships.
		/// </summary>
		public static Shipset DefaultFriendly = new Shipset("DefaultFriendly", true);

		/// <summary>
		/// The default shipset for enemy ships.
		/// </summary>
		public static Shipset DefaultEnemy = new Shipset("DefaultEnemy", false);

		/// <summary>
		/// Loads a shipset.
		/// </summary>
		/// <param name="name"></param>
		public Shipset(string name, bool isOurs)
		{
			Name = name;
			IsOurs = isOurs;
			ShipCoreImage = TryLoadImage("ShipCore.png");
			ConstructionBaysImage = TryLoadImage("ConstructionBays.png");
			ThrustersImage = TryLoadImage("Thrusters.png");
			WeaponsImage = TryLoadImage("Weapons.png");
			BeamImage = TryLoadImage("Beam.png");
			DamageImage = TryLoadImage("Damage.png");
			ShieldFlareImage = TryLoadImage("ShieldFlare.png");
			ExplosionImage = TryLoadImage("Explosion.png");
		}

		private Image TryLoadImage(string filename)
		{
			var path = Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Path.Combine("Shipsets", Name)), filename);
			try
			{
				return Image.FromFile(path);
			}
			catch
			{
				var missing = Path.GetFullPath(path);
				if (IsOurs && this.Name != "DefaultFriendly")
				{
					var img = DefaultFriendly.TryLoadImage(filename);
					return img;
				}
				else if (!IsOurs && this.Name != "DefaultEnemy")
				{
					var img = DefaultEnemy.TryLoadImage(filename);
					return img;
				}
				MessageBox.Show("Warning: Default shipset image\n" + missing + "\nis missing!");
				return EmptyBitmap;
			}
		}

		/// <summary>
		/// The name of the shipset.
		/// Shipset files will be searched for in this directory under the Shipsets directory of the game.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The ship core image.
		/// All ships have this.
		/// File: ShipCore.png.
		/// </summary>
		public Image ShipCoreImage { get; private set; }

		/// <summary>
		/// The construction bays image.
		/// Only motherships have this.
		/// File: ConstructionBays.png.
		/// </summary>
		public Image ConstructionBaysImage { get; private set; }

		/// <summary>
		/// The thrusters image.
		/// Only mobile ships have this.
		/// File: Thrusters.png.
		/// </summary>
		public Image ThrustersImage { get; private set; }

		/// <summary>
		/// The weapons image.
		/// Only armed ships have this.
		/// File: Weapons.png.
		/// </summary>
		public Image WeaponsImage { get; private set; }

		/// <summary>
		/// The beam image.
		/// Used in battles.
		/// File: Beam.png.
		/// </summary>
		public Image BeamImage { get; private set; }

		/// <summary>
		/// The damage image.
		/// Used in battles.
		/// File: Damage.png.
		/// </summary>
		public Image DamageImage { get; private set; }

		/// <summary>
		/// The shield flare image.
		/// Used in battles.
		/// File: ShieldFlare.png.
		/// </summary>
		public Image ShieldFlareImage { get; private set; }

		/// <summary>
		/// The explosion image.
		/// Used in battles.
		/// File: Explosion.png.
		/// </summary>
		public Image ExplosionImage { get; private set; }

		/// <summary>
		/// Builds a ship image.
		/// </summary>
		/// <param name="imgSize">The size of the desired image.</param>
		/// <param name="config">Configuration of the ship.</param>
		/// <returns></returns>
		public Image BuildShipImage(int imgSize, ShipConfig config)
		{
			// look in cache
			if (configs.ContainsKey(imgSize) && configs[imgSize].ContainsKey(config))
				return configs[imgSize][config];

			var scale = config.ProportionalSize * (float)imgSize;
			var offset = ((float)imgSize - scale) / 2f;
			var img = new Bitmap(imgSize, imgSize);
			var g = Graphics.FromImage(img);
			g.DrawImage(ShipCoreImage, new RectangleF(offset, offset, scale, scale));
			if (config.IsMothership)
				g.DrawImage(ConstructionBaysImage, new RectangleF(offset, offset, scale, scale));
			if (config.IsMobile)
				g.DrawImage(ThrustersImage, new RectangleF(offset, offset, scale, scale));
			if (config.IsArmed)
				g.DrawImage(WeaponsImage, new RectangleF(offset, offset, scale, scale));


			// save in cache
			if (!configs.ContainsKey(imgSize))
				configs.Add(imgSize, new Dictionary<ShipConfig, Image>());
			configs[imgSize].Add(config, img);

			return img;
		}

		/// <summary>
		/// Is this our shipset? If so, it will fall back on DefaultFriendly if the image could not be found. Otherwise, it will fall back on DefaultEnemy.
		/// </summary>
		public bool IsOurs { get; private set; }
	}
}
