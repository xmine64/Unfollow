using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

using AndroidX.AppCompat.App;
using Google.Android.Material.Button;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;

namespace madamin.unfollow
{
    [Activity(Label = "@string/login", Theme = "@style/AppTheme",
        MainLauncher = false, ParentActivity = typeof(MainActivity))]
    public class LoginActivity : AppCompatActivity
    {
        private TextInputEditText _et_username, _et_password;
        private TextInputLayout _et_password_layout;
        private MaterialButton _btn_login;
        private Instagram _instagram;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var session_data = Intent?.GetStringExtra("session_data_path");
            if (session_data == null)
            {
                Finish();
                return;
            }
            _instagram = new Instagram(session_data);

            SetContentView(Resource.Layout.activity_login);

            _et_username = FindViewById<TextInputEditText>(Resource.Id.login_et_username);
            _et_password = FindViewById<TextInputEditText>(Resource.Id.login_et_password);
            _et_password_layout = FindViewById<TextInputLayout>(Resource.Id.login_layout_password);
            _btn_login = FindViewById<MaterialButton>(Resource.Id.login_button);
            
            _btn_login.Click += LoginBtn_Click;

            FindViewById<MaterialButton>(Resource.Id.login_cancel_button).Click += (button, args) =>
            {
                Finish();
            };
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_et_username.Text) ||
                    string.IsNullOrWhiteSpace(_et_password.Text))
                return;
            try
            {
                _et_username.Enabled = false;
                _et_password.Enabled = false;
                _btn_login.Enabled = false;

                 await _instagram.Login(_et_username.Text, _et_password.Text);
                _instagram.Save();

                var main_intent = new Intent(this, typeof(MainActivity));
                StartActivity(main_intent);
                Finish();
            }
            catch (WrongPasswordException)
            {
                _et_password_layout.Error = GetString(Resource.String.err_password);
                _et_password.TextChanged += (et, args) =>
                {
                    _et_password_layout.ErrorEnabled = false;
                };
            }
            catch (Exception ex)
            {
                new MaterialAlertDialogBuilder(this)
                .SetTitle(Resource.String.error)
                .SetMessage(GetString(Resource.String.err_login) + ":\n" + ex.Message)
                .SetNeutralButton(Android.Resource.String.Ok, (dialog, args) => { })
                .Show();
            }
            finally
            {
                _et_username.Enabled = true;
                _et_password.Enabled = true;
                _btn_login.Enabled = true;
            }
        }
    }
}