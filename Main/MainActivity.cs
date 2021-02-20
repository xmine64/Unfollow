using System;
using Java.Util;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using Google.Android.Material.BottomNavigation;
using Madamin.Unfollow.Fragments;
using Madamin.Unfollow.Instagram;
using AndroidX.AppCompat.Widget;
using AndroidX.Transitions;

namespace Madamin.Unfollow.Main
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round",
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public partial class MainActivity : AppCompatActivity
    {
        private readonly AccountsFragment _accountsFragment = new AccountsFragment();
        private readonly SettingsFragment _settingsFragment = new SettingsFragment();

        protected override void AttachBaseContext(Context context)
        {
            _preferences = PreferenceManager.GetDefaultSharedPreferences(context);

            var config = context.Resources?.Configuration;

            if (config == null)
                return;

            var appTheme = ((IPreferenceContainer)this).GetString(
                SettingsFragment.PreferenceKeyTheme,
                SettingsFragment.ThemeAdaptive);
            AppCompatDelegate.DefaultNightMode = appTheme switch
            {
                SettingsFragment.ThemeAdaptive => AppCompatDelegate.ModeNightFollowSystem,
                SettingsFragment.ThemeLight => AppCompatDelegate.ModeNightNo,
                SettingsFragment.ThemeDark => AppCompatDelegate.ModeNightYes,
                _ => AppCompatDelegate.DefaultNightMode
            };

            var appLang = ((IPreferenceContainer)this).GetString(
                SettingsFragment.PreferenceKeyLanguage,
                SettingsFragment.LanguageSystem);
            if (appLang == SettingsFragment.LanguageSystem ||
                appLang == null)
            {
                config.SetLocale(Locale.Default);
            }
            else
            {
                var locale = new Locale(appLang);
                config.SetLocale(locale);
            }

            base.AttachBaseContext(context.CreateConfigurationContext(config));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SupportFragmentManager.BackStackChanged += OnBackStackChanged;

            SetContentView(Resource.Layout.activity_main);

            SetSupportActionBar(FindViewById<Toolbar>(Resource.Id.main_appbar));

            _updateServer = new UpdateServerApi(this);

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                Window?.SetStatusBarColor(Color.Black);
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.OMr1)
            {
                Window?.SetNavigationBarColor(Color.Black);
            }

            var navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            if (navbar != null)
            {
                navbar.NavigationItemSelected += Navbar_NavigationItemSelected;
            }

            _currentPackage = PackageManager?.GetPackageInfo(PackageName, 0);

            if (((IPreferenceContainer)this).GetBoolean(
                SettingsFragment.PreferenceKeyAutoUpdate,
                true))
            {
                ((IUpdateChecker)this).CheckForUpdate(false);
            }

            try
            {
                _accounts = new Accounts(((IDataStorage)this).GetAccountsDir(),
                    ((IDataStorage)this).GetCacheDir());
            }
            catch (Exception ex)
            {
                ((IErrorHandler)this).ShowError(ex);
            }

            if (savedInstanceState != null)
            {
                // TODO: Load Accounts Data

                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    navbar.Visibility = ViewStates.Gone;
                }

                return;
            }

            BeginTransition();
            SupportFragmentManager
                .BeginTransaction()
                .Add(Resource.Id.main_container, _accountsFragment)
                .Commit();
        }

        private void OnBackStackChanged(object sender, EventArgs e)
        {
            var visible = SupportFragmentManager.BackStackEntryCount > 0;

            SupportActionBar.SetDisplayHomeAsUpEnabled(visible);

            var navbar = FindViewById(Resource.Id.main_navbar);
            navbar.Visibility = visible ? ViewStates.Gone : ViewStates.Visible;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            // TODO: Save account data

            base.OnSaveInstanceState(outState);
        }

        public override void OnBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
                ((IFragmentContainer)this).PopFragment();
            else
                Finish();
        }

        public override bool OnSupportNavigateUp()
        {
            if (SupportFragmentManager.BackStackEntryCount <= 0)
                return false;

            ((IFragmentContainer)this).PopFragment();
            return true;
        }

        private void BeginTransition()
        {
            //var rootView = FindViewById<ViewGroup>(Resource.Id.root);
            //TransitionManager.BeginDelayedTransition(rootView);
        }

        private void Navbar_NavigationItemSelected(object sender,
            BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navbar_main_item_accounts:
                    ((IFragmentContainer)this).NavigateTo(_accountsFragment, false);
                    break;

                case Resource.Id.navbar_main_item_settings:
                    ((IFragmentContainer)this).NavigateTo(_settingsFragment, false);
                    break;
            }
        }
    }
}
