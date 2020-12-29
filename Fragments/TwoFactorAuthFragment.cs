using System;
using System.Diagnostics;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Fragments
{
    public class TwoFactorAuthFragment : FragmentBase
    {
        public TwoFactorAuthFragment(Account account) :
            base(Resource.Layout.fragment_login_2fa)
        {
            _account = account;

            Create += TwoFactorAuthFragment_Create;
        }

        private void TwoFactorAuthFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            Title = GetString(Resource.String.title_2fa);

            _btnResend = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_2fa_resend);
            _textInput = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_2fa_code_input);
            _btnVerify = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_2fa_verify);

            Debug.Assert(_btnVerify != null &&
                         _btnResend != null);

            _btnVerify.Click += VerifyButton_OnClick;
            _btnResend.Click += ResendTextView_OnClick;
        }

        private async void VerifyButton_OnClick(object sender, EventArgs e)
        {
            _textInput.Enabled = false;
            _btnResend.Enabled = false;
            _btnVerify.Enabled = false;
            try
            {
                var ig = ((IInstagramHost) Activity).Accounts;
                await ig.CompleteLoginAsync(
                    _account,
                    _textInput.Text);

                PopFragment();
            }
            catch (Exception ex)
            {
                _textInput.Enabled = true;
                _btnResend.Enabled = true;
                _btnVerify.Enabled = true;
                ((IErrorHost) Activity).ShowError(ex);
            }
        }

        private async void ResendTextView_OnClick(object sender, EventArgs e)
        {
            _btnResend.Enabled = false;
            try
            {
                await _account.TwoFactorSendSms();
            }
            catch (Exception ex)
            {
                _btnResend.Enabled = true;
                ((IErrorHost) Activity).ShowError(ex);
            }
        }

        private readonly Account _account;

        private MaterialButton _btnVerify, _btnResend;
        private TextInputEditText _textInput;
    }
}
