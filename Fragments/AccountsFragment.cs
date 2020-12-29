using System;

using Android.OS;
using AndroidX.Preference;
using Google.Android.Material.Dialog;
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

        private void AccountsFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            Title = GetString(Resource.String.app_name);
            EmptyText = GetString(Resource.String.msg_no_account);
            // TODO: set ErrorText
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            var accounts = ((IInstagramHost)Activity).Accounts;

            _adapter = new AccountAdapter(accounts, this);
            SetAdapter(_adapter);

            if (accounts.IsStateRestored)
            {
                ViewMode = RecyclerViewMode.Data;
            }
            else
            {
                DoTask(accounts.RestoreStateAsync(), _adapter.NotifyDataSetChanged);
            }

            if (accounts.Count < 1 && !_hasPushedToLoginFragment)
            {
                ((IFragmentHost) Activity)
                    .PushFullScreenFragment(new LoginFragment());
                _hasPushedToLoginFragment = true;
                return;
            }

            var pfman = PreferenceManager
                .GetDefaultSharedPreferences(Activity);
            if (!pfman.GetBoolean(TipIsShownKey, false))
            {
                var dialog = new MaterialAlertDialogBuilder(Activity);
                dialog.SetTitle(Resource.String.title_tip);
                dialog.SetMessage(Resource.String.msg_tip);
                dialog.Show();

                var pfedit = pfman.Edit();
                pfedit.PutBoolean(TipIsShownKey, true);
                pfedit.Apply();
            }
        }

        private const string TipIsShownKey = "tip_is_shown";

        private bool _hasPushedToLoginFragment;

        public void OnItemOpenUnfollowers(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(AccountIndex, position);
            var fragment = new UnfollowFragment
            {
                Arguments = bundle
            };
            PushFragment(fragment);
        }

        public void OnItemOpenInstagram(int position)
        {
            var userName = _adapter.GetItem(position).Data.User.Username;
            ((IInstagramHost)Activity).OpenInInstagram(userName);
        }

        public void OnItemLogout(int position)
        {
            DoTask(((IInstagramHost)Activity).Accounts.LogoutAccountAtAsync(position),
                _adapter.NotifyDataSetChanged);
        }

        public void OnItemRefresh(int position)
        {
            DoTask(_adapter.GetItem(position).RefreshAsync(),
                _adapter.NotifyDataSetChanged);
        }

        public void OnItemOpenFans(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(AccountIndex, position);
            var fragment = new FansFragment
            {
                Arguments = bundle
            };
            PushFragment(fragment);
        }

        private void AccountsFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    ((IFragmentHost)Activity)
                        .PushFullScreenFragment(new LoginFragment());
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