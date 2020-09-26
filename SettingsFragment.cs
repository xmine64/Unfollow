using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.Preference;

namespace madamin.unfollow
{
    public class SettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);
            PreferenceManager.GetDefaultSharedPreferences(Activity)
                .RegisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == "theme")
            {
                var apptheme = sharedPreferences.GetString("theme", "Adapative");
                if (apptheme == "Adaptive")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                else if (apptheme == "Light")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                else if (apptheme == "Dark")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
            }
        }
    }
}