﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Enums;

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
            _api.SetApiVersion(InstaApiVersionType.Version126);
        }

        public AccountData Data { get; private set; }

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

                case InstaLoginResult.TwoFactorRequired:
                    throw new TwoFactorAuthException(this);

                case InstaLoginResult.BadPassword:
                    throw new WrongPasswordException();

                case InstaLoginResult.ChallengeRequired:
                    throw new ChallengeException();

                case InstaLoginResult.InvalidUser:
                    throw new InvalidCredentialException();

                default:
                    throw result.Info.Exception ??
                          new InstagramException(result.Info.Message);
            }
        }

        public async Task TwoFactorSendSms()
        {
            await _api.SendTwoFactorLoginSMSAsync();
        }

        internal async Task CompleteLoginAsync(string code)
        {
            var result = await _api.TwoFactorLoginAsync(code);
            if (result.Value != InstaLoginTwoFactorResult.Success)
                throw new InstagramException("Login error");
            await _api.SendRequestsAfterLoginAsync();
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
            using var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            new BinaryFormatter().Serialize(file, state);
        }

        internal void LoadState(string path)
        {
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
            var state = (StateData) new BinaryFormatter().Deserialize(file);
            _api.LoadStateDataFromObject(state);
        }

        internal void SaveCache(string path)
        {
            if (Data == null)
                throw new AccountDataNotAvailableException();

            using var file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            new BinaryFormatter().Serialize(file, Data);
        }

        internal void LoadCache(string path)
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
            Data = (AccountData) new BinaryFormatter().Deserialize(file);
        }

        public async Task RefreshAsync()
        {
            if (!_api.IsUserAuthenticated)
                throw new UserNotAuthenticatedException();

            // request user data
            var userEditReq = await _api.AccountProcessor.GetRequestForEditProfileAsync();
            if (!userEditReq.Succeeded)
                throw userEditReq.Info.Exception ??
                      new InstagramException(userEditReq.Info.Message);

            var user = new User(
                userEditReq.Value.Pk,
                userEditReq.Value.Username,
                userEditReq.Value.FullName);

            // request user followers
            var followersReq = await _api.UserProcessor
                .GetCurrentUserFollowersAsync(PaginationParameters.Empty);
            if (!followersReq.Succeeded)
                throw followersReq.Info.Exception ??
                      new InstagramException(followersReq.Info.Message);

            var followers = from follower in followersReq.Value
                select new User(follower.Pk, follower.UserName, follower.FullName);

            // request user followings
            var followingsReq = await _api.UserProcessor
                .GetUserFollowingAsync(
                    user.Username,
                    PaginationParameters.Empty);
            if (!followingsReq.Succeeded)
                throw followingsReq.Info.Exception ??
                      new InstagramException(followersReq.Info.Message);

            var followings = from following in followingsReq.Value
                select new User(following.Pk, following.UserName, following.FullName);

            // save data
            Data = new AccountData(user, followers, followings);
        }

        public async Task UnfollowAsync(User user)
        {
            var result = await _api.UserProcessor.UnFollowUserAsync(user.Id);
            if (!result.Succeeded)
                throw result.Info.Exception ??
                      new InstagramException(result.Info.Message);

            Data.Followings.Remove(user);
        }

        public async Task FollowAsync(User user)
        {
            var result = await _api.UserProcessor.FollowUserAsync(user.Id);
            if (!result.Succeeded)
                throw result.Info.Exception ??
                      new InstagramException(result.Info.Message);

            Data.Followings.Add(user);
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

        private readonly IInstaApi _api;

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
            public IEnumerable<User> Fans => Followers.Except(Followings);

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

    public class AccountDataNotAvailableException : Exception
    {
    }

    public class UserNotAuthenticatedException : Exception
    {
    }

    public class WrongPasswordException : Exception
    {
    }

    public class ChallengeException : Exception
    {
    }

    public class InvalidCredentialException : Exception
    {
    }

    public class InstagramException : Exception
    {
        public InstagramException(string message) : base(message)
        {
        }
    }

    public class TwoFactorAuthException : Exception
    {
        public TwoFactorAuthException(Account account)
        {
            Account = account;
        }

        public Account Account { get; }
    }
}