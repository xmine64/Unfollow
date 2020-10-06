using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Adapters
{
    class UnfollowerAdapter : RecyclerView.Adapter
    {
        public UnfollowerAdapter(Account data)
        {
            _data = data;
            Refresh();
        }

        public override int ItemCount => _unfollowers_cache.Length;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var unfollow_view_holder = holder as UnfollowerViewHolder;
            if (unfollow_view_holder == null)
                return;

            unfollow_view_holder
                .BindData(_unfollowers_cache[position]);

            unfollow_view_holder.BindEvents(
                // Item Click Event
                (sender, args) =>
                {
                    // TODO: if any item selected, (de)select more items on normal click
                    Console.WriteLine("Item Clicked: " + position);

                    ItemClick?
                        .Invoke(sender, new UnfollowClickEventArgs(_unfollowers_cache[position].User));
                },
                // Item LongClick Event
                (sender, args) =>
                {
                    Console.WriteLine("Item LongClicked: " + position);
                    SelectOrDeselectItem(position);

                    ItemLongClick?
                        .Invoke(sender, new UnfollowClickEventArgs(_unfollowers_cache[position].User));

                    args.Handled = true; // TODO?
                },
                // Unfollow Button Click Event
                (sender, args) =>
                {
                    ItemUnfollowClick?
                        .Invoke(sender, new UnfollowClickEventArgs(_unfollowers_cache[position].User));
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
            _unfollowers_cache = _data.Data.Unfollowers
                .Select(user => new Unfollower(user))
                .ToArray();
        }

        public void SelectOrDeselectItem(int position)
        {
            Console.WriteLine("[Debug]: Item Selection Changed: " + position);
            var state = _unfollowers_cache[position].Selected;
            _unfollowers_cache[position].Selected = !state;
            NotifyItemChanged(position);

            if (_unfollowers_cache.Any(unfollower => unfollower.Selected))
            {

            }
        }

        public event EventHandler<UnfollowClickEventArgs> ItemClick;
        public event EventHandler<UnfollowClickEventArgs> ItemLongClick;
        public event EventHandler<UnfollowClickEventArgs> ItemUnfollowClick;

        private Account _data;
        private Unfollower[] _unfollowers_cache;

        class UnfollowerViewHolder : RecyclerView.ViewHolder
        {
            private MaterialTextView _tv_fullname;
            private MaterialTextView _tv_username;
            private MaterialButton _btn_unfollow;
            private MaterialCardView _card;

            public UnfollowerViewHolder(View item) : base(item)
            {
                _card = item.FindViewById<MaterialCardView>(Resource.Id.item_unfollower_card);
                _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_fullname);
                _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_username);
                _btn_unfollow = item.FindViewById<MaterialButton>(Resource.Id.item_unfollower_unfollow_button);
            }

            public void BindData(Unfollower unfollower)
            {
                _tv_fullname.Text = unfollower.User.Fullname;
                _tv_username.Text = "@" + unfollower.User.Username;
                _card.Checked = unfollower.Selected;
            }

            public void BindEvents(
                EventHandler click_handler,
                EventHandler<View.LongClickEventArgs> long_click_handler,
                EventHandler unfollow_handler)
            {
                ItemView.Click += click_handler;
                ItemView.LongClick += long_click_handler;
                _btn_unfollow.Click += unfollow_handler;
            }
        }

        class Unfollower
        {
            public Unfollower(User user)
            {
                User = user;
                Selected = false;
            }
            public User User { get; }
            public bool Selected { get; set; }
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