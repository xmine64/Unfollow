using Android.Net;
using Android.Content;
using AndroidX.Browser.CustomTabs;

namespace Madamin.Unfollow.Main
{
    public interface IUrlHandler
    {
        void LaunchBrowser(Uri uri);
        void LaunchInstagram(Uri uri);
        void LaunchBrowser(string url);
        void LaunchInstagram(string userName);
    }

    public partial class MainActivity : IUrlHandler
    {
        private const string InstagramUserUrlTemplate = "https://instagram.com/_u/{0}";
        private const string InstagramPackageName = "com.instagram.android";

        public static Uri ParseUri(string url)
        {
            return Uri.Parse(url);
        }

        public static Uri GetInstagramUriForUser(string username)
        {
            return ParseUri(string.Format(InstagramUserUrlTemplate, username));
        }

        void IUrlHandler.LaunchBrowser(Uri uri)
        {
            var builder = new CustomTabsIntent.Builder();
            builder.SetShowTitle(true);
            builder.SetUrlBarHidingEnabled(true);
            var customTabs = builder.Build();
            customTabs.LaunchUrl(this, uri);
        }

        void IUrlHandler.LaunchInstagram(Uri uri)
        {
            var intent = new Intent(Intent.ActionView, uri);
            intent?.SetPackage(InstagramPackageName);
            try
            {
                StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                ((IUrlHandler) this).LaunchBrowser(uri);
            }
            catch (System.Exception ex)
            {
                ((IErrorHandler) this).ShowError(ex);
            }
        }

        void IUrlHandler.LaunchBrowser(string url)
        {
            ((IUrlHandler) this).LaunchBrowser(ParseUri(url));
        }

        void IUrlHandler.LaunchInstagram(string userName)
        {
            ((IUrlHandler) this).LaunchInstagram(GetInstagramUriForUser(userName));
        }
    }
}
