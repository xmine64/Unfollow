using System;
using System.Reflection;

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

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            var tvVersion = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_version);
            var btnTelegram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_telegram);
            var btnInstagram = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_instagram);
            var btnGithub = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_github);

            if (tvVersion == null ||
                btnTelegram == null ||
                btnInstagram == null ||
                btnGithub == null)
                return;

            tvVersion.Text = string.Format(GetString(Resource.String.msg_version), version);
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