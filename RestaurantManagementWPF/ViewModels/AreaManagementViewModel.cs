using BusinessLogicLayer.Services.Implementations;
using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.ViewModels.Models;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DataAccessLayer.Repositories.Implementations;

namespace RestaurantManagementWPF.ViewModels
{
    public class AreaManagementViewModel : BaseViewModel
    {
        private readonly AreaService _areaService;
        private readonly TableService _tableService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Area> _areas = new();
        private Area? _selectedArea;
        private ObservableCollection<TableViewModel> _tablesInSelectedArea = new();
        private ObservableCollection<TableViewModel> _selectedTables = new();
        private bool _isLoading;
        private bool _isSelectionModeEnabled = false;

        public AreaManagementViewModel()
        {
            _areaService = new AreaService(AreaRepository.Instance);
            _tableService = new TableService(TableRepository.Instance);
            _dialogService = new DialogService();

            AddAreaCommand = new RelayCommand(ExecuteAddArea);
            EditAreaCommand = new RelayCommand(ExecuteEditArea, _ => SelectedArea != null);
            DeleteAreaCommand = new RelayCommand(ExecuteDeleteArea, _ => SelectedArea != null);
            RefreshCommand = new RelayCommand(async _ => await LoadAreasAsync());

            AddTableCommand = new RelayCommand(ExecuteAddTable, _ => SelectedArea != null);
            // ? FIX: Always enabled - will check parameter inside Execute
            EditTableCommand = new RelayCommand(ExecuteEditTable);
            DeleteTableCommand = new RelayCommand(ExecuteDeleteTable);
            SelectTableCommand = new RelayCommand(ExecuteSelectTable);
            ClearSelectionCommand = new RelayCommand(_ => ClearTableSelection());
            ToggleSelectionModeCommand = new RelayCommand(ExecuteToggleSelectionMode);

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
                    ClearTableSelection();
                }
            }
        }

        public ObservableCollection<TableViewModel> TablesInSelectedArea
        {
            get => _tablesInSelectedArea;
            set => SetProperty(ref _tablesInSelectedArea, value);
        }

        public ObservableCollection<TableViewModel> SelectedTables
        {
            get => _selectedTables;
            set => SetProperty(ref _selectedTables, value);
        }

        public TableViewModel? SelectedTable => SelectedTables.FirstOrDefault();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsSelectionModeEnabled
        {
            get => _isSelectionModeEnabled;
            set
            {
                if (SetProperty(ref _isSelectionModeEnabled, value))
                {
                    OnPropertyChanged(nameof(SelectionModeText));
                    OnPropertyChanged(nameof(SelectionModeIcon));
                    if (!value) ClearTableSelection();
                }
            }
        }

        public string SelectionModeText => IsSelectionModeEnabled ? "Disable Selection" : "Enable Selection";
        public string SelectionModeIcon => IsSelectionModeEnabled ? "?" : "?";
        public string SelectedCountText => SelectedTables.Count > 0 
            ? $"{SelectedTables.Count} table(s) selected" 
            : "No tables selected";

        #endregion

        #region Commands

        public ICommand AddAreaCommand { get; }
        public ICommand EditAreaCommand { get; }
        public ICommand DeleteAreaCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddTableCommand { get; }
        public ICommand EditTableCommand { get; }
        public ICommand DeleteTableCommand { get; }
        public ICommand SelectTableCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand ToggleSelectionModeCommand { get; }

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
                        foreach (var area in areas) Areas.Add(area);
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
                        TablesInSelectedArea.Add(new TableViewModel
                        {
                            TableId = table.TableId,
                            TableName = table.TableName,
                            Status = table.Status ?? "Empty",
                            AreaId = table.AreaId,
                            AreaName = SelectedArea.AreaName,
                            IsSelected = false
                        });
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
                    var newArea = new Area
                    {
                        AreaName = vm.AreaName,
                        AreaStatus = vm.AreaStatus,
                        Tables = null
                    };

                    _areaService.AddArea(newArea);
                    
                    if (newArea.AreaId == 0)
                    {
                        _dialogService.ShowError("Error: AreaId was not generated from database!");
                        return;
                    }

                    if (vm.AutoCreateTables && vm.NumberOfTables > 0)
                    {
                        CreateTablesForArea(newArea.AreaId, vm.NumberOfTables);
                    }

                    _dialogService.ShowSuccess($"Area '{newArea.AreaName}' created successfully!");
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error creating area: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
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
                    Status = "Empty",
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
            if (!IsSelectionModeEnabled)
            {
                System.Diagnostics.Debug.WriteLine("Selection mode is disabled. Click ignored.");
                return;
            }

            if (parameter is TableViewModel table)
            {
                System.Diagnostics.Debug.WriteLine($"=== ExecuteSelectTable ===");
                System.Diagnostics.Debug.WriteLine($"Table: {table.TableName}, Selected: {table.IsSelected}");

                if (table.IsSelected)
                {
                    table.IsSelected = false;
                    SelectedTables.Remove(table);
                }
                else
                {
                    table.IsSelected = true;
                    SelectedTables.Add(table);
                }

                OnPropertyChanged(nameof(SelectedTable));
                OnPropertyChanged(nameof(SelectedCountText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ClearTableSelection()
        {
            foreach (var table in TablesInSelectedArea)
            {
                table.IsSelected = false;
            }
            SelectedTables.Clear();
            OnPropertyChanged(nameof(SelectedTable));
            OnPropertyChanged(nameof(SelectedCountText));
            CommandManager.InvalidateRequerySuggested();
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
                var areas = new List<Area> { SelectedArea };
                var viewModel = new Dialogs.AddTableDialogViewModel(areas);
                viewModel.SelectedArea = SelectedArea;
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    if (viewModel.IsSingleMode)
                    {
                        var newTable = new Table
                        {
                            TableName = viewModel.TableName,
                            AreaId = viewModel.SelectedArea.AreaId,
                            Status = viewModel.SelectedStatus
                        };
                        _tableService.AddTable(newTable);
                        _dialogService.ShowSuccess($"Table '{newTable.TableName}' added successfully!");
                    }
                    else
                    {
                        for (int i = 1; i <= viewModel.NumberOfTables; i++)
                        {
                            var newTable = new Table
                            {
                                TableName = $"Table {i:D2}",
                                AreaId = viewModel.SelectedArea.AreaId,
                                Status = viewModel.SelectedStatus
                            };
                            _tableService.AddTable(newTable);
                        }
                        _dialogService.ShowSuccess($"{viewModel.NumberOfTables} tables added successfully!");
                    }

                    LoadTablesForSelectedArea();
                    _ = LoadAreasAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to add table(s): {ex.Message}");
            }
        }

        private void ExecuteEditTable(object? parameter)
        {
            TableViewModel? tableToEdit = null;

            // Case 1: Parameter passed from button click (individual table)
            if (parameter is TableViewModel paramTable)
            {
                tableToEdit = paramTable;
            }
            // Case 2: From SelectedTables (when selection mode ON)
            else if (SelectedTables.Count == 1)
            {
                tableToEdit = SelectedTables.First();
            }

            if (tableToEdit == null)
            {
                _dialogService.ShowWarning("Please select exactly one table to edit!", "Invalid Selection");
                return;
            }

            try
            {
                var dialog = new Views.Dialogs.EditTableDialog();
                var areas = Areas.ToList();
                var viewModel = new Dialogs.EditTableDialogViewModel(tableToEdit, areas);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.DialogResult)
                {
                    var updatedTable = new Table
                    {
                        TableId = tableToEdit.TableId,
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
            List<TableViewModel> tablesToDelete = new();

            // Case 1: Parameter passed from button click (individual table)
            if (parameter is TableViewModel paramTable)
            {
                tablesToDelete.Add(paramTable);
            }
            // Case 2: From SelectedTables (when selection mode ON)
            else if (SelectedTables.Count > 0)
            {
                tablesToDelete.AddRange(SelectedTables);
            }

            if (tablesToDelete.Count == 0)
            {
                _dialogService.ShowWarning("Please select at least one table to delete!", "No Table Selected");
                return;
            }

            var message = tablesToDelete.Count == 1
                ? $"Are you sure you want to delete table '{tablesToDelete[0].TableName}'?\n\nThis action cannot be undone if the table has no orders."
                : $"Are you sure you want to delete {tablesToDelete.Count} selected tables?\n\nThis action cannot be undone for tables with no orders.";

            var confirm = _dialogService.ShowConfirmation(message, "Delete Table(s)");

            if (confirm)
            {
                try
                {
                    int successCount = 0;
                    int failCount = 0;
                    var errors = new List<string>();

                    foreach (var table in tablesToDelete.ToList())
                    {
                        try
                        {
                            _tableService.DeleteTable(table.TableId);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"'{table.TableName}': {ex.Message}");
                        }
                    }

                    if (failCount == 0)
                    {
                        _dialogService.ShowSuccess($"{successCount} table(s) deleted successfully!");
                    }
                    else
                    {
                        var errorMessage = $"Deleted {successCount} table(s) successfully.\n\n" +
                                          $"Failed to delete {failCount} table(s):\n" +
                                          string.Join("\n", errors);
                        _dialogService.ShowWarning(errorMessage, "Partial Success");
                    }

                    ClearTableSelection();
                    LoadTablesForSelectedArea();
                    _ = LoadAreasAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Failed to delete table(s): {ex.Message}");
                }
            }
        }

        private void ExecuteToggleSelectionMode(object? parameter)
        {
            IsSelectionModeEnabled = !IsSelectionModeEnabled;
            System.Diagnostics.Debug.WriteLine($"Selection mode {(IsSelectionModeEnabled ? "ENABLED" : "DISABLED")}");
        }

        #endregion

        #endregion
    }
}
