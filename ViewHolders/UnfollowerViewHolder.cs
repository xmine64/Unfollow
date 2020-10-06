using System;

using Android.Views;

using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.ViewHolders
{
    class UnfollowerViewHolder : RecyclerView.ViewHolder
    {
        public UnfollowerViewHolder(
            View item,
            IUnfollowerItemClickListener listener) 
            : base(item)
        {
            _card = item.FindViewById<MaterialCardView>(Resource.Id.item_unfollower_card);
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_username);
            _btn_unfollow = item.FindViewById<MaterialButton>(Resource.Id.item_unfollower_unfollow_button);

            _listener = listener;

            _card.Click += Item_Click;
            _card.LongClick += Item_LongClick;
            _btn_unfollow.Click += Unfollow_Click;
        }

        public void BindData(User user, bool selected)
        {
            _tv_fullname.Text = user.Fullname;
            _tv_username.Text = "@" + user.Username;
            _card.Checked = selected;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            _listener.OnItemClick(AdapterPosition);
        }

        private void Item_LongClick(object sender, View.LongClickEventArgs e)
        {
            _listener.OnItemLongClick(AdapterPosition);
        }

        private void Unfollow_Click(object sender, EventArgs e)
        {
            _listener.OnItemUnfollowClick(AdapterPosition);
        }

        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialButton _btn_unfollow;
        private MaterialCardView _card;

        private IUnfollowerItemClickListener _listener;
    }

    interface IUnfollowerItemClickListener
    {
        void OnItemClick(int position);
        void OnItemLongClick(int position);
        void OnItemUnfollowClick(int position);
    }
}