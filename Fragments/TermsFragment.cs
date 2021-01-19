using System.IO;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Android.Webkit;
using AndroidX.Preference;

namespace Madamin.Unfollow.Fragments
{
    internal class TermsFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = new WebView(Context);

            ((IFragmentHost) Activity).ActionBarTitle = 
                GetString(Resource.String.title_terms);

            var prefs = PreferenceManager.GetDefaultSharedPreferences(Context);
            var lang = prefs.GetString("lang", "en");

            if (lang == "sysdef")
            {
                lang = Resources.Configuration.Locales.Get(0).Language;
            }

            var asset = Activity.Assets.Open($"terms_{lang}.html");

            using (var sr = new StreamReader(asset))
            {
                view.LoadData(sr.ReadToEnd(), "text/html", "utf-8");
            }
            
            return view;
        }
    }
}