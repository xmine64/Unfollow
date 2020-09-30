using System;
using System.Collections.Generic;

using Android.Views;

using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Adapters
{
    class AccountAdapter : RecyclerView.Adapter
    {
        public AccountAdapter(Accounts data)
        {
            _data = data;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var account_view_holder = holder as AccountViewHolder;
            if (account_view_holder == null)
                return;

            account_view_holder.BindData(_data[position].Data);
            account_view_holder.BindEvents(
                (sender, args) =>
                {
                    ItemClick?.Invoke(sender, new AccountClickEventArgs(position));
                },
                (sender, args) =>
                {
                    ItemLogoutClick?.Invoke(sender, new AccountClickEventArgs(position));
                });
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_account, parent, false);
            return new AccountViewHolder(view_item);
        }

        public override int ItemCount => _data.Count;

        public event EventHandler<AccountClickEventArgs> ItemClick;
        public event EventHandler<AccountClickEventArgs> ItemLogoutClick;

        private Accounts _data;

        class AccountViewHolder : RecyclerView.ViewHolder
        {
            public AccountViewHolder(View item) : base(item)
            {
                _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_account_fullname);
                _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_account_username);
                _tv_followers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followers);
                _btn_logout = item.FindViewById<MaterialButton>(Resource.Id.item_account_logout);
            }

            public void BindData(Account.AccountData data)
            {
                _tv_fullname.Text = data.User.Fullname;
                _tv_username.Text = "@" + data.User.Username;
                _tv_followers.Text = string.Format(
                    _tv_followers.Text, 
                    data.Followings.Count,
                    data.Followers.Count);
            }

            public void BindEvents(EventHandler click_handler, EventHandler logout_handler)
            {
                ItemView.Click += click_handler;
                _btn_logout.Click += logout_handler;
            }
            
            private MaterialTextView _tv_fullname;
            private MaterialTextView _tv_username;
            private MaterialTextView _tv_followers;
            private MaterialButton _btn_logout;
        }
    }

    class AccountClickEventArgs : EventArgs
    {
        public AccountClickEventArgs(int position)
        {
            Position = position;
        }

        public int Position { get; }
    }
}