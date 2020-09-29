using System;

using Android.OS;
using Android.Views;
using Xamarin.Essentials;

using AndroidX.Fragment.App;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;

using Madamin.Unfollow.Fragments;
using System.Collections.Generic;

namespace Madamin.Unfollow
{
    public class FragmentHostBase :
        AppCompatActivity,
        IFragmentHost,
        FragmentManager.IOnBackStackChangedListener
    {
        protected FragmentHostBase(
            int layout,
            int menu,
            int actionbar,
            int container)
        {
            _layout = layout;
            _menu = menu;
            _actionbar = actionbar;
            _container = container;

            Fragments = new List<Fragment>();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            SupportFragmentManager.AddOnBackStackChangedListener(this);

            BeforeSetContentView?.Invoke(this, new EventArgs());

            SetContentView(_layout);
            SetSupportActionBar(FindViewById<Toolbar>(_actionbar));

            Create?.Invoke(this, new EventArgs());

            if (savedInstanceState != null) return;

            SupportFragmentManager
                .BeginTransaction()
                .Add(_container, Fragments[0])
                .Commit();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var args = new OnMenuItemSelectedEventArgs(item.ItemId);
            MenuItemSelected?.Invoke(this, args);
            if (args.Finished)
                return true;
            return base.OnOptionsItemSelected(item);
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

        public string ActionbarTitle
        {
            get => SupportActionBar.Title;
            set => SupportActionBar.Title = value;
        }

        public event EventHandler Create;
        public event EventHandler BeforeSetContentView;
        public event EventHandler<OnMenuItemSelectedEventArgs> MenuItemSelected;
        public event EventHandler<OnBackButtonVisibilityChange> BackButtonVisibilityChange;

        public List<Fragment> Fragments { get; }

        private int _layout, _menu, _actionbar, _container;

        private bool _back_button_visible = false;
    }

    public class OnMenuItemSelectedEventArgs : EventArgs
    {
        public OnMenuItemSelectedEventArgs(int item_id)
        {
            ItemId = item_id;
            Finished = true;
        }

        public int ItemId { get; }
        public bool Finished { get; set; }
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