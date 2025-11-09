using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class CategoryManagementViewModel : BaseViewModel
    {
        private readonly CategoryService _categoryService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Category> _categories = new();
        private Category? _selectedCategory;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public CategoryManagementViewModel()
        {
            _categoryService = new CategoryService();
            _dialogService = new DialogService();

            // Commands
            AddCategoryCommand = new RelayCommand(ExecuteAddCategory);
            EditCategoryCommand = new RelayCommand(ExecuteEditCategory, _ => SelectedCategory != null);
            DeleteCategoryCommand = new RelayCommand(ExecuteDeleteCategory, _ => SelectedCategory != null);
            RefreshCommand = new RelayCommand(async _ => await LoadCategoriesAsync());
            SearchCommand = new RelayCommand(_ => ApplySearch());

            // Load data
            _ = LoadCategoriesAsync();
        }

        #region Properties

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplySearch();
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

        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }

        #endregion

        #region Methods

        private async Task LoadCategoriesAsync()
        {
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    var categories = _categoryService.GetCategories();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Categories.Clear();
                        foreach (var category in categories)
                        {
                            Categories.Add(category);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading categories: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplySearch()
        {
            // Reload and filter
            _ = LoadCategoriesAsync();
        }

        private void ExecuteAddCategory(object? parameter)
        {
            var dialog = new Views.Dialogs.AddCategoryDialog();
            if (dialog.ShowDialog() == true && dialog.DataContext is ViewModels.Dialogs.AddCategoryDialogViewModel vm)
            {
                try
                {
                    var newCategory = new Category
                    {
                        Name = vm.CategoryName,
                        Description = vm.Description
                    };

                    _categoryService.AddCategory(newCategory);
                    _dialogService.ShowSuccess($"Category '{newCategory.Name}' created successfully!");
                    _ = LoadCategoriesAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error creating category: {ex.Message}");
                }
            }
        }

        private void ExecuteEditCategory(object? parameter)
        {
            if (SelectedCategory == null) return;

            var dialog = new Views.Dialogs.EditCategoryDialog(SelectedCategory);
            if (dialog.ShowDialog() == true && dialog.DataContext is ViewModels.Dialogs.EditCategoryDialogViewModel vm)
            {
                try
                {
                    var updatedCategory = new Category
                    {
                        CategoryId = SelectedCategory.CategoryId,
                        Name = vm.CategoryName,
                        Description = vm.Description
                    };

                    _categoryService.UpdateCategory(updatedCategory);
                    _dialogService.ShowSuccess($"Category '{updatedCategory.Name}' updated successfully!");
                    _ = LoadCategoriesAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error updating category: {ex.Message}");
                }
            }
        }

        private void ExecuteDeleteCategory(object? parameter)
        {
            if (SelectedCategory == null) return;

            var dishCount = SelectedCategory.Dishes?.Count ?? 0;

            var message = dishCount > 0
                ? $"Are you sure you want to delete category '{SelectedCategory.Name}'?\n\n" +
                  $"Warning: This category has {dishCount} dish(es).\n" +
                  "All dishes in this category will also be deleted!"
                : $"Are you sure you want to delete category '{SelectedCategory.Name}'?";

            var confirm = _dialogService.ShowConfirmation(message, "Confirm Delete");

            if (confirm)
            {
                try
                {
                    _categoryService.DeleteCategory(SelectedCategory.CategoryId);
                    _dialogService.ShowSuccess($"Category '{SelectedCategory.Name}' deleted successfully!");
                    SelectedCategory = null;
                    _ = LoadCategoriesAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Cannot delete category: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
