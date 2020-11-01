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

            Create?.Invoke(this, new OnActivityCreateEventArgs(savedInstanceState));

            if (savedInstanceState != null)
            {
                if (savedInstanceState.GetBoolean(BACKBUTTON_VISIBLITY))
                {
                    _back_button_visible = true;
                    OnBackButtonVisibilityChanged(true);
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
            outState.PutBoolean(BACKBUTTON_VISIBLITY, _back_button_visible);

            // TODO: save fragments state

            SaveState?.Invoke(this, new OnSaveStateEventArgs(outState));

            base.OnSaveInstanceState(outState);
        }

        private const string BACKBUTTON_VISIBLITY = "backbutton_visibility";

        public void NavigateTo(int index)
        {
            NavigateTo(Fragments[index], false);
        }

        private void BeginTransition()
        {
            var rootView = (ViewGroup)FindViewById(Resource.Id.root);
            TransitionManager.BeginDelayedTransition(rootView);
        }

        public void NavigateTo(Fragment fragment, bool add_to_back_stack)
        {
            BeginTransition();
            var tx = SupportFragmentManager.BeginTransaction().Replace(_container, fragment);
            if (add_to_back_stack)
            { 
                tx.AddToBackStack(null);
            }
            tx.Commit();
        }

        public override bool OnSupportNavigateUp()
        {
            PopFragment();
            return true;
        }

        public void OnBackStackChanged()
        {
            var back_button_visible = SupportFragmentManager.BackStackEntryCount > 0;
            if (back_button_visible != _back_button_visible)
            {
                _back_button_visible = back_button_visible;
                OnBackButtonVisibilityChanged(_back_button_visible);
            }
        }

        protected virtual void OnBackButtonVisibilityChanged(bool visibility)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(visibility);
            BackButtonVisibilityChange?.Invoke(
                this,
                new OnBackButtonVisibilityChangeEventArgs(visibility));
        }

        public void PushFragment(FragmentBase fragment)
        {
            NavigateTo(fragment, true);
        }

        public void PopFragment()
        {
            BeginTransition();
            SupportFragmentManager.PopBackStack();
        }

        public string ActionbarTitle
        {
            get => SupportActionBar.Title;
            set => SupportActionBar.Title = value;
        }

        public event EventHandler<OnActivityCreateEventArgs> Create;
        public event EventHandler<OnSaveStateEventArgs> SaveState;
        public event EventHandler<OnBackButtonVisibilityChangeEventArgs> BackButtonVisibilityChange;

        public List<Fragment> Fragments { get; }

        private int _layout, _actionbar, _container;

        private bool _back_button_visible = false;
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