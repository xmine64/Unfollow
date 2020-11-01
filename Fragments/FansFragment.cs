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
        }

        private void FansFragment_Create(object sender, OnFragmentCreateEventArgs e)
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
            EmptyText = GetString(Resource.String.msg_no_fan);
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            _adapter = new FansAdapter(_account, this);

            var wl_file_name = _account.Data.User.Id + ".whitelist_fans";
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

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(ACCOUNT_INDEX, _account_position);
            base.OnSaveInstanceState(outState);
        }

        private void FansFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_fans_item_refresh:
                    DoTask(_account.RefreshAsync(), _refresh_adapter_data);
                    break;
                case Resource.Id.appbar_fans_item_followall:
                    DoTask(
                        BatchFollowAsync(_account.Data.Unfollowers.Except(_adapter.Whitelist).ToArray()),
                        _refresh_adapter_data);
                    break;
                case Resource.Id.appbar_fans_item_clear_whitelist:
                    _adapter.Whitelist.Clear();
                    ((IDataContainer)Activity).SaveData(_account.Data.User.Id + ".whitelist", _adapter.Whitelist);
                    _refresh_adapter_data();
                    break;
                case Resource.Id.appbar_fans_item_logout:
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

        public void OnItemFollow(int position)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                DoTask(
                    _account.FollowAsync(_adapter.GetItem(position)),
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
            ((IDataContainer)Activity).SaveData(_account.Data.User.Id + ".whitelist_fans", _adapter.Whitelist);
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
                case Resource.Id.appbar_fans_item_selectall:
                    _adapter.SelectAll();
                    _action_mode.Subtitle = string.Format(
                        GetString(Resource.String.title_selected),
                        _adapter.SelectedItems.Count);
                    return true;
                case Resource.Id.appbar_fans_item_follow:
                    DoTask(BatchFollowAsync(
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
                .Inflate(Resource.Menu.appbar_menu_fans_contextual, menu);
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

        private async Task BatchFollowAsync(User[] users)
        {
            for (var i = 0; i < users.Length; i++)
            {
                _update_progress(i, users.Length);
                await _account.FollowAsync(users[i]);
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
        private FansAdapter _adapter;
        private ActionMode _action_mode;
    }
}
 