using System.IO;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Android.Webkit;
using AndroidX.Preference;
using Java.Util;

namespace Madamin.Unfollow.Fragments
{
    internal class TermsFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = new WebView(Context);

            // Get preferred language
            var prefs = PreferenceManager.GetDefaultSharedPreferences(Context);
            var lang = prefs.GetString("lang", "en");

            // Set title
            ((IFragmentHost) Activity).ActionBarTitle = GetString(Resource.String.title_terms);

            // If system default is preferred, then Get system language
            if (lang == "sysdef")
            {
                System.Diagnostics.Debug.Assert(Locale.Category.Display != null);
                lang = Locale.GetDefault(Locale.Category.Display).Language;
            }

            System.Diagnostics.Debug.Assert(Activity.Assets != null);
            
            // Load HTML asset
            using var sr = new StreamReader(
                Activity.Assets.Open($"terms_{lang}.html"));

            // Load content
            view.LoadData(sr.ReadToEnd(), "text/html", "utf-8");

            return view;
        }
    }
}