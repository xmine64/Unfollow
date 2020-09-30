using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using InstagramApiSharp.Classes;
using IInstaApi = InstagramApiSharp.API.IInstaApi;
using InstaApiBuilder = InstagramApiSharp.API.Builder.InstaApiBuilder;

namespace Madamin.Unfollow.Instagram
{
    public class Account
    {
        internal Account()
        {
            _api = InstaApiBuilder.CreateBuilder()
                .SetUser(UserSessionData.Empty)
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();
            _api.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version126);
        }

        internal async Task LoginAsync(string username, string password)
        {
            // no need to authenticate again
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
                    throw result.Info.Exception ??
                        new InstagramException(result.Info.Message);
            }
        }

        internal async Task LogoutAsync()
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            var result = await _api.LogoutAsync();

            if (!result.Succeeded)
                throw result.Info.Exception ??
                    new InstagramException(result.Info.Message);
        }

        internal void SaveState(string path)
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            var state = _api.GetStateDataAsObject();
            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, state);
            }
        }

        internal void LoadState(string path)
        {
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var state = (StateData)new BinaryFormatter().Deserialize(file);
                _api.LoadStateDataFromObject(state);
            }
        }

        internal void SaveCache(string path)
        {
            if (Data == null)
                throw new AccountDataNotAvailableException();

            using (var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(file, Data);
            }
        }

        internal void LoadCache(string path)
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Data = (AccountData)new BinaryFormatter().Deserialize(file);
            }
        }

        public async Task RefreshAsync()
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            // request user data
            var user_edit_req = await _api.AccountProcessor.GetRequestForEditProfileAsync();
            if (!user_edit_req.Succeeded)
                throw user_edit_req.Info.Exception ??
                    new InstagramException(user_edit_req.Info.Message);

            var user = new User(
                user_edit_req.Value.Pk,
                user_edit_req.Value.Username,
                user_edit_req.Value.FullName);

            // request user followers
            var followers_req = await _api.UserProcessor
                .GetCurrentUserFollowersAsync(InstagramApiSharp.PaginationParameters.Empty);
            if (!followers_req.Succeeded)
                throw followers_req.Info.Exception ??
                    new InstagramException(followers_req.Info.Message);
            
            var followers = from follower in followers_req.Value
                            select new User(follower.Pk, follower.UserName, follower.FullName);

            // request user followings
            var followings_req = await _api.UserProcessor
                .GetUserFollowingAsync(
                user.Username,
                InstagramApiSharp.PaginationParameters.Empty);
            if (!followings_req.Succeeded)
                throw followings_req.Info.Exception ??
                    new InstagramException(followers_req.Info.Message);

            var followings = from following in followings_req.Value
                             select new User(following.Pk, following.UserName, following.FullName);

            // save data
            Data = new AccountData(user, followers, followings);
        }

        public async Task UnfollowAsync(User user)
        {
            var result = await _api.UserProcessor.UnFollowUserAsync(user.Id);
            if (!result.Succeeded)
                throw result.Info.Exception ??
                    new InstagramException(result.Info.Message); ;

            Data.Followings.Remove(user);
        }

        public override bool Equals(object obj)
        {
            return obj is Account account &&
                   EqualityComparer<AccountData>.Default.Equals(Data, account.Data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Data);
        }

        public AccountData Data { get; private set; }

        private IInstaApi _api;

        [Serializable]
        public class AccountData
        {
            public AccountData(
                User user,
                IEnumerable<User> followers,
                IEnumerable<User> followings)
            {
                User = user;
                Followers = followers.ToList();
                Followings = followings.ToList();
            }
            public User User { get; }

            public List<User> Followers { get; }
            public List<User> Followings { get; }

            public IEnumerable<User> Unfollowers => Followings.Except(Followers);

            public override bool Equals(object obj)
            {
                return obj is AccountData data &&
                       EqualityComparer<User>.Default.Equals(User, data.User);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(User);
            }
        }
    }

    [Serializable]
    public class User
    {
        public User(
            long userid,
            string username,
            string fullname)
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
            return obj is User user &&
                   Id == user.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }

    public class AccountDataNotAvailableException : Exception { }
    public class UserNotAuthenticatedException : Exception { }
    public class WrongPasswordException : Exception { }
    public class ChallengeException : Exception { }
    public class InstagramException : Exception 
    { 
        public InstagramException(string message) : base (message) {}
    }
}