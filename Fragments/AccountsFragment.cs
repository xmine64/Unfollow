using System;
using Android.OS;
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
        private const string PreferenceKeyTipIsShown = "tip_is_shown";

        private bool _hasPushedToLoginFragment;
        private bool _tipShown;

        private AccountAdapter _adapter;

        private AccountAdapter AccountAdapter
        {
            get => _adapter;
            set
            {
                _adapter = value;
                SetAdapter(value);
            }
        }

        public AccountsFragment() :
            base(Resource.Menu.appbar_menu_accounts)
        {
            Create += AccountsFragment_Create;
            MenuItemSelected += AccountsFragment_MenuItemSelected;
            RetryClick += AccountsFragment_RetryClick;
            ViewModeChanged += AccountsFragment_ViewModeChanged;
        }

        private void AccountsFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            // Setup fragment
            Title = GetString(Resource.String.app_name);
            ActionBarVisible = true;
            EmptyText = GetString(Resource.String.msg_no_account);
            SetEmptyImage(Resource.Drawable.ic_person_add_black_48dp);

            var accounts = ((IInstagramHost) Activity).Accounts;
            AccountAdapter = new AccountAdapter(accounts, this);

            // Check if Tip has been shown already
            _tipShown = ((IPreferenceManager)Activity)
                .GetBoolean(PreferenceKeyTipIsShown, false);

            // Check if accounts data are not loaded, load them
            if (accounts.IsStateRestored)
            {
                if (accounts.NeedRefresh)
                {
                    DoTask(accounts.FixNeedRefresh(), _adapter.NotifyDataSetChanged);
                }
                else
                {
                    ViewMode = RecyclerViewMode.Data;
                }
            }
            else
            {
                DoTask(accounts.RestoreStateAsync(), _adapter.NotifyDataSetChanged);
            }
        }

        private void AccountsFragment_ViewModeChanged(object sender, RecyclerViewMode mode)
        {
            // Push to login fragment, for the first time
            if (mode == RecyclerViewMode.Empty &&
                !_hasPushedToLoginFragment)
            {
                ((IFragmentHost) Activity).PushFragment(new LoginFragment());
                _hasPushedToLoginFragment = true;
                return;
            }

            // Show tip, for the first time
            if (mode == RecyclerViewMode.Data &&
                !_tipShown)
            {
                var dialog = new MaterialAlertDialogBuilder(Activity);
                dialog.SetTitle(Resource.String.title_tip);
                dialog.SetMessage(Resource.String.msg_tip);
                dialog.Show();

                ((IPreferenceManager)Activity).SetBoolean(PreferenceKeyTipIsShown, true);

                _tipShown = true;

                // Don't invoke this method again
                ViewModeChanged -= AccountsFragment_ViewModeChanged;
            }
        }

        private void AccountsFragment_MenuItemSelected(object sender, OnMenuItemSelectedEventArgs e)
        {
            switch (e.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    ((IFragmentHost) Activity).PushFragment(new LoginFragment());
                    break;

                case Resource.Id.appbar_home_item_refresh:
                    DoTask(
                        ((IInstagramHost) Activity).Accounts.RefreshAllAsync(),
                        AccountAdapter.NotifyDataSetChanged);
                    break;

                default:
                    e.Finished = false;
                    break;
            }
        }

        private void AccountsFragment_RetryClick(object sender, EventArgs e)
        {
            var accounts = ((IInstagramHost) Activity).Accounts;

            if (accounts.IsStateRestored)
            {
                DoTask(accounts.RefreshAllAsync(), AccountAdapter.NotifyDataSetChanged);
                return;
            }

            DoTask(accounts.RestoreStateAsync(), AccountAdapter.NotifyDataSetChanged);
        }

        public void OnItemOpenUnfollowers(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(BundleKeyAccountIndex, position);

            var fragment = new UnfollowFragment
            {
                Arguments = bundle
            };
            PushFragment(fragment);
        }

        public void OnItemOpenFans(int position)
        {
            var bundle = new Bundle();
            bundle.PutInt(BundleKeyAccountIndex, position);

            var fragment = new FansFragment
            {
                Arguments = bundle
            };
            PushFragment(fragment);
        }

        public void OnItemOpenInstagram(int position)
        {
            var userName = AccountAdapter.GetItem(position).Data.User.Username;
            ((IInstagramHost) Activity).OpenInInstagram(userName);
        }

        public void OnItemLogout(int position)
        {
            var accounts = ((IInstagramHost) Activity).Accounts;
            DoTask(accounts.LogoutAccountAtAsync(position), AccountAdapter.NotifyDataSetChanged);
        }

        public void OnItemRefresh(int position)
        {
            DoTask(_adapter.GetItem(position).RefreshAsync(), AccountAdapter.NotifyDataSetChanged);
        }
    }
}
