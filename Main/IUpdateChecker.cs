using System;
using System.Diagnostics;
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
        void IUpdateChecker.CheckForUpdate()
        {
            Debug.WriteLine("MainActivity.IUpdateChecker.CheckForUpdate() called.");
        }

        void IUpdateChecker.ReportBug(Exception exception)
        {
            Debug.WriteLine("MainActivity.IUpdateChecker.ReportBug() called.");
            Debug.WriteLine(exception);
        }

#if TGBUILD || DEBUG
        void IUpdateChecker.DidLogin()
        {
            Debug.WriteLine("MainActivity.IUpdateChecker.DidLogin() called.");
        }
#endif
    }
}
