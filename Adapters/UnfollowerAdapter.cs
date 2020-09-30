using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Adapters
{
    class UnfollowerAdapter : RecyclerView.Adapter
    {
        public UnfollowerAdapter(Account data)
        {
            _data = data;
            _unfollowers_cache = _data.Data.Unfollowers.ToArray();
        }

        public override int ItemCount => _unfollowers_cache.Length;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var unfollow_view_holder = holder as UnfollowerViewHolder;
            if (unfollow_view_holder == null)
                return;

            unfollow_view_holder.BindData(_unfollowers_cache[position]);
            unfollow_view_holder.BindEvents(
                (sender, args) =>
                {
                    ItemClick?.Invoke(sender, new UnfollowClickEventArgs(_unfollowers_cache[position]));
                },
                (sender, args) =>
                {
                    ItemUnfollowClick?.Invoke(sender, new UnfollowClickEventArgs(_unfollowers_cache[position]));
                });
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_unfollower, parent, false);
            return new UnfollowerViewHolder(view_item);
        }

        public void Refresh()
        {
            _unfollowers_cache = _data.Data.Unfollowers.ToArray();
        }

        public event EventHandler<UnfollowClickEventArgs> ItemClick;
        public event EventHandler<UnfollowClickEventArgs> ItemUnfollowClick;

        private Account _data;
        private User[] _unfollowers_cache;

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
        public UnfollowClickEventArgs(User user)
        {
            User = user;
        }

        public User User { get; }
    }
}