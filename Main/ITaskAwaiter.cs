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
                return;

            _container.ShowLoadingView();
            
            try
            {
                await task;
            }
            catch (Exception exception)
            {
                _container.ShowErrorView(exception);
            }

            TaskDone?.Invoke(this, null);

            _container.ShowContentView();
        }

        public event EventHandler TaskDone;
    }
}