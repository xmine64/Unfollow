using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    public class SettingsFragment :
        PreferenceFragmentCompat,
        ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public const string PrivacyPolicyEnglishUrl = "https://unfollowapp.herokuapp.com/docs/en/terms.html";
        public const string DonateEnglishUrl = "https://unfollowapp.herokuapp.com/docs/en/donate.html";

        public const string PrivacyPolicyPersianUrl = "https://unfollowapp.herokuapp.com/docs/fa/terms.html";
        public const string DonatePersianUrl = "https://unfollowapp.herokuapp.com/docs/fa/donate.html";

        public const string PreferenceKeyTheme = "theme";
        public const string PreferenceKeyLanguage = "lang";
        public const string PreferenceKeyAutoUpdate = "auto_update_check";

        public const string ThemeAdaptive = "adaptive";
        public const string ThemeLight = "light";
        public const string ThemeDark = "dark";

        public const string LanguageSystem = "sysdef";

        private const string PreferenceKeyUpdateCheck = "update_check";
        private const string PreferenceKeyTerms = "terms";
        private const string PreferenceKeyDonate = "donate";
        private const string PreferenceKeyAbout = "about";

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_settings);
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            SetPreferencesFromResource(Resource.Xml.settings, rootKey);

            PreferenceManager.GetDefaultSharedPreferences(Context)
                .RegisterOnSharedPreferenceChangeListener(this);

            FindPreference(PreferenceKeyUpdateCheck).PreferenceClick += UpdateCheck_Click;
            FindPreference(PreferenceKeyTerms).PreferenceClick += Terms_Click;
            FindPreference(PreferenceKeyDonate).PreferenceClick += Donate_Click;
            FindPreference(PreferenceKeyAbout).PreferenceClick += About_Click;
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (new[]
            {
                PreferenceKeyTheme,
                PreferenceKeyLanguage
            }.Contains(key))
            {
                Activity?.Recreate();
            }
        }

        private void UpdateCheck_Click(object sender, Preference.PreferenceClickEventArgs args)
        {
            ((IUpdateChecker)Activity).CheckForUpdate();
        }

        private void About_Click(object sender, Preference.PreferenceClickEventArgs args)
        {
            ((IFragmentContainer)Activity).PushFragment(new AboutFragment());
        }

        private void Terms_Click(object sender, Preference.PreferenceClickEventArgs args)
        {
            // TODO: Persian support
            ((IUrlHandler)Activity).LaunchBrowser(PrivacyPolicyEnglishUrl);
        }

        private void Donate_Click(object sender, Preference.PreferenceClickEventArgs args)
        {
            // TODO: Persian support
            ((IUrlHandler)Activity).LaunchBrowser(DonateEnglishUrl);
        }
    }
}