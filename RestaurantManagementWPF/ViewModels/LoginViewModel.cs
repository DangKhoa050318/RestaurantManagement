using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using System.Windows;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthenticationService _authService;
        private readonly DialogService _dialogService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;

        public LoginViewModel()
        {
            _authService = AuthenticationService.Instance;
            _dialogService = new DialogService();

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }

        #region Properties

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Methods

        private bool CanExecuteLogin(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoading;
        }

        private void ExecuteLogin(object? parameter)
        {
            IsLoading = true;

            try
            {
                // Validate credentials
                bool isValid = _authService.Login(Username, Password);

                if (isValid)
                {
                    // Open main window
                    var mainWindow = new Views.AdminShellWindow();
                    mainWindow.Show();

                    // Close login window
                    CloseLoginWindow();
                }
                else
                {
                    _dialogService.ShowError(
                        "Tên ??ng nh?p ho?c m?t kh?u không ?úng!\n\nVui lòng th? l?i.",
                        "??ng nh?p th?t b?i"
                    );
                    
                    // Clear password
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Có l?i x?y ra khi ??ng nh?p:\n{ex.Message}",
                    "L?i"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CloseLoginWindow()
        {
            // Find and close the login window
            foreach (Window window in Application.Current.Windows)
            {
                if (window is Views.LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        #endregion
    }
}
