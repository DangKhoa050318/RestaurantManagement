using Microsoft.Extensions.Configuration;

namespace RestaurantManagementWPF.Services
{
    /// <summary>
    /// Service for reading configuration from appsettings.json
    /// </summary>
    public class ConfigurationService
    {
        private static ConfigurationService? _instance;
        private readonly IConfiguration _configuration;

        private ConfigurationService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        public static ConfigurationService Instance
        {
            get
            {
                _instance ??= new ConfigurationService();
                return _instance;
            }
        }

        public string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name) ?? string.Empty;
        }

        public string GetAppSetting(string key)
        {
            // Support nested keys with colon separator (e.g., "Authentication:AdminUsername")
            return _configuration[key] ?? string.Empty;
        }

        public T? GetSection<T>(string sectionName) where T : class
        {
            return _configuration.GetSection(sectionName).Get<T>();
        }
    }
}
