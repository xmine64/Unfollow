using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Preference;

namespace madamin.unfollow
{
    public class SettingsFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            ((IFragmentHost)Activity).ActionbarTitle = GetString(Resource.String.settings);
        }

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
                var apptheme = sharedPreferences.GetString("theme", "adaptive");
                if (apptheme == "adaptive")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                else if (apptheme == "light")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                else if (apptheme == "dark")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
            }
        }
    }
}