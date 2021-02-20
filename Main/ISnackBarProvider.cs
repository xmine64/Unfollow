using Google.Android.Material.Snackbar;

namespace Madamin.Unfollow.Main
{
    public interface ISnackBarProvider
    {
        void ShowSnackbar(int res);
    }

    public partial class MainActivity : ISnackBarProvider
    {
        void ISnackBarProvider.ShowSnackbar(int res)
        {
            var container = FindViewById(Resource.Id.main_container);
            var snack = Snackbar.Make(container, res, Snackbar.LengthLong);
            //if (_navbar.Visibility == ViewStates.Visible)
            //snack.SetAnchorView(_navbar);
            snack.Show();
        }
    }
}
