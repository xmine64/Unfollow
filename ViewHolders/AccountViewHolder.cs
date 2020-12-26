using System;
using System.Linq;

using Android.Views;

using AndroidX.AppCompat.View;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Card;
using Google.Android.Material.TextView;

using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.ViewHolders
{
    internal class AccountViewHolder : 
        RecyclerView.ViewHolder,
        MenuBuilder.ICallback
    {
        public AccountViewHolder(
            View item,
            IAccountItemClickListener listener) :
            base(item)
        {
            _tvFullName = item.FindViewById<MaterialTextView>(Resource.Id.item_account_fullname);
            _tvUserName = item.FindViewById<MaterialTextView>(Resource.Id.item_account_username);
            _tvFollowings = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followings);
            _tvFollowers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followers);
            _tvUnfollowers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_unfollowers);

            var menu = new MenuBuilder(ItemView.Context);
            menu.SetCallback(this);

            var inflater = new SupportMenuInflater(ItemView.Context);
            inflater.Inflate(Resource.Menu.popup_account, menu);

            _popup = new MenuPopupHelper(ItemView.Context, menu);
            _popup.SetAnchorView(ItemView);
            _popup.SetForceShowIcon(true);

            _listener = listener;

            var card = item.FindViewById<MaterialCardView>(Resource.Id.item_account_card);
            if (card == null) return;
            card.Click += Item_Click;
            card.LongClick += Item_LongClick;
        }

        public void BindData(Account.AccountData data)
        {
            _tvFullName.Text = data.User.Fullname;
            _tvUserName.Text = "@" + data.User.Username;
            _tvFollowings.Text = data.Followings.Count.ToString();
            _tvFollowers.Text = data.Followers.Count.ToString();
            _tvUnfollowers.Text = data.Unfollowers.Count().ToString();
        }

        private void Item_Click(object sender, EventArgs e)
        {
            _listener.OnItemOpenUnfollowers(AdapterPosition);
        }

        private void Item_LongClick(object sender, EventArgs e)
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
                case Resource.Id.popup_account_item_fans:
                    _listener.OnItemOpenFans(AdapterPosition);
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

        private readonly MaterialTextView _tvFullName;
        private readonly MaterialTextView _tvUserName;
        private readonly MaterialTextView _tvFollowings;
        private readonly MaterialTextView _tvFollowers;
        private readonly MaterialTextView _tvUnfollowers;

        private readonly MenuPopupHelper _popup;

        private readonly IAccountItemClickListener _listener;
    }

    internal interface IAccountItemClickListener
    {
        void OnItemOpenInstagram(int position);
        void OnItemOpenUnfollowers(int position);
        void OnItemOpenFans(int position);
        void OnItemLogout(int position);
        void OnItemRefresh(int position);
    }
}