using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.OS;
using Android.Views;

using AndroidX.AppCompat.App;

using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace Madamin.Unfollow.Fragments
{
    public class FansFragment :
        RecyclerViewFragmentBase,
        IFanItemClickListener,
        ActionMode.ICallback
    {

        public FansFragment() :
            base(Resource.Menu.appbar_menu_fans)
        {
            Create += FansFragment_Create;
            MenuItemSelected += FansFragment_MenuItemSelected;
            RetryClick += FansFragment_RetryClick;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_fans_item_selectall:
                    _adapter.SelectAll();
                    _actionMode.Subtitle = string.Format(
                        GetString(Resource.String.title_selected),
                        _adapter.SelectedItems.Count);
                    return true;
                case Resource.Id.appbar_fans_item_follow:
                    DoTask(
                        BatchFollowAsync(_adapter.GetSelected()),
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
                Resource.Menu.appbar_menu_fans_contextual,
                menu);
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

        public void OnItemFollow(int position)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                DoTask(
                    _account.FollowAsync(_adapter.GetItem(position)),
                    RefreshAdapterData);
            }
            catch (Exception ex)
            {
                //_btn_unfollow.Enabled = true;
                ((IErrorHost) Activity).ShowError(ex);
            }
        }

        public void OnItemAddToWhitelist(int position)
        {
            _adapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataContainer) Activity).SaveData(
                _account.Data.User.Id + ".whitelist_fans",
                _adapter.Whitelist);
            RefreshAdapterData();
        }

        private void FansFragment_Create(object sender, OnFragmentCreateEventArgs e)
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
            EmptyText = GetString(Resource.String.msg_no_fan);
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            _adapter = new FansAdapter(_account, this);

            var wlFileName = _account.Data.User.Id + ".whitelist_fans";
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

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(BundleKeyAccountIndex, _accountPosition);
            base.OnSaveInstanceState(outState);
        }

        private void FansFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_fans_item_refresh:
                    DoTask(_account.RefreshAsync(), RefreshAdapterData);
                    break;
                case Resource.Id.appbar_fans_item_followall:
                    DoTask(
                        BatchFollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()),
                        RefreshAdapterData);
                    break;
                case Resource.Id.appbar_fans_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataContainer) Activity).SaveData(
                        _account.Data.User.Id + ".whitelist", 
                        _adapter.Whitelist);
                    RefreshAdapterData();
                    break;
                case Resource.Id.appbar_fans_item_logout:
                    var ig = ((IInstagramHost) Activity).Accounts;
                    DoTask(
                        ig.LogoutAccountAsync(_account),
                        () =>
                        {
                            ((IFragmentHost) Activity).PopFragment();
                        });
                    break;
                default:
                    e.Finished = false;
                    break;
            }
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

        private async Task BatchFollowAsync(IReadOnlyList<User> users)
        {
            for (var i = 0; i < users.Count; i++)
            {
                UpdateProgress(i, users.Count);
                await _account.FollowAsync(users[i]);
            }

            ProgressText = GetString(Resource.String.title_loading);
        }

        private void UpdateProgress(int i, int total)
        {
            ProgressText = string.Format(
                GetString(Resource.String.title_batch_unfollow), i, total);
        }

        private void RefreshAdapterData()
        {
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();
            ((IInstagramHost) Activity).Accounts.SaveAccountCache(_account);
        }

        private void FansFragment_RetryClick(object sender, EventArgs e)
        {
            DoTask(_account.RefreshAsync(), RefreshAdapterData);
        }

        private Account _account;

        private int _accountPosition;
        private ActionMode _actionMode;
        private FansAdapter _adapter;
    }
}