using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Fragments
{
    public abstract class FragmentBase : Fragment
    {
        private readonly int _layout;
        private readonly int _menu;

        private IFragmentHost _host;

        protected FragmentBase(int layout)
        {
            _layout = layout;
        }

        protected FragmentBase(int layout, int menu) : this(layout)
        {
            HasOptionsMenu = true;
            _menu = menu;
        }

        protected string Title
        {
            get => _host.ActionBarTitle;
            set => _host.ActionBarTitle = value;
        }

        protected bool ActionBarVisible
        {
            get => _host.ActionBarVisible;
            set => _host.ActionBarVisible = value;
        }

        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container,
            Bundle savedInstanceState)
        {
            _host = (IFragmentHost) Context;
            var view = inflater.Inflate(_layout, container, false);
            Create?.Invoke(this, new OnFragmentCreateEventArgs(savedInstanceState, view));
            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            if (HasOptionsMenu)
                inflater.Inflate(_menu, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (!HasOptionsMenu)
                return base.OnOptionsItemSelected(item);

            var e = new OnMenuItemSelectedEventArgs(item.ItemId);
            MenuItemSelected?.Invoke(this, e);

            return e.Finished || base.OnOptionsItemSelected(item);
        }

        protected void PushFragment(Fragment fragment)
        {
            _host.PushFragment(fragment);
        }

        protected void PopFragment()
        {
            _host.PopFragment();
        }

        protected void NavigateTo(Fragment fragment, bool addToBackStack)
        {
            _host.NavigateTo(fragment, addToBackStack);
        }

        public event EventHandler<OnFragmentCreateEventArgs> Create;
        public event EventHandler<OnMenuItemSelectedEventArgs> MenuItemSelected;
    }

    public class OnFragmentCreateEventArgs : EventArgs
    {
        public OnFragmentCreateEventArgs(Bundle savedInstanceState, View view)
        {
            SavedInstanceState = savedInstanceState;
            View = view;
        }

        public Bundle SavedInstanceState { get; }
        public View View { get; }
    }

    public class OnMenuItemSelectedEventArgs : EventArgs
    {
        public OnMenuItemSelectedEventArgs(int itemId)
        {
            ItemId = itemId;
            Finished = true;
        }

        public int ItemId { get; }
        public bool Finished { get; set; }
    }
}