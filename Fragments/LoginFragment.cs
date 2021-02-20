using System;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    internal class LoginFragment : FragmentBase
    {
        private MaterialButton _btnLogin;
        private TextInputLayout _elUserName, _elPassword;
        private TextInputEditText _etUserName, _etPassword;
        private MaterialTextView _tvTerms;
        private bool _didTwoFactorAuthentication;

        public LoginFragment() : base(Resource.Layout.fragment_login)
        {
            Create += LoginFragment_Create;
        }

        private void LoginFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            if (_didTwoFactorAuthentication)
            {
                PopFragment();
            }

            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_addaccount);
            ((IActionBarContainer)Activity).Hide();

            _etUserName = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_username_input);
            _etPassword = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_password_input);
            _elUserName = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_username_layout);
            _elPassword = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_password_layout);
            _btnLogin = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_login);
            _tvTerms = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_login_terms);

            if (_btnLogin == null ||
                _tvTerms == null)
                return;

            _btnLogin.Click += LoginBtn_Click;

            // Show a "Privacy Policy" link
            var termsText = new SpannableStringBuilder();

            termsText.Append(GetString(Resource.String.msg_terms0));
            var spanStart = termsText.Length();
            termsText.Append(GetString(Resource.String.msg_terms1));
            var spanEnd = termsText.Length();
            termsText.Append(GetString(Resource.String.msg_terms2));

            termsText.SetSpan(
                new TermsSpan(Context),
                spanStart,
                spanEnd,
                SpanTypes.InclusiveExclusive);

            _tvTerms.SetText(termsText, TextView.BufferType.Spannable);

            _tvTerms.MovementMethod = LinkMovementMethod.Instance;
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

            try
            {
                _etUserName.Enabled = false;
                _etPassword.Enabled = false;
                _btnLogin.Enabled = false;

                await ((IInstagramAccounts)Activity).Accounts
                    .AddAccountAsync(_etUserName.Text, _etPassword.Text);

#if TGBUILD || DEBUG
                ((IUpdateChecker)Activity).DidLogin();
#endif

                PopFragment();
            }
            catch (TwoFactorAuthException twoFactorAuth)
            {
                // Navigate to 2FA fragment
                var twoFactorAuthFragment = new TwoFactorAuthFragment(twoFactorAuth.Account);
                PushFragment(twoFactorAuthFragment);
                _didTwoFactorAuthentication = true;
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
            catch (DuplicateAccountException)
            {
                _elUserName.Error = GetString(Resource.String.error_duplicate_account);
                _elPassword.Error = " ";

                _etUserName.TextChanged += ErrorEditLayoutChangeHandler;
                _etPassword.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (ChallengeException ex)
            {
                var challengeFragment = new ChallengeFragment(ex.Account);
                PushFragment(challengeFragment);
                _didTwoFactorAuthentication = true;
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
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

        private class TermsSpan : ClickableSpan
        {
            private readonly Context _context;

            public TermsSpan(Context context)
            {
                _context = context;
            }

            public override void OnClick(View widget)
            {
                var url = Android.Net.Uri.Parse(
                    _context.GetString(Resource.String.url_terms));
                ((IUrlHandler)_context).LaunchBrowser(url);
            }
        }
    }
}