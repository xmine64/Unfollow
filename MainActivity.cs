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
    public interface IInstagramActivity
    {
        Instagram Instagram { get; }

        void Logout();

        Task RefreshCache();
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
    public class MainActivity : AppCompatActivity, INavigationHost, IInstagramActivity, 
        ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private string _session_data_path;
        private string _cache_data_path;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            _session_data_path = Path.Combine(DataDir.AbsolutePath, "session_data");
            if (!File.Exists(_session_data_path))
            {
                var login_intent = new Intent(this, typeof(LoginActivity));
                login_intent.PutExtra("session_data_path", _session_data_path);
                StartActivity(login_intent);
                Finish();
                return;
            }

            var pref = PreferenceManager.GetDefaultSharedPreferences(this);
            var apptheme = pref.GetString("theme", "Adaptive");
            if (apptheme == "Light")
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            else if (apptheme == "Dark")
                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
            pref.RegisterOnSharedPreferenceChangeListener(this);

            Instagram = new Instagram(_session_data_path);
            Instagram.Load();

            SetContentView(Resource.Layout.activity_main);

            var navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);            
            navbar.NavigationItemSelected += NavBar_NavigationItemSelected;

            _cache_data_path = Path.Combine(CacheDir.AbsolutePath, "cache_data");
            if (File.Exists(_cache_data_path))
            {
                Instagram.LoadCache(_cache_data_path);
            }
            else
            {
                await RefreshCache();
            }

            _fragment_home = new HomeFragment();
            _fragment_unfollow = new UnfollowFragment();
            _fragment_settings = new SettingsFragment();

            if (savedInstanceState != null) return;
            SupportFragmentManager.BeginTransaction().Add(Resource.Id.main_container, _fragment_home).Commit();
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == "theme")
            {
                var apptheme = sharedPreferences.GetString("theme", "Adapative");
                if (apptheme == "Adaptive")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
                else if (apptheme == "Light")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                else if (apptheme == "Dark")
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
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
            Instagram?.Save();
            Instagram?.SaveCache(_cache_data_path);
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

        public async void Logout()
        {
            await Instagram.Logout();
            File.Delete(_session_data_path);
            File.Delete(_cache_data_path);
            StartActivity(typeof(MainActivity));
            Finish();
        }

        public async Task RefreshCache()
        {
            await Instagram.Refresh();
            Instagram.SaveCache(_cache_data_path);
        }

        public Instagram Instagram { get; private set; }
    }
}

