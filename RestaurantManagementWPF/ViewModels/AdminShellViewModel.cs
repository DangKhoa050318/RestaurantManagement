using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class AdminShellViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private readonly DialogService _dialogService;
        private string _currentUsername = "Admin";
        private Frame? _mainFrame;

        public AdminShellViewModel()
        {
            _authService = AuthenticationService.Instance;
            _dialogService = new DialogService();

            // Get current username from auth service
            CurrentUsername = _authService.CurrentUsername ?? "Admin";

            // Commands
            NavigateCommand = new RelayCommand(ExecuteNavigate);
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        #region Properties

        public string CurrentUsername
        {
            get => _currentUsername;
            set => SetProperty(ref _currentUsername, value);
        }

        public Frame? MainFrame
        {
            get => _mainFrame;
            set => SetProperty(ref _mainFrame, value);
        }

        #endregion

        #region Commands

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        #endregion

        #region Methods

        private void ExecuteNavigate(object? parameter)
        {
            if (parameter is string pageName && MainFrame != null)
            {
                Page? page = pageName switch
                {
                    "Dashboard" => new Views.Pages.DashboardPage(),
                    "POS" => new Views.Pages.POSPage(),
                    "Area" => new Views.Pages.AreaManagementPage(),
                    "Dish" => new Views.Pages.DishManagementPage(),
                    "Category" => new Views.Pages.CategoryManagementPage(),
                    "Customer" => new Views.Pages.CustomerManagementPage(),
                    "Report" => null, // new Views.Pages.OrderReportPage(),
                    "Chatbot" => new Views.Pages.ChatbotPage(),
                    _ => null
                };

                if (page != null)
                {
                    MainFrame.Navigate(page);
                }
                else
                {
                    _dialogService.ShowMessage(
                        $"Page '{pageName}' is under construction!",
                        "Coming Soon"
                    );
                }
            }
        }

        private void ExecuteLogout(object? parameter)
        {
            var confirm = _dialogService.ShowConfirmation(
                "Are you sure you want to logout?",
                "Confirm Logout"
            );

            if (confirm)
            {
                // Logout from auth service
                _authService.Logout();

                // Open login window
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();

                // Close current window
                CloseCurrentWindow();
            }
        }

        private void CloseCurrentWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.AdminShellWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        #endregion
    }
}
