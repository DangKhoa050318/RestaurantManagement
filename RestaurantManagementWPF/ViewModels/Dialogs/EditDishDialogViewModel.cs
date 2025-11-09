using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataAccessLayer.Repositories.Implementations;
namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class EditDishDialogViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;
        private readonly DebounceHelper _debounceHelper = new();
        
        private int _dishId;
        private string _dishName = string.Empty;
        private string _description = string.Empty;
        private string _price = string.Empty;
        private string _imageUrl = string.Empty;
        private Category? _selectedCategory;
        private string? _selectedUnit;

        public EditDishDialogViewModel(Dish dish)
        {
            _categoryService = new CategoryService(CategoryRepository.Instance);

            // Initialize Units
            Units = new ObservableCollection<string>
            {
                "portion", "dish", "bowl", "plate", "cup", "glass", "bottle", "can"
            };

            // Load Categories first
            LoadCategories();

            // Pre-fill with existing dish data
            DishId = dish.DishId;
            DishName = dish.Name;
            Price = dish.Price.ToString("F2");
            SelectedUnit = dish.UnitOfCalculation ?? "portion";
            Description = dish.Description ?? string.Empty;
            ImageUrl = dish.ImgUrl ?? string.Empty;

            // Find and select the matching category
            if (dish.CategoryId.HasValue)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == dish.CategoryId.Value);
            }

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public ObservableCollection<Category> Categories { get; set; } = new();
        public ObservableCollection<string> Units { get; set; }

        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

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
                var categories = _categoryService.GetCategories();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch
            {
                // Suppress exceptions from category loading
            }
        }

        private bool CanExecuteOK(object? parameter)
        {
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
