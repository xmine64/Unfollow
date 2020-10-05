using System;

using Android.OS;
using Android.Views;
using Android.Content;

using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Fragments
{
    interface IFragmentHost
    {
        void NavigateTo(Fragment fragment, bool add_to_backstack);
        void PushFragment(FragmentBase fragment);
        void PopFragment();
        string ActionbarTitle { get; set; }
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

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _host = (IFragmentHost)context;

            Title = GetString(Resource.String.app_name);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(_layout, container, false);
            
            Create?.Invoke(this, new OnCreateEventArgs(view));

            _host.ActionbarTitle = Title;

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
            if (HasOptionsMenu)
            {
                var e = new OnMenuItemSelectedEventArgs(item.ItemId);
                MenuItemSelected?.Invoke(this, e);
                if (e.Finished)
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void Refresh()
        {
            _host.NavigateTo(this, false);
        }

        public void PushFragment(FragmentBase fragment)
        {
            _host.PushFragment(fragment);
        }

        public void PopFragment()
        {
            _host.PopFragment();
        }

        public string Title { get; set; }

        public event EventHandler<OnCreateEventArgs> Create;
        public event EventHandler<OnMenuItemSelectedEventArgs> MenuItemSelected;

        private int _layout, _menu;
        private IFragmentHost _host;
    }

    public class OnCreateEventArgs : EventArgs
    {
        public OnCreateEventArgs(View view)
        {
            View = view;
        }

        public View View { get; }
    }
}