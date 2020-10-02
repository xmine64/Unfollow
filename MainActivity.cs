using System;
using System.IO;

using Android.App;
using Android.Views;
using Android.Content;
using Android.Runtime;
using Xamarin.Essentials;

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
            Resource.Menu.appbar_menu_main,
            Resource.Id.main_appbar,
            Resource.Id.main_container)
        {
            Create += MainActivity_OnCreate;
            MenuItemSelected += MainActivity_OnMenuItemSelected;
            BackButtonVisibilityChange += MainActivity_OnBackButtonVisibilityChange;
        }

        protected override void AttachBaseContext(Context context)
        {
            var prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var config = context.Resources.Configuration;

            var apptheme = prefs.GetString("theme", "");
            if (apptheme == "light")
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
                config.SetLocale(Java.Util.Locale.Default);
            }
            else
            {
                var locale = new Java.Util.Locale(applang);
                config.SetLocale(locale);
            }

            base.AttachBaseContext(context.CreateConfigurationContext(config));
        }

        private void MainActivity_OnCreate(object sender, EventArgs e)
        {
            try
            {
                Accounts = new Accounts
                {
                    DataDir = Path.Combine(DataDir.AbsolutePath, "session_data"),
                    CacheDir = CacheDir.AbsolutePath
                };
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

        private void MainActivity_OnMenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_main_item_about:
                    new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.title_about)
                        .SetMessage(Resource.String.msg_about)
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) => { })
                        .Show();
                    break;

                case Resource.Id.appbar_main_item_exit:
                    Finish();
                    break;

                default:
                    e.Finished = false;
                    break;
            }
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public Accounts Accounts { get; private set; }

        private BottomNavigationView _navbar;
    }
}

