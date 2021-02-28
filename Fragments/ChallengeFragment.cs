using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.TextView;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using InstagramApiSharp.Classes;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    public class ChallengeFragment : Fragment
    {
        private readonly Account _account;
        private InstaChallengeRequireVerifyMethod _challenge;

        private MaterialTextView _phoneTextView, _methodPhoneTextView, _methodEmailTextView;

        private TextInputLayout _phoneInputLayout, _otpInputLayout;
        private TextInputEditText _phoneEditText, _otpEditText;

        private MaterialButton _submitButton, _resendButton, _methodPhoneButton, _methodEmailButton;

        private ChallengeMethod _method = ChallengeMethod.None;

        public ChallengeFragment(Account account)
        {
            _account = account;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_login_challenge, container, false);
        }

        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            ((IFragmentContainer)Activity).ShowLoadingView();
            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_challenge);
            ((IActionBarContainer)Activity).Hide();

            _phoneTextView = view.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_phone);
            _methodPhoneTextView = view.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_methods_phone);
            _methodEmailTextView = view.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_methods_email);

            _phoneInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.fragment_login_challenge_phone_input_layout);
            _phoneEditText = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_challenge_phone_input);

            _otpInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.fragment_login_challenge_code_input_layout);
            _otpEditText = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_challenge_code_input);

            _submitButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_submit);
            _resendButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_button_resend);
            _methodPhoneButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_button_phone);
            _methodEmailButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_button_email);

            _submitButton.Click += SubmitButton_Click;
            _resendButton.Click += ResendButton_Click;
            _methodPhoneButton.Click += PhoneButton_Click;
            _methodEmailButton.Click += EmailButton_Click;

            try
            {
                _challenge = await _account.StartChallengeAsync();

                if (_challenge.SubmitPhoneRequired)
                {
                    _submitButton.Visibility = ViewStates.Visible;
                    _phoneTextView.Visibility = ViewStates.Visible;
                    return;
                }

                if (!string.IsNullOrEmpty(_challenge.StepData.PhoneNumber))
                {
                    _methodPhoneTextView.Text = GetString(Resource.String.msg_challenge_methods_phone,
                        _challenge.StepData.PhoneNumber);
                    _methodPhoneTextView.Visibility = ViewStates.Visible;
                    _methodPhoneButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(_challenge.StepData.Email))
                {
                    _methodEmailTextView.Text = GetString(Resource.String.msg_challenge_methods_email,
                        _challenge.StepData.Email);
                    _methodEmailTextView.Visibility = ViewStates.Visible;
                    _methodEmailButton.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }

            ((IFragmentContainer)Activity).ShowContentView();
        }

        private async void ResendButton_Click(object sender, EventArgs e)
        {
            _resendButton.Enabled = false;

            try
            {
                if (_method == ChallengeMethod.None)
                    return;

                if (_method == ChallengeMethod.Phone)
                {
                    await _account.ResendChallengePhoneCodeAsync();
                    return;
                }

                if (_method == ChallengeMethod.Email)
                {
                    await _account.ResendChallengeEmailCodeAsync();
                    return;
                }
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
                _resendButton.Enabled = true;
            }

        }

        private async void SubmitButton_Click(object sender, EventArgs e)
        {
            _submitButton.Enabled = false;
            _resendButton.Enabled = false;
            _otpInputLayout.Enabled = false;
            _phoneInputLayout.Enabled = false;

            try
            {
                if (_challenge.SubmitPhoneRequired)
                {
                    await ((IInstagramAccounts)Activity).SubmitPhoneNumberAsync(_account, _phoneEditText.Text);

#if TGBUILD || DEBUG
                    ((IUpdateChecker)Activity).DidLogin();
#endif

                    ((IFragmentContainer)Activity).PopFragment();
                }
                else
                {
                    await ((IInstagramAccounts)Activity).CompleteChallengeAsync(_account, _otpEditText.Text);

#if TGBUILD || DEBUG
                    ((IUpdateChecker)Activity).DidLogin();
#endif

                    ((IFragmentContainer)Activity).PopFragment();
                }
            }
            catch (Exception ex)
            {
                _submitButton.Enabled = true;
                _resendButton.Enabled = true;
                _otpInputLayout.Enabled = true;
                _phoneInputLayout.Enabled = true;

                ((IErrorHandler)Activity).ShowError(ex);
            }
        }

        private async void PhoneButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _account.DoVerifyPhoneChallengeAsync();

                _methodPhoneButton.Visibility = ViewStates.Gone;
                _methodEmailButton.Visibility = ViewStates.Gone;
                _methodEmailTextView.Visibility = ViewStates.Gone;
                _submitButton.Visibility = ViewStates.Visible;
                _otpInputLayout.Visibility = ViewStates.Visible;
                _resendButton.Visibility = ViewStates.Visible;

                _method = ChallengeMethod.Phone;
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }
        }

        private async void EmailButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _account.DoVerifyEmailChallengeAsync();

                _methodPhoneButton.Visibility = ViewStates.Gone;
                _methodEmailButton.Visibility = ViewStates.Gone;
                _methodPhoneTextView.Visibility = ViewStates.Gone;
                _submitButton.Visibility = ViewStates.Visible;
                _otpInputLayout.Visibility = ViewStates.Visible;
                _resendButton.Visibility = ViewStates.Visible;

                _method = ChallengeMethod.Email;
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }
        }

        enum ChallengeMethod
        {
            None, Phone, Email
        }
    }
}