using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Java.Util;
using Android.App;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using Google.Android.Material.Dialog;
using Google.Android.Material.Snackbar;
using Google.Android.Material.BottomNavigation;
using Madamin.Unfollow.Fragments;
using Madamin.Unfollow.Instagram;
using Path = System.IO.Path;
using AndroidX.Browser.CustomTabs;

namespace Madamin.Unfollow
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round",
        WindowSoftInputMode = SoftInput.AdjustResize)]
    public class MainActivity :
        FragmentHostBase,
        IInstagramHost,
        IDataContainer,
        IUpdateServerHost,
        IErrorHost,
        ISnackBarHost,
        IPreferenceManager,
        IVersionProvider,
        ICustomTabProvider
    {
        public MainActivity() : base(
            Resource.Layout.activity_main,
            Resource.Id.main_appbar,
            Resource.Id.main_container)
        {
            Create += MainActivity_OnCreate;
            SaveState += MainActivity_OnSaveState;
            BackButtonVisibilityChange += MainActivity_OnBackButtonVisibilityChange;
        }

        private string cache_path = null;
        private string data_path = null;
        
        private const string AccountsFolder = "accounts";

        protected override void AttachBaseContext(Context context)
        {
            _preferences = PreferenceManager.GetDefaultSharedPreferences(context);

            var config = context.Resources?.Configuration;

            if (config == null)
                return;

            var appTheme = ((IPreferenceManager)this).GetString(
                SettingsFragment.PreferenceKeyTheme,
                SettingsFragment.ThemeAdaptive);
            AppCompatDelegate.DefaultNightMode = appTheme switch
            {
                SettingsFragment.ThemeAdaptive => AppCompatDelegate.ModeNightFollowSystem,
                SettingsFragment.ThemeLight => AppCompatDelegate.ModeNightNo,
                SettingsFragment.ThemeDark => AppCompatDelegate.ModeNightYes,
                _ => AppCompatDelegate.DefaultNightMode
            };

            var appLang = ((IPreferenceManager)this).GetString(
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

        private void MainActivity_OnCreate(object sender, OnActivityCreateEventArgs e)
        {
            _updateServer = new UpdateServerApi(this);

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                Window?.SetStatusBarColor(Color.Black);
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.OMr1)
            {
                Window?.SetNavigationBarColor(Color.Black);
            }

            if (FilesDir == null ||
                CacheDir == null)
                return;
            data_path = FilesDir.AbsolutePath;
            cache_path = CacheDir.AbsolutePath;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.P &&
                DataDir != null) {
                var old_accounts_path = Path.Combine(DataDir.AbsolutePath, AccountsFolder);
                if (Directory.Exists(old_accounts_path))
                {
                    data_path = DataDir.AbsolutePath;
                }
            }

            try
            {
                var accounts_path = Path.Combine(data_path, AccountsFolder);
                Accounts = new Accounts(accounts_path, cache_path);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

            _navbar = FindViewById<BottomNavigationView>(Resource.Id.main_navbar);
            if (_navbar != null)
            {
                _navbar.NavigationItemSelected += Navbar_NavigationItemSelected;
            }

            Fragments.Add(new AccountsFragment());
            Fragments.Add(new SettingsFragment());

            if (PackageName == null) return;
            var package = PackageManager?.GetPackageInfo(PackageName, 0);
            _currentPackage = package;

            if (e.SavedInstanceBundle != null)
            {
                // TODO: Load Accounts Data
                return;
            }

            if (((IPreferenceManager)this).GetBoolean(
                SettingsFragment.PreferenceKeyAutoUpdate,
                true))
            {
                CheckForUpdate(false);
            }
        }

        private void MainActivity_OnSaveState(object sender, OnSaveStateEventArgs e)
        {
            // TODO: Save Accounts Data
        }

        private void Navbar_NavigationItemSelected(object sender,
            BottomNavigationView.NavigationItemSelectedEventArgs e)
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

        private void MainActivity_OnBackButtonVisibilityChange(object sender, OnBackButtonVisibilityChangeEventArgs e)
        {
            _navbar.Visibility = e.Visible ? ViewStates.Gone : ViewStates.Visible;
        }

        public Accounts Accounts { get; private set; }

        public void OpenInInstagram(string username)
        {
            var url = Android.Net.Uri.Parse(
                GetString(Resource.String.url_instagram_user, username));
            var intent = new Intent(Intent.ActionView, url);
            intent?.SetPackage(GetString(Resource.String.pkg_instagram));
            try
            {
                StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                ((ICustomTabProvider)this).LaunchBrowser(url);
            }
            catch (Exception ex)
            {
                ((IErrorHost)this).ShowError(ex);
            }
        }

        public void SaveData(string fileName, object data)
        {
            if (data_path == null)
                return;

            var filePath = Path.Combine(data_path, fileName);
            using var file = new FileStream(
                filePath,
                FileMode.OpenOrCreate,
                FileAccess.Write);
            new BinaryFormatter().Serialize(file, data);
        }

        public object LoadData(string fileName)
        {
            if (data_path == null)
                return null;

            var filePath = Path.Combine(data_path, fileName);
            using var file = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read);
            return new BinaryFormatter().Deserialize(file);
        }

        public bool DataExists(string fileName)
        {
            if (data_path == null)
                return false;

            return File.Exists(Path.Combine(data_path, fileName));
        }

        public async void CheckForUpdate(bool verbose)
        {
            try
            {
                // Request update information
#if TGBUILD
                var lang = Resources?.Configuration?.Locales.Get(0)?.Language ?? 
                           UpdateServerApi.LanguageEnglish;
#else
                var lang = UpdateServerApi.LanguageGithubChannel;
#endif
                var result = await _updateServer.CheckUpdate(
                    ((IVersionProvider) this).GetAppVersionCode(), lang);

                // Check response
                if (result.Status != UpdateServerApi.StatusOk)
                    throw new Exception(result.Message);

                // Stop if update is not available
                if (!result.Available)
                {
                    if (verbose)
                        ((ISnackBarHost) this).ShowSnackbar(Resource.String.msg_up_to_date);
                    return;
                }

                // Show an update dialog
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.title_update_available);
                dialog.SetMessage(result.Update.Message);

                dialog.SetPositiveButton(
                    result.Update.ButtonLabel,
                    (sender, args) =>
                    {
                        var intent = Intent.ParseUri(result.Update.ButtonUrl, IntentUriType.None);
                        try
                        {
                            StartActivity(intent);
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex);
                        }
                    });

                dialog.SetNegativeButton(
                    Android.Resource.String.Cancel,
                    (sender, args) => { });

                dialog.Show();
            }
            catch (Exception exception)
            {
                ((IErrorHost) this).ShowError(exception);
            }
        }

