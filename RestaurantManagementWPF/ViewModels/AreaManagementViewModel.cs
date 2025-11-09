using BusinessLogicLayer.Services.Implementations;
using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.ViewModels.Models;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class AreaManagementViewModel : BaseViewModel
    {
        private readonly AreaService _areaService;
        private readonly TableService _tableService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Area> _areas = new();
        private Area? _selectedArea;
        private ObservableCollection<Table> _tablesInSelectedArea = new();
        private Table? _selectedTable;
        private bool _isLoading;

        public AreaManagementViewModel()
        {
            _areaService = new AreaService();
            _tableService = new TableService();
            _dialogService = new DialogService();

            // Area Commands
            AddAreaCommand = new RelayCommand(ExecuteAddArea);
            EditAreaCommand = new RelayCommand(ExecuteEditArea, _ => SelectedArea != null);
            DeleteAreaCommand = new RelayCommand(ExecuteDeleteArea, _ => SelectedArea != null);
            RefreshCommand = new RelayCommand(async _ => await LoadAreasAsync());

            // Table Commands
            AddTableCommand = new RelayCommand(ExecuteAddTable, _ => SelectedArea != null);
            EditTableCommand = new RelayCommand(ExecuteEditTable);
            DeleteTableCommand = new RelayCommand(ExecuteDeleteTable);
            SelectTableCommand = new RelayCommand(ExecuteSelectTable);

            // Load data
            _ = LoadAreasAsync();
        }

        #region Properties

        public ObservableCollection<Area> Areas
        {
            get => _areas;
            set => SetProperty(ref _areas, value);
        }

        public Area? SelectedArea
        {
            get => _selectedArea;
            set
            {
                if (SetProperty(ref _selectedArea, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                    LoadTablesForSelectedArea();
                }
            }
        }

        public ObservableCollection<Table> TablesInSelectedArea
        {
            get => _tablesInSelectedArea;
            set => SetProperty(ref _tablesInSelectedArea, value);
        }

        public Table? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    CommandManager.InvalidateRequerySuggested();
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

        public ICommand AddAreaCommand { get; }
        public ICommand EditAreaCommand { get; }
        public ICommand DeleteAreaCommand { get; }
        public ICommand RefreshCommand { get; }

        // Table Management Commands
        public ICommand AddTableCommand { get; }
        public ICommand EditTableCommand { get; }
        public ICommand DeleteTableCommand { get; }
        public ICommand SelectTableCommand { get; }

        #endregion

        #region Methods

        private async Task LoadAreasAsync()
        {
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    var areas = _areaService.GetAreas();
                    
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Areas.Clear();
                        foreach (var area in areas)
                        {
                            Areas.Add(area);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading areas: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadTablesForSelectedArea()
        {
            TablesInSelectedArea.Clear();

            if (SelectedArea != null)
            {
                try
                {
                    var tables = _tableService.GetTablesByArea(SelectedArea.AreaId);
                    foreach (var table in tables)
                    {
                        TablesInSelectedArea.Add(table);
                    }
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error loading tables: {ex.Message}");
                }
            }
        }

        private void ExecuteAddArea(object? parameter)
        {
            var dialog = new Views.Dialogs.AddAreaDialog();
            if (dialog.ShowDialog() == true && dialog.DataContext is ViewModels.Dialogs.AddAreaDialogViewModel vm)
            {
                try
                {
                    // Create new area WITHOUT navigation property
                    var newArea = new Area
                    {
                        AreaName = vm.AreaName,
                        AreaStatus = vm.AreaStatus,
                        Tables = null // Don't initialize collection when adding
                    };

                    _areaService.AddArea(newArea);
                    
                    // Debug: Check if AreaId was populated
                    if (newArea.AreaId == 0)
                    {
                        _dialogService.ShowError("Error: AreaId was not generated from database!");
                        return;
                    }

                    // Auto create tables if requested
                    if (vm.AutoCreateTables && vm.NumberOfTables > 0)
                    {
                        CreateTablesForArea(newArea.AreaId, vm.NumberOfTables);
                    }

                    _dialogService.ShowSuccess($"Area '{newArea.AreaName}' created successfully!");
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    // Show inner exception details
                    var errorMessage = $"Error creating area: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
                        
                        // Show even deeper if exists
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessage += $"\n\nDeepest Exception: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    _dialogService.ShowError(errorMessage);
                }
            }
        }

        private void CreateTablesForArea(int areaId, int numberOfTables)
        {
            for (int i = 1; i <= numberOfTables; i++)
            {
                var table = new Table
                {
                    TableName = $"Table {i:D2}",
                    Status = "Empty", // Valid values: Empty, Using, Booked, Maintenance
                    AreaId = areaId
                };

                _tableService.AddTable(table);
            }
        }

        private void ExecuteEditArea(object? parameter)
        {
            if (SelectedArea == null) return;

            var dialog = new Views.Dialogs.EditAreaDialog(SelectedArea);
            if (dialog.ShowDialog() == true && dialog.DataContext is ViewModels.Dialogs.EditAreaDialogViewModel vm)
            {
                try
                {
                    var updatedArea = new Area
                    {
                        AreaId = SelectedArea.AreaId,
                        AreaName = vm.AreaName,
                        AreaStatus = vm.AreaStatus
                    };

                    _areaService.UpdateArea(updatedArea);
                    _dialogService.ShowSuccess($"Area '{updatedArea.AreaName}' updated successfully!");
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error updating area: {ex.Message}");
                }
            }
        }

        private void ExecuteDeleteArea(object? parameter)
        {
            if (SelectedArea == null) return;

            var tableCount = SelectedArea.Tables?.Count ?? 0;
            
            var message = tableCount > 0
                ? $"Are you sure you want to delete area '{SelectedArea.AreaName}'?\n\n" +
                  $"This will also DELETE {tableCount} table(s) in this area!\n\n" +
                  "Note: Areas with tables that have active orders cannot be deleted."
                : $"Are you sure you want to delete area '{SelectedArea.AreaName}'?";

            var confirm = _dialogService.ShowConfirmation(message, "Confirm Delete");

            if (confirm)
            {
                try
                {
                    _areaService.DeleteArea(SelectedArea.AreaId);
                    _dialogService.ShowSuccess($"Area '{SelectedArea.AreaName}' and all its tables deleted successfully!");
                    SelectedArea = null;
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Cannot delete area: {ex.Message}");
                }
            }
        }

        #region Table Management Methods

        private void ExecuteSelectTable(object? parameter)
        {
            if (parameter is Table table)
            {
                SelectedTable = table;
            }
        }

        private void ExecuteAddTable(object? parameter)
        {
            if (SelectedArea == null)
            {
                _dialogService.ShowWarning("Please select an area first!", "No Area Selected");
                return;
            }

            try
            {
                var dialog = new Views.Dialogs.AddTableDialog();
                var areas = new List<Area> { SelectedArea }; // Only current area
                var viewModel = new Dialogs.AddTableDialogViewModel(areas);
                viewModel.SelectedArea = SelectedArea; // Pre-select current area
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    var newTable = new Table
                    {
                        TableName = viewModel.TableName,
                        AreaId = viewModel.SelectedArea.AreaId,
                        Status = viewModel.SelectedStatus
                    };

                    _tableService.AddTable(newTable);
                    _dialogService.ShowSuccess($"Table '{newTable.TableName}' added successfully!");
                    LoadTablesForSelectedArea();
                    _ = LoadAreasAsync(); // Refresh to update table count
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to add table: {ex.Message}");
            }
        }

        private void ExecuteEditTable(object? parameter)
        {
            if (parameter is not Table table)
            {
                if (SelectedTable == null)
                {
                    _dialogService.ShowWarning("Please select a table first!", "No Table Selected");
                    return;
                }
                table = SelectedTable;
            }

            try
            {
                var dialog = new Views.Dialogs.EditTableDialog();
                var tableViewModel = new ViewModels.Models.TableViewModel
                {
                    TableId = table.TableId,
                    TableName = table.TableName,
                    Status = table.Status,
                    AreaId = table.AreaId,
                    AreaName = SelectedArea?.AreaName ?? ""
                };

                var areas = Areas.ToList();
                var viewModel = new Dialogs.EditTableDialogViewModel(tableViewModel, areas);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    var updatedTable = new Table
                    {
                        TableId = table.TableId,
                        TableName = viewModel.TableName,
                        AreaId = viewModel.SelectedArea.AreaId,
                        Status = viewModel.SelectedStatus
                    };

                    _tableService.UpdateTable(updatedTable);
                    _dialogService.ShowSuccess($"Table '{updatedTable.TableName}' updated successfully!");
                    LoadTablesForSelectedArea();
                    _ = LoadAreasAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to update table: {ex.Message}");
            }
        }

        private void ExecuteDeleteTable(object? parameter)
        {
            if (parameter is not Table table)
            {
                if (SelectedTable == null)
                {
                    _dialogService.ShowWarning("Please select a table first!", "No Table Selected");
                    return;
                }
                table = SelectedTable;
            }

            var confirm = _dialogService.ShowConfirmation(
                $"Are you sure you want to delete table '{table.TableName}'?\n\nThis action cannot be undone if the table has no orders.",
                "Delete Table"
            );

            if (confirm)
            {
                try
                {
                    _tableService.DeleteTable(table.TableId);
                    _dialogService.ShowSuccess($"Table '{table.TableName}' deleted successfully!");
                    LoadTablesForSelectedArea();
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Failed to delete table: {ex.Message}");
                }
            }
        }

        #endregion

        #endregion
    }
}
