using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Views;
using Xamarin.Essentials;
using AndroidX.AppCompat.App;

using Fragment = AndroidX.Fragment.App.Fragment;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Dialog;
using AndroidX.Preference;

namespace madamin.unfollow
{
    interface IInstagramActivity
    {
        Instagram Instagram { get; }
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
    public class MainActivity : AppCompatActivity, INavigationHost, IInstagramActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            var apptheme = PreferenceManager.GetDefaultSharedPreferences(this)
                .GetString("theme", "");
            if (apptheme == "light")
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            else if (apptheme == "dark")
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;

            SetContentView(Resource.Layout.activity_main);

            var navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            navbar.NavigationItemSelected += NavBar_NavigationItemSelected;

            var appbar = FindViewById<MaterialToolbar>(Resource.Id.main_appbar);
            appbar.MenuItemClick += Appbar_MenuItemClick;

            Instagram = new Instagram
            {
                DataDir = Path.Combine(Environment.DataDirectory.AbsolutePath),
                CacheDir = CacheDir.AbsolutePath
            };

            Instagram.LoadData();
            Instagram.LoadCache();

            _fragment_home = new HomeFragment();
            _fragment_unfollow = new UnfollowFragment();
            _fragment_settings = new SettingsFragment();

            if (savedInstanceState != null) return;
            
            if (Instagram.Count < 1)
            {
                // TODO: Show Login Fragment
            }
            SupportFragmentManager.BeginTransaction().Add(Resource.Id.main_container, _fragment_home).Commit();
        }

        private async void Appbar_MenuItemClick(object sender, AndroidX.AppCompat.Widget.Toolbar.MenuItemClickEventArgs args)
        {
            switch (args.Item.ItemId)
            {
                case Resource.Id.appmenu_item_refresh:
                    await Instagram.RefreshAll();
                    NavigateTo(_fragment_home, false);
                    break;
                case Resource.Id.appmenu_item_about:
                    new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.menu_about)
                        .SetMessage(Resource.String.msg_about)
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) => { })
                        .Show();
                    break;
                case Resource.Id.appmenu_item_exit:
                    Finish();
                    break;

            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.appmenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        private Fragment _fragment_home, _fragment_unfollow, _fragment_settings;

        private void NavBar_NavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.navbar_item_home:
                    NavigateTo(_fragment_home, false);
                    break;
                case Resource.Id.navbar_item_unfollows:
                    NavigateTo(_fragment_unfollow, false);
                    break;
                case Resource.Id.navbar_item_settings:
                    NavigateTo(_fragment_settings, false);
                    break;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Instagram.SaveData();
            Instagram.SaveCache();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void NavigateTo(Fragment fragment, bool add_to_back_stack)
        {
            var tx = SupportFragmentManager.BeginTransaction().Replace(Resource.Id.main_container, fragment);
            if (add_to_back_stack)
                tx.AddToBackStack(null);
            tx.Commit();
        }

        public Instagram Instagram { get; private set; }
    }
}

