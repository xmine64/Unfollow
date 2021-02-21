using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Main
{
    public interface IFragmentContainer
    {
        void NavigateTo(Fragment fragment, bool addToBackStack);
        void PushFragment(Fragment fragment);
        void PopFragment();
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
        }
    }
}
