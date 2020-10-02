using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;

using AndroidX.AppCompat.App;
using AndroidX.Preference;
using Java.Util;
using System.Linq;

namespace Madamin.Unfollow.Fragments
{
    public class SettingsFragment : 
        PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
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