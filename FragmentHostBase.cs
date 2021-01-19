using System;
using System.Collections.Generic;

using Android.OS;
using Android.Views;

using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.Transitions;

using Xamarin.Essentials;

using Madamin.Unfollow.Fragments;

namespace Madamin.Unfollow
{
    public class FragmentHostBase :
        AppCompatActivity,
        IFragmentHost,
        FragmentManager.IOnBackStackChangedListener
    {
        protected FragmentHostBase(
            int layout,
            int actionBar,
            int container)
        {
            _layout = layout;
            _actionBar = actionBar;
            _container = container;

            Fragments = new List<Fragment>();
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
                if (savedInstanceState.GetBoolean(BackButtonVisibilityStateKey))
                {
                    _backButtonVisible = true;
                    OnBackButtonVisibilityChanged(true);
                }

                if (!savedInstanceState.GetBoolean(ActionBarVisibilityStateKey))
                {
                    ActionBarVisible = false;
                }

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
            outState.PutBoolean(BackButtonVisibilityStateKey, _backButtonVisible);
            outState.PutBoolean(ActionBarVisibilityStateKey, ActionBarVisible);

            // TODO: save fragments state

            SaveState?.Invoke(this, new OnSaveStateEventArgs(outState));

            base.OnSaveInstanceState(outState);
        }

        private const string BackButtonVisibilityStateKey = "backbutton_visibility";
        private const string ActionBarVisibilityStateKey = "actionbar_visibility";

        protected void NavigateTo(int index)
        {
            NavigateTo(Fragments[index], false, false);
        }

        private void BeginTransition()
        {
            var rootView = (ViewGroup)FindViewById(Resource.Id.root);
            TransitionManager.BeginDelayedTransition(rootView);
        }

        public void NavigateTo(Fragment fragment, bool fullscreen, bool addToBackStack)
        {
            BeginTransition();
            var tx = SupportFragmentManager.BeginTransaction().Replace(_container, fragment);

            if (addToBackStack)
            {
                tx.AddToBackStack(null);
            }

            ActionBarVisible = !fullscreen;

            tx.Commit();
        }

        public override bool OnSupportNavigateUp()
        {
            PopFragment();
            return true;
        }

        public override void OnBackPressed()
        {
            if (_backButtonVisible)
            {
                PopFragment();
            }
            else
            {
                Finish();
            }
        }

        public void OnBackStackChanged()
        {
            var backButtonVisible = SupportFragmentManager.BackStackEntryCount > 0;
            if (backButtonVisible == _backButtonVisible) return;
            _backButtonVisible = backButtonVisible;
            OnBackButtonVisibilityChanged(_backButtonVisible);
        }

        private void OnBackButtonVisibilityChanged(bool visible)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(visible);
            BackButtonVisibilityChange?.Invoke(
                this,
                new OnBackButtonVisibilityChangeEventArgs(visible));
        }

        private void OnActionBarVisibilityChanged(bool visible)
        {
            var actionBar = FindViewById(_actionBar);
            if (actionBar == null) return;
            actionBar.Visibility = visible ?
                ViewStates.Visible :
                ViewStates.Gone;
        }

        public void PushFragment(FragmentBase fragment)
        {
            NavigateTo(fragment, false, true);
        }

        public void PopFragment()
        {
            BeginTransition();
            ActionBarVisible = true;
            SupportFragmentManager.PopBackStack();
        }

        public void PushFullScreenFragment(FragmentBase fragment)
        {
            NavigateTo(fragment, true, true);
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
                if (ActionBarVisible == value) return;
                _actionBarVisible = value;
                OnActionBarVisibilityChanged(value);
            }
        }

        public event EventHandler<OnActivityCreateEventArgs> Create;
        public event EventHandler<OnSaveStateEventArgs> SaveState;
        public event EventHandler<OnBackButtonVisibilityChangeEventArgs> BackButtonVisibilityChange;

        protected List<Fragment> Fragments { get; }

        private readonly int _layout;
        private readonly int _actionBar;
        private readonly int _container;

        private bool _backButtonVisible,
            _actionBarVisible = true;
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