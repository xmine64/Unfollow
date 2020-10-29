using System;
using System.Collections.Generic;

using Android.OS;

using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;

using Xamarin.Essentials;

using Google.Android.Material.Dialog;
using Google.Android.Material.Snackbar;

using Madamin.Unfollow.Fragments;

namespace Madamin.Unfollow
{
    public class FragmentHostBase :
        AppCompatActivity,
        IFragmentHost,
        FragmentManager.IOnBackStackChangedListener,
        IErrorHost,
        ISnackBarHost
    {
        protected FragmentHostBase(
            int layout,
            int actionbar,
            int container)
        {
            _layout = layout;
            _actionbar = actionbar;
            _container = container;

            Fragments = new List<Fragment>();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            SupportFragmentManager.AddOnBackStackChangedListener(this);

            SetContentView(_layout);
            SetSupportActionBar(FindViewById<Toolbar>(_actionbar));

            Create?.Invoke(this, new EventArgs());

            if (savedInstanceState != null) return;

            SupportFragmentManager
                .BeginTransaction()
                .Add(_container, Fragments[0])
                .Commit();
        }

        public void NavigateTo(int index)
        {
            NavigateTo(Fragments[index], false);
        }

        public void NavigateTo(Fragment fragment, bool add_to_back_stack)
        {
            var tx = SupportFragmentManager.BeginTransaction().Replace(_container, fragment);
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
            bool back_button_visible;
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                back_button_visible = true;
            }
            else
            {
                back_button_visible = false;
            }
            if (back_button_visible != _back_button_visible)
            {
                _back_button_visible = back_button_visible;
                SupportActionBar.SetDisplayHomeAsUpEnabled(_back_button_visible);
                BackButtonVisibilityChange?.Invoke(
                    this,
                    new OnBackButtonVisibilityChange(_back_button_visible));
            }
        }

        public void PushFragment(FragmentBase fragment)
        {
            NavigateTo(fragment, true);
        }

        public void PopFragment()
        {
            SupportFragmentManager.PopBackStack();
        }

        public void ShowError(Exception exception)
        {
            Android.Widget.Toast.MakeText(
                this,
                exception.Message,
                Android.Widget.ToastLength.Long);
#if DEBUG
            new MaterialAlertDialogBuilder(this)
                        .SetTitle(Resource.String.title_error)
                        .SetMessage(exception.ToString())
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args) => { })
                        .Show();
#endif
        }

        public void ShowSnackbar(int res)
        {
            var container = FindViewById(_container);
            var snack = Snackbar.Make(container, res, Snackbar.LengthLong);
            snack.Show();
        }

        public string ActionbarTitle
        {
            get => SupportActionBar.Title;
            set => SupportActionBar.Title = value;
        }

        public event EventHandler Create;
        public event EventHandler<OnBackButtonVisibilityChange> BackButtonVisibilityChange;

        public List<Fragment> Fragments { get; }

        private int _layout, _actionbar, _container;

        private bool _back_button_visible = false;
    }

    interface ISnackBarHost
    {
        void ShowSnackbar(int res);
    }

    interface IErrorHost
    {
        void ShowError(Exception ex);
    }

    public class OnBackButtonVisibilityChange
    {
        public OnBackButtonVisibilityChange(bool visible)
        {
            Visible = visible;
        }

        public bool Visible { get; }
    }
}