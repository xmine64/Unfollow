using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AndroidX.Fragment.App;

namespace Madamin.Unfollow.Fragments
{
    internal class HtmlFragment : Fragment
    {
        private const string HtmlMimeType = "text/html";

        private readonly string _title, _path;
        private readonly HtmlSource _source;
        private WebView _webView;

        public HtmlFragment(string title, string path, HtmlSource source)
        {
            _title = title;
            _path = path;
            _source = source;
        }

        private void LoadAsset()
        {
            if (Context.Assets == null)
                return;
            using var asset = new StreamReader(Context.Assets.Open(_path));
            _webView.LoadData(asset.ReadToEnd(), HtmlMimeType, Encoding.UTF8.WebName);
        }

        private void LoadUrl()
        {
            _webView.LoadUrl(_path);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Fragment setup
            var host = (IFragmentHost) Activity;
            host.ActionBarTitle = _title;
            host.ActionBarVisible = true;

            _webView = new WebView(Context);

            // Enable dark mode
            if (Resources.Configuration?.IsNightModeActive ?? false)
            {
                _webView.Settings.ForceDark = ForceDarkMode.On;
            }

            var sources = new Dictionary<HtmlSource, Action>
            {
                {HtmlSource.Assets, LoadAsset},
                {HtmlSource.Url, LoadUrl}
            };

            sources[_source]();
            
            return _webView;
        }

        internal enum HtmlSource
        {
            Assets,
            Url
        }
    }
}
