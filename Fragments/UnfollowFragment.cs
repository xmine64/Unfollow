using System;

using Android.Content;
using Android.Widget;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment : RecyclerViewFragmentBase
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
            // set ErrorText
            // set EmptyText
            SetEmptyImage(Resource.Drawable.ic_person_remove_black_48dp);

            _adapter = new UnfollowerAdapter(_account);
            _adapter.ItemClick += Adapter_OnItemClick;
            _adapter.ItemUnfollowClick += Adapter_OnItemUnfollowClick;

            Adapter = _adapter;

            ViewMode = RecyclerViewMode.Data;
        }

        private void Adapter_OnItemClick(object sender, UnfollowClickEventArgs e)
        {
            var intent = Intent.ParseUri("https://instagram.com/_u/" + e.User.Username, IntentUriType.None);
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

        private void Adapter_OnItemUnfollowClick(object sender, UnfollowClickEventArgs e)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                DoTask(_account.UnfollowAsync(e.User));
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

        private Account _account;
        private RecyclerView _recycler;
        private UnfollowerAdapter _adapter;
    }
}
