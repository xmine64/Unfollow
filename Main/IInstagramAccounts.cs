using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Main
{
    public interface IInstagramAccounts
    {
        Accounts Accounts { get; }
    }

    public partial class MainActivity : IInstagramAccounts
    {
        private Accounts _accounts;
        Accounts IInstagramAccounts.Accounts => _accounts;
    }
}