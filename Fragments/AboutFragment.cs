using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    internal class AboutFragment : Fragment
    {
        private const string TelegramChannelUrl = "https://t.me/unfollowapp";
        private const string InstagramPageName = "minimalunfollowapp";
        private const string GithubRepoUrl = "https://github.com/mmdmine/unfollow";

        private TextView versionTextView, libVersionTextView;
        private Button telegramButton, instagramButton, githubButton;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_about, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_about);
            ((IActionBarContainer)Activity).Hide();

            versionTextView = view.FindViewById<TextView>(Resource.Id.fragment_about_app_version);
            libVersionTextView = view.FindViewById<TextView>(Resource.Id.fragment_about_instasharp_version);

            telegramButton = view.FindViewById<Button>(Resource.Id.fragment_about_telegram);
            instagramButton = view.FindViewById<Button>(Resource.Id.fragment_about_instagram);
            githubButton = view.FindViewById<Button>(Resource.Id.fragment_about_github);

            // Show versions
            var versionProvider = (IVersionProvider)Activity;
            versionTextView.Text = GetString(Resource.String.msg_app_version,
                versionProvider.GetAppVersionName(),
                versionProvider.GetAppVersionCode());

            libVersionTextView.Text = GetString(
                Resource.String.msg_using_x_version_y,
                versionProvider.GetLibraryAssemblyName().Name,
                versionProvider.GetLibraryAssemblyName().Version.ToString());

            telegramButton.Click += Telegram_Click;
            instagramButton.Click += Instagram_Click;
            githubButton.Click += Github_Click;
        }

        private void Telegram_Click(object sender, EventArgs e)
        {
            ((IUrlHandler)Activity).LaunchBrowser(TelegramChannelUrl);
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            ((IUrlHandler)Activity).LaunchInstagram(InstagramPageName);
        }

        private void Github_Click(object sender, EventArgs e)
        {
            ((IUrlHandler)Activity).LaunchBrowser(GithubRepoUrl);
        }
    }
}