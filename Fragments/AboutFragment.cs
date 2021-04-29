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
        private TextView versionTextView, libVersionTextView;

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

            // Show versions
            var versionProvider = (IVersionProvider)Activity;
            versionTextView.Text = GetString(Resource.String.msg_app_version,
                versionProvider.GetAppVersionName(),
                versionProvider.GetAppVersionCode());

            libVersionTextView.Text = GetString(
                Resource.String.msg_using_x_version_y,
                versionProvider.GetLibraryAssemblyName().Name,
                versionProvider.GetLibraryAssemblyName().Version.ToString());
        }
    }
}