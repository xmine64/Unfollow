using System;
using Google.Android.Material.TextView;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Madamin.Unfollow.Instagram;
using InstagramApiSharp.Classes;
using Android.Views;

namespace Madamin.Unfollow.Fragments
{
    public class ChallengeFragment : FragmentBase
    {
        private readonly Account _account;
        private InstaChallengeRequireVerifyMethod _challenge;

        private MaterialTextView _phoneTextView, _methodPhoneTextView, _methodEmailTextView;

        private TextInputLayout _phoneInputLayout, _otpInputLayout;
        private TextInputEditText _phoneEditText, _otpEditText;

        private MaterialButton _submitButton, _phoneButton, _emailButton;

        public ChallengeFragment(Account account) :
            base(Resource.Layout.fragment_login_challenge)
        {
            _account = account;

            Create += ChallengeFragment_Create;
        }

        private async void ChallengeFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            Title = GetString(Resource.String.title_challenge);
            ActionBarVisible = false;

            _phoneTextView = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_phone);
            _methodPhoneTextView = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_methods_phone);
            _methodEmailTextView = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_login_challenge_methods_email);

            _phoneInputLayout = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_challenge_phone_input_layout);
            _phoneEditText = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_challenge_phone_input);

            _otpInputLayout = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_challenge_code_input_layout);
            _otpEditText = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_challenge_code_input);

            _submitButton = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_submit);
            _phoneButton = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_button_phone);
            _emailButton = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_challenge_button_email);

            _submitButton.Click += SubmitButton_Click;
            _phoneButton.Click += PhoneButton_Click;
            _emailButton.Click += EmailButton_Click;

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
                    _phoneButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(_challenge.StepData.Email))
                {
                    _methodPhoneTextView.Text = GetString(Resource.String.msg_challenge_methods_email,
                        _challenge.StepData.Email);
                    _methodEmailTextView.Visibility = ViewStates.Visible;
                    _emailButton.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception ex)
            {
                ((IErrorHost)Activity).ShowError(ex);
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_challenge.SubmitPhoneRequired)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                ((IErrorHost)Activity).ShowError(ex);
            }
        }

        private void PhoneButton_Click(object sender, EventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                ((IErrorHost)Activity).ShowError(ex);
            }
        }

        private void EmailButton_Click(object sender, EventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                ((IErrorHost)Activity).ShowError(ex);
            }
        }
    }
}