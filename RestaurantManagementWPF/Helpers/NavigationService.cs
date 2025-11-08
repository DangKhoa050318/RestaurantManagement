using System.Windows.Controls;

namespace RestaurantManagementWPF.Helpers
{
    /// <summary>
    /// Service for handling navigation between views
    /// </summary>
    public class NavigationService
    {
        private Frame? _mainFrame;

        public Frame? MainFrame
        {
            get => _mainFrame;
            set => _mainFrame = value;
        }

        public void NavigateTo(Page page)
        {
            MainFrame?.Navigate(page);
        }

        public void GoBack()
        {
            if (MainFrame?.CanGoBack == true)
                MainFrame.GoBack();
        }

        public void GoForward()
        {
            if (MainFrame?.CanGoForward == true)
                MainFrame.GoForward();
        }
    }
}
