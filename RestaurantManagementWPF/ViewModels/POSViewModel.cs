using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.ViewModels.Models;
using Services.Implementations;
using BusinessLogicLayer.Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class POSViewModel : BaseViewModel
    {
        private readonly AreaService _areaService;
        private readonly TableService _tableService;
        private readonly CategoryService _categoryService;
        private readonly DishService _dishService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Area> _areas = new();
        private ObservableCollection<TableViewModel> _tables = new();
        private ObservableCollection<Category> _categories = new();
        private ObservableCollection<Dish> _dishes = new();
        private ObservableCollection<OrderItemViewModel> _orderItems = new();
        private ObservableCollection<Customer> _customers = new();

        private Area? _selectedArea;
        private TableViewModel? _selectedTable;
        private Category? _selectedCategory;
        private OrderItemViewModel? _selectedOrderItem;
        private Customer? _selectedCustomer;

        private decimal _totalAmount;
        private int? _currentOrderId; // Track current order ID

        public POSViewModel()
        {
            _areaService = new AreaService();
            _tableService = new TableService();
            _categoryService = new CategoryService();
            _dishService = new DishService();
            _dialogService = new DialogService();

            // Commands
            SelectTableCommand = new RelayCommand(ExecuteSelectTable);
            AddDishCommand = new RelayCommand(ExecuteAddDish);
            IncreaseQuantityCommand = new RelayCommand(ExecuteIncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand(ExecuteDecreaseQuantity);
            RemoveItemCommand = new RelayCommand(ExecuteRemoveItem);
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer);
            SaveOrderCommand = new RelayCommand(ExecuteSaveOrder, _ => OrderItems.Count > 0 && SelectedTable != null);
            PaymentCommand = new RelayCommand(ExecutePayment, _ => OrderItems.Count > 0 && SelectedTable != null);
            ClearOrderCommand = new RelayCommand(ExecuteClearOrder, _ => OrderItems.Count > 0);

            // Load data
            LoadAreas();
            LoadCategories();
            LoadCustomers();
        }

        #region Properties

        public ObservableCollection<Area> Areas
        {
            get => _areas;
            set => SetProperty(ref _areas, value);
        }

        public ObservableCollection<TableViewModel> Tables
        {
            get => _tables;
            set => SetProperty(ref _tables, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set => SetProperty(ref _dishes, value);
        }

        public ObservableCollection<OrderItemViewModel> OrderItems
        {
            get => _orderItems;
            set => SetProperty(ref _orderItems, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Area? SelectedArea
        {
            get => _selectedArea;
            set
            {
                if (SetProperty(ref _selectedArea, value))
                {
                    LoadTablesForArea();
                }
            }
        }

        public TableViewModel? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    OnPropertyChanged(nameof(SelectedTableName));
                    OnPropertyChanged(nameof(SelectedAreaName));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    LoadDishesForCategory();
                }
            }
        }

        public OrderItemViewModel? SelectedOrderItem
        {
            get => _selectedOrderItem;
            set => SetProperty(ref _selectedOrderItem, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public string SelectedTableName => SelectedTable?.TableName ?? "Not selected";
        public string SelectedAreaName => SelectedTable?.AreaName ?? "-";

        #endregion

        #region Commands

        public ICommand SelectTableCommand { get; }
        public ICommand AddDishCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand PaymentCommand { get; }
        public ICommand ClearOrderCommand { get; }
        public ICommand AddCustomerCommand { get; }

        #endregion

        #region Methods

        private void LoadAreas()
        {
            try
            {
                var areas = _areaService.GetAreas();
                Areas.Clear();
                foreach (var area in areas)
                {
                    Areas.Add(area);
                }

                if (Areas.Count > 0)
                {
                    SelectedArea = Areas[0];
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading areas: {ex.Message}");
            }
        }

        private void LoadTablesForArea()
        {
            if (SelectedArea == null) return;

            try
            {
                var tables = _tableService.GetTablesByArea(SelectedArea.AreaId);
                Tables.Clear();
                
                foreach (var table in tables)
                {
                    Tables.Add(new TableViewModel
                    {
                        TableId = table.TableId,
                        TableName = table.TableName,
                        Status = table.Status ?? "Empty",
                        AreaId = table.AreaId,
                        AreaName = SelectedArea.AreaName
                    });
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading tables: {ex.Message}");
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _categoryService.GetCategories();
                Categories.Clear();
                
                // Add "All" option
                Categories.Add(new Category { CategoryId = 0, Name = "All Dishes" });
                
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                if (Categories.Count > 0)
                {
                    SelectedCategory = Categories[0];
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading categories: {ex.Message}");
            }
        }

        private void LoadDishesForCategory()
        {
            try
            {
                List<Dish> dishes;

                if (SelectedCategory == null || SelectedCategory.CategoryId == 0)
                {
                    dishes = _dishService.GetDishes();
                }
                else
                {
                    dishes = _dishService.GetDishesByCategoryId(SelectedCategory.CategoryId);
                }

                Dishes.Clear();
                foreach (var dish in dishes)
                {
                    Dishes.Add(dish);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading dishes: {ex.Message}");
            }
        }

        private void LoadCustomers()
        {
            try
            {
                var customerService = new global::Services.Implementations.CustomerService();
                var customers = customerService.GetCustomers();
                Customers.Clear();
                
                // Add "No Customer" option
                Customers.Add(new Customer { CustomerId = 0, Fullname = "Walk-in Customer" });
                
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                // Default to walk-in
                SelectedCustomer = Customers[0];
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading customers: {ex.Message}");
            }
        }

        private void ExecuteSelectTable(object? parameter)
        {
            if (parameter is TableViewModel table)
            {
                if (table.Status?.ToLower() == "using")
                {
                    // Table is occupied - load existing order
                    var confirm = _dialogService.ShowConfirmation(
                        $"Table {table.TableName} is currently in use.\nDo you want to load the existing order?",
                        "Table In Use"
                    );

                    if (confirm)
                    {
                        SelectedTable = table;
                        LoadExistingOrderForTable(table.TableId);
                    }
                    return;
                }

                // Clear previous order when selecting empty table
                OrderItems.Clear();
                _currentOrderId = null;
                CalculateTotal();
                
                SelectedTable = table;
                _dialogService.ShowSuccess($"Selected table: {table.TableName}");
            }
        }

        private void LoadExistingOrderForTable(int tableId)
        {
            try
            {
                var orderService = new global::Services.Implementations.OrderService();
                var orders = orderService.GetOrdersByTableId(tableId);

                // Find pending/scheduled order (not completed)
                var pendingOrder = orders.FirstOrDefault(o => o.Status != "Completed" && o.Status != "Cancelled");

                if (pendingOrder != null)
                {
                    _currentOrderId = pendingOrder.OrderId;

                    // Load order details
                    var orderDetailService = new global::Services.Implementations.OrderDetailService();
                    var orderDetails = orderDetailService.GetOrderDetailsByOrderId(pendingOrder.OrderId);

                    OrderItems.Clear();
                    foreach (var detail in orderDetails)
                    {
                        OrderItems.Add(new OrderItemViewModel
                        {
                            DishId = detail.DishId,
                            DishName = detail.Dish?.Name ?? "Unknown",
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice
                        });
                    }

                    CalculateTotal();
                    _dialogService.ShowSuccess($"Loaded existing order (ID: {pendingOrder.OrderId}) for table {SelectedTable.TableName}");
                }
                else
                {
                    _dialogService.ShowWarning($"No pending order found for table {SelectedTable.TableName}");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to load order: {ex.Message}");
            }
        }

        private void ExecuteAddDish(object? parameter)
        {
            if (parameter is Dish dish)
            {
                // Check if table is selected
                if (SelectedTable == null)
                {
                    _dialogService.ShowWarning("Please select a table first!", "No Table Selected");
                    return;
                }

                // Check if dish already in order
                var existingItem = OrderItems.FirstOrDefault(x => x.DishId == dish.DishId);
                
                if (existingItem != null)
                {
                    // Increase quantity
                    existingItem.Quantity++;
                }
                else
                {
                    // Add new item
                    OrderItems.Add(new OrderItemViewModel
                    {
                        DishId = dish.DishId,
                        DishName = dish.Name,
                        Quantity = 1,
                        UnitPrice = dish.Price
                    });
                }

                CalculateTotal();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteIncreaseQuantity(object? parameter)
        {
            if (parameter is OrderItemViewModel item)
            {
                item.Quantity++;
                CalculateTotal();
            }
        }

        private void ExecuteDecreaseQuantity(object? parameter)
        {
            if (parameter is OrderItemViewModel item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    CalculateTotal();
                }
                else
                {
                    // Remove if quantity becomes 0
                    ExecuteRemoveItem(item);
                }
            }
        }

        private void ExecuteRemoveItem(object? parameter)
        {
            if (parameter is OrderItemViewModel item)
            {
                OrderItems.Remove(item);
                CalculateTotal();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void CalculateTotal()
        {
            TotalAmount = OrderItems.Sum(x => x.Subtotal);
        }

        private void ExecutePayment(object? parameter)
        {
            if (SelectedTable == null || OrderItems.Count == 0)
            {
                _dialogService.ShowWarning("Please select a table and add items to order!", "Cannot Process Payment");
                return;
            }

            try
            {
                // Open Payment Dialog
                var dialog = new Views.Dialogs.PaymentDialog();
                var viewModel = new Dialogs.PaymentDialogViewModel(
                    SelectedTable.TableName,
                    SelectedTable.AreaName,
                    OrderItems,
                    TotalAmount
                );
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    // Process payment - Save order to database
                    SaveOrderToDatabase();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Payment failed: {ex.Message}");
            }
        }

        private void SaveOrderToDatabase()
        {
            try
            {
                var orderService = new global::Services.Implementations.OrderService();
                var orderDetailService = new global::Services.Implementations.OrderDetailService();

                if (_currentOrderId.HasValue)
                {
                    // UPDATE existing order to Completed
                    System.Diagnostics.Debug.WriteLine($"Updating existing order {_currentOrderId.Value} to Completed");
                    
                    var existingOrder = orderService.GetOrderById(_currentOrderId.Value);
                    if (existingOrder != null)
                    {
                        // Update status to Completed
                        orderService.UpdateOrderStatus(_currentOrderId.Value, "Completed");

                        // Update Table Status to "Empty"
                        _tableService.UpdateTableStatus(SelectedTable.TableId, "Empty");

                        // Show success message
                        _dialogService.ShowSuccess($"Payment completed successfully!\n\nOrder ID: {_currentOrderId.Value}\nTotal: {TotalAmount:N0} VND");

                        // Clear current order
                        OrderItems.Clear();
                        _currentOrderId = null;
                        SelectedTable = null;
                        CalculateTotal();
                        CommandManager.InvalidateRequerySuggested();

                        // Reload tables to update status
                        LoadTablesForArea();
                        return;
                    }
                }

                // CREATE new order (if no existing order - walk-in payment without save)
                System.Diagnostics.Debug.WriteLine("Creating new completed order (direct payment)");
                
                var newOrder = new Order
                {
                    TableId = SelectedTable.TableId,
                    CustomerId = (SelectedCustomer != null && SelectedCustomer.CustomerId > 0) ? SelectedCustomer.CustomerId : null,
                    OrderTime = DateTime.Now,
                    TotalAmount = TotalAmount,
                    Status = "Completed"
                };

                orderService.AddOrder(newOrder);

                // Save Order Details
                foreach (var item in OrderItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        DishId = item.DishId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    orderDetailService.AddOrderDetail(orderDetail);
                }

                // Update Table Status to "Empty"
                _tableService.UpdateTableStatus(SelectedTable.TableId, "Empty");

                // Show success message
                _dialogService.ShowSuccess($"Payment completed successfully!\n\nOrder ID: {newOrder.OrderId}\nTotal: {TotalAmount:N0} VND");

                // Clear current order
                OrderItems.Clear();
                SelectedTable = null;
                _currentOrderId = null;
                CalculateTotal();
                CommandManager.InvalidateRequerySuggested();

                // Reload tables to update status
                LoadTablesForArea();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save order: {ex.Message}", ex);
            }
        }

        private void ExecuteSaveOrder(object? parameter)
        {
            if (SelectedTable == null || OrderItems.Count == 0)
            {
                _dialogService.ShowWarning("Please select a table and add items to order!", "Cannot Save Order");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("=== ExecuteSaveOrder START ===");
                System.Diagnostics.Debug.WriteLine($"Table: {SelectedTable.TableName}, Items: {OrderItems.Count}, CurrentOrderId: {_currentOrderId}");

                var orderService = new global::Services.Implementations.OrderService();
                var orderDetailService = new global::Services.Implementations.OrderDetailService();

                if (_currentOrderId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"Updating existing order: {_currentOrderId.Value}");
                    
                    // Update existing order
                    var existingOrder = orderService.GetOrderById(_currentOrderId.Value);
                    if (existingOrder != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Found existing order, deleting old details...");
                        
                        // Delete old order details
                        var oldDetails = orderDetailService.GetOrderDetailsByOrderId(_currentOrderId.Value);
                        System.Diagnostics.Debug.WriteLine($"Old details count: {oldDetails.Count}");
						
						if (oldDetails.Count > 0)
						{
							foreach (var detail in oldDetails)
							{
								System.Diagnostics.Debug.WriteLine($"Deleting detail ID: {detail.OrderDetailId}");
								orderDetailService.DeleteOrderDetail(detail.OrderDetailId);
							}
						}

                        System.Diagnostics.Debug.WriteLine("Adding new details...");
                        // Add new order details
                        foreach (var item in OrderItems)
                        {
                            System.Diagnostics.Debug.WriteLine($"Adding: {item.DishName} x {item.Quantity}");
                            var orderDetail = new OrderDetail
                            {
                                OrderId = _currentOrderId.Value,
                                DishId = item.DishId,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            };
                            orderDetailService.AddOrderDetail(orderDetail);
                        }

                        System.Diagnostics.Debug.WriteLine("Order updated successfully!");
                        _dialogService.ShowSuccess($"Order updated successfully!\n\nOrder ID: {_currentOrderId.Value}");
                        LoadTablesForArea();
                        return;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Creating new order...");
                
                // Create new order
                var newOrder = new Order
                {
                    TableId = SelectedTable.TableId,
                    CustomerId = (SelectedCustomer != null && SelectedCustomer.CustomerId > 0) ? SelectedCustomer.CustomerId : null,
                    OrderTime = DateTime.Now,
                    TotalAmount = TotalAmount,
                    Status = "Scheduled" // Changed from "Pending" to match DB constraint
                };

                System.Diagnostics.Debug.WriteLine($"Order data: TableId={newOrder.TableId}, Total={newOrder.TotalAmount}, Status={newOrder.Status}");
                orderService.AddOrder(newOrder);
                System.Diagnostics.Debug.WriteLine($"Order created with ID: {newOrder.OrderId}");

                // Save Order Details
                System.Diagnostics.Debug.WriteLine("Saving order details...");
                foreach (var item in OrderItems)
                {
                    System.Diagnostics.Debug.WriteLine($"Adding detail: {item.DishName} x {item.Quantity} @ {item.UnitPrice}");
                    var orderDetail = new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        DishId = item.DishId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    orderDetailService.AddOrderDetail(orderDetail);
                }

                System.Diagnostics.Debug.WriteLine($"Updating table {SelectedTable.TableId} status to Using...");
                // Update Table Status to "Using"
                _tableService.UpdateTableStatus(SelectedTable.TableId, "Using");

                // Track current order ID
                _currentOrderId = newOrder.OrderId;
                System.Diagnostics.Debug.WriteLine($"Tracked order ID: {_currentOrderId}");

                _dialogService.ShowSuccess($"Order saved successfully!\nTable {SelectedTable.TableName} is now marked as 'Using'.\n\nOrder ID: {newOrder.OrderId}");
                LoadTablesForArea();
                
                System.Diagnostics.Debug.WriteLine("=== ExecuteSaveOrder END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? ERROR in ExecuteSaveOrder: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack: {ex.InnerException.StackTrace}");
                }
                _dialogService.ShowError($"Failed to save order: {ex.Message}\n\nDetails: {ex.InnerException?.Message}");
            }
        }

        private void ExecuteClearOrder(object? parameter)
        {
            var confirm = _dialogService.ShowConfirmation(
                "Are you sure you want to clear the current order?",
                "Clear Order"
            );

            if (confirm)
            {
                OrderItems.Clear();
                SelectedTable = null;
                CalculateTotal();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ExecuteAddCustomer(object? parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.AddCustomerDialog();
                var viewModel = new Dialogs.AddCustomerDialogViewModel();
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    var customerService = new global::Services.Implementations.CustomerService();
                    var newCustomer = new Customer
                    {
                        Fullname = viewModel.CustomerName,
                        Phone = string.IsNullOrWhiteSpace(viewModel.PhoneNumber) ? null : viewModel.PhoneNumber
                    };

                    customerService.AddCustomer(newCustomer);
                    _dialogService.ShowSuccess($"Customer '{newCustomer.Fullname}' added successfully!");
                    
                    // Reload customers and select the new one
                    LoadCustomers();
                    SelectedCustomer = Customers.FirstOrDefault(c => c.Fullname == newCustomer.Fullname);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to add customer: {ex.Message}");
            }
        }

        #endregion
    }
}
