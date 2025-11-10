using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class EditCategoryDialogViewModel : BaseViewModel
    {
        private string _categoryName = string.Empty;
        private string _description = string.Empty;

        public EditCategoryDialogViewModel(Category category)
        {
            // Pre-fill with existing data
            CategoryName = category.Name;
            Description = category.Description ?? string.Empty;

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (SetProperty(ref _categoryName, value))
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

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

        private bool CanExecuteOK(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(CategoryName);
        }

        private void ExecuteOK(object? parameter)
        {
            DialogResult = true;
        }

        #endregion
    }
}
