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

        private Area? _selectedArea;
        private TableViewModel? _selectedTable;
        private Category? _selectedCategory;
        private OrderItemViewModel? _selectedOrderItem;

        private decimal _totalAmount;

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
            PaymentCommand = new RelayCommand(ExecutePayment, _ => OrderItems.Count > 0 && SelectedTable != null);
            ClearOrderCommand = new RelayCommand(ExecuteClearOrder, _ => OrderItems.Count > 0);

            // Load data
            LoadAreas();
            LoadCategories();
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
        public ICommand PaymentCommand { get; }
        public ICommand ClearOrderCommand { get; }

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

        private void ExecuteSelectTable(object? parameter)
        {
            if (parameter is TableViewModel table)
            {
                if (table.Status?.ToLower() == "using")
                {
                    _dialogService.ShowWarning("This table is currently in use!", "Table Occupied");
                    return;
                }

                SelectedTable = table;
                _dialogService.ShowSuccess($"Selected table: {table.TableName}");
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
            // TODO: Open PaymentDialog
            _dialogService.ShowMessage(
                $"Payment functionality will be implemented next!\n\n" +
                $"Table: {SelectedTableName}\n" +
                $"Total: {TotalAmount:N0} VND",
                "Payment"
            );
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

        #endregion
    }
}
