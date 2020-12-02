﻿using System;
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
        private TextInputLayout _elPassword;
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
            _elPassword = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_password_layout);
            _btnLogin = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_login);

            var btnCancel = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_cancel);

            if (_btnLogin == null ||
                btnCancel == null)
                return;

            _btnLogin.Click += LoginBtn_Click;
            btnCancel.Click += CancelBtn_Click;
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_etUserName.Text))
                return; // TODO: Show an error
            if (string.IsNullOrWhiteSpace(_etPassword.Text))
                return; // TODO: Show an error

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
                await twoFactorAuth.Account.TwoFactorSendSms();

                var input = new TextInputEditText(Activity)
                {
                    InputType = InputTypes.ClassNumber
                };

                new MaterialAlertDialogBuilder(Activity)
                    .SetTitle("2FA")
                    .SetView(input)
                    .SetPositiveButton(
                        Android.Resource.String.Ok,
                        async (dialog, args) =>
                        {
                            var twoFactorDialog = new MaterialAlertDialogBuilder(Activity)
                                .SetView(new ProgressBar(Activity))
                                .Create();
                            twoFactorDialog.Show();
                            try
                            {
                                await ig.CompleteLoginAsync(twoFactorAuth.Account, input.Text);
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
                        })
                    .SetNegativeButton(
                        Android.Resource.String.Cancel,
                        (dialog, args) => { PopFragment(); })
                    .Show();
            }
            catch (WrongPasswordException)
            {
                _elPassword.Error = GetString(Resource.String.error_invalid_password);
                _etPassword.TextChanged += (et, args) => { _elPassword.ErrorEnabled = false; };
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
    }
}