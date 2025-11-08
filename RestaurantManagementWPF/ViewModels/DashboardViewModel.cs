using RestaurantManagementWPF.Helpers;
using Services.Implementations;
using System.Windows.Input;
using System.Threading.Tasks;

namespace RestaurantManagementWPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TableService _tableService;
        private readonly OrderService _orderService;
        private readonly DishService _dishService;

        private int _totalTables;
        private int _availableTables;
        private int _activeOrders;
        private int _totalDishes;
        private bool _isLoading;

        public DashboardViewModel()
        {
            _tableService = new TableService();
            _orderService = new OrderService();
            _dishService = new DishService();

            // Commands
            NavigateToPOSCommand = new RelayCommand(_ => NavigateTo("POS"));
            NavigateToAreaCommand = new RelayCommand(_ => NavigateTo("Area"));
            NavigateToDishCommand = new RelayCommand(_ => NavigateTo("Dish"));
            NavigateToReportCommand = new RelayCommand(_ => NavigateTo("Report"));

            // Load statistics asynchronously (non-blocking)
            _ = LoadStatisticsAsync();
        }

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int TotalTables
        {
            get => _totalTables;
            set => SetProperty(ref _totalTables, value);
        }

        public int AvailableTables
        {
            get => _availableTables;
            set => SetProperty(ref _availableTables, value);
        }

        public int ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
        }

        public int TotalDishes
        {
            get => _totalDishes;
            set => SetProperty(ref _totalDishes, value);
        }

        #endregion

        #region Commands

        public ICommand NavigateToPOSCommand { get; }
        public ICommand NavigateToAreaCommand { get; }
        public ICommand NavigateToDishCommand { get; }
        public ICommand NavigateToReportCommand { get; }

        #endregion

        #region Methods

        private async Task LoadStatisticsAsync()
        {
            IsLoading = true;

            try
            {
                // Run database calls on background thread
                await Task.Run(() =>
                {
                    // Get all tables
                    var tables = _tableService.GetTables();
                    TotalTables = tables.Count;

                    // Count available tables
                    AvailableTables = tables.Count(t => 
                        t.Status != null && 
                        t.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));

                    // Get active orders (not completed or cancelled)
                    var orders = _orderService.GetOrders();
                    ActiveOrders = orders.Count(o => 
                        o.Status != null && 
                        !o.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                        !o.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase));

                    // Get total dishes
                    var dishes = _dishService.GetDishes();
                    TotalDishes = dishes.Count;
                });
            }
            catch (Exception ex)
            {
                // In case of error, set default values
                TotalTables = 0;
                AvailableTables = 0;
                ActiveOrders = 0;
                TotalDishes = 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateTo(string pageName)
        {
            // This will be handled by AdminShellViewModel's NavigateCommand
            // For now, just a placeholder
        }

        #endregion
    }
}
