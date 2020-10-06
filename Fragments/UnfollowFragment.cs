using System;

using Android.Content;
using Android.Widget;

using Google.Android.Material.Dialog;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.ViewHolders;
using AndroidX.AppCompat.View;

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
            ViewMode = RecyclerViewMode.Data;
        }

        public void OnItemClick(int position)
        {
            if (_adapter.SelectedItems.Count > 0)
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
                DoTask(_account.UnfollowAsync(_adapter.GetItem(position)));
                _adapter.Refresh();
                _adapter.NotifyDataSetChanged();
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

            if (_adapter.SelectedItems.Count > 0 &&
                _action_mode == null)
            {
                var parent = (FragmentHostBase)Activity;
                _action_mode = parent.StartSupportActionMode(this);
            }
            else if (_adapter.SelectedItems.Count == 0)
            {
                // _action_mode should not be null here, it's a bug
                _action_mode.Finish();
            }
        }

        public bool OnActionItemClicked(ActionMode mode, Android.Views.IMenuItem item)
        {
            // TODO
            return false;
        }

        public bool OnCreateActionMode(ActionMode mode, Android.Views.IMenu menu)
        {
            // TODO
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            _action_mode = null;
        }

        public bool OnPrepareActionMode(ActionMode mode, Android.Views.IMenu menu)
        {
            return false;
        }

        private Account _account;
        private UnfollowerAdapter _adapter;
        private ActionMode _action_mode = null;
    }
}
