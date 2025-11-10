using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.Views.Dialogs;
using RestaurantManagementWPF.ViewModels.Dialogs;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataAccessLayer.Repositories.Implementations;

namespace RestaurantManagementWPF.ViewModels
{
    public class DishManagementViewModel : BaseViewModel
    {
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Dish> _dishes = new();
        private ObservableCollection<Dish> _allDishes = new(); // Cache all dishes for filtering
        private ObservableCollection<Category> _categories = new();
        private Dish? _selectedDish;
        private Category? _selectedCategoryFilter;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isInitializing = true; // Prevent filter during initialization

        public DishManagementViewModel()
        {
            _dishService = new DishService(DishRepository.Instance);
            _categoryService = new CategoryService(CategoryRepository.Instance);
            _dialogService = new DialogService();

            // Commands
            AddDishCommand = new RelayCommand(ExecuteAddDish);
            EditDishCommand = new RelayCommand(ExecuteEditDish, _ => SelectedDish != null);
            DeleteDishCommand = new RelayCommand(ExecuteDeleteDish, _ => SelectedDish != null);
            RefreshCommand = new RelayCommand(async _ => await LoadDishesAsync());
            SearchCommand = new RelayCommand(_ => ApplyFilter());
            ClearFilterCommand = new RelayCommand(ExecuteClearFilter);

            // Load data asynchronously
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            _isInitializing = true;
            await LoadCategoriesAsync();
            await LoadDishesAsync();
            _isInitializing = false;
        }

        #region Properties

