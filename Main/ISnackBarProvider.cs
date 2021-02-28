using System;
using Android.Views;
using Google.Android.Material.Snackbar;

namespace Madamin.Unfollow.Main
{
    public interface ISnackBarProvider
    {
        void ShowSnackBar(int textResource);
        void ShowSnackBar(int textResource, params Java.Lang.Object[] format);
        void ShowSnackBar(int textResource, int buttonTextResource, Action<View> action);
    }

    public partial class MainActivity : ISnackBarProvider
    {
        void ISnackBarProvider.ShowSnackBar(int textResource)
        {
            var snack = Snackbar.Make(_mainContainer, textResource, Snackbar.LengthLong);
            snack.SetAnchorView(Resource.Id.main_navbar);
            snack.Show();
        }

        void ISnackBarProvider.ShowSnackBar(int textResource, params Java.Lang.Object[] format)
        {
            var snack = Snackbar.Make(_mainContainer, GetString(textResource, format), Snackbar.LengthLong);
            snack.SetAnchorView(Resource.Id.main_navbar);
            snack.Show();
        }

        void ISnackBarProvider.ShowSnackBar(int textResource, int buttonTextResource, Action<View> action)
        {
            var snack = Snackbar.Make(_mainContainer, textResource, Snackbar.LengthLong);
            snack.SetAnchorView(Resource.Id.main_navbar);
            snack.SetAction(buttonTextResource, action);
            snack.Show();
        }
    }
}
