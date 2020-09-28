using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextField;
using System;

namespace madamin.unfollow
{
    class LoginFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_login, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((IFragmentHost)Activity).ActionbarTitle = GetString(Resource.String.menu_addaccount);

            _et_username = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_et_username);
            _et_password = view.FindViewById<TextInputEditText>(Resource.Id.fragment_login_et_password);
            _et_layout_password = view.FindViewById<TextInputLayout>(Resource.Id.fragment_login_et_layout_password);
            _btn_login = view.FindViewById<MaterialButton>(Resource.Id.fragment_login_btn_login);
            _btn_login.Click += LoginBtn_Click;
            view.FindViewById<MaterialButton>(Resource.Id.fragment_login_btn_cancel).Click += CancelBtn_Click;
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_et_username.Text) ||
                    string.IsNullOrWhiteSpace(_et_password.Text))
                return; // TODO: Show an error

            var ig = ((IInstagramActivity)Activity).Instagram;

            try
            {
                _et_username.Enabled = false;
                _et_password.Enabled = false;
                _btn_login.Enabled = false;

                await ig.AddAccount(_et_username.Text, _et_password.Text);

                Activity.SupportFragmentManager.PopBackStack();
            }
            catch (WrongPasswordException)
            {
                _et_layout_password.Error = GetString(Resource.String.err_password);
                _et_password.TextChanged += (et, args) =>
                {
                    _et_layout_password.ErrorEnabled = false;
                };
            }
            catch (Exception ex)
            {
                new MaterialAlertDialogBuilder(Activity)
                .SetTitle(Resource.String.error)
                .SetMessage(GetString(Resource.String.err_login) + ":\n" + ex.Message
#if DEBUG
                + "\n" + ex.ToString()
#endif
                )
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

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            Activity.SupportFragmentManager.PopBackStack();
        }

        private TextInputEditText _et_username, _et_password;
        private TextInputLayout _et_layout_password;
        private MaterialButton _btn_login;
    }
}