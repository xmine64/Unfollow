using System;
using Android.Content;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Fragments
{
    internal class AboutFragment : FragmentBase
    {
        public AboutFragment() :
            base(Resource.Layout.fragment_about)
        {
            Create += AboutFragment_Create;
        }

        private void AboutFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            // Setup fragment
            Title = GetString(Resource.String.title_about);
            ActionBarVisible = false;

            // Find views
            var tvVersion = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_app_version);
            var tvLibVersion = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_instasharp_version);

            var btnTelegram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_telegram);
            var btnInstagram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_instagram);
            var btnGithub = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_github);

            if (tvVersion == null ||
                tvLibVersion == null ||
                btnTelegram == null ||
                btnInstagram == null ||
                btnGithub == null)
                return;

            // Show versions
            var versionProvider = (IVersionProvider) Activity;
            tvVersion.Text = GetString(Resource.String.msg_app_version, 
                versionProvider.GetAppVersionName());

            tvLibVersion.Text = GetString(
                Resource.String.msg_using_x_version_y,
                versionProvider.GetLibraryAssemblyName().Name,
                versionProvider.GetLibraryAssemblyName().Version.ToString()
            );

            // Add click handlers
            btnTelegram.Click += Telegram_Click;
            btnInstagram.Click += Instagram_Click;
            btnGithub.Click += Github_Click;
        }

        private void Telegram_Click(object sender, EventArgs e)
        {
            var url = Android.Net.Uri.Parse(
                GetString(Resource.String.url_telegram));
            ((ICustomTabProvider)Activity).LaunchBrowser(url);
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            ((IInstagramHost) Activity).OpenInInstagram(
                GetString(Resource.String.url_instagram));
        }

        private void Github_Click(object sender, EventArgs e)
        {
            var url = Android.Net.Uri.Parse(
                GetString(Resource.String.url_github));
            ((ICustomTabProvider)Activity).LaunchBrowser(url);
        }
    }
}