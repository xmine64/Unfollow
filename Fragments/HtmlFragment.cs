using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Fragments
{
    internal class HtmlFragment : Fragment
    {
        private readonly string _path;
        private readonly HtmlSource _source;
        private WebView _webView;

        private HtmlFragment(string path, HtmlSource source)
        {
            _path = path;
            _source = source;
        }

        private void LoadAsset()
        {
            System.Diagnostics.Debug.Assert(Context.Assets != null);
            using var asset = new StreamReader(
                Context.Assets.Open(_path));
            _webView.LoadData(asset.ReadToEnd(), "text/html", "utf-8");
        }

        private void LoadUrl()
        {
            _webView.LoadUrl(_path);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ((IFragmentHost) Activity).ActionBarTitle = GetString(Resource.String.title_terms);

            _webView = new WebView(Context);

            if (Resources.Configuration?.IsNightModeActive ?? false)
            {
                _webView.Settings.ForceDark = ForceDarkMode.On;
            }

            var sources = new Dictionary<HtmlSource, Action>()
            {
                {HtmlSource.Assets, LoadAsset},
                {HtmlSource.Url, LoadUrl}
            };

            sources[_source]();
            
            return _webView;
        }

        public static HtmlFragment NewTermsFragment(Context context)
        {
            return new HtmlFragment(
                context.GetString(Resource.String.url_terms),
                HtmlSource.Assets);
        }

        public static HtmlFragment NewDonateFragment(Context context)
        {
            return new HtmlFragment(
                context.GetString(Resource.String.url_donate),
                HtmlSource.Url);
        }
    }

    internal enum HtmlSource
    {
        Assets,
        Url
    }
}
