using System;
using System.Threading.Tasks;

using Android.Views;
using Android.Content;
using Android.Widget;

using AndroidX.AppCompat.App;
using ActionMode = AndroidX.AppCompat.View.ActionMode;

using Google.Android.Material.Dialog;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment :
        RecyclerViewFragmentBase,
        IUnfollowerItemClickListener,
        ActionMode.ICallback
    {
        public UnfollowFragment(Account account) :
            base()
        {
            _account = account;
            Create += UnfollowFragment_Create;
        }

        private void UnfollowFragment_Create(object sender, OnCreateEventArgs e)
        {
            Title = _account.Data.User.Fullname;
            // TODO: set ErrorText
            // TODO: set EmptyText
            SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            _adapter = new UnfollowerAdapter(_account, this);
            Adapter = _adapter;
            _adapter.Refresh();
            ViewMode = RecyclerViewMode.Data;
        }

        public void OnItemClick(int position)
        {
            if (_action_mode != null)
            {
                _select_or_deselect_item(position);
                return;
            }

            var user = _adapter.GetItem(position);
            var intent = Intent.ParseUri("https://instagram.com/_u/" + user.Username, IntentUriType.None);
            intent.SetPackage("com.instagram.android");
            try
            {
                Activity.StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                Toast.MakeText(Activity, Resource.String.error_ig_not_installed, ToastLength.Long);
            }
            catch (Exception ex)
            {
                new MaterialAlertDialogBuilder(Activity)
                        .SetTitle(Resource.String.title_error)
#if DEBUG
                        .SetMessage(ex.ToString())
#else
                        .SetMessage(ex.Message)
#endif
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) => { })
                        .Show();
            }
        }

        public void OnItemLongClick(int position)
        {
            _select_or_deselect_item(position);
        }

        public void OnItemUnfollowClick(int position)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                DoTask(_account.UnfollowAsync(_adapter.GetItem(position)), () =>
                {
                    _adapter.Refresh();
                    _adapter.NotifyDataSetChanged();
                    ((IInstagramHost)Activity).Accounts.SaveAccountCache(_account);
                });
            }
            catch (Exception ex)
            {
                //_btn_unfollow.Enabled = true;
                new MaterialAlertDialogBuilder(Activity)
                        .SetTitle(Resource.String.title_error)
#if DEBUG
                        .SetMessage(ex.ToString())
#else
                        .SetMessage(ex.Message)
#endif
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) =>
                        {
                            Activity.Finish();
                        })
                        .Show();
            }
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
                    DoTask(BatchUnfollowAsync(_adapter.GetSelected()), () => 
                    {
                        _adapter.NotifyDataSetChanged();
                        ((IInstagramHost)Activity).Accounts.SaveAccountCache(_account);
                    });
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

        private Account _account;
        private UnfollowerAdapter _adapter;
        private ActionMode _action_mode;
    }
}
