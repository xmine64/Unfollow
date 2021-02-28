using System;
using System.Threading.Tasks;

namespace Madamin.Unfollow.Main
{
    public class TaskAwaiter
    {
        private IFragmentContainer _container;

        public TaskAwaiter(IFragmentContainer fragmentContainer)
        {
            _container = fragmentContainer;
        }

        public async void AwaitTask(Task task)
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
                if (Error != null)
                {
                    Error.Invoke(this, exception);
                }
                else
                {
                    _container.ShowErrorView(exception);
                }
            }

            TaskDone?.Invoke(this, null);
        }

        public event EventHandler TaskDone;
        public event EventHandler<Exception> Error;
    }
}