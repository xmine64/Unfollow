using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Android.OS;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.SessionHandlers;
using IInstaApi = InstagramApiSharp.API.IInstaApi;
using InstaApiBuilder = InstagramApiSharp.API.Builder.InstaApiBuilder;

namespace madamin.unfollow
{
    public class WrongPasswordException : Exception { }
    public class ChallengeException : Exception { }

    public class Instagram
    {
        private IInstaApi _api;
        
        public InstagramData Data { get; private set; }

        public Instagram(string session_file)
        {
            _api = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetSessionHandler(new FileSessionHandler { FilePath = session_file })
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();
            _api.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version126);
        }

        public async Task Login(string username, string password)
        {
            if (_api.IsUserAuthenticated) return;

            _api.SetUser(username, password);

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

        public void Save()
        {
            if (_api?.IsUserAuthenticated ?? false)
                _api.SessionHandler.Save();
        }

        public void Load()
        {
            _api.SessionHandler.Load();
        }

        public void LoadCache(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Data = (InstagramData)new BinaryFormatter().Deserialize(file);
            }
        }

        public void SaveCache(string path)
        {
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, Data);
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

        public async Task Logout()
        {
            if ((await _api.LogoutAsync()).Value)
                return;
            throw new Exception("Logout Error");
        }

        public async Task Unfollow(InstagramUser user)
        {
            var result = await _api.UserProcessor.UnFollowUserAsync(user.Id);
            if (result.Succeeded)
                return;
            throw result.Info.Exception;
        }

        public bool IsUserAuthenticated => _api.IsUserAuthenticated;
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