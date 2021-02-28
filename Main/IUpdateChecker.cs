using System;
using Google.Android.Material.Dialog;

namespace Madamin.Unfollow.Main
{
    public interface IUpdateChecker
    {
        void CheckForUpdate();

        void ReportBug(Exception exception);

#if TGBUILD || DEBUG
        void DidLogin();
#endif
    }

    public partial class MainActivity : IUpdateChecker
    {
        private UpdateServerApi _updateServer = new UpdateServerApi();

        async void IUpdateChecker.CheckForUpdate()
        {
            try
            {
                // Request update information
#if TGBUILD || DEBUG
                var lang = Resources?.Configuration?.Locales.Get(0)?.Language ??
                           UpdateServerApi.LanguageEnglish;
#else
                var lang = UpdateServerApi.LanguageGithubChannel;
#endif
                var result = await _updateServer.CheckUpdate(
                    ((IVersionProvider)this).GetAppVersionCode(), lang);

                // Check response
                if (result.Status != UpdateServerApi.StatusOk)
                    throw new Exception(result.Message);

                // Stop if update is not available
                if (!result.Available)
                {
                    ((ISnackBarProvider)this).ShowSnackBar(Resource.String.msg_up_to_date);
                }

                // Show an update dialog
                var dialog = new MaterialAlertDialogBuilder(this);
                dialog.SetTitle(Resource.String.title_update_available);
                dialog.SetMessage(result.Update.Message);

                dialog.SetPositiveButton(
                    result.Update.ButtonLabel,
                    (sender, args) =>
                    {
                        var url = Android.Net.Uri.Parse(result.Update.ButtonUrl);
                        ((IUrlHandler)this).LaunchBrowser(url);
                    });

                dialog.SetNegativeButton(
                    Android.Resource.String.Cancel,
                    (sender, args) => { });

                dialog.Show();
            }
            catch (Exception exception)
            {
                ((IErrorHandler)this).ShowError(exception);
            }
        }

        async void IUpdateChecker.ReportBug(Exception exception)
        {
            var result = await _updateServer.BugReport(exception);
            if (result.Status != UpdateServerApi.StatusOk)
                throw new Exception(result.Message);
        }

#if TGBUILD || DEBUG
        async void IUpdateChecker.DidLogin()
        {
            await _updateServer.DidLogin(((IVersionProvider)this).GetAppVersionCode());
        }
#endif
    }
}
