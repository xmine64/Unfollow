using System;
using System.Diagnostics;
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
            Title = GetString(Resource.String.title_about);

            Debug.Assert(
                Activity?.PackageManager != null &&
                Activity.PackageName != null
            );

            var package = Activity.PackageManager.GetPackageInfo(
                Activity.PackageName,
                0);

            Debug.Assert(package != null);

            var igVersion = typeof(InstagramApiSharp.API.IInstaApi)
                .Assembly.GetName();

            var tvVersion = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_app_version);
            var tvIgShVersion = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_instasharp_version);

            var btnTelegram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_telegram);
            var btnInstagram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_instagram);
            var btnGithub = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_github);

            Debug.Assert(
                tvVersion != null &&
                tvIgShVersion != null &&
                btnTelegram != null &&
                btnInstagram != null &&
                btnGithub != null
            );

            tvVersion.Text = string.Format(
                GetString(Resource.String.msg_app_version),
                package.VersionName
            );

            tvIgShVersion.Text = string.Format(
                GetString(Resource.String.msg_instasharp_version),
                igVersion.Name,
                igVersion.Version
            );

            btnTelegram.Click += Telegram_Click;
            btnInstagram.Click += Instagram_Click;
            btnGithub.Click += Github_Click;
        }

        private void Telegram_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = Intent.ParseUri(
                    "https://t.me/unfollowapp",
                    IntentUriType.AndroidAppScheme);
                Activity.StartActivity(intent);
            }
            catch
            {
                // ignore
            }
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            ((IInstagramHost) Activity).OpenInInstagram("minimalunfollowapp");
        }

        private void Github_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = Intent.ParseUri(
                    "https://github.com/mmdmine/unfollow",
                    IntentUriType.AndroidAppScheme);
                Activity.StartActivity(intent);
            }
            catch
            {
                // ignore
            }
        }
    }
}