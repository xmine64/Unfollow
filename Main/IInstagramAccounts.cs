using System.Threading.Tasks;
using Madamin.Unfollow.Adapters;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Main
{
    public interface IInstagramAccounts
    {
        void AddAccount(string userName, string password);
        Task InitializeIfNeededAsync();
        AccountAdapter CreateAccountAdapter(IAccountItemClickListener listener);
        Task RefreshAsync();
        Task LogoutAsync(int index);
        Task CompleteTwoFactorLoginAsync(Account account, string code);
        Task CompleteChallengeAsync(Account account, string code);
        Task SubmitPhoneNumberAsync(Account account, string phoneNumber);
        Account GetAccount(int index);
    }

    public partial class MainActivity : IInstagramAccounts
    {
        private Accounts _accounts;

        async void IInstagramAccounts.AddAccount(string userName, string password)
        {
            await _accounts.AddAccountAsync(userName, password);
        }

        Task IInstagramAccounts.InitializeIfNeededAsync()
        {
            if (!_accounts.IsStateRestored)
            {
                return _accounts.RestoreStateAsync();
            }
            else if (_accounts.NeedRefresh)
            {
                return _accounts.FixNeedRefresh();
            }
            return null;
        }

        AccountAdapter IInstagramAccounts.CreateAccountAdapter(IAccountItemClickListener listener)
        {
            return new AccountAdapter(_accounts, listener);
        }

        Task IInstagramAccounts.RefreshAsync()
        {
            return _accounts.RefreshAllAsync();
        }

        Task IInstagramAccounts.LogoutAsync(int index)
        {
            return _accounts.LogoutAccountAtAsync(index);
        }

        Task IInstagramAccounts.CompleteChallengeAsync(Account account, string code)
        {
            return _accounts.CompleteChallengeAsync(account, code);
        }

        Task IInstagramAccounts.SubmitPhoneNumberAsync(Account account, string phoneNumber)
        {
            return _accounts.CompleteSubmitPhoneChallengeAsync(account, phoneNumber);
        }

        Account IInstagramAccounts.GetAccount(int index)
        {
            return _accounts[index];
        }

        Task IInstagramAccounts.CompleteTwoFactorLoginAsync(Account account, string code)
        {
            return _accounts.CompleteLoginAsync(account, code);
        }
    }
}