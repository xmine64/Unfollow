using Android.Views;

using AndroidX.RecyclerView.Widget;

using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.ViewHolders;

namespace Madamin.Unfollow.Adapters
{
    class AccountAdapter : RecyclerView.Adapter
    {
        public AccountAdapter(
            Accounts data,
            IAccountItemClickListener listener)
        {
            _data = data;
            _listener = listener;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var account_view_holder = holder as AccountViewHolder;
            if (account_view_holder == null)
                return;

            account_view_holder.BindData(_data[position].Data);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view_item = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.item_account, parent, false);
            return new AccountViewHolder(view_item, _listener);
        }

        public Account GetItem(int position)
        {
            return _data[position];
        }

        public override int ItemCount => _data.Count;

        private Accounts _data;

        private IAccountItemClickListener _listener;
    }
}