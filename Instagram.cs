using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using IInstaApi = InstagramApiSharp.API.IInstaApi;
using InstaApiBuilder = InstagramApiSharp.API.Builder.InstaApiBuilder;

namespace madamin.unfollow
{
    public class WrongPasswordException : Exception { }
    public class ChallengeException : Exception { }

    public class Instagram : IEnumerable<InstagramAccount>
    {
        public async Task AddAccount(string username, string password)
        {
            var account = new InstagramAccount(username);
            await account.Login(password);
            await account.Refresh();
            _accounts.Add(account);
        }

        public Task LogoutAccount(string username)
        {
            return LogoutAccountAt(_accounts.FindIndex(a => a.UserName == username));
        }

        public async Task LogoutAccountAt(int i)
        {
            await _accounts[i].Logout();
            _accounts.RemoveAt(i);
        }

        public async Task RefreshAll()
        {
            foreach (var account in _accounts)
            {
                await account.Refresh();
            }
        }

        public void SaveData()
        {

        }

        public void LoadData()
        {

        }

        public void SaveCache()
        {

        }

        public void LoadCache()
        {

        }

        public InstagramAccount this[int i] => _accounts[i];

        public IEnumerator<InstagramAccount> GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _accounts.GetEnumerator();
        }

        private List<InstagramAccount> _accounts = new List<InstagramAccount>();

        public string DataDir { get; set; }
        public string CacheDir { get; set; }
        public int Count => _accounts.Count;
    }

    public class InstagramAccount
    {
        public InstagramAccount(string username)
        {
            UserName = username;
            _api = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();
            _api.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version126);
        }

        internal async Task Login(string password)
        {
            if (_api.IsUserAuthenticated) return;

            _api.SetUser(UserName, password);

            await _api.SendRequestsBeforeLoginAsync();
            await Task.Delay(5000);
            var result = await _api.LoginAsync();
            switch (result.Value)
            {
                case InstaLoginResult.Success:
                    await _api.SendRequestsAfterLoginAsync();
                    return;
                case InstaLoginResult.BadPassword:
                    throw new WrongPasswordException();
                case InstaLoginResult.ChallengeRequired:
                    throw new ChallengeException();
                default:
                    throw result.Info.Exception;
            }
        }

        internal async Task Logout()
        {
            var result = await _api.LogoutAsync();
            if (!result.Succeeded)
                throw result.Info.Exception;
        }

        public void Save(string path)
        {
            if (_api?.IsUserAuthenticated ?? false)
            {
                using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    _api.LoadStateDataFromObject(
                        (StateData)new BinaryFormatter().Deserialize(file));
                }
            }
        }

        public void Load(string path)
        {
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, _api.GetStateDataAsObject());
            }
        }

        public void SaveCache(string path)
        {
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, Data);
            }
        }

        public void LoadCache(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Data = (InstagramData)new BinaryFormatter().Deserialize(file);
            }
        }

        public async Task Refresh()
        {
            var account = await _api.AccountProcessor.GetRequestForEditProfileAsync();
            if (!account.Succeeded)
                throw account.Info.Exception;
            var account_user = new InstagramUser(account.Value.Pk,
                account.Value.Username, account.Value.FullName);
            var followers = await _api.UserProcessor.GetCurrentUserFollowersAsync(
                InstagramApiSharp.PaginationParameters.Empty);
            if (!followers.Succeeded)
                throw followers.Info.Exception;
            var followers_users = followers.Value.Select(a =>
                new InstagramUser(a.Pk, a.UserName, a.FullName)).ToArray();
            var followings = await _api.UserProcessor.GetUserFollowingAsync(
                account_user.Username,
                InstagramApiSharp.PaginationParameters.Empty);
            if (!followings.Succeeded)
                throw followings.Info.Exception;
            var followings_users = followings.Value.Select(a =>
                new InstagramUser(a.Pk, a.UserName, a.FullName)).ToArray();
            Data = new InstagramData(account_user, followers_users, followings_users);
        }

        public async Task Unfollow(InstagramUser user)
        {
            var result = await _api.UserProcessor.UnFollowUserAsync(user.Id);
            if (!result.Succeeded)
                throw result.Info.Exception;
            Data.Followings.Remove(user);
        }

        public string UserName { get; }
        public InstagramData Data { get; private set; }
        public bool IsUserAuthenticated => _api.IsUserAuthenticated;

        private IInstaApi _api;
    }

    [Serializable]
    public class InstagramUser : IEquatable<InstagramUser>
    {
        public InstagramUser(long userid,
            string username, string fullname)
        {
            Id = userid;
            Username = username;
            Fullname = fullname;
        }

        public string Username { get; }
        public string Fullname { get; }
        public long Id { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as InstagramUser);
        }

        public bool Equals(InstagramUser other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(InstagramUser left, InstagramUser right)
        {
            return EqualityComparer<InstagramUser>.Default.Equals(left, right);
        }

        public static bool operator !=(InstagramUser left, InstagramUser right)
        {
            return !(left == right);
        }
    }

    [Serializable]
    public class InstagramData
    {
        public InstagramData(InstagramUser user,
            IEnumerable<InstagramUser> followers, IEnumerable<InstagramUser> followings)
        {
            User = user;
            Followers = followers.ToList();
            Followings = followings.ToList();
        }
        public InstagramUser User { get; }

        public List<InstagramUser> Followers { get; }
        public List<InstagramUser> Followings { get; }

        public IEnumerable<InstagramUser> Unfollowers => Followings.Except(Followers);
    }
}