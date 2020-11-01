using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.Views;

using AndroidX.AppCompat.App;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.ViewHolders;
using Android.OS;

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
        }

        private void UnfollowFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            if (e.SavedInstanceState != null)
            {
                PopFragment();
                return;
            }

            _account_position = Arguments.GetInt(ACCOUNT_INDEX, -1);
            if (_account_position < 0)
                throw new ArgumentException(); // TODO
            _account = ((IInstagramHost)Activity).Accounts[_account_position];

            Title = _account.Data.User.Fullname;
            // TODO: set ErrorText
            EmptyText = GetString(Resource.String.msg_no_unfollower);
            SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            _adapter = new UnfollowerAdapter(_account, this);

            var wl_file_name = _account.Data.User.Id + ".whitelist";
            var data_container = (IDataContainer)Activity;

            if (data_container.DataExists(wl_file_name))
            {
                var wl = (List<User>)data_container.LoadData(wl_file_name);
                _adapter.Whitelist.AddRange(wl);
            }

            Adapter = _adapter;
            _adapter.Refresh();
            ViewMode = RecyclerViewMode.Data;
        }

        private void UnfollowFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_refresh:
                    DoTask(_account.RefreshAsync(), _refresh_adapter_data);
                    break;
                case Resource.Id.appbar_unfollow_item_unfollowall:
                    DoTask(
                        BatchUnfollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()),
                        _refresh_adapter_data);
                    break;
                case Resource.Id.appbar_unfollow_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataContainer)Activity).SaveData(_account.Data.User.Id + ".whitelist", _adapter.Whitelist);
                    _refresh_adapter_data();
                    break;
                case Resource.Id.appbar_unfollow_item_logout:
                    var ig = ((IInstagramHost)Activity).Accounts;
                    DoTask(ig.LogoutAccountAsync(_account), () =>
                    {
                        ((IFragmentHost)Activity).PopFragment();
                    });
                    break;
                default:
                    e.Finished = false;
                    break;
            }
        }

        public bool OnItemClick(int position)
        {
            if (_action_mode != null)
            {
                _select_or_deselect_item(position);
                return true;
            }
            return false;
        }

        public void OnItemLongClick(int position)
        {
            _select_or_deselect_item(position);
        }

        public void OnItemOpen(int position)
        {
            var user = _adapter.GetItem(position);
            ((IInstagramHost)Activity).OpenInInstagram(user.Username);
        }

        public void OnItemSelect(int position)
        {
            _select_or_deselect_item(position);
        }

        public void OnItemUnfollow(int position)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                DoTask(
                    _account.UnfollowAsync(_adapter.GetItem(position)),
                    _refresh_adapter_data);
            }
            catch (Exception ex)
            {
                //_btn_unfollow.Enabled = true;
                ((IErrorHost)Activity).ShowError(ex);
            }
        }

        public void OnItemAddToWhitelist(int position)
        {
            _adapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataContainer)Activity).SaveData(_account.Data.User.Id + ".whitelist", _adapter.Whitelist);
            _refresh_adapter_data();
        }

        private void _select_or_deselect_item(int pos)
        {
            _adapter.SelectOrDeselectItem(pos);

            if (_adapter.SelectedItems.Count <= 0)
            {
                _action_mode?.Finish();
                return;
            }

            if (_action_mode == null)
            {
                _action_mode = ((AppCompatActivity)Activity).StartSupportActionMode(this);
                _action_mode.Title = _account.Data.User.Fullname;
            }

            _action_mode.Subtitle = string.Format(
                GetString(Resource.String.title_selected),
                _adapter.SelectedItems.Count);
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_unfollow_item_selectall:
                    _adapter.SelectAll();
                    _action_mode.Subtitle = string.Format(
                        GetString(Resource.String.title_selected),
                        _adapter.SelectedItems.Count);
                    return true;
                case Resource.Id.appbar_unfollow_item_unfollow:
                    DoTask(BatchUnfollowAsync(
                        _adapter.GetSelected()),
                        _refresh_adapter_data);
                    mode.Finish();
                    return true;
                default:
                    return false;
            }
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            mode.MenuInflater
                .Inflate(Resource.Menu.appbar_menu_unfollow_contextual, menu);
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            _adapter.DeselectAll();
            mode.Dispose();
            _action_mode = null;
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        private async Task BatchUnfollowAsync(User[] users)
        {
            for (var i = 0; i < users.Length; i++)
            {
                _update_progress(i, users.Length);
                await _account.UnfollowAsync(users[i]);
            }
            ProgressText = GetString(Resource.String.title_loading);
        }

        private void _update_progress(int i, int total)
        {
            ProgressText = string.Format(
                    GetString(Resource.String.title_batch_unfollow), i, total);
        }

        private void _refresh_adapter_data()
        {
            _adapter.Refresh();
            _adapter.NotifyDataSetChanged();
            ((IInstagramHost)Activity).Accounts.SaveAccountCache(_account);
        }

        private int _account_position;
        private Account _account;
        private UnfollowerAdapter _adapter;
        private ActionMode _action_mode;
    }
}
