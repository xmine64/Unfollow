using System;
using System.Threading.Tasks;

namespace Madamin.Unfollow.Main
{
    public class TaskAwaiter
    {
        private IFragmentContainer _container;
        private Task _lastTask;

        public TaskAwaiter(IFragmentContainer fragmentContainer)
        {
            _container = fragmentContainer;
        }

        public async void AwaitTask(Task task)
        {
            await AwaitTaskAsync(task);
        }

        public async Task AwaitTaskAsync(Task task)
        {
            if (task == null)
            { 
                TaskDone?.Invoke(this, null);
                return;
            }

            _container.ShowLoadingView();

            try
            {
                await task;
                _container.ShowContentView();
            }
            catch (Exception exception)
            {
                _lastTask = task;

                if (Error != null)
                {
                    _container.ShowErrorView();
                    Error.Invoke(this, exception);
                }
                else
                {
                    _container.ShowErrorView(exception);
                }
            }

            TaskDone?.Invoke(this, null);
        }

        public async void Retry()
        {
            if (_lastTask != null)
                await AwaitTaskAsync(_lastTask);
        }

        public event EventHandler TaskDone;
        public event EventHandler<Exception> Error;
    }
}