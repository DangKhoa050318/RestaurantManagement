namespace RestaurantManagementWPF.Services
{
    /// <summary>
    /// Service for handling user authentication
    /// </summary>
    public class AuthenticationService
    {
        private static AuthenticationService? _instance;
        private readonly ConfigurationService _configService;
        
        private string? _currentUsername;
        private bool _isAuthenticated;

        private AuthenticationService()
        {
            _configService = ConfigurationService.Instance;
        }

        public static AuthenticationService Instance
        {
            get
            {
                _instance ??= new AuthenticationService();
                return _instance;
            }
        }

        /// <summary>
        /// Current logged in username
        /// </summary>
        public string? CurrentUsername => _currentUsername;

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated => _isAuthenticated;

        /// <summary>
        /// Validate user credentials
        /// </summary>
        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            // Get credentials from appsettings.json
            var adminUsername = _configService.GetAppSetting("Authentication:AdminUsername");
            var adminPassword = _configService.GetAppSetting("Authentication:AdminPassword");

            // Validate credentials
            if (username.Equals(adminUsername, StringComparison.OrdinalIgnoreCase) &&
                password.Equals(adminPassword))
            {
                _currentUsername = username;
                _isAuthenticated = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        public void Logout()
        {
            _currentUsername = null;
            _isAuthenticated = false;
        }
    }
}
