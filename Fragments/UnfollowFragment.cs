using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using AndroidX.AppCompat.App;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment :
        RecyclerViewFragmentBase,
        IUnfollowerItemClickListener,
        ActionMode.ICallback
    {
        public UnfollowFragment() :
            base(Resource.Menu.appbar_menu_unfollow)
        {
            Create += UnfollowFragment_Create;
            MenuItemSelected += UnfollowFragment_MenuItemSelected;
            RetryClick += UnfollowFragment_RetryClick;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_select_all:
                    _adapter.SelectAll();
                    _actionMode.Subtitle = string.Format(
                        GetString(Resource.String.title_selected),
                        _adapter.SelectedItems.Count);
                    return true;
                case Resource.Id.appbar_unfollow_item_unfollow:
                    DoTask(
                        BatchUnfollowAsync(_adapter.GetSelected()),
                        RefreshAdapterData);
                    mode.Finish();
                    return true;
                case Resource.Id.appbar_unfollow_item_block:
                    DoTask(
                        BatchBlockAsync(_adapter.GetSelected()),
                        RefreshAdapterData);
                    mode.Finish();
                    return true;
                default:
                    return false;
            }
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

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
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
            ((IInstagramHost) Activity).OpenInInstagram(user.Username);
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
            _adapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataContainer) Activity).SaveData(_account.Data.User.Id + ".whitelist", _adapter.Whitelist);
            RefreshAdapterData();
        }

        public void OnItemBlock(int position)
        {
            DoTask(
                _account.BlockAsync(_adapter.GetItem(position)),
                RefreshAdapterData);
        }

        private void UnfollowFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            if (e.SavedInstanceState != null)
            {
                PopFragment();
                return;
            }

            _accountPosition = Arguments.GetInt(BundleKeyAccountIndex, -1);
            if (_accountPosition < 0)
                throw new ArgumentException(); // TODO
            _account = ((IInstagramHost) Activity).Accounts[_accountPosition];

            Title = _account.Data.User.Fullname;
            // TODO: set ErrorText
            EmptyText = GetString(Resource.String.msg_no_unfollower);
            SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            _adapter = new UnfollowerAdapter(_account, this);

            var wlFileName = _account.Data.User.Id + ".whitelist";
            var dataContainer = (IDataContainer) Activity;

            if (dataContainer.DataExists(wlFileName))
            {
                var wl = (List<User>) dataContainer.LoadData(wlFileName);
                _adapter.Whitelist.AddRange(wl);
            }

            _adapter.Refresh();
            SetAdapter(_adapter);

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
                        BatchUnfollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()),
                        RefreshAdapterData);
                    break;
                case Resource.Id.appbar_unfollow_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataContainer) Activity).SaveData(
                        _account.Data.User.Id + ".whitelist",
                        _adapter.Whitelist);
                    RefreshAdapterData();
                    break;
                case Resource.Id.appbar_unfollow_item_logout:
                    var ig = ((IInstagramHost) Activity).Accounts;
                    DoTask(
                        ig.LogoutAccountAsync(_account),
                        () => { ((IFragmentHost) Activity).PopFragment(); });
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
            _adapter.SelectOrDeselectItem(pos);

            if (_adapter.SelectedItems.Count <= 0)
            {
                _actionMode?.Finish();
                return;
            }

            if (_actionMode == null)
            {
                _actionMode = ((AppCompatActivity) Activity).StartSupportActionMode(this);
                _actionMode.Title = _account.Data.User.Fullname;
            }

            _actionMode.Subtitle = string.Format(
                GetString(Resource.String.title_selected),
                _adapter.SelectedItems.Count);
        }

        private async Task BatchUnfollowAsync(User[] users)
        {
            for (var i = 0; i < users.Length; i++)
            {
                UpdateProgress(i, 
                    users.Length,
                    Resource.String.title_batch_unfollow);
                await _account.UnfollowAsync(users[i]);
            }

            ProgressText = GetString(Resource.String.title_loading);
        }

        private async Task BatchBlockAsync(User[] users)
        {
            for (var i = 0; i < users.Length; i++)
            {
                UpdateProgress(i, 
                    users.Length,
                    Resource.String.title_batch_block);
                await _account.BlockAsync(users[i]);
            }

            ProgressText = GetString(Resource.String.title_loading);
        }

        private void UpdateProgress(int i, int total, int textFormatResource)
        {
            ProgressText = string.Format(
                GetString(textFormatResource), i, total);
        }

        private void RefreshAdapterData()
        {
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();
            ((IInstagramHost) Activity).Accounts.SaveAccountCache(_account);
        }

        private Account _account;

        private int _accountPosition;
        private ActionMode _actionMode;
        private UnfollowerAdapter _adapter;
    }
}