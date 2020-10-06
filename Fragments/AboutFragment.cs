using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;

namespace Madamin.Unfollow.Fragments
{
    class AboutFragment : FragmentBase
    {
        public AboutFragment() : 
            base(Resource.Layout.fragment_about)
        {
            Create += AboutFragment_Create;
        }

        private void AboutFragment_Create(object sender, OnCreateEventArgs e)
        {
            Title = GetString(Resource.String.title_about);
            e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_about_version)
                .Text = string.Format(
                    GetString(Resource.String.msg_version),
                    Assembly.GetExecutingAssembly().GetName().Version);
            e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_telegram)
                .Click += Telegram_Click;
            e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_instagram)
                .Click += Instagram_Click;
            e.View.FindViewById<MaterialButton>(Resource.Id.fragment_about_github)
                .Click += Github_Click;
        }

        private void Telegram_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = Intent.ParseUri("https://t.me/unfollowapp", IntentUriType.AndroidAppScheme);
                Activity.StartActivity(intent);
            }
            catch
            {
                // ignore
            }
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = Intent.ParseUri("https://instagram.com/_u/minimalunfollowapp", IntentUriType.AndroidAppScheme);
                Activity.StartActivity(intent);
            }
            catch
            {
                // ignore
            }
        }

        private void Github_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = Intent.ParseUri("https://github.com/mmdmine/unfollow", IntentUriType.AndroidAppScheme);
                Activity.StartActivity(intent);
            }
            catch
            {
                // ignore
            }
        }
    }
}