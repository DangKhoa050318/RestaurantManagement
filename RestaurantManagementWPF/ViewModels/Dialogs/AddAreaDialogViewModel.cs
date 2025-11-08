using RestaurantManagementWPF.Helpers;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class AddAreaDialogViewModel : BaseViewModel
    {
        private string _areaName = string.Empty;
        private string _areaStatus = "Using";
        private bool _autoCreateTables;
        private int _numberOfTables = 10;

        public AddAreaDialogViewModel()
        {
            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
            IncrementTablesCommand = new RelayCommand(_ => NumberOfTables++);
            DecrementTablesCommand = new RelayCommand(_ => { if (NumberOfTables > 1) NumberOfTables--; });
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

        public bool AutoCreateTables
        {
            get => _autoCreateTables;
            set => SetProperty(ref _autoCreateTables, value);
        }

        public int NumberOfTables
        {
            get => _numberOfTables;
            set
            {
                if (value < 1) value = 1;
                if (value > 100) value = 100;
                SetProperty(ref _numberOfTables, value);
            }
        }

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand IncrementTablesCommand { get; }
        public ICommand DecrementTablesCommand { get; }

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
