using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Madamin.Unfollow.Main
{
    public interface IDataStorage
    {
        string GetDataDir();
        string GetCacheDir();
        string GetAccountsDir();

        void SaveData(string fileName, object data);
        object LoadData(string fileName);
        bool DataExists(string fileName);
    }

    public partial class MainActivity : IDataStorage
    {
        private string _dataPath = null;
        private string _cachePath = null;

        private const string AccountsFolder = "accounts";

        string IDataStorage.GetDataDir()
        {
            if (_dataPath != null)
                return _dataPath;

            try
            {
                // Compatiblity with older versions of app
                if (DataDir != null &&
                    Directory.Exists(Path.Combine(DataDir.AbsolutePath, AccountsFolder)))
                {
                    _dataPath = DataDir.AbsolutePath;
                    return _dataPath;
                }
            }
            catch (Java.Lang.NoSuchMethodError)
            {
                // ignore
            }

            if (FilesDir == null)
                return null;
            
            _dataPath = FilesDir.AbsolutePath;

            return _dataPath;
        }

        string IDataStorage.GetCacheDir()
        {
            if (_cachePath != null)
                return _cachePath;
            
            if (CacheDir == null)
                return null;

            _cachePath = CacheDir.AbsolutePath;

            if (!Directory.Exists(_cachePath))
                Directory.CreateDirectory(_cachePath);

            return _cachePath;
        }

        string IDataStorage.GetAccountsDir()
        {
            var path = Path.Combine(((IDataStorage)this).GetDataDir(), AccountsFolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        void IDataStorage.SaveData(string fileName, object data)
        {
            var filePath = Path.Combine(((IDataStorage)this).GetDataDir(), fileName);
            using var file = new FileStream(
                filePath,
                FileMode.OpenOrCreate,
                FileAccess.Write);
            new BinaryFormatter().Serialize(file, data);
        }

        object IDataStorage.LoadData(string fileName)
        {
            var filePath = Path.Combine(((IDataStorage)this).GetDataDir(), fileName);
            using var file = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read);
            return new BinaryFormatter().Deserialize(file);
        }

        bool IDataStorage.DataExists(string fileName)
        {
            return File.Exists(Path.Combine(((IDataStorage)this).GetDataDir(), fileName));
        }
    }
}
