using Android.Views;
using Android.Widget;

namespace Madamin.Unfollow.Main
{
    public interface IErrorView
    {
        void SetErrorText(string text);
        void ResetErrorText();
        void SetErrorIcon(int resid);
        void ResetErrorIcon();
    }

    public interface IRetryHandler
    {
        void OnClick();
    }

    public partial class MainActivity : IErrorView, IRetryHandler
    {
        private View _errorView;
        private TextView _errorTextView;
        private ImageView _errorImageView;
        private Button _retryButton;
        private IRetryHandler _retryHandler;

        void IErrorView.SetErrorText(string text)
        {
            _errorTextView.Text = text;
        }

        void IErrorView.ResetErrorText()
        {
            _errorTextView.SetText(Resource.String.error_recyclerview);
        }

        void IErrorView.SetErrorIcon(int resid)
        {
            _errorImageView.SetImageDrawable(GetDrawable(resid));
        }

        void IErrorView.ResetErrorIcon()
        {
            _errorImageView.SetImageResource(Resource.Drawable.ic_error_black_48dp);
        }

        void IRetryHandler.OnClick()
        {
            // TODO ?
        }
    }
}