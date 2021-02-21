using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.App;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;
using Madamin.Unfollow.ViewHolders;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment :
        RecyclerViewFragmentBase,
        IUnfollowerItemClickListener,
        ActionMode.ICallback
    {
        private Account _account;
        private int _accountPosition;

        private ActionMode _actionMode;

        private UnfollowerAdapter _adapter;

        public UnfollowFragment() :
            base(Resource.Menu.appbar_menu_unfollow)
        {
            Create += UnfollowFragment_Create;
            MenuItemSelected += UnfollowFragment_MenuItemSelected;
            RetryClick += UnfollowFragment_RetryClick;
        }

        private UnfollowerAdapter UnfollowerAdapter
        {
            get => _adapter;
            set
            {
                _adapter = value;
                SetAdapter(_adapter);
            }
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater.Inflate(
                Resource.Menu.appbar_menu_unfollow_contextual, menu);
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            _adapter.DeselectAll();
            mode.Dispose();
            _actionMode = null;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_select_all:
                    UnfollowerAdapter.SelectAll();
                    _actionMode.Subtitle = GetString(Resource.String.title_selected,
                        UnfollowerAdapter.SelectedItems.Count);
                    return true;

                case Resource.Id.appbar_unfollow_item_unfollow:
                    DoTask(
                        BatchUnfollowAsync(UnfollowerAdapter.GetSelected()),
                        RefreshAdapterData);
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_unfollow_item_block:
                    DoTask(
                        BatchBlockAsync(UnfollowerAdapter.GetSelected()),
                        RefreshAdapterData);
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_unfollow_item_whitelist:
                    UnfollowerAdapter.Whitelist.AddRange(
                        UnfollowerAdapter.GetSelected());
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(),
                        UnfollowerAdapter.Whitelist);
                    UnfollowerAdapter.Refresh();
                    UnfollowerAdapter.NotifyDataSetChanged();
                    mode.Finish();
                    return true;

                default:
                    return false;
            }
        }

        public bool OnItemClick(int position)
        {
            if (_actionMode == null) return false;
            SelectOrDeselectItem(position);
            return true;
        }

        public void OnItemLongClick(int position)
        {
            SelectOrDeselectItem(position);
        }

        public void OnItemOpen(int position)
        {
            var user = _adapter.GetItem(position);
            ((IUrlHandler)Activity).LaunchInstagram(user.Username);
        }

        public void OnItemSelect(int position)
        {
            SelectOrDeselectItem(position);
        }

        public void OnItemUnfollow(int position)
        {
            DoTask(
                _account.UnfollowAsync(_adapter.GetItem(position)),
                RefreshAdapterData);
        }

        public void OnItemAddToWhitelist(int position)
        {
            UnfollowerAdapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), UnfollowerAdapter.Whitelist);
            UnfollowerAdapter.Refresh();
            UnfollowerAdapter.NotifyDataSetChanged();
        }

        public void OnItemBlock(int position)
        {
            DoTask(
                _account.BlockAsync(_adapter.GetItem(position)),
                RefreshAdapterData);
        }

        private void UnfollowFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            // Right now, reloading from SavedInstanceState is not supported
            if (e.SavedInstanceState != null)
            {
                PopFragment();
                return;
            }

            _accountPosition = Arguments.GetInt(BundleKeyAccountIndex, -1);
            if (_accountPosition < 0)
                throw new ArgumentException();
            _account = ((IInstagramAccounts)Activity).GetAccount(_accountPosition);

            ((IActionBarContainer)Activity).SetTitle(_account.Data.User.Fullname);

            EmptyText = GetString(Resource.String.msg_no_unfollower);
            SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            UnfollowerAdapter = new UnfollowerAdapter(_account, this);

            var whitelistFileName = GetWhitelistFileName();
            var dataContainer = (IDataStorage)Activity;
            if (dataContainer.DataExists(whitelistFileName))
            {
                var wl = (List<User>)dataContainer.LoadData(whitelistFileName);
                _adapter.Whitelist.AddRange(wl);
            }

            UnfollowerAdapter.Refresh();

            ViewMode = RecyclerViewMode.Data;
        }

        private void UnfollowFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_refresh:
                    DoTask(_account.RefreshAsync(), RefreshAdapterData);
                    break;

                case Resource.Id.appbar_unfollow_item_unfollowall:
                    DoTask(
                        BatchUnfollowAsync(
                            _account.Data.Unfollowers.Except(UnfollowerAdapter.Whitelist).ToArray()),
                        RefreshAdapterData);
                    break;

                case Resource.Id.appbar_unfollow_item_clear_whitelist:
                    UnfollowerAdapter.Whitelist.Clear();
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
                    RefreshAdapterData();
                    break;

                case Resource.Id.appbar_unfollow_item_logout:
                    DoTask(
                        ((IInstagramAccounts)Activity).LogoutAsync(_accountPosition),
                        ((IFragmentContainer)Activity).PopFragment);
                    break;

                default:
                    e.Finished = false;
                    break;
            }
        }

        private void UnfollowFragment_RetryClick(object sender, EventArgs e)
        {
            DoTask(_account.RefreshAsync(), RefreshAdapterData);
        }

        private void SelectOrDeselectItem(int pos)
        {
            UnfollowerAdapter.SelectOrDeselectItem(pos);

            if (UnfollowerAdapter.SelectedItems.Count <= 0)
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

            ProgressText = GetString(Resource.String.title_loading);
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

            ProgressText = GetString(Resource.String.title_loading);
        }

        private void UpdateProgress(int i, int total, int textFormatResource)
        {
            ProgressText = GetString(textFormatResource, i, total);
        }

        private void RefreshAdapterData()
        {
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();
        }

        private string GetWhitelistFileName()
        {
            return GetString(
                Resource.String.filename_unfollowers_whitelist,
                _account.Data.User.Id);
        }
    }
}