        public ObservableCollection<Dish> Dishes
        {
            get => _dishes;
            set => SetProperty(ref _dishes, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Dish? SelectedDish
        {
            get => _selectedDish;
            set
            {
                if (SetProperty(ref _selectedDish, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Category? SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set
            {
                if (SetProperty(ref _selectedCategoryFilter, value))
                {
                    // Only apply filter after initialization complete
                    if (!_isInitializing)
                    {
                        ApplyFilter();
                    }
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Only apply filter after initialization complete
                    if (!_isInitializing)
                    {
                        ApplyFilter();
                    }
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

        public ICommand AddDishCommand { get; }
        public ICommand EditDishCommand { get; }
        public ICommand DeleteDishCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFilterCommand { get; }

        #endregion

        #region Methods

        private async Task LoadCategoriesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var categories = _categoryService.GetCategories();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Categories.Clear();
                        // Add "All Categories" option
                        Categories.Add(new Category { CategoryId = 0, Name = "All Categories" });
                        foreach (var category in categories)
                        {
                            Categories.Add(category);
                        }
                        // This won't trigger filter because _isInitializing = true
                        SelectedCategoryFilter = Categories[0];
                    });
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading categories: {ex.Message}");
            }
        }

        private async Task LoadDishesAsync()
        {
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    var dishes = _dishService.GetDishes();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Store all dishes in cache
                        _allDishes.Clear();
                        foreach (var dish in dishes)
                        {
                            _allDishes.Add(dish);
                        }
                        
                        // Apply current filter to display
                        ApplyFilterInternal();
                    });
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading dishes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            // Filter on cached data - NO RELOAD!
            ApplyFilterInternal();
        }

        private void ApplyFilterInternal()
        {
            // Start with all dishes
            var filtered = _allDishes.AsEnumerable();

            // Filter by category
            if (SelectedCategoryFilter != null && SelectedCategoryFilter.CategoryId != 0)
            {
                filtered = filtered.Where(d => d.CategoryId == SelectedCategoryFilter.CategoryId);
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(d => 
                    d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (d.Description != null && d.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            // Update displayed dishes
            Dishes.Clear();
            foreach (var dish in filtered)
            {
                Dishes.Add(dish);
            }
        }

        private void ExecuteClearFilter(object? parameter)
        {
            // Prevent triggering filter multiple times
            _isInitializing = true;
            SearchText = string.Empty;
            SelectedCategoryFilter = Categories.FirstOrDefault();
            _isInitializing = false;
            
            // Apply filter once
            ApplyFilter();
        }

        private void ExecuteAddDish(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("=== ExecuteAddDish called ===");
            
            try
            {
                var dialog = new AddDishDialog();
                System.Diagnostics.Debug.WriteLine("Dialog created");
                
                var viewModel = new AddDishDialogViewModel();
                System.Diagnostics.Debug.WriteLine($"ViewModel created. Categories count: {viewModel.Categories.Count}");
                
                dialog.DataContext = viewModel;
                System.Diagnostics.Debug.WriteLine("DataContext set");

                System.Diagnostics.Debug.WriteLine("Calling dialog.ShowDialog()...");
                var dialogResult = dialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"Dialog closed. Result: {dialogResult}, ViewModel.DialogResult: {viewModel.DialogResult}");

                if (dialogResult == true && viewModel.DialogResult)
                {
                    System.Diagnostics.Debug.WriteLine("? User clicked CREATE, processing dish...");
                    System.Diagnostics.Debug.WriteLine($"Data: Name='{viewModel.DishName}', Price='{viewModel.Price}', Category={viewModel.SelectedCategory?.Name}, Unit='{viewModel.SelectedUnit}'");
                    
                    // Parse price with proper error handling
                    if (!decimal.TryParse(viewModel.Price, System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out decimal priceValue))
                    {
                        System.Diagnostics.Debug.WriteLine("? ERROR: Price parsing failed");
                        _dialogService.ShowError("Invalid price format. Please enter a valid number.");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"? Price parsed successfully: {priceValue}");

                    // Create new Dish object
                    var newDish = new Dish
                    {
                        Name = viewModel.DishName,
                        CategoryId = viewModel.SelectedCategory?.CategoryId,
                        Price = priceValue,
                        UnitOfCalculation = viewModel.SelectedUnit,
                        Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description,
                        ImgUrl = string.IsNullOrWhiteSpace(viewModel.ImageUrl) ? null : viewModel.ImageUrl
                    };

                    System.Diagnostics.Debug.WriteLine($"? Dish object created: ID={newDish.DishId}, Name='{newDish.Name}', CategoryId={newDish.CategoryId}, Price={newDish.Price}");
                    System.Diagnostics.Debug.WriteLine("?? Calling _dishService.AddDish()...");

                    // Save to database
                    _dishService.AddDish(newDish);

                    System.Diagnostics.Debug.WriteLine("? Dish saved to database successfully!");

                    // Show success message
                    _dialogService.ShowSuccess($"Dish '{newDish.Name}' created successfully!");

                    // Reload dishes
                    System.Diagnostics.Debug.WriteLine("?? Reloading dishes...");
                    _ = LoadDishesAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("? User cancelled the dialog or DialogResult = false");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"?? EXCEPTION in ExecuteAddDish: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                _dialogService.ShowError($"Error creating dish: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine("=== ExecuteAddDish completed ===\n");
        }

        private void ExecuteEditDish(object? parameter)
        {
            if (SelectedDish == null) return;

            // System.Diagnostics.Debug.WriteLine("=== ExecuteEditDish called ===");
            
            try
            {
                var dialog = new EditDishDialog();
                var viewModel = new EditDishDialogViewModel(SelectedDish);
                dialog.DataContext = viewModel;

                var dialogResult = dialog.ShowDialog();
                // System.Diagnostics.Debug.WriteLine($"Dialog closed. Result: {dialogResult}");

                if (dialogResult == true && viewModel.DialogResult)
                {
                    // System.Diagnostics.Debug.WriteLine("User clicked SAVE, updating dish...");
                    
                    // Parse price with proper error handling
                    if (!decimal.TryParse(viewModel.Price, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal priceValue))
                    {
                        // System.Diagnostics.Debug.WriteLine("ERROR: Price parsing failed");
                        _dialogService.ShowError("Invalid price format. Please enter a valid number.");
                        return;
                    }

                    // Update dish object with new values
                    var updatedDish = new Dish
                    {
                        DishId = viewModel.DishId,
                        Name = viewModel.DishName,
                        CategoryId = viewModel.SelectedCategory?.CategoryId,
                        Price = priceValue,
                        UnitOfCalculation = viewModel.SelectedUnit,
                        Description = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description,
                        ImgUrl = string.IsNullOrWhiteSpace(viewModel.ImageUrl) ? null : viewModel.ImageUrl
                    };

                    // System.Diagnostics.Debug.WriteLine("Updating dish in database...");

                    // Save changes to database
                    _dishService.UpdateDish(updatedDish);

                    // System.Diagnostics.Debug.WriteLine("Dish updated successfully!");

                    // Show success message
                    _dialogService.ShowSuccess($"Dish '{updatedDish.Name}' updated successfully!");

                    // Reload dishes
                    _ = LoadDishesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in ExecuteEditDish: {ex.Message}");
                _dialogService.ShowError($"Error updating dish: {ex.Message}");
            }
            
            // System.Diagnostics.Debug.WriteLine("=== ExecuteEditDish completed ===");
        }

        private void ExecuteDeleteDish(object? parameter)
        {
            if (SelectedDish == null) return;

            var confirm = _dialogService.ShowConfirmation(
                $"Are you sure you want to delete dish '{SelectedDish.Name}'?\n\n" +
                "This action cannot be undone.",
                "Confirm Delete"
            );

            if (confirm)
            {
                try
                {
                    _dishService.DeleteDish(SelectedDish.DishId);
                    _dialogService.ShowSuccess($"Dish '{SelectedDish.Name}' deleted successfully!");
                    SelectedDish = null;
                    _ = LoadDishesAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Cannot delete dish: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
