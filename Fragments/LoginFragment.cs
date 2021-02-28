using System;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using Madamin.Unfollow.Instagram;
using Madamin.Unfollow.Main;

namespace Madamin.Unfollow.Fragments
{
    internal class LoginFragment : Fragment
    {
        private MaterialButton _loginButton;
        private TextInputLayout _userNameInputLayout, _passwordInputLayout;
        private TextInputEditText _userNameEditText, _passwordEditText;
        private MaterialTextView _privacyPolicyTextView;

        private bool _didTwoFactorAuthentication;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_login, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (_didTwoFactorAuthentication)
            {
                ((IFragmentContainer)Activity).PopFragment();
            }

            ((IActionBarContainer)Activity).SetTitle(Resource.String.title_addaccount);
            ((IActionBarContainer)Activity).Hide();

            _userNameEditText = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_username_input);
            _passwordEditText = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_password_input);
            _userNameInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.fragment_login_username_layout);
            _passwordInputLayout = view.FindViewById<TextInputLayout>(Resource.Id.fragment_login_password_layout);
            _loginButton = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_login);
            _privacyPolicyTextView = view.FindViewById<MaterialTextView>(Resource.Id.fragment_login_terms);

            _loginButton.Click += LoginButton_Click;

            // Show a "Privacy Policy" link
            // create spanned string
            var termsText = new SpannableStringBuilder();
            // append text before link
            termsText.Append(GetString(Resource.String.msg_terms0));
            // append link
            var spanStart = termsText.Length();
            termsText.Append(GetString(Resource.String.msg_terms1));
            var spanEnd = termsText.Length();
            // append text after link
            termsText.Append(GetString(Resource.String.msg_terms2));
            // add link span
            termsText.SetSpan(
                new TermsSpan(Context),
                spanStart,
                spanEnd,
                SpanTypes.InclusiveExclusive);

            _privacyPolicyTextView.SetText(termsText, TextView.BufferType.Spannable);
            _privacyPolicyTextView.MovementMethod = LinkMovementMethod.Instance;
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            var usernameIsNull = string.IsNullOrEmpty(_userNameEditText.Text);
            var passwordIsNull = string.IsNullOrWhiteSpace(_passwordEditText.Text);

            if (usernameIsNull && passwordIsNull)
            {
                _userNameInputLayout.Error = " ";
                _passwordInputLayout.Error = GetString(Resource.String.error_required_field);

                _userNameEditText.TextChanged += ErrorEditLayoutChangeHandler;
                _passwordEditText.TextChanged += ErrorEditLayoutChangeHandler;

                return;
            }

            if (usernameIsNull)
            {
                _userNameInputLayout.Error = GetString(Resource.String.error_required_field);
                _userNameEditText.TextChanged += ErrorEditLayoutChangeHandler;
                return;
            }

            if (passwordIsNull)
            {
                _passwordInputLayout.Error = GetString(Resource.String.error_required_field);
                _passwordEditText.TextChanged += ErrorEditLayoutChangeHandler;
                return;
            }

            try
            {
                _userNameEditText.Enabled = false;
                _passwordEditText.Enabled = false;
                _loginButton.Enabled = false;

                await ((IInstagramAccounts)Activity).AddAccountAsync(_userNameEditText.Text, _passwordEditText.Text);

#if TGBUILD || DEBUG
                ((IUpdateChecker)Activity).DidLogin();
#endif

                ((IFragmentContainer)Activity).PopFragment();
            }
            catch (TwoFactorAuthException twoFactorAuth)
            {
                // Navigate to 2FA fragment
                var twoFactorAuthFragment = new TwoFactorAuthFragment(twoFactorAuth.Account);
                ((IFragmentContainer)Activity).PushFragment(twoFactorAuthFragment);
                _didTwoFactorAuthentication = true;
            }
            catch (WrongPasswordException)
            {
                _passwordEditText.Error = GetString(Resource.String.error_invalid_password);
                _passwordEditText.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (InvalidCredentialException)
            {
                _userNameInputLayout.Error = " ";
                _passwordInputLayout.Error = GetString(Resource.String.error_invalid_credential);

                _userNameEditText.TextChanged += ErrorEditLayoutChangeHandler;
                _passwordEditText.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (DuplicateAccountException)
            {
                _userNameInputLayout.Error = GetString(Resource.String.error_duplicate_account);
                _passwordInputLayout.Error = " ";

                _userNameEditText.TextChanged += ErrorEditLayoutChangeHandler;
                _passwordEditText.TextChanged += ErrorEditLayoutChangeHandler;
            }
            catch (ChallengeException ex)
            {
                var challengeFragment = new ChallengeFragment(ex.Account);
                ((IFragmentContainer)Activity).PushFragment(challengeFragment);
                _didTwoFactorAuthentication = true;
            }
            catch (Exception ex)
            {
                ((IErrorHandler)Activity).ShowError(ex);
            }
            finally
            {
                _userNameEditText.Enabled = true;
                _passwordEditText.Enabled = true;
                _loginButton.Enabled = true;
            }
        }

        private void ErrorEditLayoutChangeHandler(object et, TextChangedEventArgs args)
        {
            if (_userNameInputLayout.ErrorEnabled)
            {
                _userNameInputLayout.ErrorEnabled = false;
                _userNameEditText.TextChanged -= ErrorEditLayoutChangeHandler;
            }

            if (_passwordInputLayout.ErrorEnabled)
            {
                _passwordInputLayout.ErrorEnabled = false;
                _passwordEditText.TextChanged -= ErrorEditLayoutChangeHandler;
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
                // TODO: Persian page
                ((IUrlHandler)_context).LaunchBrowser(SettingsFragment.PrivacyPolicyEnglishUrl);
            }
        }
    }
}