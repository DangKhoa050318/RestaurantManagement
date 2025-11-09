using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class DishManagementViewModel : BaseViewModel
    {
        private readonly DishService _dishService;
        private readonly CategoryService _categoryService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Dish> _dishes = new();
        private ObservableCollection<Category> _categories = new();
        private Dish? _selectedDish;
        private Category? _selectedCategoryFilter;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public DishManagementViewModel()
        {
            _dishService = new DishService();
            _categoryService = new CategoryService();
            _dialogService = new DialogService();

            // Commands
            AddDishCommand = new RelayCommand(ExecuteAddDish);
            EditDishCommand = new RelayCommand(ExecuteEditDish, _ => SelectedDish != null);
            DeleteDishCommand = new RelayCommand(ExecuteDeleteDish, _ => SelectedDish != null);
            RefreshCommand = new RelayCommand(async _ => await LoadDishesAsync());
            SearchCommand = new RelayCommand(_ => ApplyFilter());
            ClearFilterCommand = new RelayCommand(ExecuteClearFilter);

            // Load data
            _ = LoadCategoriesAsync();
            _ = LoadDishesAsync();
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
                    ApplyFilter();
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
                    ApplyFilter();
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
                        SelectedCategoryFilter = Categories[0]; // Select "All"
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
                        Dishes.Clear();
                        foreach (var dish in dishes)
                        {
                            Dishes.Add(dish);
                        }
                    });
                });

                ApplyFilter();
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
            // Will implement search and category filter
            _ = LoadDishesAsync();
        }

        private void ExecuteClearFilter(object? parameter)
        {
            SearchText = string.Empty;
            SelectedCategoryFilter = Categories.FirstOrDefault();
        }

        private void ExecuteAddDish(object? parameter)
        {
            _dialogService.ShowMessage("Add Dish - Coming soon!");
        }

        private void ExecuteEditDish(object? parameter)
        {
            if (SelectedDish == null) return;
            _dialogService.ShowMessage($"Edit Dish: {SelectedDish.Name} - Coming soon!");
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
