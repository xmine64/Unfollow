using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using System;

namespace madamin.unfollow
{
    public class HomeFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HasOptionsMenu = true;
            return inflater.Inflate(Resource.Layout.fragment_home, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _adapter = new AccountAdapter((IInstagramActivity)Activity);
            var recycler = view.FindViewById<RecyclerView>(Resource.Id.fragment_home_accounts_recycler);
            recycler.SetLayoutManager(new LinearLayoutManager(Activity));
            recycler.SetAdapter(_adapter);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.appbar_menu_home, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.appbar_home_item_addaccount:
                    ((INavigationHost)Activity).NavigateTo(new LoginFragment(), true);
                    _adapter.NotifyDataSetChanged();
                    return true;
                case Resource.Id.appbar_home_item_refresh:
                    new Action(async () => 
                    { 
                        await ((IInstagramActivity)Activity).Instagram.RefreshAll(); 
                    }).Invoke();
                    _adapter.NotifyDataSetChanged();
                    return true;
            }
            return false;
        }

        private AccountAdapter _adapter;
    }

    class AccountViewHolder : RecyclerView.ViewHolder
    {
        private RecyclerView.Adapter _adapter;
        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialTextView _tv_followers;
        private MaterialButton _btn_logout;

        public AccountViewHolder(View item, RecyclerView.Adapter adapter) : base(item)
        {
            _adapter = adapter;
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_account_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_account_username);
            _tv_followers = item.FindViewById<MaterialTextView>(Resource.Id.item_account_followers);
            _btn_logout = item.FindViewById<MaterialButton>(Resource.Id.item_account_logout);
        }

        public void SetData(Instagram instagram, int position)
        {
            _tv_fullname.Text = instagram[position].Data.User.Fullname;
            _tv_username.Text = "@" + instagram[position].Data.User.Username;
            _tv_followers.Text = string.Format(
                _tv_followers.Text,
                instagram[position].Data.Followings.Count,
                instagram[position].Data.Followers.Count);
            _btn_logout.Click += async (sender, args) =>
            {
                await instagram.LogoutAccountAt(position);
                _adapter.NotifyDataSetChanged();
            };
        }
    }

    class AccountAdapter : RecyclerView.Adapter
    {
        public AccountAdapter(IInstagramActivity instagram)
        {
            _instagram = instagram;
        }

        public override int ItemCount => _instagram.Instagram.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            (holder as AccountViewHolder).
                SetData(_instagram.Instagram, position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_account, parent, false);
            return new AccountViewHolder(view_item, this);
        }

        private IInstagramActivity _instagram;
    }
}