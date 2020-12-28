using System.Linq;

using Android.Content;
using Android.OS;
using Android.Views;

using AndroidX.Preference;

namespace Madamin.Unfollow.Fragments
{
    public class SettingsFragment :
        PreferenceFragmentCompat,
        ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            var keys = new[] {"theme", "lang"};

            if (keys.Contains(key))
            {
                Activity?.Recreate();
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            ((IFragmentHost) Activity).ActionBarTitle = GetString(Resource.String.title_settings);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);
            PreferenceManager.GetDefaultSharedPreferences(Activity)
                .RegisterOnSharedPreferenceChangeListener(this);
            FindPreference("update_check").PreferenceClick += (sender, args) =>
            {
                ((IUpdateServerHost) Activity).CheckForUpdate(true);
            };
            FindPreference("about").PreferenceClick += (sender, args) =>
            {
                ((IFragmentHost) Activity).PushFullScreenFragment(new AboutFragment());
            };
            FindPreference("exit").PreferenceClick += (sender, args) => { Activity.Finish(); };
        }
    }
}