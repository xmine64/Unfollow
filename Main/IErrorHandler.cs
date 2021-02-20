using System;
using Google.Android.Material.Dialog;
using Google.Android.Material.Snackbar;

namespace Madamin.Unfollow.Main
{
    public interface IErrorHandler
    {
        void ShowError(Exception ex);
    }

    public partial class MainActivity : IErrorHandler
    {
        void IErrorHandler.ShowError(Exception exception)
        {
            var container = FindViewById(Resource.Id.main_container);

            var snack = Snackbar.Make(
                container, 
                Resource.String.msg_error, 
                Snackbar.LengthLong);
            snack.SetAnchorView(Resource.Id.main_navbar);

            snack.SetAction(Resource.String.button_text_details, view =>
            {
                // Show error details in an alert dialog
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.title_error);
                dialog.SetMessage(exception.ToString());

                // Send bug report
                dialog.SetPositiveButton(Resource.String.button_text_report, async (sender, args) =>
                {
                    try
                    {
                        ((ISnackBarProvider) this).ShowSnackbar(Resource.String.msg_sending_report);

                        var response = await _updateServer.BugReport(exception);

                        if (response.Status == UpdateServerApi.StatusOk)
                        {
                            ((ISnackBarProvider) this).ShowSnackbar(Resource.String.msg_report_sent);
                        }
                        else
                        {
                            throw new Exception(response.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        ((IErrorHandler) this).ShowError(ex);
                    }
                });
                
                dialog.SetNegativeButton(
                    Android.Resource.String.Cancel,
                    (sender, args) => {});

                dialog.Show();
            });
            
            //if (navbar.Visibility == ViewStates.Visible)
            //    snack.SetAnchorView(navbar);

            snack.Show();
        }
    }
}
