using System;

using Android.Views;

using AndroidX.AppCompat.View;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.TextView;
using Google.Android.Material.Card;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.ViewHolders
{
    class UnfollowerViewHolder : 
        RecyclerView.ViewHolder,
        MenuBuilder.ICallback
    {
        public UnfollowerViewHolder(
            View item,
            IUnfollowerItemClickListener listener) 
            : base(item)
        {
            _card = item.FindViewById<MaterialCardView>(Resource.Id.item_user_card);
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_user_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_user_username);
            var option_menu_button = item.FindViewById(Resource.Id.item_user_more);

            _menu = new MenuBuilder(ItemView.Context);
            _menu.SetCallback(this);
            var inflater = new SupportMenuInflater(ItemView.Context);
            inflater.Inflate(Resource.Menu.popup_unfollower, _menu);

            _popup = new MenuPopupHelper(ItemView.Context, _menu);
            _popup.SetAnchorView(option_menu_button);
            _popup.SetForceShowIcon(true);

            _listener = listener;

            _card.Click += Item_Click;
            _card.LongClick += Item_LongClick;
            option_menu_button.Click += More_Click;
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
                _listener.OnItemOpen(AdapterPosition);
            }
        }

        private void Item_LongClick(object sender, View.LongClickEventArgs e)
        {
            _listener.OnItemLongClick(AdapterPosition);
        }

        private void More_Click(object sender, EventArgs e)
        {
            _popup.Show();
        }

        public bool OnMenuItemSelected(MenuBuilder builder, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.popup_unfollower_item_open:
                    _listener.OnItemOpen(AdapterPosition);
                    return true;
                case Resource.Id.popup_unfollower_item_select:
                    _listener.OnItemSelect(AdapterPosition);
                    return true;
                case Resource.Id.popup_unfollower_item_unfollow:
                    _listener.OnItemUnfollow(AdapterPosition);
                    return true;
                case Resource.Id.popup_unfollower_item_add_whitelist:
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

        private IUnfollowerItemClickListener _listener;
    }

    interface IUnfollowerItemClickListener
    {
        bool OnItemClick(int position);
        void OnItemLongClick(int position);
        void OnItemOpen(int position);
        void OnItemSelect(int position);
        void OnItemUnfollow(int position);
        void OnItemAddToWhitelist(int position);
    }
}