#if TGBUILD
        public async void DidLogin()
        {
            await _updateServer.DidLogin(((IVersionProvider) this).GetAppVersionCode());
        }
#endif

#if DEBUG
        public async void DidLogin()
        {
            await _updateServer.DidLogin(-1);
        }
#endif

        public void ShowError(Exception exception)
        {
            var container = FindViewById(Resource.Id.main_container);

            var snack = Snackbar.Make(
                container, 
                Resource.String.msg_error, 
                Snackbar.LengthLong);
            snack.SetAnchorView(Resource.Id.main_navbar);

            snack.SetAction(Resource.String.button_text_details, view =>
            {
                // Show error details in an alert dialog
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.title_error);
                dialog.SetMessage(exception.ToString());

                // Send bug report
                dialog.SetPositiveButton(Resource.String.button_text_report, async (sender, args) =>
                {
                    try
                    {
                        ((ISnackBarHost)this).ShowSnackbar(Resource.String.msg_sending_report);

                        var response = await _updateServer.BugReport(exception);

                        if (response.Status == UpdateServerApi.StatusOk)
                        {
                            ((ISnackBarHost)this).ShowSnackbar(Resource.String.msg_report_sent);
                        }
                        else
                        {
                            throw new Exception(response.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        ((IErrorHost) this).ShowError(ex);
                    }
                });
                
                dialog.SetNegativeButton(
                    Android.Resource.String.Cancel,
                    (sender, args) => {});

                dialog.Show();
            });
            
            if (_navbar.Visibility == ViewStates.Visible)
                snack.SetAnchorView(_navbar);

            snack.Show();
        }

        public void ShowSnackbar(int res)
        {
            var rootView = FindViewById(Resource.Id.root);
            var snack = Snackbar.Make(rootView, res, Snackbar.LengthLong);
            //if (_navbar.Visibility == ViewStates.Visible)
            //snack.SetAnchorView(_navbar);
            snack.Show();
        }

        private BottomNavigationView _navbar;

        private UpdateServerApi _updateServer;

        private ISharedPreferences _preferences;

        public string GetString(string key, string defaultValue)
        {
            return _preferences.GetString(key, defaultValue) ?? defaultValue;
        }

        public bool GetBoolean(string key, bool defaultValue)
        {
            return _preferences.GetBoolean(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            _preferences.Edit()?.PutString(key, value)?.Apply();
        }

        public void SetBoolean(string key, bool value)
        {
            _preferences.Edit()?.PutBoolean(key, value)?.Apply();
        }

        private Android.Content.PM.PackageInfo _currentPackage;

        public long GetAppVersionCode()
        {
            if (_currentPackage == null)
                return -1;
            return _currentPackage.LongVersionCode;
        }

        public string GetAppVersionName()
        {
            if (_currentPackage == null)
                return string.Empty;
            return _currentPackage.VersionName;
        }

        public AssemblyName GetAppAssemblyName()
        {
            return GetType().Assembly.GetName();
        }

        public AssemblyName GetLibraryAssemblyName()
        {
            return typeof(InstagramApiSharp.API.IInstaApi).Assembly.GetName();
        }

        public void LaunchBrowser(Android.Net.Uri url)
        {
            var builder = new CustomTabsIntent.Builder();
            builder.SetShowTitle(true);
            builder.SetUrlBarHidingEnabled(true);
            var customTabs = builder.Build();
            customTabs.LaunchUrl(this, url);
        }
    }

    public interface IDataContainer
    {
        void SaveData(string fileName, object data);
        object LoadData(string fileName);
        bool DataExists(string fileName);
    }

    public interface IUpdateServerHost
    {
        void CheckForUpdate(bool verbose);

#if TGBUILD || DEBUG
        void DidLogin();
#endif
    }

    public interface ISnackBarHost
    {
        void ShowSnackbar(int res);
    }

    public interface IErrorHost
    {
        void ShowError(Exception ex);
    }

    public interface IPreferenceManager
    {
        string GetString(string key, string defaultValue);
        bool GetBoolean(string key, bool defaultValue);
        void SetString(string key, string value);
        void SetBoolean(string key, bool value);
    }

    public interface IVersionProvider
    {
        long GetAppVersionCode();
        string GetAppVersionName();
        AssemblyName GetAppAssemblyName();
        AssemblyName GetLibraryAssemblyName();
    }

    public interface ICustomTabProvider
    {
        void LaunchBrowser(Android.Net.Uri url);
    }
}
