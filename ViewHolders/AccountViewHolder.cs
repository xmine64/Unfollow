using System;
using System.Linq;

using Android.Views;

using AndroidX.RecyclerView.Widget;
using AndroidX.AppCompat.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;
using AndroidX.AppCompat.View.Menu;
using AndroidX.AppCompat.View;

namespace Madamin.Unfollow.ViewHolders
{
    class AccountViewHolder : 
        RecyclerView.ViewHolder,
        MenuBuilder.ICallback
    {
        public AccountViewHolder(
            View item,
            IAccountItemClickListener listener) :
            base(item)
        {
            _card = item.FindViewById<MaterialCardView>(Resource.Id.item_account_card);
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_account_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_account_username);
            _tv_followers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followers);

            _menu = new MenuBuilder(ItemView.Context);
            _menu.SetCallback(this);
            var inflater = new SupportMenuInflater(ItemView.Context);
            inflater.Inflate(Resource.Menu.popup_account, _menu);

            _popup = new MenuPopupHelper(ItemView.Context, _menu);
            _popup.SetAnchorView(ItemView);
            _popup.SetForceShowIcon(true);

            _listener = listener;

            _card.Click += Item_Click;
        }

        public void BindData(Account.AccountData data)
        {
            _tv_fullname.Text = data.User.Fullname;
            _tv_username.Text = "@" + data.User.Username;
            _tv_followers.Text = string.Format(
                _tv_followers.Text,
                data.Followings.Count,
                data.Followers.Count,
                data.Unfollowers.Count());
        }

        private void Item_Click(object sender, EventArgs e)
        {
            _popup.Show();
        }

        public bool OnMenuItemSelected(MenuBuilder builder, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.popup_account_item_unfollowers:
                    _listener.OnItemOpenUnfollowers(AdapterPosition);
                    return true;
                case Resource.Id.popup_account_item_open:
                    _listener.OnItemOpenInstagram(AdapterPosition);
                    return true;
                case Resource.Id.popup_account_item_logout:
                    _listener.OnItemLogout(AdapterPosition);
                    return true;
                case Resource.Id.popup_account_item_refresh:
                    _listener.OnItemRefresh(AdapterPosition);
                    return true;
                default:
                    return false;
            }
        }

        public void OnMenuModeChange(MenuBuilder builder) {}

        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialTextView _tv_followers;
        private MaterialCardView _card;

        private MenuBuilder _menu;
        private MenuPopupHelper _popup;

        private IAccountItemClickListener _listener;
    }

    interface IAccountItemClickListener
    {
        void OnItemOpenInstagram(int position);
        void OnItemOpenUnfollowers(int position);
        void OnItemLogout(int position);
        void OnItemRefresh(int position);
    }
}