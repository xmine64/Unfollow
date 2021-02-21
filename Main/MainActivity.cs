using System;
using Java.Util;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using AndroidX.AppCompat.Widget;
using AndroidX.Transitions;
using AndroidX.Fragment.App;
using Google.Android.Material.BottomNavigation;
using Madamin.Unfollow.Fragments;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Main
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round",
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public partial class MainActivity : AppCompatActivity
    {
        private readonly AccountsFragment _accountsFragment = new AccountsFragment();
        private readonly SettingsFragment _settingsFragment = new SettingsFragment();

        private Toolbar _appBar;
        private BottomNavigationView _navBar;
        private FragmentContainerView _mainContainer;

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

            SetContentView(Resource.Layout.activity_main);

            _appBar = FindViewById<Toolbar>(Resource.Id.main_appbar);
            _navBar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            _mainContainer = FindViewById<FragmentContainerView>(Resource.Id.main_container);
            _currentPackage = PackageManager?.GetPackageInfo(PackageName, 0);

            SetSupportActionBar(_appBar);
            SupportFragmentManager.BackStackChanged += OnBackStackChanged;
            _navBar.NavigationItemSelected += Navbar_NavigationItemSelected;

            // Work around for white icons on white background
            // on status bar and navigation bar on older versions of Android
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                Window?.SetStatusBarColor(Color.Black);
            if (Build.VERSION.SdkInt < BuildVersionCodes.OMr1)
                Window?.SetNavigationBarColor(Color.Black);

            _updateServer = new UpdateServerApi(this);

            var accountsDir = ((IDataStorage)this).GetAccountsDir();
            var cacheDir = ((IDataStorage)this).GetCacheDir();

            if (accountsDir == null ||
                cacheDir == null)
            {
                // TODO: Panic
                return;
            }

            _accounts = new Accounts(accountsDir, cacheDir);

            if (savedInstanceState != null)
            {
                // TODO: Load Accounts Data

                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    _navBar.Visibility = ViewStates.Gone;
                }

                return;
            }

            BeginTransition();
            SupportFragmentManager
                .BeginTransaction()
                .Add(Resource.Id.main_container, _accountsFragment)
                .Commit();

            if (((IPreferenceContainer)this).GetBoolean(
                SettingsFragment.PreferenceKeyAutoUpdate,
                true))
            {
                ((IUpdateChecker)this).CheckForUpdate();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _updateServer.Dispose();
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
#if DEBUG
            TransitionManager.BeginDelayedTransition(_mainContainer);
#endif
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
