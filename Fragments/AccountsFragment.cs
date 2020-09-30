using System;
using Google.Android.Material.Button;
using Google.Android.Material.Dialog;

using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Fragments
{
    public class AccountsFragment : RecyclerViewFragmentBase
    {
        public AccountsFragment() :
            base(Resource.Menu.appbar_menu_accounts)
        {
            Create += AccountsFragment_Create;
            MenuItemSelected += AccountsFragment_MenuItemSelected;
            RetryClick += AccountsFragment_RetryClick;
        }

        private void AccountsFragment_Create(object sender, OnCreateEventArgs e)
        {
            Title = GetString(Resource.String.app_name);
            // TODO: set EmptyText
            // TODO: set ErrorText
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            var accounts = ((IInstagramHost)Activity).Accounts;

            _adapter = new AccountAdapter(accounts);

            _adapter.ItemClick += Adapter_OnItemClick;
            _adapter.ItemLogoutClick += Adapter_OnItemLogoutClick;

            Adapter = _adapter;

            if (accounts.IsStateRestored)
            {
                ViewMode = RecyclerViewMode.Data;
            }
            else
            {
                DoTask(accounts.RestoreStateAsync());
                _adapter.NotifyDataSetChanged();
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
            var button = (MaterialButton)sender;
            button.Enabled = false;
            try
            {
                await ((IInstagramHost)Activity).Accounts.LogoutAccountAtAsync(args.Position);
                _adapter.NotifyDataSetChanged();
                ViewMode = RecyclerViewMode.Data;
            }
            catch (Exception ex)
            {
                button.Enabled = true;
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

        private void AccountsFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    PushFragment(new LoginFragment());
                    ViewMode = RecyclerViewMode.Data;
                    break;

                case Resource.Id.appbar_home_item_refresh:
                    DoTask(((IInstagramHost)Activity)
                        .Accounts
                        .RefreshAllAsync());
                    _adapter.NotifyDataSetChanged();
                    break;

                default:
                    e.Finished = false;
                    break;
            }
        }

        private void AccountsFragment_RetryClick(object sender, EventArgs e)
        {
            var accounts = ((IInstagramHost)Activity).Accounts;
            if (accounts.IsStateRestored)
            {
                DoTask(accounts.RestoreStateAsync());
            }
            else
            {
                DoTask(accounts.RefreshAllAsync());
            }
        }

        private AccountAdapter _adapter;
    }
}