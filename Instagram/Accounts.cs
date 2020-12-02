using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Madamin.Unfollow.Instagram
{
    public class Accounts : IEnumerable<Account>
    {

        public Accounts(string statePath, string cachePath)
        {
            DataDir = statePath;
            CacheDir = cachePath;
            IsStateRestored = false;

            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            if (!Directory.Exists(CacheDir))
                Directory.CreateDirectory(CacheDir);
        }

        private string DataDir { get; }
        private string CacheDir { get; }

        public bool IsStateRestored { get; private set; }

        public async Task AddAccountAsync(string username, string password)
        {
            var account = new Account();
            await account.LoginAsync(username, password);
            await RefreshAccountAsync(account);
            SaveAccountState(account);
            _accounts.Add(account);
        }

        public async Task CompleteLoginAsync(Account account, string code)
        {
            await account.CompleteLoginAsync(code);
            await RefreshAccountAsync(account);
            SaveAccountState(account);
            _accounts.Add(account);
        }

        public async Task LogoutAccountAsync(Account account)
        {
            await LogoutAccountAtAsync(_accounts.FindIndex(a => Equals(a, account)));
        }

        public async Task LogoutAccountAtAsync(int i)
        {
            var statePath = GetAccountStatePath(_accounts[i]);

            await _accounts[i].LogoutAsync();

            _accounts.RemoveAt(i);
            File.Delete(statePath);
        }

        public async Task RefreshAllAsync()
        {
            foreach (var account in _accounts) await RefreshAccountAsync(account);
        }

        public async Task RestoreStateAsync()
        {
            if (IsStateRestored)
                throw new AlreadyRestoredException();

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

                if (_accounts.Contains(account))
                    throw new DuplicateAccountException();

                _accounts.Add(account);
            }

            IsStateRestored = true;
        }

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

        public void SaveAccountCache(Account account)
        {
            account.SaveCache(GetAccountCachePath(account));
        }

        private async Task RefreshAccountAsync(Account account)
        {
            await account.RefreshAsync();
            SaveAccountCache(account);
        }

        private readonly List<Account> _accounts = new List<Account>();

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
    }

    internal interface IInstagramHost
    {
        Accounts Accounts { get; }

        void OpenInInstagram(string username);
    }

    public class AlreadyRestoredException : Exception
    {
    }

    public class DuplicateAccountException : Exception
    {
    }
}