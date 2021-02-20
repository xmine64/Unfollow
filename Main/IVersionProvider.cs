using System.Reflection;
using AndroidX.Core.Content.PM;

namespace Madamin.Unfollow.Main
{
    public interface IVersionProvider
    {
        long GetAppVersionCode();
        string GetAppVersionName();
        AssemblyName GetAppAssemblyName();
        AssemblyName GetLibraryAssemblyName();
    }

    public partial class MainActivity : IVersionProvider
    {
        private Android.Content.PM.PackageInfo _currentPackage;

        public long GetAppVersionCode()
        {
            #if DEBUG
            System.Diagnostics.Debug.Assert(_currentPackage != null);
            #else
            if (_currentPackage == null)
                return -1;
            #endif
            
            var longVersionCode = PackageInfoCompat.GetLongVersionCode(_currentPackage);

            #if DEBUG
            System.Diagnostics.Debug.Assert(longVersionCode != -1);

            return -1;
            #else

            return longVersionCode;
            #endif
        }

        public string GetAppVersionName()
        {
            if (_currentPackage == null)
                return string.Empty;
            return _currentPackage.VersionName;
        }

        public AssemblyName GetAppAssemblyName()
        {
            return GetType().Assembly.GetName();
        }

        public AssemblyName GetLibraryAssemblyName()
        {
            return typeof(InstagramApiSharp.API.IInstaApi).Assembly.GetName();
        }
    }
}
