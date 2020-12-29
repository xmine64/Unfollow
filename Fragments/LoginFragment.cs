using System;
using System.Diagnostics;
using Android.Text;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Fragments
{
    internal class LoginFragment : FragmentBase
    {
        private MaterialButton _btnLogin;
        private TextInputLayout _elUserName, _elPassword;
        private TextInputEditText _etUserName, _etPassword;

        public LoginFragment() : base(Resource.Layout.fragment_login)
        {
            Create += LoginFragment_Create;
        }

        private void LoginFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            Title = GetString(Resource.String.title_addaccount);

            _etUserName = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_username_input);
            _etPassword = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_password_input);
            _elUserName = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_username_layout);
            _elPassword = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_password_layout);
            _btnLogin = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_login);

            Debug.Assert(_btnLogin != null);
            
            _btnLogin.Click += LoginBtn_Click;
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            var usernameIsNull = string.IsNullOrEmpty(_etUserName.Text);
            var passwordIsNull = string.IsNullOrWhiteSpace(_etPassword.Text);

            if (usernameIsNull && passwordIsNull)
            {
                _elUserName.Error = " ";
                _elPassword.Error = GetString(Resource.String.error_required_field);

                _etUserName.TextChanged += ErrorEditLayoutChangeHandler;
                _etPassword.TextChanged += ErrorEditLayoutChangeHandler;

                return;
            }

            if (usernameIsNull)
            {
                _elUserName.Error = GetString(Resource.String.error_required_field);

                _etUserName.TextChanged += ErrorEditLayoutChangeHandler;

                return;
            }

            if (passwordIsNull)
            {
                _elPassword.Error = GetString(Resource.String.error_required_field);

                _etPassword.TextChanged += ErrorEditLayoutChangeHandler;

                return;
            }

            var ig = ((IInstagramHost) Activity).Accounts;

            try
            {
                _etUserName.Enabled = false;
                _etPassword.Enabled = false;
                _btnLogin.Enabled = false;

                await ig.AddAccountAsync(_etUserName.Text, _etPassword.Text);

                PopFragment();
            }
            catch (TwoFactorAuthException twoFactorAuth)
            {
                var authFragment = 
                    new TwoFactorAuthFragment(twoFactorAuth.Account);

                PopFragment();
                PushFragment(authFragment);
            }
            catch (WrongPasswordException)
            {
                _elPassword.Error = GetString(Resource.String.error_invalid_password);

                _etPassword.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (InvalidCredentialException)
            {
                _elUserName.Error = " ";
                _elPassword.Error = GetString(Resource.String.error_invalid_credential);

                _etUserName.TextChanged += ErrorEditLayoutChangeHandler;
                _etPassword.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (Exception ex)
            {
                ((IErrorHost) Activity).ShowError(ex);
            }
            finally
            {
                _etUserName.Enabled = true;
                _etPassword.Enabled = true;
                _btnLogin.Enabled = true;
            }
        }

        private void ErrorEditLayoutChangeHandler(object et, TextChangedEventArgs args)
        {
            if (_elUserName.ErrorEnabled)
            {
                _elUserName.ErrorEnabled = false;
                _etUserName.TextChanged -= ErrorEditLayoutChangeHandler;
            }

            if (_elPassword.ErrorEnabled)
            {
                _elPassword.ErrorEnabled = false;
                _etPassword.TextChanged -= ErrorEditLayoutChangeHandler;
            }
        }
    }
}