using System.Collections.Generic;
using System.Linq;

using Android.Views;
using AndroidX.RecyclerView.Widget;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    internal class UnfollowerAdapter : RecyclerView.Adapter
    {

        public UnfollowerAdapter(
            Account data,
            IUnfollowerItemClickListener listener)
        {
            _data = data;
            _listener = listener;
            _unfollowersCache = new List<User>();
        }

        public override int ItemCount => _unfollowersCache.Count;

        public List<int> SelectedItems { get; private set; }

        public List<User> Whitelist { get; } = new List<User>();

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is UnfollowerViewHolder unfollowViewHolder)
            {
                unfollowViewHolder?.BindData(
                    _unfollowersCache[position], 
                    SelectedItems.Contains(position));
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var viewItem = LayoutInflater.From(parent.Context)?.Inflate(
                Resource.Layout.item_user,
                parent,
                false);
            return new UnfollowerViewHolder(viewItem, _listener);
        }

        public User GetItem(int position)
        {
            return _unfollowersCache[position];
        }

        public void Refresh()
        {
            _unfollowersCache.Clear();
            _unfollowersCache.AddRange(_data.Data.Unfollowers.Except(Whitelist));
            SelectedItems = new List<int>();
        }

        public void SelectOrDeselectItem(int position)
        {
            if (SelectedItems.Contains(position))
                SelectedItems.Remove(position);
            else
                SelectedItems.Add(position);
            NotifyItemChanged(position);
        }

        public void SelectAll()
        {
            DeselectAll();
            SelectedItems.AddRange(Enumerable.Range(0, _unfollowersCache.Count));
            NotifyDataSetChanged();
        }

        public void DeselectAll()
        {
            SelectedItems.Clear();
            NotifyDataSetChanged();
        }

        public User[] GetSelected()
        {
            return SelectedItems.Select(pos => _unfollowersCache[pos]).ToArray();
        }

        private readonly Account _data;

        private readonly IUnfollowerItemClickListener _listener;
        private readonly List<User> _unfollowersCache;
    }
}