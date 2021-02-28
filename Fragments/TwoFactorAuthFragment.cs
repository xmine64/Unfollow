using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    public class TwoFactorAuthFragment : Fragment
    {
        private readonly Account _account;

        private MaterialButton _verifyButton, _resendButton;
        private TextInputEditText _textInput;

        private bool _didChallenge;

        public TwoFactorAuthFragment(Account account)
        {
            _account = account;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_login_2fa, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (_didChallenge)
            {
                ((IFragmentContainer)Activity).PopFragment();
            }

            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_2fa);
            ((IActionBarContainer)Activity).Hide();

            _textInput = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_2fa_code_input);
            _resendButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_2fa_resend);
            _verifyButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_2fa_verify);

            _verifyButton.Click += VerifyButton_OnClick;
            _resendButton.Click += ResendTextView_OnClick;
        }

        private async void VerifyButton_OnClick(object sender, EventArgs e)
        {
            _textInput.Enabled = false;
            _resendButton.Enabled = false;
            _verifyButton.Enabled = false;
            try
            {
                await ((IInstagramAccounts)Activity)
                    .CompleteTwoFactorLoginAsync(_account, _textInput.Text);

#if TGBUILD || DEBUG
                ((IUpdateChecker)Activity).DidLogin();
#endif

                ((IFragmentContainer)Activity).PopFragment();
            }
            catch (ChallengeException ex)
            {
                var challengeFragment = new ChallengeFragment(ex.Account);
                ((IFragmentContainer)Activity).PushFragment(challengeFragment);
                _didChallenge = true;
            }
            catch (InvalidTwoFactorCodeException)
            {
                ((ISnackBarProvider)Activity).ShowSnackBar(Resource.String.error_invalid_2fa);
            }
            catch (TwoFactorCodeExpiredException)
            {
                ((ISnackBarProvider)Activity).ShowSnackBar(Resource.String.error_2fa_expired);
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }
            finally
            {
                _textInput.Enabled = true;
                _resendButton.Enabled = true;
                _verifyButton.Enabled = true;
            }
        }

        private async void ResendTextView_OnClick(object sender, EventArgs e)
        {
            _resendButton.Enabled = false;
            try
            {
                await _account.TwoFactorSendSms();
            }
            catch (Exception ex)
            {
                _resendButton.Enabled = true;
                ((IErrorHandler)Activity).ShowError(ex);
            }
        }
    }
}