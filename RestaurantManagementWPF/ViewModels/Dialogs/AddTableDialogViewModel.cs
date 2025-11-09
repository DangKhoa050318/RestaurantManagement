using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class AddTableDialogViewModel : BaseViewModel
    {
        private string _tableName = string.Empty;
        private Area? _selectedArea;
        private string _selectedStatus = "Empty";
        private ObservableCollection<string> _statuses = new();
        private bool _isSingleMode = true;
        private int _numberOfTables = 1;

        public AddTableDialogViewModel(List<Area> areas)
        {
            Areas = new ObservableCollection<Area>(areas);
            
            // Load statuses
            Statuses.Add("Empty");
            Statuses.Add("Booked");
            Statuses.Add("Maintenance");
            
            SelectedStatus = "Empty";

            // Select first area if available
            if (Areas.Count > 0)
            {
                SelectedArea = Areas[0];
            }

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
        }

        #region Properties

        public ObservableCollection<Area> Areas { get; set; }

        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public string TableName
        {
            get => _tableName;
            set
            {
                if (SetProperty(ref _tableName, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Area? SelectedArea
        {
            get => _selectedArea;
            set
            {
                if (SetProperty(ref _selectedArea, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public bool IsSingleMode
        {
            get => _isSingleMode;
            set
            {
                if (SetProperty(ref _isSingleMode, value))
                {
                    OnPropertyChanged(nameof(IsMultipleMode));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsMultipleMode
        {
            get => !_isSingleMode;
            set
            {
                IsSingleMode = !value;
            }
        }

        public int NumberOfTables
        {
            get => _numberOfTables;
            set
            {
                if (SetProperty(ref _numberOfTables, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }

        #endregion

        #region Methods

        private bool CanExecuteOK(object? parameter)
        {
            if (SelectedArea == null)
                return false;

            if (IsSingleMode)
            {
                return !string.IsNullOrWhiteSpace(TableName);
            }
            else
            {
                return NumberOfTables > 0 && NumberOfTables <= 50; // Limit to 50 tables
            }
        }

        private void ExecuteOK(object? parameter)
        {
            DialogResult = true;
            
            if (System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault(w => w.DataContext == this) is System.Windows.Window window)
            {
                window.DialogResult = true;
            }
        }

        #endregion
    }
}
