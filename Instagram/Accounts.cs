using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Madamin.Unfollow.Instagram
{
    public class Accounts : IEnumerable<Account>
    {
        private string DataDir { get; }
        private string CacheDir { get; }

        public bool IsStateRestored { get; private set; }
        public bool NeedRefresh { get; private set; }

        public Accounts(string statePath, string cachePath)
        {
            DataDir = statePath;
            CacheDir = cachePath;
            IsStateRestored = false;
        }

        public async Task AddAccountAsync(string username, string password)
        {
            var account = new Account();
            await account.LoginAsync(username, password);
            AddAccount(account);
        }

        public async Task CompleteLoginAsync(Account account, string code)
        {
            await account.CompleteLoginAsync(code);
            AddAccount(account);
        }

        public async Task CompleteChallengeAsync(Account account, string code)
        {
            await account.CompleteChallengeAsync(code);
            AddAccount(account);
        }

        public async Task CompleteSubmitPhoneChallengeAsync(Account account, string phone)
        {
            await account.CompleteSubmitPhoneChallengeAsync(phone);
            AddAccount(account);
        }

        public async Task LogoutAccountAtAsync(int i)
        {
            var statePath = GetAccountStatePath(_accounts[i]);

            await _accounts[i].LogoutAsync();

            _accounts.RemoveAt(i);
            File.Delete(statePath);
        }

        public void RemoveAccount(Account account)
        {
            _accounts.Remove(account);
            File.Delete(GetAccountStatePath(account));
        }

        public async Task RefreshAllAsync()
        {
            foreach (var account in _accounts) await RefreshAccountAsync(account);
        }

        public async Task FixNeedRefresh()
        {
            foreach (var account in 
                from account in _accounts
                where account.Data == null
                select account)
            {
                await RefreshAccountAsync(account);
            }

            NeedRefresh = false;
        }

        public async Task RestoreStateAsync()
        {
            if (IsStateRestored)
                throw new AlreadyRestoredException();

            foreach (var file in Directory.GetFiles(DataDir))
            {
                var account = new Account();
                account.LoadState(file);

                var cache = GetAccountCachePath(account);

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

        private void AddAccount(Account account)
        {
            if (_accounts.Contains(account))
                throw new DuplicateAccountException();
            SaveAccountState(account);
            _accounts.Add(account);
            NeedRefresh = true;
        }

        private string GetAccountStatePath(Account account)
        {
            return Path.Combine(DataDir, account.GetPk().ToString());
        }

        private string GetAccountCachePath(Account account)
        {
            return Path.Combine(CacheDir, account.GetPk().ToString());
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

    public class AlreadyRestoredException : Exception
    {
    }

    public class DuplicateAccountException : Exception
    {
    }
}