using System;

using Android.Content;
using Android.OS;
using Android.Views;

using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Fragments
{
    internal interface IFragmentHost
    {
        string ActionBarTitle { get; set; }

        void NavigateTo(Fragment fragment, bool addToBackStack);

        void PushFragment(FragmentBase fragment);
        void PopFragment();

        void PushFullScreenFragment(FragmentBase fragment);
    }

    public abstract class FragmentBase : Fragment
    {

        protected FragmentBase(int layout)
        {
            _layout = layout;
        }

        protected FragmentBase(int layout, int menu) : this(layout)
        {
            HasOptionsMenu = true;
            _menu = menu;
        }

        protected string Title { get; set; }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _host = (IFragmentHost) context;

            Title = GetString(Resource.String.app_name);
        }

        public override View OnCreateView(
            LayoutInflater inflater,
            ViewGroup container, 
            Bundle savedInstanceState)
        {
            var view = inflater.Inflate(_layout, container, false);

            Create?.Invoke(this, new OnFragmentCreateEventArgs(savedInstanceState, view));

            _host.ActionBarTitle = Title;

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

        public void Refresh()
        {
            _host.NavigateTo(this, false);
        }

        protected void PushFragment(FragmentBase fragment)
        {
            _host.PushFragment(fragment);
        }

        protected void PopFragment()
        {
            _host.PopFragment();
        }

        public event EventHandler<OnFragmentCreateEventArgs> Create;
        public event EventHandler<OnMenuItemSelectedEventArgs> MenuItemSelected;

        private IFragmentHost _host;

        private readonly int _layout;
        private readonly int _menu;
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
        public OnMenuItemSelectedEventArgs(int item_id)
        {
            ItemId = item_id;
            Finished = true;
        }

        public int ItemId { get; }
        public bool Finished { get; set; }
    }
}