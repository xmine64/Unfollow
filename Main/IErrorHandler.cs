using System;
using Android.Content;
using AndroidX.AppCompat.App;
using Google.Android.Material.Dialog;

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
            ((ISnackBarProvider)this).ShowSnackBar(
                Resource.String.msg_error,
                Resource.String.button_text_details,
                view => { new ErrorReportDialog(this, this, exception).Show(); });
        }

        private class ErrorReportDialog
        {
            private readonly Exception _exception;
            private readonly AlertDialog _dialog;
            private readonly IUpdateChecker _updateServer;

            public ErrorReportDialog(Context context, IUpdateChecker server, Exception exception)
            {
                _exception = exception;
                _updateServer = server;

                var builder = new MaterialAlertDialogBuilder(context);
                builder.SetTitle(Resource.String.title_error);
#if DEBUG
                builder.SetMessage(exception.ToString());
#else
                builder.SetMessage(exception.Message);
#endif
                builder.SetPositiveButton(Resource.String.button_text_report, OnPositiveButtonClick);
                builder.SetNegativeButton(Android.Resource.String.Cancel, (sender, args) => { });
                _dialog = builder.Create();
            }

            public void Show()
            {
                _dialog.Show();
            }

            private void OnPositiveButtonClick(object sender, DialogClickEventArgs args)
            {
                try
                {
                    ((ISnackBarProvider)this).ShowSnackBar(Resource.String.msg_sending_report);
                    _updateServer.ReportBug(_exception);
                    ((ISnackBarProvider)this).ShowSnackBar(Resource.String.msg_report_sent);
                }
                catch (Exception ex)
                {
                    ((IErrorHandler)this).ShowError(ex);
                }
            }
        }
    }
}
