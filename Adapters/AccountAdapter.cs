using Android.Views;
using AndroidX.RecyclerView.Widget;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    public class AccountAdapter : RecyclerView.Adapter
    {
        public AccountAdapter(
            Accounts data,
            IAccountItemClickListener listener)
        {
            _data = data;
            _listener = listener;
        }

        public override int ItemCount => _data.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is AccountViewHolder accountViewHolder)
                accountViewHolder.BindData(_data[position].Data);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var viewItem = LayoutInflater.From(parent.Context)?.Inflate(
                Resource.Layout.item_account,
                parent,
                false);
            return new AccountViewHolder(viewItem, _listener);
        }

        public Account GetItem(int position)
        {
            return _data[position];
        }

        private readonly Accounts _data;
        private readonly IAccountItemClickListener _listener;
    }

    public interface IAccountItemClickListener
    {
        void OnItemOpenInstagram(int position);
        void OnItemOpenUnfollowers(int position);
        void OnItemOpenFans(int position);
        void OnItemLogout(int position);
        void OnItemRefresh(int position);
    }
}