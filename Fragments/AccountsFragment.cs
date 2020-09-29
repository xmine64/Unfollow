using System;
using System.Linq;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Adapters;

namespace Madamin.Unfollow.Fragments
{
    public class AccountsFragment : FragmentBase
    {
        public AccountsFragment() : base(
            Resource.Layout.fragment_recycler,
            Resource.Menu.appbar_menu_accounts)
        {
            Create += AccountsFragment_Create;
            MenuItemSelected += AccountsFragment_MenuItemSelected;
        }

        private void AccountsFragment_Create(object sender, OnCreateEventArgs e)
        {
            try
            {
                Title = GetString(Resource.String.app_name);

                _adapter = new AccountAdapter(((IInstagramHost)Activity)
                    .Accounts
                    .Select(a => a.Data)
                    .ToList());
                _adapter.ItemClick += Adapter_OnItemClick;
                _adapter.ItemLogoutClick += Adapter_OnItemLogoutClick;

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

        private void Adapter_OnItemClick(object sender, AccountClickEventArgs args)
        {
            try
            {
                var user = ((IInstagramHost)Activity).Accounts[args.Position];
                PushFragment(new UnfollowFragment(user));
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

        private async void Adapter_OnItemLogoutClick(object sender, AccountClickEventArgs args)
        {
            try
            {
                await ((IInstagramHost)Activity).Accounts.LogoutAccountAtAsync(args.Position);
                _adapter.Remove(args.Position);
                _adapter.NotifyDataSetChanged();
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

        private void AccountsFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    PushFragment(new LoginFragment());
                    break;

                case Resource.Id.appbar_home_item_refresh:
                    new Action(async () =>
                    {
                        try
                        {
                            await ((IInstagramHost)Activity).Accounts.RefreshAllAsync();
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
                    }).Invoke();
                    Refresh();
                    break;

                default:
                    e.Finished = false;
                    break;
            }
        }

        private RecyclerView _recycler;
        private AccountAdapter _adapter;
    }
}