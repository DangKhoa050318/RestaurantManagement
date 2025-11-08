using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class EditAreaDialogViewModel : BaseViewModel
    {
        private string _areaName = string.Empty;
        private string _areaStatus = "Using";

        public EditAreaDialogViewModel(Area area)
        {
            // Pre-fill with existing data
            AreaName = area.AreaName;
            AreaStatus = area.AreaStatus;

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public string AreaName
        {
            get => _areaName;
            set
            {
                if (SetProperty(ref _areaName, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string AreaStatus
        {
            get => _areaStatus;
            set => SetProperty(ref _areaStatus, value);
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
            return !string.IsNullOrWhiteSpace(AreaName);
        }

        private void ExecuteOK(object? parameter)
        {
            DialogResult = true;
        }

        #endregion
    }
}
