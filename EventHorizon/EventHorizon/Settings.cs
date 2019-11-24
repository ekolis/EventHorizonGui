using System;
using System.Linq;
using System.Configuration;

namespace EventHorizon.Properties
{
	// This class allows you to handle specific events on the settings class:
	//  The SettingChanging event is raised before a setting's value is changed.
	//  The PropertyChanged event is raised after a setting's value is changed.
	//  The SettingsLoaded event is raised after the setting values are loaded.
	//  The SettingsSaving event is raised before the setting values are saved.
	internal sealed partial class Settings
	{

		public Settings()
		{
		}

		protected override void OnSettingChanging(object sender, SettingChangingEventArgs e)
		{
			base.OnSettingChanging(sender, e);
			Mood? mood = Enum.GetValues(typeof(Mood)).Cast<Mood>().SingleOrDefault(m => m.ToString() + "MusicPath" == e.SettingKey);
		}
	}
}
