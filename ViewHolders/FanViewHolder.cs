using System;

using Android.Views;

using AndroidX.AppCompat.View;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.ViewHolders
{
    class FanViewHolder : 
        RecyclerView.ViewHolder,
        MenuBuilder.ICallback
    {
        public FanViewHolder(
            View item,
            IFanItemClickListener listener) 
            : base(item)
        {
            _card = item.FindViewById<MaterialCardView>(Resource.Id.item_unfollower_card);
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_username);

            _menu = new MenuBuilder(ItemView.Context);
            _menu.SetCallback(this);
            var inflater = new SupportMenuInflater(ItemView.Context);
            inflater.Inflate(Resource.Menu.popup_fan, _menu);

            _popup = new MenuPopupHelper(ItemView.Context, _menu);
            _popup.SetAnchorView(ItemView);
            _popup.SetForceShowIcon(true);

            _listener = listener;

            _card.Click += Item_Click;
            _card.LongClick += Item_LongClick;
        }

        public void BindData(User user, bool selected)
        {
            _tv_fullname.Text = user.Fullname;
            _tv_username.Text = "@" + user.Username;
            _card.Checked = selected;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (!_listener.OnItemClick(AdapterPosition))
            {
                _popup.Show();
            }
        }

        private void Item_LongClick(object sender, View.LongClickEventArgs e)
        {
            _listener.OnItemLongClick(AdapterPosition);
        }

        private void Unfollow_Click(object sender, EventArgs e)
        {
            _listener.OnItemFollow(AdapterPosition);
        }

        public bool OnMenuItemSelected(MenuBuilder builder, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.popup_fan_item_open:
                    _listener.OnItemOpen(AdapterPosition);
                    return true;
                case Resource.Id.popup_fan_item_select:
                    _listener.OnItemSelect(AdapterPosition);
                    return true;
                case Resource.Id.popup_fan_item_follow:
                    _listener.OnItemFollow(AdapterPosition);
                    return true;
                case Resource.Id.popup_fan_item_add_whitelist:
                    _listener.OnItemAddToWhitelist(AdapterPosition);
                    return true;
                default:
                    return false;
            }
        }

        public void OnMenuModeChange(MenuBuilder builder) {}

        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialCardView _card;

        private MenuBuilder _menu;
        private MenuPopupHelper _popup;

        private IFanItemClickListener _listener;
    }

    interface IFanItemClickListener
    {
        bool OnItemClick(int position);
        void OnItemLongClick(int position);
        void OnItemOpen(int position);
        void OnItemSelect(int position);
        void OnItemFollow(int position);
        void OnItemAddToWhitelist(int position);
    }
}