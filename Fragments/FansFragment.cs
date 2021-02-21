using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;
using Madamin.Unfollow.ViewHolders;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

namespace Madamin.Unfollow.Fragments
{
    public class FansFragment :
        RecyclerViewFragmentBase,
        IFanItemClickListener,
        ActionMode.ICallback
    {
        private Account _account;
        private int _accountPosition;

        private ActionMode _actionMode;

        private FansAdapter _adapter;

        public FansFragment() :
            base(Resource.Menu.appbar_menu_fans)
        {
            Create += FansFragment_Create;
            MenuItemSelected += FansFragment_MenuItemSelected;
            RetryClick += FansFragment_RetryClick;
        }

        private FansAdapter FansAdapter
        {
            get => _adapter;
            set
            {
                _adapter = value;
                SetAdapter(value);
            }
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
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
            FansAdapter.DeselectAll();
            mode.Dispose();
            _actionMode = null;
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_fans_item_select_all:
                    FansAdapter.SelectAll();
                    _actionMode.Subtitle = GetString(Resource.String.title_selected, FansAdapter.SelectedItems.Count);
                    return true;

                case Resource.Id.appbar_fans_item_follow:
                    DoTask(
                        BatchFollowAsync(FansAdapter.GetSelected()),
                        RefreshAdapterData);
                    mode.Finish();
                    return true;

                case Resource.Id.appbar_fans_item_whitelist:
                    FansAdapter.Whitelist.AddRange(FansAdapter.GetSelected());
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(),
                        FansAdapter.Whitelist);
                    FansAdapter.Refresh();
                    FansAdapter.NotifyDataSetChanged();
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

        public void OnItemFollow(int position)
        {
            try
            {
                DoTask(
                    _account.FollowAsync(_adapter.GetItem(position)),
                    RefreshAdapterData);
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }
        }

        public void OnItemAddToWhitelist(int position)
        {
            FansAdapter.Whitelist.Add(_adapter.GetItem(position));
            ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
            FansAdapter.Refresh();
            FansAdapter.NotifyDataSetChanged();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(BundleKeyAccountIndex, _accountPosition);
            base.OnSaveInstanceState(outState);
        }

        private void FansFragment_Create(object sender, OnFragmentCreateEventArgs e)
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
            EmptyText = GetString(Resource.String.msg_no_fan);
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            FansAdapter = new FansAdapter(_account, this);

            var whitelistFileName = GetWhitelistFileName();
            var dataContainer = (IDataStorage)Activity;
            if (dataContainer.DataExists(whitelistFileName))
            {
                var whitelist = (List<User>)dataContainer.LoadData(whitelistFileName);
                FansAdapter.Whitelist.AddRange(whitelist);
            }

            FansAdapter.Refresh();

            ViewMode = RecyclerViewMode.Data;
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
                        BatchFollowAsync(_account.Data.Unfollowers.Except(FansAdapter.Whitelist).ToArray()),
                        RefreshAdapterData);
                    break;

                case Resource.Id.appbar_fans_item_clear_whitelist:
                    FansAdapter.Whitelist.Clear();
                    ((IDataStorage)Activity).SaveData(GetWhitelistFileName(), _adapter.Whitelist);
                    FansAdapter.Refresh();
                    FansAdapter.NotifyDataSetChanged();
                    break;

                case Resource.Id.appbar_fans_item_logout:
                    DoTask(
                        ((IInstagramAccounts)Activity).LogoutAsync(_accountPosition),
                        ((IFragmentContainer)Activity).PopFragment);
                    break;

                default:
                    e.Finished = false;
                    break;
            }
        }

        private void FansFragment_RetryClick(object sender, EventArgs e)
        {
            DoTask(_account.RefreshAsync(), RefreshAdapterData);
        }

        private void SelectOrDeselectItem(int pos)
        {
            FansAdapter.SelectOrDeselectItem(pos);

            if (FansAdapter.SelectedItems.Count <= 0)
            {
                _actionMode?.Finish();
                return;
            }

            if (_actionMode == null)
            {
                _actionMode = ((AppCompatActivity)Activity).StartSupportActionMode(this);
                _actionMode.Title = _account.Data.User.Fullname;
            }

            _actionMode.Subtitle = GetString(Resource.String.title_selected, FansAdapter.SelectedItems.Count);
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
            ProgressText = GetString(Resource.String.title_batch_unfollow, i, total);
        }

        private void RefreshAdapterData()
        {
            FansAdapter.Refresh();
            FansAdapter.NotifyDataSetChanged();
        }

        private string GetWhitelistFileName()
        {
            return GetString(
                Resource.String.filename_fan_whitelist,
                _account.Data.User.Id);
        }
    }
}