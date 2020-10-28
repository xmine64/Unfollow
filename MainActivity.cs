using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

using Java.Util;

using Android.App;
using Android.Views;
using Android.Widget;
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
    public class MainActivity : 
        FragmentHostBase, 
        IInstagramHost, 
        IDataContainer,
        IUpdateServerHost
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
            }
            catch (Exception ex)
            {
                ShowError(ex);
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

        public void OpenInInstagram(string username)
        {
            var intent = Intent.ParseUri(
                "https://instagram.com/_u/" + username,
                IntentUriType.None);
            intent.SetPackage("com.instagram.android");
            try
            {
                StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(this, Resource.String.error_ig_not_installed, ToastLength.Long);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        public void SaveData(string fileName, object data)
        {
            using (var file = new FileStream(
                Path.Combine(DataDir.AbsolutePath, fileName),
                FileMode.OpenOrCreate,
                FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, data);
            }
        }

        public object LoadData(string fileName)
        {
            using (var file = new FileStream(
                Path.Combine(DataDir.AbsolutePath, fileName),
                FileMode.Open,
                FileAccess.Read))
            {
                return new BinaryFormatter().Deserialize(file);
            }
        }

        public bool DataExists(string fileName)
        {
            return File.Exists(Path.Combine(DataDir.AbsolutePath, fileName));
        }

        public void CheckForUpdate()
        {
            RunOnUiThread(async () => {
                var result = await _update_server.CheckUpdate(
                    new CheckUpdateRequest { Version = 8 });
                if (result.Update != null)
                {
                    new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.title_update_available)
                        .SetMessage(result.Update.Message)
                        .SetPositiveButton(result.Update.Label, (sender, args) => {
                            if (result.Update.Url == "unfollow:ok")
                                return;
                            var intent = Intent.ParseUri(result.Update.Url, IntentUriType.None);
                            try
                            {
                                StartActivity(intent);
                            }
                            catch (Exception ex)
                            {
                                ShowError(ex);
                            }
                        })
                        .SetCancelable(true)
                        .Show();
                }
            });
        }

        private BottomNavigationView _navbar;

        // TODO: Don't hardcode this strings
        private UpdateServerApi _update_server = 
            new UpdateServerApi(
                "https://unfollowapp.herokuapp.com/api",
                "UnfollowApp/v0.5"
            );
    }

    interface IDataContainer
    {
        void SaveData(string fileName, object data);
        object LoadData(string fileName);
        bool DataExists(string fileName);
    }

    interface IUpdateServerHost
    {
        void CheckForUpdate();
    }
}

