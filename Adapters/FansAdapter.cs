using System.Linq;
using System.Collections.Generic;

using Android.Views;

using AndroidX.RecyclerView.Widget;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    class FansAdapter : RecyclerView.Adapter
    {
        public FansAdapter(
            Account data,
            IFanItemClickListener listener)
        {
            _data = data;
            _listener = listener;
            _fans_cache = new List<User>();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var fan_view_holder = holder as FanViewHolder;
            if (fan_view_holder == null)
                return;

            fan_view_holder.BindData(
                _fans_cache[position],
                SelectedItems.Contains(position));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_user, parent, false);
            return new FanViewHolder(view_item, _listener);
        }

        public User GetItem(int position)
        {
            return _fans_cache[position];
        }

        public void Refresh()
        {
            _fans_cache.Clear();
            _fans_cache.AddRange(_data.Data.Fans.Except(Whitelist));
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
            SelectedItems.AddRange(Enumerable.Range(0, _fans_cache.Count));
            NotifyDataSetChanged();
        }

        public void DeselectAll()
        {
            SelectedItems.Clear();
            NotifyDataSetChanged();
        }

        public User[] GetSelected()
        {
            return SelectedItems.Select(pos => _fans_cache[pos]).ToArray();
        }

        public override int ItemCount => _fans_cache.Count;

        public List<int> SelectedItems { get; private set; }

        public List<User> Whitelist { get; } = new List<User>();

        private Account _data;
        private List<User> _fans_cache;

        private IFanItemClickListener _listener;
    }
}