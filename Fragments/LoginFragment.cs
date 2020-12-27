using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Text;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.Dialog;
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
                await TwoFactorAuthenticateAsync(twoFactorAuth.Account);
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

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            PopFragment();
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

        private async Task TwoFactorAuthenticateAsync(Account account)
        {
            await account.TwoFactorSendSms();

            var input = new TextInputEditText(Activity)
            {
                InputType = InputTypes.ClassNumber
            };

            var dialog = new MaterialAlertDialogBuilder(Activity);
            dialog.SetTitle("2FA");
            dialog.SetView(input);
            dialog.SetPositiveButton(
                Android.Resource.String.Ok,
                async (sender, args) =>
                {
                    var twoFactorDialog = new MaterialAlertDialogBuilder(Activity)
                        .SetView(new ProgressBar(Activity))
                        .Create();
                    twoFactorDialog.Show();
                    try
                    {
                        await ((IInstagramHost) Activity).Accounts
                            .CompleteLoginAsync(account, input.Text);
                    }
                    catch (Exception ex)
                    {
                        ((IErrorHost) Activity).ShowError(ex);
                    }
                    finally
                    {
                        PopFragment();
                        twoFactorDialog.Dismiss();
                        twoFactorDialog.Dispose();
                    }
                });
            dialog.SetNegativeButton(
                Android.Resource.String.Cancel,
                (sender, args) => { PopFragment(); });
            dialog.Show();
        }
    }
}