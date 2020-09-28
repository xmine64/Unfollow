using System;
using System.IO;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Dialog;
using Xamarin.Essentials;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;

namespace madamin.unfollow
{
    interface IInstagramActivity
    {
        Instagram Instagram { get; }
    }

    interface IFragmentHost
    {
        void NavigateTo(Fragment fragment, bool add_to_backstack);
        void PushFragment(Fragment fragment);
        void PopFragment();
        string ActionbarTitle { get; set; }
    }

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round")]
    public class MainActivity : AppCompatActivity, IFragmentHost, IInstagramActivity,
        FragmentManager.IOnBackStackChangedListener, BottomNavigationView.IOnNavigationItemSelectedListener
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

            _navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            _navbar.SetOnNavigationItemSelectedListener(this);

            SetSupportActionBar(
                FindViewById<MaterialToolbar>(Resource.Id.main_appbar));


            Instagram = new Instagram
            {
                DataDir = Path.Combine(DataDir.AbsolutePath, "session_data"),
                CacheDir = CacheDir.AbsolutePath
            };
            Instagram.LoadData();

            _fragment_home = new HomeFragment();
            _fragment_settings = new SettingsFragment();

            SupportFragmentManager.AddOnBackStackChangedListener(this);

            if (savedInstanceState != null) return;
            SupportFragmentManager.BeginTransaction().Add(Resource.Id.main_container, _fragment_home).Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.appbar_menu_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_main_item_about:
                    new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.menu_about)
                        .SetMessage(Resource.String.msg_about)
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) => { })
                        .Show();
                    return true;
                case Resource.Id.appbar_main_item_exit:
                    Finish();
                    return true;

            }
            return false;
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navbar_main_item_home:
                    NavigateTo(_fragment_home, false);
                    return true;
                case Resource.Id.navbar_main_item_settings:
                    NavigateTo(_fragment_settings, false);
                    return true;
            }
            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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

        public override bool OnSupportNavigateUp()
        {
            SupportFragmentManager.PopBackStack();
            return true;
        }

        public void OnBackStackChanged()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                _navbar.Visibility = ViewStates.Gone;
            }
            else
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                _navbar.Visibility = ViewStates.Visible;
            }
        }

        public void PushFragment(Fragment fragment)
        {
            NavigateTo(fragment, true);
        }

        public void PopFragment()
        {
            SupportFragmentManager.PopBackStack();
        }

        public string ActionbarTitle 
        { 
            get => SupportActionBar.Title;
            set => SupportActionBar.Title = value;
        }

        public Instagram Instagram { get; private set; }

        private Fragment _fragment_home, _fragment_settings;
        private BottomNavigationView _navbar;
    }
}

