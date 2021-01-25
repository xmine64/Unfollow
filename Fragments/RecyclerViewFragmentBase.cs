using System;
using System.Threading.Tasks;

using Android.Views;

using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;

using Google.Android.Material.Button;
using Google.Android.Material.TextView;

namespace Madamin.Unfollow.Fragments
{
    public class RecyclerViewFragmentBase :
        FragmentBase,
        View.IOnClickListener
    {
        protected const string BundleKeyAccountIndex = "account_index";


        protected RecyclerViewFragmentBase(int menuRes) :
            base(Resource.Layout.fragment_recyclerview, menuRes)
        {
            Initialize();
        }

        public RecyclerViewMode ViewMode
        {
            get => _mode;
            set
            {
                HideAll();
                ShowView(value);
                _mode = value;
            }
        }

        public string EmptyText
        {
            get => _tvEmpty.Text;
            set => _tvEmpty.Text = value;
        }

        public string ErrorText
        {
            get => _tvError.Text;
            set => _tvError.Text = value;
        }

        public string ProgressText
        {
            get => _tvProgress.Text;
            set => _tvProgress.Text = value;
        }

        public void OnClick(View v)
        {
            RetryClick?.Invoke(v, new EventArgs());
        }

        private void Initialize()
        {
            Create += OnCreate;
        }

        private void OnCreate(object sender, OnFragmentCreateEventArgs e)
        {
            _recycler = e.View.FindViewById<RecyclerView>(Resource.Id.fragment_recyclerview_view);

            _viewLoading = e.View.FindViewById(
                Resource.Id.fragment_recyclerview_loading);
            _viewEmpty = e.View.FindViewById(
                Resource.Id.fragment_recyclerview_empty);
            _viewError = e.View.FindViewById(
                Resource.Id.fragment_recyclerview_error);

            _tvEmpty = e.View.FindViewById<MaterialTextView>(
                Resource.Id.fragment_recyclerview_empty_text);
            _tvError = e.View.FindViewById<MaterialTextView>(
                Resource.Id.fragment_recyclerview_error_text);
            _tvProgress = e.View.FindViewById<MaterialTextView>(
                Resource.Id.fragment_recyclerview_loading_textview);

            _imageEmpty = e.View.FindViewById<AppCompatImageView>(
                Resource.Id.fragment_recyclerview_empty_image);
            _imageError = e.View.FindViewById<AppCompatImageView>(
                Resource.Id.fragment_recyclerview_error_image);

            e.View.FindViewById<MaterialButton>(Resource.Id.fragment_recyclerview_error_retry)?
                .SetOnClickListener(this);
        }

        protected void DoTask(Task task, Action postAction)
        {
            Activity.RunOnUiThread(async () =>
            {
                try
                {
                    ViewMode = RecyclerViewMode.Loading;

                    await task;

                    postAction();

                    ViewMode = RecyclerViewMode.Data;
                }
                catch (Exception ex)
                {
                    ViewMode = RecyclerViewMode.Error;
                    ((IErrorHost) Activity).ShowError(ex);
                }
            });
        }

        public void SetEmptyImage(int image)
        {
            _imageEmpty.SetImageResource(image);
        }

        public void SetErrorImage(int image)
        {
            _imageError.SetImageResource(image);
        }

        protected void SetAdapter(RecyclerView.Adapter adapter)
        {
            _adapter = adapter;
            _recycler.SetAdapter(_adapter);
        }

        private void HideAll()
        {
            _recycler.Visibility = ViewStates.Gone;
            _viewLoading.Visibility = ViewStates.Gone;
            _viewEmpty.Visibility = ViewStates.Gone;
            _viewError.Visibility = ViewStates.Gone;
        }

        private void ShowView(RecyclerViewMode mode)
        {
            switch (mode)
            {
                case RecyclerViewMode.Loading:
                    _viewLoading.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Empty:
                    _viewEmpty.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Error:
                    _viewError.Visibility = ViewStates.Visible;
                    break;
                case RecyclerViewMode.Data:
                    if (_adapter.ItemCount > 0)
                    {
                        _recycler.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        ViewMode = RecyclerViewMode.Empty;
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            ViewModeChanged?.Invoke(this, mode);
        }

        public event EventHandler RetryClick;
        public event EventHandler<RecyclerViewMode> ViewModeChanged;

        private RecyclerView.Adapter _adapter;
        private AppCompatImageView _imageEmpty, _imageError;
        private RecyclerViewMode _mode;

        private RecyclerView _recycler;
        private MaterialTextView _tvEmpty, _tvError, _tvProgress;

        private View _viewLoading, _viewEmpty, _viewError;
    }

    public enum RecyclerViewMode
    {
        Loading,
        Empty,
        Error,
        Data
    }
}