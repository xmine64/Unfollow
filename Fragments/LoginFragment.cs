using System;
using System.Diagnostics;
using Android.Content;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Text.Style;
using Android.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using Madamin.Unfollow.Instagram;

namespace Madamin.Unfollow.Fragments
{
    internal class LoginFragment : FragmentBase
    {
        private MaterialButton _btnLogin;
        private TextInputLayout _elUserName, _elPassword;
        private TextInputEditText _etUserName, _etPassword;
        private MaterialTextView _tvTerms;

        public LoginFragment() : base(Resource.Layout.fragment_login)
        {
            Create += LoginFragment_Create;
        }

        private class TermsSpan : ClickableSpan
        {
            public TermsSpan(Context context, IFragmentHost fragmentHost)
            {
                _fragmentHost = fragmentHost;
                _context = context;
            }

            public override void OnClick(View widget)
            {
                _fragmentHost.NavigateTo(
                    HtmlFragment.NewTermsFragment(_context),
                    false,
                    true);
            }

            private readonly IFragmentHost _fragmentHost;
            private readonly Context _context;
        }

        private void LoginFragment_Create(object sender, OnFragmentCreateEventArgs e)
        {
            Title = GetString(Resource.String.title_addaccount);

            _etUserName = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_username_input);
            _etPassword = e.View.FindViewById<TextInputEditText>(Resource.Id.fragment_login_password_input);
            _elUserName = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_username_layout);
            _elPassword = e.View.FindViewById<TextInputLayout>(Resource.Id.fragment_login_password_layout);
            _btnLogin = e.View.FindViewById<MaterialButton>(Resource.Id.fragment_login_login);
            _tvTerms = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_login_terms);

            Debug.Assert(_btnLogin != null &&
                         _tvTerms != null);

            _btnLogin.Click += LoginBtn_Click;

            var termsText = new SpannableStringBuilder();
            termsText.Append(GetString(Resource.String.msg_terms0));
            var spanStart = termsText.Length();
            termsText.Append(GetString((Resource.String.title_terms)));
            var spanEnd = termsText.Length();
            termsText.Append(GetString(Resource.String.msg_terms1));
            termsText.SetSpan(
                new TermsSpan(Context, (IFragmentHost)Activity), 
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