using System;
using Android.Views;
using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Main
{
    public interface IFragmentContainer
    {
        void NavigateTo(Fragment fragment, bool addToBackStack);
        void PushFragment(Fragment fragment);
        void PopFragment();

        void ShowLoadingView();
        void ShowEmptyView();
        void ShowErrorView();
        void ShowErrorView(Exception exception);
        void ShowContentView();
    }

    public partial class MainActivity : IFragmentContainer
    {
        void IFragmentContainer.NavigateTo(Fragment fragment, bool addToBackStack)
        {
            // Show actionbar again, if last fragment hid it
            SupportActionBar.Show();

            // Show animations
            BeginTransition();

            // Replace content of main container with new fragment
            var tx = SupportFragmentManager.BeginTransaction();
            tx.Replace(Resource.Id.main_container, fragment);
            if (addToBackStack)
                tx.AddToBackStack(null);
            tx.Commit();

            if (fragment is IRetryHandler handler)
            {
                _retryHandler = handler;
            }
            else
            {
                _retryHandler = this;
            }

            ((IFragmentContainer)this).ShowContentView();
        }

        void IFragmentContainer.PushFragment(Fragment fragment)
        {
            ((IFragmentContainer)this).NavigateTo(fragment, true);
        }

        void IFragmentContainer.PopFragment()
        {
            SupportActionBar.Show();
            BeginTransition();
            SupportFragmentManager.PopBackStack();

            ((IFragmentContainer)this).ShowContentView();
        }

        void IFragmentContainer.ShowContentView()
        {
            _mainContainer.Visibility = ViewStates.Visible;
            _loadingView.Visibility = ViewStates.Gone;
            _emptyView.Visibility = ViewStates.Gone;
            _errorView.Visibility = ViewStates.Gone;
        }

        void IFragmentContainer.ShowLoadingView()
        {
            _loadingView.Visibility = ViewStates.Visible;
            _mainContainer.Visibility = ViewStates.Gone;
            _emptyView.Visibility = ViewStates.Gone;
            _errorView.Visibility = ViewStates.Gone;
        }

        void IFragmentContainer.ShowEmptyView()
        {
            _emptyView.Visibility = ViewStates.Visible;
            _mainContainer.Visibility = ViewStates.Gone;
            _loadingView.Visibility = ViewStates.Gone;
            _errorView.Visibility = ViewStates.Gone;
        }

        void IFragmentContainer.ShowErrorView()
        {
            _errorView.Visibility = ViewStates.Visible;
            _mainContainer.Visibility = ViewStates.Gone;
            _loadingView.Visibility = ViewStates.Gone;
            _emptyView.Visibility = ViewStates.Gone;
        }

        void IFragmentContainer.ShowErrorView(Exception exception)
        {
            ((IFragmentContainer)this).ShowErrorView();
            ((IErrorHandler)this).ShowError(exception);
        }
    }
}
