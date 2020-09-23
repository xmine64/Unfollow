using AndroidX.Fragment.App;

namespace madamin.unfollow
{
    interface INavigationHost
    {
        void NavigateTo(Fragment fragment, bool add_to_back_stack);
    }
}