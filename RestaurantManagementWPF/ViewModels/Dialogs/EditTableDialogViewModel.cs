using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.ViewModels.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class EditTableDialogViewModel : BaseViewModel
    {
        private string _tableName = string.Empty;
        private Area? _selectedArea;
        private string _selectedStatus = "Empty";
        private ObservableCollection<string> _statuses = new();

        public EditTableDialogViewModel(TableViewModel table, List<Area> areas)
        {
            Areas = new ObservableCollection<Area>(areas);
            
            // Load statuses
            Statuses.Add("Empty");
            Statuses.Add("Using");
            Statuses.Add("Booked");
            Statuses.Add("Maintenance");

            // Pre-fill data
            TableName = table.TableName;
            SelectedStatus = table.Status ?? "Empty";
            SelectedArea = Areas.FirstOrDefault(a => a.AreaId == table.AreaId);

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

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }

        #endregion

        #region Methods

        private bool CanExecuteOK(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(TableName) && SelectedArea != null;
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
