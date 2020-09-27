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
using Google.Android.Material.Button;
using Google.Android.Material.TextView;

namespace madamin.unfollow
{
    public class UnfollowFragment : Fragment
    {
        public UnfollowFragment(InstagramAccount account)
        {
            _account = account;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_unfollow, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            
            var recycler = view.FindViewById<RecyclerView>(Resource.Id.fragment_unfollow_recycler);
            recycler.SetLayoutManager(new LinearLayoutManager(Activity));
            recycler.SetAdapter(new UnfollowerAdapter((MainActivity)Activity, _account));
        }

        private InstagramAccount _account;
    }

    public class UnfollowerViewHolder : RecyclerView.ViewHolder
    {
        private MainActivity _context;
        private UnfollowerAdapter _adapter;

        private MaterialTextView _tv_fullname;
        private MaterialTextView _tv_username;
        private MaterialButton _btn_open;
        private MaterialButton _btn_unfollow;

        public UnfollowerViewHolder(View item, UnfollowerAdapter adapter, MainActivity context) : base(item)
        {
            _context = context;
            _adapter = adapter;
            _tv_fullname = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_fullname);
            _tv_username = item.FindViewById<MaterialTextView>(Resource.Id.item_unfollower_username);
            _btn_unfollow = item.FindViewById<MaterialButton>(Resource.Id.item_unfollower_unfollow_button);
            _btn_open = item.FindViewById<MaterialButton>(Resource.Id.item_unfollower_open_button);
        }

        public void SetData(InstagramAccount account, InstagramUser user)
        {
            _tv_fullname.Text = user.Fullname;
            _tv_username.Text = "@" + user.Username;
            _btn_open.Click += (sender, args) =>
            {
                var ig = Intent.ParseUri("https://instagram.com/_u/" + user.Username, IntentUriType.None);
                ig.SetPackage("com.instagram.android");
                try
                {
                    _context.StartActivity(ig);
                }
                catch
                {
                    Toast.MakeText(_context, Resource.String.error, ToastLength.Long).Show();
                }
            };
            _btn_unfollow.Click += async (sender, args) =>
            {
                _btn_unfollow.Enabled = false;
                try
                {
                    await account.Unfollow(user);
                    _adapter.Remove(user);
                    _adapter.NotifyDataSetChanged();
                }
                catch
                {
                    _btn_unfollow.Enabled = true;
                    Toast.MakeText(_context, Resource.String.error, ToastLength.Long).Show();
                }
            };
        }
    }

    public class UnfollowerAdapter : RecyclerView.Adapter
    {
        private MainActivity _context;
        private InstagramAccount _account;
        private List<InstagramUser> _unfollowers;

        public UnfollowerAdapter(MainActivity context, InstagramAccount account)
        {
            _context = context;
            _account = account;
            _unfollowers = _account.Data.Unfollowers.ToList();
        }

        public override int ItemCount => _unfollowers.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            (holder as UnfollowerViewHolder)?
                .SetData(_account, _unfollowers[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_unfollower, parent, false);
            return new UnfollowerViewHolder(view_item, this, _context);
        }

        public void Remove(InstagramUser user)
        {
            _unfollowers.Remove(user);
        }
    }
}