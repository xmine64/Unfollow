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
    internal class UnfollowerViewHolder :
        RecyclerView.ViewHolder,
        MenuBuilder.ICallback
    {
        public UnfollowerViewHolder(
            View item,
            IUnfollowerItemClickListener listener)
            : base(item)
        {
            _card = item.FindViewById<MaterialCardView>(Resource.Id.item_user_card);
            _tvFullName = item.FindViewById<MaterialTextView>(Resource.Id.item_user_fullname);
            _tvUserName = item.FindViewById<MaterialTextView>(Resource.Id.item_user_username);

            var menu = new MenuBuilder(ItemView.Context);
            menu.SetCallback(this);
            var inflater = new SupportMenuInflater(ItemView.Context);
            inflater.Inflate(Resource.Menu.popup_unfollower, menu);

            var optionMenuButton = item.FindViewById(Resource.Id.item_user_more);
            _popup = new MenuPopupHelper(ItemView.Context, menu);
            _popup.SetAnchorView(optionMenuButton);
            _popup.SetForceShowIcon(true);

            _listener = listener;

            if (_card == null || optionMenuButton == null)
                return;

            _card.Click += Item_Click;
            _card.LongClick += Item_LongClick;
            optionMenuButton.Click += More_Click;
        }

        public void BindData(User user, bool selected)
        {
            _tvFullName.Text = user.Fullname;
            _tvUserName.Text = "@" + user.Username;
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
                case Resource.Id.popup_unfollower_item_block:
                    _listener.OnItemBlock(AdapterPosition);
                    return true;
                default:
                    return false;
            }
        }

        public void OnMenuModeChange(MenuBuilder builder)
        {
        }

        private readonly MaterialTextView _tvFullName;
        private readonly MaterialTextView _tvUserName;
        private readonly MaterialCardView _card;

        private readonly MenuPopupHelper _popup;

        private readonly IUnfollowerItemClickListener _listener;
    }

    internal interface IUnfollowerItemClickListener
    {
        bool OnItemClick(int position);
        void OnItemLongClick(int position);
        void OnItemOpen(int position);
        void OnItemSelect(int position);
        void OnItemUnfollow(int position);
        void OnItemAddToWhitelist(int position);
        void OnItemBlock(int position);
    }
}