using System;
using System.Linq;

using Android.Content;
using Android.Widget;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;

namespace Madamin.Unfollow.Fragments
{
    public class UnfollowFragment : FragmentBase
    {
        public UnfollowFragment(Account account) : base(Resource.Layout.fragment_recycler)
        {
            _account = account;
            Create += UnfollowFragment_Create;
        }

        private void UnfollowFragment_Create(object sender, OnCreateEventArgs e)
        {
            try
            {
                Title = _account.Data.User.Fullname;

                _adapter = new UnfollowerAdapter(_account.Data.Unfollowers.ToList());
                _adapter.ItemClick += Adapter_OnItemClick;
                _adapter.ItemUnfollowClick += Adapter_OnItemUnfollowClick;

                _recycler = e.View.FindViewById<RecyclerView>(Resource.Id.fragment_recycler_view);
                _recycler.SetAdapter(_adapter);
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
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) =>
                        {
                            Activity.Finish();
                        })
                        .Show();
            }
        }

        private void Adapter_OnItemClick(object sender, UnfollowClickEventArgs e)
        {
            var intent = Intent.ParseUri("https://instagram.com/_u/" + e.User.Username, IntentUriType.None);
            intent.SetPackage("com.instagram.android");
            try
            {
                Activity.StartActivity(intent);
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
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) =>
                        {
                            Activity.Finish();
                        })
                        .Show();
            }
        }

        private async void Adapter_OnItemUnfollowClick(object sender, UnfollowClickEventArgs e)
        {
            //_btn_unfollow.Enabled = false;
            try
            {
                await _account.UnfollowAsync(e.User);
                _adapter.Remove(e.Position);
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
