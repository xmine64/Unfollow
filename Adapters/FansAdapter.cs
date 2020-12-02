using System.Collections.Generic;
using System.Linq;

using Android.Views;
using AndroidX.RecyclerView.Widget;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    internal class FansAdapter : RecyclerView.Adapter
    {
        public FansAdapter(
            Account data,
            IFanItemClickListener listener)
        {
            _data = data;
            _listener = listener;
            _fansCache = new List<User>();
        }

        public override int ItemCount => _fansCache.Count;

        public List<int> SelectedItems { get; private set; }

        public List<User> Whitelist { get; } = new List<User>();

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is FanViewHolder fanViewHolder)
                fanViewHolder.BindData(
                    _fansCache[position], 
                    SelectedItems.Contains(position));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var viewItem = LayoutInflater.From(parent.Context)?.Inflate(
                Resource.Layout.item_user,
                parent,
                false);
            return new FanViewHolder(viewItem, _listener);
        }

        public User GetItem(int position)
        {
            return _fansCache[position];
        }

        public void Refresh()
        {
            _fansCache.Clear();
            _fansCache.AddRange(_data.Data.Fans.Except(Whitelist));
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
            SelectedItems.AddRange(Enumerable.Range(0, _fansCache.Count));
            NotifyDataSetChanged();
        }

        public void DeselectAll()
        {
            SelectedItems.Clear();
            NotifyDataSetChanged();
        }

        public User[] GetSelected()
        {
            return SelectedItems.Select(pos => _fansCache[pos]).ToArray();
        }

        private readonly Account _data;
        private readonly List<User> _fansCache;

        private readonly IFanItemClickListener _listener;
    }
}