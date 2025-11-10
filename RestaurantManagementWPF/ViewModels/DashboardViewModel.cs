using RestaurantManagementWPF.Helpers;
using Services.Implementations;
using System.Windows.Input;
using System.Threading.Tasks;
using DataAccessLayer.Repositories.Implementations;

namespace RestaurantManagementWPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TableService _tableService;
        private readonly OrderService _orderService;
        private readonly DishService _dishService;
        private readonly CustomerService _customerService;

        private int _totalTables;
        private int _availableTables;
        private int _tablesInUse;
        private int _activeOrders;
        private int _completedOrdersToday;
        private int _totalDishes;
        private int _totalCustomers;
        private decimal _todayRevenue;
        private bool _isLoading;

        public DashboardViewModel()
        {
            _tableService = new TableService(TableRepository.Instance);
            _orderService = new OrderService();
            _dishService = new DishService(DishRepository.Instance);
            _customerService = new CustomerService(CustomerRepository.Instance);

            // Commands
            NavigateToPOSCommand = new RelayCommand(_ => NavigateTo("POS"));
            NavigateToAreaCommand = new RelayCommand(_ => NavigateTo("Area"));
            NavigateToDishCommand = new RelayCommand(_ => NavigateTo("Dish"));
            NavigateToReportCommand = new RelayCommand(_ => NavigateTo("Report"));
            RefreshCommand = new RelayCommand(_ => _ = LoadStatisticsAsync());

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

        public int TablesInUse
        {
            get => _tablesInUse;
            set => SetProperty(ref _tablesInUse, value);
        }

        public int ActiveOrders
        {
            get => _activeOrders;
            set => SetProperty(ref _activeOrders, value);
        }

        public int CompletedOrdersToday
        {
            get => _completedOrdersToday;
            set => SetProperty(ref _completedOrdersToday, value);
        }

        public int TotalDishes
        {
            get => _totalDishes;
            set => SetProperty(ref _totalDishes, value);
        }

        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set => SetProperty(ref _todayRevenue, value);
        }

        /// <summary>
        /// Formatted revenue string for display
        /// </summary>
        public string TodayRevenueDisplay => $"{TodayRevenue:N0} VND";

        #endregion

        #region Commands

        public ICommand NavigateToPOSCommand { get; }
        public ICommand NavigateToAreaCommand { get; }
        public ICommand NavigateToDishCommand { get; }
        public ICommand NavigateToReportCommand { get; }
        public ICommand RefreshCommand { get; }

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
                    // ? FIX 1: Get all tables with correct status
                    var tables = _tableService.GetTables();
                    TotalTables = tables.Count;

                    // ? Count EMPTY tables (not "Available")
                    AvailableTables = tables.Count(t => 
                        t.Status != null && 
                        t.Status.Equals("Empty", StringComparison.OrdinalIgnoreCase));

                    // ? Count tables in use
                    TablesInUse = tables.Count(t => 
                        t.Status != null && 
                        t.Status.Equals("Using", StringComparison.OrdinalIgnoreCase));

                    // ? FIX 2: Get active orders (only "Scheduled" status)
                    var allOrders = _orderService.GetOrders();
                    ActiveOrders = allOrders.Count(o => 
                        o.Status != null && 
                        o.Status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase));

                    // ? Get completed orders today
                    var today = DateTime.Now.Date;
                    var todayOrders = allOrders.Where(o => 
                        o.OrderTime.Date == today &&
                        o.Status != null &&
                        o.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                    
                    CompletedOrdersToday = todayOrders.Count();

                    // ? Calculate today's revenue
                    TodayRevenue = todayOrders.Sum(o => o.TotalAmount);

                    // ? Get total dishes
                    var dishes = _dishService.GetDishes();
                    TotalDishes = dishes.Count;

                    // ? Get total customers
                    var customers = _customerService.GetCustomers();
                    TotalCustomers = customers.Count;

                    // Notify revenue display updated
                    OnPropertyChanged(nameof(TodayRevenueDisplay));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard statistics: {ex.Message}");
                
                // In case of error, set default values
                TotalTables = 0;
                AvailableTables = 0;
                TablesInUse = 0;
                ActiveOrders = 0;
                CompletedOrdersToday = 0;
                TotalDishes = 0;
                TotalCustomers = 0;
                TodayRevenue = 0;
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
            System.Diagnostics.Debug.WriteLine($"Navigate to: {pageName}");
        }

        #endregion
    }
}
