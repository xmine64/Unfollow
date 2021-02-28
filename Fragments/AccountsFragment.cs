using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    public class AccountsFragment : Fragment, IAccountItemClickListener
    {
        private const string PreferenceKeyTipIsShown = "tip_is_shown";
        private const string PreferenceKeyAutoRefresh = "auto_refresh";

        private RecyclerView _recyclerView;

        private bool _hasPushedToLoginFragment;
        private bool _tipShown;
        private bool _didRefresh;

        private AccountAdapter _adapter;
        private TaskAwaiter _taskAwaiter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            return inflater.Inflate(Resource.Layout.fragment_recyclerview, container, false);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.appbar_menu_accounts, menu);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            ((IActionBarContainer)Activity).SetTitle(Resource.String.app_name);

            // TODO: EmptyText = GetString(Resource.String.msg_no_account);
            // TODO: SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragment_recyclerview_view);

            _adapter = ((IInstagramAccounts)Activity).CreateAccountAdapter(this);
            _recyclerView.SetAdapter(_adapter);

            _taskAwaiter = new TaskAwaiter((IFragmentContainer)Activity);
            _taskAwaiter.TaskDone += TaskAwaiter_TaskDone;

            _taskAwaiter.AwaitTask(((IInstagramAccounts)Activity).InitializeIfNeededAsync());

            if (!_didRefresh)
            {
                _didRefresh = true;
                if (((IPreferenceContainer)Activity).GetBoolean(PreferenceKeyAutoRefresh, true))
                {
                    _taskAwaiter.AwaitTask(((IInstagramAccounts)Activity).RefreshAsync());
                }
            }
            else if (_adapter.ItemCount <= 0)
            {
                // Push LoginFragment on first run
                if (!_hasPushedToLoginFragment)
                {
                    ((IFragmentContainer)Activity).PushFragment(new LoginFragment());
                    _hasPushedToLoginFragment = true;
                }
            }
            
            // Show tip on first run
            _tipShown = ((IPreferenceContainer)Activity).GetBoolean(PreferenceKeyTipIsShown, false);
            if (!_tipShown)
            {
                var dialog = new MaterialAlertDialogBuilder(Activity);
                dialog.SetTitle(Resource.String.title_tip);
                dialog.SetMessage(Resource.String.msg_tip);
                dialog.Show();

                ((IPreferenceContainer)Activity).SetBoolean(PreferenceKeyTipIsShown, true);

                _tipShown = true;
            }
        }

        private void TaskAwaiter_TaskDone(object sender, EventArgs e)
        {
            _adapter.NotifyDataSetChanged();

            if (_adapter.ItemCount <= 0)
            {
                ((IFragmentContainer)Activity).ShowEmptyView();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    ((IFragmentContainer)Activity).PushFragment(new LoginFragment());
                    return true;

                case Resource.Id.appbar_home_item_refresh:
                    _taskAwaiter.AwaitTask(((IInstagramAccounts)Activity).RefreshAsync());
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        //private void AccountsFragment_RetryClick(object sender, EventArgs e)
        //{
        //    var task = ((IInstagramAccounts)Activity).InitializeIfNeededAsync();
        //    if (task == null)
        //        task = ((IInstagramAccounts)Activity).RefreshAsync();
        //    _taskAwaiter.AwaitTask(task);
        //}

        void IAccountItemClickListener.OnItemOpenUnfollowers(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(UnfollowFragment.AccountIndexBundleKey, position);

            var fragment = new UnfollowFragment
            {
                Arguments = bundle
            };
            ((IFragmentContainer)Activity).PushFragment(fragment);
        }

        void IAccountItemClickListener.OnItemOpenFans(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(FansFragment.AccountIndexBundleKey, position);

            var fragment = new FansFragment
            {
                Arguments = bundle
            };
            ((IFragmentContainer)Activity).PushFragment(fragment);
        }

        void IAccountItemClickListener.OnItemOpenInstagram(int position)
        {
            var user = _adapter.GetItem(position).Data.User;
            ((IUrlHandler)Activity).LaunchInstagram(user.Username);
        }

        void IAccountItemClickListener.OnItemLogout(int position)
        {
            _taskAwaiter.AwaitTask(((IInstagramAccounts)Activity).LogoutAsync(position));
        }

        void IAccountItemClickListener.OnItemRefresh(int position)
        {
            _taskAwaiter.AwaitTask(_adapter.GetItem(position).RefreshAsync());
        }
    }
}
