using System;
using System.Linq;

using Android.Views;

using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.ViewHolders
{
    class AccountViewHolder : RecyclerView.ViewHolder
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
            _btn_logout = item.FindViewById<MaterialButton>(Resource.Id.item_account_logout);

            _listener = listener;

            _card.Click += Item_Click;
            _btn_logout.Click += Logout_Click;
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
            _listener.OnItemClick(AdapterPosition);
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            _listener.OnItemLogoutClick(AdapterPosition);
        }

        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialTextView _tv_followers;
        private MaterialButton _btn_logout;
        private MaterialCardView _card;

        private IAccountItemClickListener _listener;
    }

    interface IAccountItemClickListener
    {
        void OnItemClick(int position);
        void OnItemLogoutClick(int position);
    }
}