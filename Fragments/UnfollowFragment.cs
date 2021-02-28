using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;
using Madamin.Unfollow.ViewHolders;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment : Fragment, IUnfollowerItemClickListener, ActionMode.ICallback, IRetryHandler
    {
        public const string AccountIndexBundleKey = "account_index";
        public const string WhiteListFileNameFormat = "{0}-unfollowers-whitelist.bin";

        private Account _account;
        private int _accountPosition;

        private RecyclerView _recyclerView;

        private ActionMode _actionMode;

        private UnfollowerAdapter _adapter;

        private TaskAwaiter _taskAwaiter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            return inflater.Inflate(Resource.Layout.fragment_recyclerview, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                ((IFragmentContainer)Activity).PopFragment();
                return;
            }

            _taskAwaiter = new TaskAwaiter((IFragmentContainer)Activity);
            _taskAwaiter.TaskDone += TaskAwaiter_TaskDone;

            _accountPosition = Arguments.GetInt(AccountIndexBundleKey, -1);
            if (_accountPosition < 0)
                throw new ArgumentException();
            _account = ((IInstagramAccounts)Activity).GetAccount(_accountPosition);

            ((IActionBarContainer)Activity).SetTitle(_account.Data.User.Fullname);

            ((IEmptyView)Activity).SetEmptyText(Resource.String.msg_no_unfollower);
            ((IEmptyView)Activity).SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragment_recyclerview_view);

            _adapter = new UnfollowerAdapter(_account, this);
            _recyclerView.SetAdapter(_adapter);

            var whitelistFileName = GetWhitelistFileName();
            var dataContainer = (IDataStorage)Activity;
            if (dataContainer.DataExists(whitelistFileName))
            {
                var whiteList = (List<User>)dataContainer.LoadData(whitelistFileName);
                _adapter.Whitelist.AddRange(whiteList);
            }

            _adapter.Refresh();

            if (_adapter.ItemCount <= 0)
            {
                ((IFragmentContainer)Activity).ShowEmptyView();
            }
        }

        private void TaskAwaiter_TaskDone(object sender, EventArgs e)
        {
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();

            if (_adapter.ItemCount <= 0)
            {
                ((IFragmentContainer)Activity).ShowEmptyView();
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.appbar_menu_unfollow, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_refresh:
                    _taskAwaiter.AwaitTask(_account.RefreshAsync());
                    return true;

                case Resource.Id.appbar_unfollow_item_unfollowall:
                    _taskAwaiter.AwaitTask(BatchUnfollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()));
                    return true;

                case Resource.Id.appbar_unfollow_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
                    _adapter.Refresh();
                    _adapter.NotifyDataSetChanged();
                    if (_adapter.ItemCount > 0)
                    {
                        ((IFragmentContainer)Activity).ShowContentView();
                    }
                    return true;

                case Resource.Id.appbar_unfollow_item_logout:
                    var awaiter = new TaskAwaiter((IFragmentContainer)Activity);
                    awaiter.TaskDone += (source, args) =>
                    {
                        ((IFragmentContainer)Activity).PopFragment();
                    };
                    awaiter.AwaitTask(((IInstagramAccounts)Activity).LogoutAsync(_accountPosition));
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(
                Resource.Menu.appbar_menu_unfollow_contextual, menu);
            return true;
        }

        void ActionMode.ICallback.OnDestroyActionMode(ActionMode mode)
        {
            _adapter.DeselectAll();
            mode.Dispose();
            _actionMode = null;
        }

        bool ActionMode.ICallback.OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_select_all:
                    _adapter.SelectAll();
                    _actionMode.Subtitle = GetString(Resource.String.title_selected, _adapter.SelectedItems.Count);
                    return true;

                case Resource.Id.appbar_unfollow_item_unfollow:
                    _taskAwaiter.AwaitTask(BatchUnfollowAsync(_adapter.GetSelected()));
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_unfollow_item_block:
                    _taskAwaiter.AwaitTask(BatchBlockAsync(_adapter.GetSelected()));
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_unfollow_item_whitelist:
                    _adapter.Whitelist.AddRange(_adapter.GetSelected());
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
                    _adapter.Refresh();
                    _adapter.NotifyDataSetChanged();
                    if (_adapter.ItemCount <= 0)
                    {
                        ((IFragmentContainer)Activity).ShowEmptyView();
                    }
                    mode.Finish();
                    return true;

                default:
                    return false;
            }
        }

        bool IUnfollowerItemClickListener.OnItemClick(int position)
        {
            if (_actionMode == null) return false;
            SelectOrDeselectItem(position);
            return true;
        }

        void IUnfollowerItemClickListener.OnItemLongClick(int position)
        {
            SelectOrDeselectItem(position);
        }

        void IUnfollowerItemClickListener.OnItemOpen(int position)
        {
            var user = _adapter.GetItem(position);
            ((IUrlHandler)Activity).LaunchInstagram(user.Username);
        }

        void IUnfollowerItemClickListener.OnItemSelect(int position)
        {
            SelectOrDeselectItem(position);
        }

        void IUnfollowerItemClickListener.OnItemUnfollow(int position)
        {
            _taskAwaiter.AwaitTask(_account.UnfollowAsync(_adapter.GetItem(position)));
        }

        void IUnfollowerItemClickListener.OnItemAddToWhitelist(int position)
        {
            _adapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();
            if (_adapter.ItemCount <= 0)
            {
                ((IFragmentContainer)Activity).ShowEmptyView();
            }
        }

        void IUnfollowerItemClickListener.OnItemBlock(int position)
        {
            _taskAwaiter.AwaitTask(_account.BlockAsync(_adapter.GetItem(position)));
        }

        private void SelectOrDeselectItem(int position)
        {
            _adapter.SelectOrDeselectItem(position);

            if (_adapter.SelectedItems.Count <= 0)
            {
                _actionMode?.Finish();
                return;
            }

            if (_actionMode == null)
            {
                _actionMode = ((AppCompatActivity)Activity).StartSupportActionMode(this);
                _actionMode.Title = _account.Data.User.Fullname;
            }

            _actionMode.Subtitle = GetString(Resource.String.title_selected, _adapter.SelectedItems.Count);
        }

        private async Task BatchUnfollowAsync(IReadOnlyList<User> users)
        {
            for (var i = 0; i < users.Count; i++)
            {
                UpdateProgress(i,
                    users.Count,
                    Resource.String.title_batch_unfollow);
                await _account.UnfollowAsync(users[i]);
            }

            ((ILoadingView)Activity).ResetLoadingText();
        }

        private async Task BatchBlockAsync(IReadOnlyList<User> users)
        {
            for (var i = 0; i < users.Count; i++)
            {
                UpdateProgress(i,
                    users.Count,
                    Resource.String.title_batch_block);
                await _account.BlockAsync(users[i]);
            }

            ((ILoadingView)Activity).ResetLoadingText();
        }

        private void UpdateProgress(int i, int total, int textFormatResource)
        {
            ((ILoadingView)Activity).SetLoadingText(GetString(textFormatResource, i, total));
        }

        private string GetWhitelistFileName()
        {
            return string.Format(WhiteListFileNameFormat, _account.Data.User.Id);
        }

        void IRetryHandler.OnClick()
        {
            _taskAwaiter.Retry();
        }
    }
}