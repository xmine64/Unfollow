using Android.Content;

namespace Madamin.Unfollow.Main
{
    public interface IPreferenceContainer
    {
        string GetString(string key, string defaultValue);
        bool GetBoolean(string key, bool defaultValue);
        void SetString(string key, string value);
        void SetBoolean(string key, bool value);
    }

    public partial class MainActivity : IPreferenceContainer
    {
        private ISharedPreferences _preferences;

        string IPreferenceContainer.GetString(string key, string defaultValue)
        {
            return _preferences.GetString(key, defaultValue) ?? defaultValue;
        }

        bool IPreferenceContainer.GetBoolean(string key, bool defaultValue)
        {
            return _preferences.GetBoolean(key, defaultValue);
        }

        void IPreferenceContainer.SetString(string key, string value)
        {
            _preferences.Edit()?.PutString(key, value)?.Apply();
        }

        void IPreferenceContainer.SetBoolean(string key, bool value)
        {
            _preferences.Edit()?.PutBoolean(key, value)?.Apply();
        }
    }
}
