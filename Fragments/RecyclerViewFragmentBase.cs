using System;
using System.Threading.Tasks;

using Android.Views;

using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.Dialog;
using Google.Android.Material.TextView;

namespace Madamin.Unfollow.Fragments
{
    public class RecyclerViewFragmentBase :
        FragmentBase,
        View.IOnClickListener
    {
        protected RecyclerViewFragmentBase() :
            base(Resource.Layout.fragment_recyclerview)
        {
            _initialize();
        }

        protected RecyclerViewFragmentBase(int menuRes) :
            base(Resource.Layout.fragment_recyclerview, menuRes)
        {
            _initialize();
        }

        private void _initialize()
        {
            Create += OnCreate;
        }

        private void OnCreate(object sender, OnCreateEventArgs e)
        {
            _recycler = e.View.FindViewById<RecyclerView>(Resource.Id.fragment_recyclerview_view);

            _view_loading = e.View.FindViewById(Resource.Id.fragment_recyclerview_loading);
            _view_empty = e.View.FindViewById(Resource.Id.fragment_recyclerview_empty);
            _view_error = e.View.FindViewById(Resource.Id.fragment_recyclerview_error);

            _tv_empty = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_recyclerview_empty_text);
            _tv_error = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_recyclerview_error_text);
            _tv_progress = e.View.FindViewById<MaterialTextView>(Resource.Id.fragment_recyclerview_loading_textview);

            _image_empty = e.View.FindViewById<AppCompatImageView>(Resource.Id.fragment_recyclerview_empty_image);
            _image_error = e.View.FindViewById<AppCompatImageView>(Resource.Id.fragment_recyclerview_error_image);

            e.View.FindViewById<MaterialButton>(Resource.Id.fragment_recyclerview_error_retry).SetOnClickListener(this);
        }

        public void DoTask(Task task, Action post_action)
        {
            Activity.RunOnUiThread(async () =>
            {
                try
                {
                    ViewMode = RecyclerViewMode.Loading;

                    await task;

                    ViewMode = RecyclerViewMode.Data;

                    post_action();
                }
                catch (Exception ex)
                {
                    ViewMode = RecyclerViewMode.Error;
#if DEBUG
                    new MaterialAlertDialogBuilder(Activity)
                        .SetTitle(Resource.String.title_error)
                        .SetMessage(ex.ToString())
                        .SetPositiveButton(Android.Resource.String.Ok, (dialog, args2) => { })
                        .Show();
#endif
                }
            });
        }

        public RecyclerView.Adapter Adapter
        {
            get
            {
                return _adapter;
            }
            set
            {
                _adapter = value;
                _recycler.SetAdapter(_adapter);
            }
        }

        public RecyclerViewMode ViewMode
        {
            get
            {
                return _mode;
            }
            set
            {
                _hide_all();
                _show_view(value);
                _mode = value;
            }
        }

        public string EmptyText
        {
            get
            {
                return _tv_empty.Text;
            }
            set
            {
                _tv_empty.Text = value;
            }
        }

        public string ErrorText
        {
            get
            {
                return _tv_error.Text;
            }
            set
            {
                _tv_error.Text = value;
            }
        }

        public string ProgressText
        {
            get
            {
                return _tv_progress.Text;
            }
            set
            {
                _tv_progress.Text = value;
            }
        }

        public void SetEmptyImage(int image)
        {
            _image_empty.SetImageResource(image);
        }

        public void SetErrorImage(int image)
        {
            _image_error.SetImageResource(image);
        }

        private void _hide_all()
        {
            _recycler.Visibility = ViewStates.Gone;
            _view_loading.Visibility = ViewStates.Gone;
            _view_empty.Visibility = ViewStates.Gone;
            _view_error.Visibility = ViewStates.Gone;
        }

        private void _show_view(RecyclerViewMode mode)
        {
            switch (mode)
            {
                case RecyclerViewMode.Loading:
                    _view_loading.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Empty:
                    _view_empty.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Error:
                    _view_error.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Data:
                    if (_adapter.ItemCount > 0)
                    {
                        _recycler.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        _show_view(RecyclerViewMode.Empty);
                    }
                    break;
            }
        }

        public void OnClick(View v)
        {
            RetryClick?.Invoke(v, new EventArgs());
        }

        public event EventHandler RetryClick;

        private RecyclerView _recycler;
        private RecyclerViewMode _mode;
        private RecyclerView.Adapter _adapter;

        private View _view_loading, _view_empty, _view_error;
        private AppCompatImageView _image_empty, _image_error;
        private MaterialTextView _tv_empty, _tv_error, _tv_progress;
    }

    public enum RecyclerViewMode
    {
        Loading,
        Empty,
        Error,
        Data,
    }
}