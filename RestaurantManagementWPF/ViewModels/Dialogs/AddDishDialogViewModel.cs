using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class AddDishDialogViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;
        private readonly DebounceHelper _debounceHelper = new();
        
        private string _dishName = string.Empty;
        private string _description = string.Empty;
        private string _price = string.Empty;
        private string _imageUrl = string.Empty;
        private Category? _selectedCategory;
        private string? _selectedUnit;

        public AddDishDialogViewModel()
        {
            _categoryService = new CategoryService();

            // Initialize Units
            Units = new ObservableCollection<string>
            {
                "portion", "dish", "bowl", "plate", "cup", "glass", "bottle", "can"
            };
            SelectedUnit = "portion"; // Default

            // Load Categories
            LoadCategories();

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public ObservableCollection<Category> Categories { get; set; } = new();
        public ObservableCollection<string> Units { get; set; }

        public string DishName
        {
            get => _dishName;
            set
            {
                if (SetProperty(ref _dishName, value))
                {
                    // Debounce: only requery after 300ms of no typing
                    _debounceHelper.Debounce(300, () =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
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
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                {
                    // Debounce: only requery after 300ms of no typing
                    _debounceHelper.Debounce(300, () =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
            }
        }

        public string? SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (SetProperty(ref _selectedUnit, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private void LoadCategories()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting to load categories...");
                var categories = _categoryService.GetCategories();
                System.Diagnostics.Debug.WriteLine($"Loaded {categories.Count} categories from service");
                
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                    System.Diagnostics.Debug.WriteLine($"Added category: {category.Name}");
                }

                // Select first category by default
                if (Categories.Count > 0)
                {
                    SelectedCategory = Categories[0];
                    System.Diagnostics.Debug.WriteLine($"Selected default category: {SelectedCategory.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: No categories found in database!");
                }
            }
            catch (Exception ex)
            {
                // Log error or show message
                System.Diagnostics.Debug.WriteLine($"ERROR loading categories: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private bool CanExecuteOK(object? parameter)
        {
            // Remove debug log in production - only for debugging
            // System.Diagnostics.Debug.WriteLine($"CanExecuteOK called - Name:'{DishName}', Category:{SelectedCategory?.Name}, Price:'{Price}', Unit:'{SelectedUnit}'");
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(DishName))
                return false;

            if (SelectedCategory == null)
                return false;

            if (string.IsNullOrWhiteSpace(Price))
                return false;

            // Validate price is a valid decimal number
            if (!decimal.TryParse(Price, out decimal priceValue) || priceValue <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(SelectedUnit))
                return false;

            return true;
        }

        private void ExecuteOK(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine("ExecuteOK called - setting DialogResult = true");
            DialogResult = true;
            
            // Close the dialog by accessing the Window
            if (System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                System.Diagnostics.Debug.WriteLine("Found window, setting window.DialogResult = true");
                window.DialogResult = true;
            }
        }

        #endregion
    }
}
