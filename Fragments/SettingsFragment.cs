using System;
using System.Linq;

using Android.OS;
using Android.Views;
using Android.Content;

using AndroidX.Preference;

namespace Madamin.Unfollow.Fragments
{
    public class SettingsFragment : 
        PreferenceFragmentCompat,
        ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            ((IFragmentHost)Activity).ActionbarTitle = GetString(Resource.String.title_settings);
            
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);
            PreferenceManager.GetDefaultSharedPreferences(Activity)
                .RegisterOnSharedPreferenceChangeListener(this);
            FindPreference("update_check").PreferenceClick += (sender, args) =>
            {
                ((IUpdateServerHost)Activity).CheckForUpdate(true);
            };
            FindPreference("about").PreferenceClick += (sender, args) =>
            {
                ((IFragmentHost)Activity).PushFragment(new AboutFragment());
            };
            FindPreference("exit").PreferenceClick += (sender, args) =>
            {
                Activity.Finish();
            };
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            var keys = new[] { "theme", "lang" };

            if (keys.Contains(key))
            {
                Activity?.Recreate();
                return;
            }
        }
    }
}