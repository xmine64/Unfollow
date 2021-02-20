using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.Transitions;
using Xamarin.Essentials;

namespace Madamin.Unfollow
{
    public class FragmentHostBase :
        AppCompatActivity,
        IFragmentHost,
        FragmentManager.IOnBackStackChangedListener
    {
        private const string BundleKeyBackButtonVisible= "backbutton_visibility";
        private const string BundleKeyActionBarVisible = "actionbar_visibility";

        private readonly int _actionBar, _container, _layout;

        private bool _backButtonVisible, _actionBarVisible = true;

        protected FragmentHostBase(
            int layout,
            int actionBar,
            int container)
        {
            _layout = layout;
            _actionBar = actionBar;
            _container = container;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            SupportFragmentManager.AddOnBackStackChangedListener(this);

            SetContentView(_layout);

            SetSupportActionBar(FindViewById<Toolbar>(_actionBar));

            Create?.Invoke(this, new OnActivityCreateEventArgs(savedInstanceState));

            if (savedInstanceState != null)
            {
                if (savedInstanceState.GetBoolean(BundleKeyBackButtonVisible))
                    BackButtonVisible = true;

                if (!savedInstanceState.GetBoolean(BundleKeyActionBarVisible))
                    ActionBarVisible = false;

                return;
            }

            BeginTransition();
            SupportFragmentManager
                .BeginTransaction()
                .Add(_container, Fragments[0])
                .Commit();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(BundleKeyBackButtonVisible, _backButtonVisible);
            outState.PutBoolean(BundleKeyActionBarVisible, ActionBarVisible);

            // TODO: save fragments state

            SaveState?.Invoke(this, new OnSaveStateEventArgs(outState));

            base.OnSaveInstanceState(outState);
        }

        protected List<Fragment> Fragments { get; } = new List<Fragment>();

        public void NavigateTo(Fragment fragment, bool addToBackStack)
        {
            BeginTransition();

            var tx = SupportFragmentManager.BeginTransaction();

            tx.Replace(_container, fragment);

            if (addToBackStack)
                tx.AddToBackStack(null);

            tx.Commit();
        }

        public void PushFragment(Fragment fragment)
        {
            NavigateTo(fragment, true);
        }

        public void PopFragment()
        {
            BeginTransition();

            ActionBarVisible = true;

            SupportFragmentManager.PopBackStack();
        }

        public string ActionBarTitle
        {
            get => SupportActionBar.Title;
            set => SupportActionBar.Title = value;
        }

        public bool ActionBarVisible
        {
            get => _actionBarVisible;
            set
            {
                if (ActionBarVisible == value)
                    return;

                _actionBarVisible = value;
                OnActionBarVisibilityChanged(value);
            }
        }

        public bool BackButtonVisible
        {
            get => _backButtonVisible;
            set
            {
                if (BackButtonVisible == value)
                    return;

                _backButtonVisible = value;
                OnBackButtonVisibilityChanged(value);
            }
        }

        public bool IsDarkMode()
        {
            try
            {
                return Resources?.Configuration?.IsNightModeActive ?? false;
            }
            catch (Java.Lang.NoSuchMethodError)
            {
                return AppCompatDelegate.DefaultNightMode == AppCompatDelegate.ModeNightYes;
            }
        }

        public void OnBackStackChanged()
        {
            BackButtonVisible = SupportFragmentManager.BackStackEntryCount > 0;
        }

        protected void NavigateTo(int index)
        {
            NavigateTo(Fragments[index], false);
        }

        private void BeginTransition()
        {
            var rootView = (ViewGroup) FindViewById(Resource.Id.root);
            TransitionManager.BeginDelayedTransition(rootView);
        }

        public override bool OnSupportNavigateUp()
        {
            PopFragment();
            return true;
        }

        public override void OnBackPressed()
        {
            if (BackButtonVisible)
                PopFragment();
            else
                Finish();
        }

        private void OnActionBarVisibilityChanged(bool visible)
        {
            var actionBar = FindViewById(_actionBar);
            if (actionBar == null) return;
            actionBar.Visibility = visible ? 
                ViewStates.Visible :
                ViewStates.Gone;
        }

        private void OnBackButtonVisibilityChanged(bool visible)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(visible);
            BackButtonVisibilityChange?.Invoke(
                this,
                new OnBackButtonVisibilityChangeEventArgs(visible));
        }

        public event EventHandler<OnActivityCreateEventArgs> Create;
        public event EventHandler<OnSaveStateEventArgs> SaveState;
        public event EventHandler<OnBackButtonVisibilityChangeEventArgs> BackButtonVisibilityChange;
    }

    public interface IFragmentHost
    {
        string ActionBarTitle { get; set; }
        bool ActionBarVisible { get; set; }
        bool BackButtonVisible { get; set; }

        void NavigateTo(Fragment fragment, bool addToBackStack);

        void PushFragment(Fragment fragment);
        void PopFragment();

        bool IsDarkMode();
    }

    public class OnActivityCreateEventArgs
    {
        public OnActivityCreateEventArgs(Bundle savedInstanceBundle)
        {
            SavedInstanceBundle = savedInstanceBundle;
        }

        public Bundle SavedInstanceBundle { get; }
    }

    public class OnSaveStateEventArgs
    {
        public OnSaveStateEventArgs(Bundle state)
        {
            State = state;
        }

        public Bundle State { get; }
    }

    public class OnBackButtonVisibilityChangeEventArgs
    {
        public OnBackButtonVisibilityChangeEventArgs(bool visible)
        {
            Visible = visible;
        }

        public bool Visible { get; }
    }
}