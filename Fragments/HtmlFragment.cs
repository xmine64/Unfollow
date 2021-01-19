using System;
using System.IO;
using System.Net.Http;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Android.Webkit;
using AndroidX.Preference;
using Java.Util;

namespace Madamin.Unfollow.Fragments
{
    internal class HtmlFragment : Fragment
    {
        public interface IHtmlSource
        {
            string GetHtml();
        }

        public class HtmlAsset : IHtmlSource
        {
            public HtmlAsset(Context context, string path)
            {
                _context = context;
                _path = path;
            }

            public string GetHtml()
            {
                System.Diagnostics.Debug.Assert(_context.Assets != null);
                using var asset = new StreamReader(
                    _context.Assets.Open(_path));
                return asset.ReadToEnd();
            }

            private readonly Context _context;
            private readonly string _path;
        }

        public class HttpHtml : IHtmlSource
        {
            public HttpHtml(Uri url)
            {
                _url = url;
            }

            public string GetHtml()
            {
                var client = new HttpClient();
                var response = client.GetAsync(_url).Result;
                return response.Content.ReadAsStringAsync().Result;
            }

            private readonly Uri _url;
        }

        public HtmlFragment(IHtmlSource source)
        {
            _source = source;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ((IFragmentHost) Activity).ActionBarTitle = GetString(Resource.String.title_terms);

            var view = new WebView(Context);
            view.LoadData(_source.GetHtml(), "text/html", "utf-8");

            return view;
        }

        private readonly IHtmlSource _source;

        public static HtmlFragment NewTermsFragment(Context context)
        {
            return new HtmlFragment(
                new HtmlAsset(
                    context,
                    context.GetString(Resource.String.url_terms)));
        }

        public static HtmlFragment NewDonateFragment(Context context)
        {
            return new HtmlFragment(
                new HttpHtml(
                    new Uri(context.GetString(Resource.String.url_donate))));
        }
    }
}