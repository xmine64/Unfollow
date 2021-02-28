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
    public class FansFragment : Fragment, IFanItemClickListener, ActionMode.ICallback, IRetryHandler
    {
        public const string AccountIndexBundleKey = "account_index";
        private const string WhiteListFileNameFormat = "{0}-fans-whitelist.bin";

        private RecyclerView _recyclerView;

        private Account _account;
        private int _accountPosition;

        private ActionMode _actionMode;

        private FansAdapter _adapter;

        private TaskAwaiter _taskAwaiter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            return inflater.Inflate(Resource.Layout.fragment_recyclerview, container, false);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.appbar_menu_fans, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_fans_item_refresh:
                    _taskAwaiter.AwaitTask(_account.RefreshAsync());
                    return true;

                case Resource.Id.appbar_fans_item_followall:
                    _taskAwaiter.AwaitTask(BatchFollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()));
                    return true;

                case Resource.Id.appbar_fans_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
                    _adapter.Refresh();
                    _adapter.NotifyDataSetChanged();
                    if (_adapter.ItemCount > 0)
                    {
                        ((IFragmentContainer)Activity).ShowContentView();
                    }
                    return true;

                case Resource.Id.appbar_fans_item_logout:
                    // TODO: Really? Do this???
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            // I don't know how to save account data to bundle,
            // so I don't know how to restore them back
            // then reloading is not implemented here.
            if (savedInstanceState != null)
            {
                ((IFragmentContainer)Activity).PopFragment();
                return;
            }

            _taskAwaiter = new TaskAwaiter((IFragmentContainer)Activity);
            _taskAwaiter.TaskDone += TaskAwaiter_TaskDone;

            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragment_recyclerview_view);

            _accountPosition = Arguments.GetInt(AccountIndexBundleKey, -1);
            if (_accountPosition < 0)
                throw new ArgumentException();
            _account = ((IInstagramAccounts)Activity).GetAccount(_accountPosition);

            ((IActionBarContainer)Activity).SetTitle(_account.Data.User.Fullname);
            ((IEmptyView)Activity).SetEmptyText(Resource.String.msg_no_fan);
            ((IEmptyView)Activity).SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            _adapter = new FansAdapter(_account, this);
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

        bool ActionMode.ICallback.OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        bool ActionMode.ICallback.OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(Resource.Menu.appbar_menu_fans_contextual, menu);
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
                case Resource.Id.appbar_fans_item_select_all:
                    _adapter.SelectAll();
                    _actionMode.Subtitle = GetString(Resource.String.title_selected, _adapter.SelectedItems.Count);
                    return true;

                case Resource.Id.appbar_fans_item_follow:
                    _taskAwaiter.AwaitTask(BatchFollowAsync(_adapter.GetSelected()));
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_fans_item_whitelist:
                    _adapter.Whitelist.AddRange(_adapter.GetSelected());
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(),
                        _adapter.Whitelist);
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

        bool IFanItemClickListener.OnItemClick(int position)
        {
            if (_actionMode == null) return false;
            SelectOrDeselectItem(position);
            return true;
        }

        void IFanItemClickListener.OnItemLongClick(int position)
        {
            SelectOrDeselectItem(position);
        }

        void IFanItemClickListener.OnItemOpen(int position)
        {
            var user = _adapter.GetItem(position);
            ((IUrlHandler)Activity).LaunchInstagram(user.Username);
        }

        void IFanItemClickListener.OnItemSelect(int position)
        {
            SelectOrDeselectItem(position);
        }

        void IFanItemClickListener.OnItemFollow(int position)
        {
            _taskAwaiter.AwaitTask(_account.FollowAsync(_adapter.GetItem(position)));
        }

        void IFanItemClickListener.OnItemAddToWhitelist(int position)
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

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(AccountIndexBundleKey, _accountPosition);
            base.OnSaveInstanceState(outState);
        }

        // TODO
        //private void FansFragment_RetryClick(object sender, EventArgs e)
        //{
        //    _taskAwaiter.AwaitTask(_account.RefreshAsync());
        //}

        private void SelectOrDeselectItem(int pos)
        {
            _adapter.SelectOrDeselectItem(pos);

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

        private async Task BatchFollowAsync(IReadOnlyList<User> users)
        {
            for (var i = 0; i < users.Count; i++)
            {
                UpdateProgress(i, users.Count);
                await _account.FollowAsync(users[i]);
            }

            ((ILoadingView)Activity).ResetLoadingText();
        }

        private void UpdateProgress(int i, int total)
        {
            ((ILoadingView)Activity).SetLoadingText(GetString(Resource.String.title_batch_follow, i, total));
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
