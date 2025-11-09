using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.ViewModels.Models;
using Services.Implementations;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class OrderReportViewModel : BaseViewModel
    {
        private readonly OrderService _orderService;
        private readonly DialogService _dialogService;

        private ObservableCollection<OrderViewModel> _orders = new();
        private OrderViewModel? _selectedOrder;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _totalOrders;
        private decimal _totalRevenue;

        public OrderReportViewModel()
        {
            _orderService = new OrderService();
            _dialogService = new DialogService();

            // Initialize date range: Last 30 days
            EndDate = DateTime.Now.Date;
            StartDate = EndDate.AddDays(-30);

            // Commands
            SearchCommand = new RelayCommand(ExecuteSearch);
            ResetCommand = new RelayCommand(ExecuteReset);
            ViewDetailsCommand = new RelayCommand(ExecuteViewDetails);

            // Load initial data
            LoadOrders();
        }

        #region Properties

        public ObservableCollection<OrderViewModel> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public OrderViewModel? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public int TotalOrders
        {
            get => _totalOrders;
            set => SetProperty(ref _totalOrders, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        #endregion

        #region Commands

        public ICommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ViewDetailsCommand { get; }

        #endregion

        #region Methods

        private void LoadOrders()
        {
            try
            {
                // Validate date range
                if (StartDate > EndDate)
                {
                    _dialogService.ShowWarning("Start date must be before or equal to end date!", "Invalid Date Range");
                    return;
                }

                // Get orders from service
                var orders = _orderService.GetOrdersByDateRange(StartDate, EndDate);

                // Convert to ViewModels
                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(new OrderViewModel
                    {
                        OrderId = order.OrderId,
                        TableId = order.TableId,
                        TableName = order.Table?.TableName ?? "N/A",
                        AreaName = order.Table?.Area?.AreaName ?? "N/A",
                        CustomerId = order.CustomerId,
                        CustomerName = order.Customer?.Fullname ?? "Walk-in",
                        OrderTime = order.OrderTime,
                        PaymentTime = order.PaymentTime,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount
                    });
                }

                // Update statistics
                TotalOrders = Orders.Count;
                TotalRevenue = Orders
                    .Where(o => o.Status.ToLower() == "completed")
                    .Sum(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to load orders: {ex.Message}");
            }
        }

        private void ExecuteSearch(object? parameter)
        {
            LoadOrders();
        }

        private void ExecuteReset(object? parameter)
        {
            // Reset to last 30 days
            EndDate = DateTime.Now.Date;
            StartDate = EndDate.AddDays(-30);
            
            LoadOrders();
        }

        private void ExecuteViewDetails(object? parameter)
        {
            if (parameter is OrderViewModel order)
            {
                try
                {
                    // Get full order with details
                    var fullOrder = _orderService.GetOrderById(order.OrderId);
                    
                    if (fullOrder == null)
                    {
                        _dialogService.ShowWarning("Order not found!", "Error");
                        return;
                    }

                    // Build details message
                    var details = $"Order ID: {fullOrder.OrderId}\n" +
                                  $"Table: {fullOrder.Table?.TableName} ({fullOrder.Table?.Area?.AreaName})\n" +
                                  $"Customer: {fullOrder.Customer?.Fullname ?? "Walk-in"}\n" +
                                  $"Order Time: {fullOrder.OrderTime:dd/MM/yyyy HH:mm}\n" +
                                  $"Payment Time: {(fullOrder.PaymentTime.HasValue ? fullOrder.PaymentTime.Value.ToString("dd/MM/yyyy HH:mm") : "Not paid")}\n" +
                                  $"Status: {fullOrder.Status}\n\n" +
                                  "Order Items:\n";

                    if (fullOrder.OrderDetails != null && fullOrder.OrderDetails.Any())
                    {
                        int index = 1;
                        foreach (var detail in fullOrder.OrderDetails)
                        {
                            details += $"{index}. {detail.Dish?.Name ?? "Unknown"}\n" +
                                      $"   {detail.Quantity} x {detail.UnitPrice:N0} = {(detail.Quantity * detail.UnitPrice):N0} VND\n";
                            index++;
                        }
                    }
                    else
                    {
                        details += "No items\n";
                    }

                    details += $"\nTotal Amount: {fullOrder.TotalAmount:N0} VND";

                    _dialogService.ShowSuccess(details, "Order Details");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Failed to load order details: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
