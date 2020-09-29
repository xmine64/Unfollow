using System;
using System.Collections.Generic;

using Android.Views;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;

using User = Madamin.Unfollow.Instagram.User;

namespace Madamin.Unfollow.Adapters
{
    class UnfollowerAdapter : RecyclerView.Adapter
    {
        public UnfollowerAdapter(List<User> unfollowers)
        {
            _unfollowers = unfollowers;
        }

        public override int ItemCount => _unfollowers.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var unfollow_view_holder = holder as UnfollowerViewHolder;
            if (unfollow_view_holder == null)
                return;

            unfollow_view_holder.BindData(_unfollowers[position]);
            unfollow_view_holder.BindEvents(
                (sender, args) =>
                {
                    ItemClick?.Invoke(this, new UnfollowClickEventArgs(_unfollowers[position], position));
                },
                (sender, args) =>
                {
                    ItemUnfollowClick?.Invoke(this, new UnfollowClickEventArgs(_unfollowers[position], position));
                });
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_unfollower, parent, false);
            return new UnfollowerViewHolder(view_item);
        }

        public void Remove(int position)
        {
            _unfollowers.RemoveAt(position);
        }

        public event EventHandler<UnfollowClickEventArgs> ItemClick;
        public event EventHandler<UnfollowClickEventArgs> ItemUnfollowClick;

        private List<User> _unfollowers;

        class UnfollowerViewHolder : RecyclerView.ViewHolder
        {
            private MaterialTextView _tv_fullname;
            private MaterialTextView _tv_username;
            private MaterialButton _btn_unfollow;

            public UnfollowerViewHolder(View item) : base(item)
            {
                _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_fullname);
                _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_username);
                _btn_unfollow = item.FindViewById<MaterialButton>(Resource.Id.item_unfollower_unfollow_button);
            }

            public void BindData(User user)
            {
                _tv_fullname.Text = user.Fullname;
                _tv_username.Text = "@" + user.Username;
            }

            public void BindEvents(EventHandler click_handler, EventHandler unfollow_handler)
            {
                ItemView.Click += click_handler;
                _btn_unfollow.Click += unfollow_handler;
            }
        }
    }

    class UnfollowClickEventArgs : EventArgs
    {
        public UnfollowClickEventArgs(User user, int position)
        {
            User = user;
            Position = position;
        }

        public User User { get; }
        public int Position { get; }
    }
}