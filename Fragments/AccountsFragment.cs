using System;

using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Fragments
{
    public class AccountsFragment : 
        RecyclerViewFragmentBase,
        IAccountItemClickListener
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

            _adapter = new AccountAdapter(accounts, this);
            Adapter = _adapter;

            if (accounts.IsStateRestored)
            {
                ViewMode = RecyclerViewMode.Data;
            }
            else
            {
                DoTask(accounts.RestoreStateAsync(), _adapter.NotifyDataSetChanged);
            }
        }

        public void OnItemOpenUnfollowers(int position)
        {
            var user = _adapter.GetItem(position);
            PushFragment(new UnfollowFragment(user));
        }

        public void OnItemOpenInstagram(int position)
        {
            ((IInstagramHost)Activity).OpenInInstagram(
                _adapter.GetItem(position).Data.User.Username);
        }

        public async void OnItemLogout(int position)
        {
            //var button = (MaterialButton)sender;
            //button.Enabled = false;
            try
            {
                await ((IInstagramHost)Activity).Accounts.LogoutAccountAtAsync(position);
                _adapter.NotifyDataSetChanged();
                ViewMode = RecyclerViewMode.Data;
            }
            catch (Exception ex)
            {
                //button.Enabled = true;
                ((IFragmentHost)Activity).ShowError(ex);
            }
        }

        public void OnItemRefresh(int position)
        {
            DoTask(
                _adapter.GetItem(position).RefreshAsync(),
                _adapter.NotifyDataSetChanged);
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
                    DoTask(
                        ((IInstagramHost)Activity).Accounts.RefreshAllAsync(),
                        Activity.Recreate);
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
                DoTask(accounts.RestoreStateAsync(), _adapter.NotifyDataSetChanged);
            }
            else
            {
                DoTask(accounts.RefreshAllAsync(), _adapter.NotifyDataSetChanged);
            }
        }

        private AccountAdapter _adapter;
    }
}