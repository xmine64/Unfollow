using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Madamin.Unfollow.Instagram
{
    public class Accounts : IEnumerable<Account>
    {
        public async Task AddAccountAsync(string username, string password)
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            var account = new Account();
            await account.LoginAsync(username, password);
            await RefreshAccountAsync(account);
            SaveAccountState(account);
            _accounts.Add(account);
        }

        public async Task LogoutAccountAtAsync(int i)
        {
            var state_path = GetAccountStatePath(_accounts[i]);

            await _accounts[i].LogoutAsync();
            
            _accounts.RemoveAt(i);
            File.Delete(state_path);
        }

        public async Task RefreshAllAsync()
        {
            foreach (var account in _accounts)
            {
                await RefreshAccountAsync(account);
            }
        }

        public async Task RestoreStateAsync()
        {
            if (Directory.Exists(DataDir))
            {
                foreach (var file in Directory.GetFiles(DataDir))
                {
                    var account = new Account();
                    account.LoadState(file);

                    // can't use GetAccountCachePath() because account.Data is null
                    var cache = Path.Combine(CacheDir, Path.GetFileName(file));

                    if (File.Exists(cache))
                    {
                        account.LoadCache(cache);
                    }
                    else
                    {
                        await RefreshAccountAsync(account);
                    }

                    _accounts.Add(account);
                }
            }
        }

        public void SaveCacheAll()
        {
            foreach (var account in _accounts)
            {
                SaveAccountCache(account);
            }
        }

        public string DataDir { get; set; }
        public string CacheDir { get; set; }

        private string GetAccountStatePath(Account account)
        {
            return Path.Combine(DataDir, account.Data.User.Id.ToString());
        }

        private string GetAccountCachePath(Account account)
        {
            return Path.Combine(CacheDir, account.Data.User.Id.ToString());
        }

        private void SaveAccountState(Account account)
        {
            account.SaveState(GetAccountStatePath(account));
        }

        private void SaveAccountCache(Account account)
        {
            account.SaveCache(GetAccountCachePath(account));
        }

        private async Task RefreshAccountAsync(Account account)
        {
            await account.RefreshAsync();
            SaveAccountCache(account);
        }

        #region IEnumerable implementation
        public int Count => _accounts.Count;
        public Account this[int i] => _accounts[i];
        public IEnumerator<Account> GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }
        #endregion

        private List<Account> _accounts = new List<Account>();
    }

    interface IInstagramHost
    {
        Accounts Accounts { get; }
    }
}
