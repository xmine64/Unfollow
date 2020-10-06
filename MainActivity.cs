using System;
using System.IO;

using Java.Util;

using Android.App;
using Android.Views;
using Android.Content;

using AndroidX.AppCompat.App;
using AndroidX.Preference;

using Google.Android.Material.Dialog;
using Google.Android.Material.BottomNavigation;

using Madamin.Unfollow.Fragments;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
    public class MainActivity : FragmentHostBase, IInstagramHost
    {
        public MainActivity() : base(
            Resource.Layout.activity_main,
            Resource.Id.main_appbar,
            Resource.Id.main_container)
        {
            Create += MainActivity_OnCreate;
            BackButtonVisibilityChange += MainActivity_OnBackButtonVisibilityChange;
        }

        protected override void AttachBaseContext(Context context)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var config = context.Resources.Configuration;

            var apptheme = prefs.GetString("theme", "adaptive");
            if (apptheme == "adaptive")
            {
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
            }
            else if (apptheme == "light")
            {
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            }
            else if (apptheme == "dark")
            {
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
            }

            var applang = prefs.GetString("lang", "sysdef");
            if (applang == "sysdef")
            {
                config.SetLocale(Locale.Default);
            }
            else
            {
                var locale = new Locale(applang);
                config.SetLocale(locale);
            }

            base.AttachBaseContext(context.CreateConfigurationContext(config));
        }

        private void MainActivity_OnCreate(object sender, EventArgs e)
        {
            try
            {
                Accounts = new Accounts(
                    Path.Combine(DataDir.AbsolutePath, "accounts"),
                    CacheDir.AbsolutePath
                );

                var old_state = Path.Combine(DataDir.AbsolutePath, "session_data");
                var old_cache = Path.Combine(CacheDir.AbsolutePath, "cache_data");
                if (File.Exists(old_state))
                {
                    Accounts.RestoreDataFromOldVersion(old_state, old_cache);
                }
            }
            catch (Exception ex)
            {
                new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.title_error)
#if DEBUG
                        .SetMessage(ex.ToString())
#else
                        .SetMessage(ex.Message)
#endif
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) =>
                        {
                            Finish();
                        })
                        .Show();
            }

            _navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            _navbar.NavigationItemSelected += Navbar_NavigationItemSelected;

            Fragments.Add(new AccountsFragment());
            Fragments.Add(new SettingsFragment());
        }

        private void Navbar_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navbar_main_item_accounts:
                    NavigateTo(0);
                    break;

                case Resource.Id.navbar_main_item_settings:
                    NavigateTo(1);
                    break;
            }
        }

        private void MainActivity_OnBackButtonVisibilityChange(object sender, OnBackButtonVisibilityChange e)
        {
            if (e.Visible)
            {
                _navbar.Visibility = ViewStates.Gone;
            }
            else
            {
                _navbar.Visibility = ViewStates.Visible;
            }
        }

        public Accounts Accounts { get; private set; }

        private BottomNavigationView _navbar;
    }
}

