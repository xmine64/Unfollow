using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Button;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextView;

namespace madamin.unfollow
{
    public class HomeFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_home, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var ig = (IInstagramActivity)Activity;

            var appbar = view.FindViewById<MaterialToolbar>(Resource.Id.home_appbar);
            appbar.MenuItemClick += async (sender, args) =>
            {
                switch (args.Item.ItemId)
                {
                    case Resource.Id.appmenu_item_refresh:
                        try
                        {
                            await ig.RefreshCache();
                        }
                        catch
                        {
                            Toast.MakeText(Activity, Resource.String.error, ToastLength.Long).Show();
                        }
                        ((INavigationHost)Activity).NavigateTo(new HomeFragment(), false);
                        break;
                    case Resource.Id.appmenu_item_about:
                        new MaterialAlertDialogBuilder(Activity)
                        .SetTitle(Resource.String.menu_about)
                        .SetNeutralButton(Android.Resource.String.Ok, (dialog, Activity) => { })
                        .SetMessage(Resource.String.msg_about)
                        .Show();
                        break;
                    case Resource.Id.appmenu_item_exit:
                        Activity.Finish();
                        break;
                }
            };

            var recycler = view.FindViewById<RecyclerView>(Resource.Id.fragment_home_accounts_recycler);
            
            var layout_manager = new LinearLayoutManager(Activity);
            recycler.SetLayoutManager(layout_manager);

            var adapter = new AccountAdapter(ig.Instagram.Data);
            recycler.SetAdapter(adapter);
        }
    }

    class AccountViewHolder : RecyclerView.ViewHolder
    {
        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialTextView _tv_followers;
        private MaterialButton _btn_logout;

        public AccountViewHolder(View item) : base(item)
        {
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_account_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_account_username);
            _tv_followers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followers);
            _btn_logout = item.FindViewById<MaterialButton>(Resource.Id.item_account_logout);
        }

        public void SetData(InstagramData data)
        {
            _tv_fullname.Text = data.User.Fullname;
            _tv_username.Text = "@" + data.User.Username;
            _tv_followers.Text = string.Format(_tv_followers.Text, data.Followings.Count, data.Followers.Count);
        }
    }

    class AccountAdapter : RecyclerView.Adapter
    {
        public AccountAdapter(InstagramData data)
        {
            _account_data = data;
        }

        public override int ItemCount => 1;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            (holder as AccountViewHolder).SetData(_account_data);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_account, parent, false);
            return new AccountViewHolder(view_item);
        }

        private InstagramData _account_data;
    }
}