using Android.Views;
using Android.Widget;

namespace Madamin.Unfollow.Main
{
    public interface IEmptyView
    {
        void SetEmptyText(int resid);
        void ResetEmptyText();
        void SetEmptyImage(int resid);
    }

    public partial class MainActivity : IEmptyView
    {
        private View _emptyView;
        private TextView _emptyTextView;
        private ImageView _emptyImageView;

        void IEmptyView.SetEmptyText(int resid)
        {
            _emptyTextView.SetText(resid);
        }

        void IEmptyView.ResetEmptyText()
        {
            _emptyTextView.SetText(Resource.String.title_empty);
        }

        void IEmptyView.SetEmptyImage(int resid)
        {
            _emptyImageView.SetImageResource(resid);
        }
    }
}