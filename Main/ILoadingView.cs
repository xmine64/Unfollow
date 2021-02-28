using Android.Views;
using Android.Widget;

namespace Madamin.Unfollow.Main
{
    public interface ILoadingView
    {
        void SetLoadingText(string text);

        void ResetLoadingText();
    }

    public partial class MainActivity : ILoadingView
    {
        private View _loadingView;
        private TextView _loadingTextView;

        void ILoadingView.SetLoadingText(string text)
        {
            _loadingTextView.Text = text;
        }

        void ILoadingView.ResetLoadingText()
        {
            _loadingTextView.SetText(Resource.String.title_loading);
        }
    }
}