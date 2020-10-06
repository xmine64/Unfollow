using System.Collections.Generic;
using System.Linq;

using Android.Views;

using AndroidX.RecyclerView.Widget;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    class UnfollowerAdapter : RecyclerView.Adapter
    {
        public UnfollowerAdapter(
            Account data,
            IUnfollowerItemClickListener listener)
        {
            _data = data;
            _listener = listener;
            Refresh();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var unfollow_view_holder = holder as UnfollowerViewHolder;
            if (unfollow_view_holder == null)
                return;

            unfollow_view_holder.BindData(
                _unfollowers_cache[position],
                SelectedItems.Contains(position));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_unfollower, parent, false);
            return new UnfollowerViewHolder(view_item, _listener);
        }

        public User GetItem(int position)
        {
            return _unfollowers_cache[position];
        }

        public void Refresh()
        {
            _unfollowers_cache = _data.Data.Unfollowers.ToArray();
            SelectedItems = new List<int>();
        }

        public void SelectOrDeselectItem(int position)
        {
            if (SelectedItems.Contains(position))
            {
                SelectedItems.Remove(position);
            }
            else
            {
                SelectedItems.Add(position);
            }
            NotifyItemChanged(position);
        }

        public void SelectAll()
        {
            DeselectAll();
            SelectedItems.AddRange(Enumerable.Range(0, _unfollowers_cache.Length));
            NotifyDataSetChanged();
        }

        public void DeselectAll()
        {
            SelectedItems.Clear();
            NotifyDataSetChanged();
        }

        public override int ItemCount => _unfollowers_cache.Length;

        public List<int> SelectedItems { get; private set; }

        private Account _data;
        private User[] _unfollowers_cache;

        private IUnfollowerItemClickListener _listener;
    }
}