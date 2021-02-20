namespace Madamin.Unfollow.Main
{
    public interface IActionBarContainer
    {
        void SetTitle(int res);
        void SetTitle(string title);
        void Show();
        void Hide();
    }

    public partial class MainActivity : IActionBarContainer
    {
        void IActionBarContainer.SetTitle(int res)
        {
            SupportActionBar.SetTitle(res);
        }

        void IActionBarContainer.SetTitle(string title)
        {
            SupportActionBar.Title = title;
        }

        void IActionBarContainer.Show()
        {
            SupportActionBar.Show();
        }

        void IActionBarContainer.Hide()
        {
            SupportActionBar.Hide();
        }

    }
}
