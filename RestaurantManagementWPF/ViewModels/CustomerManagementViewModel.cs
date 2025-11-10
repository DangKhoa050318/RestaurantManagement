using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.Services;
using RestaurantManagementWPF.Views.Dialogs;
using RestaurantManagementWPF.ViewModels.Dialogs;
using Services.Implementations;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels
{
    public class CustomerManagementViewModel : BaseViewModel
    {
        private readonly CustomerService _customerService;
        private readonly DialogService _dialogService;

        private ObservableCollection<Customer> _customers = new();
        private ObservableCollection<Customer> _allCustomers = new();
        private Customer? _selectedCustomer;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public CustomerManagementViewModel()
        {
            _customerService = new CustomerService();
            _dialogService = new DialogService();

            // Commands
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer);
            EditCustomerCommand = new RelayCommand(ExecuteEditCustomer, _ => SelectedCustomer != null);
            DeleteCustomerCommand = new RelayCommand(ExecuteDeleteCustomer, _ => SelectedCustomer != null);
            RefreshCommand = new RelayCommand(async _ => await LoadCustomersAsync());

            // Load data
            _ = LoadCustomersAsync();
        }

        #region Properties

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
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

        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        private async Task LoadCustomersAsync()
        {
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    var customers = _customerService.GetCustomers();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _allCustomers.Clear();
                        foreach (var customer in customers)
                        {
                            _allCustomers.Add(customer);
                        }

                        ApplySearchInternal();
                    });
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error loading customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplySearch()
        {
            ApplySearchInternal();
        }

        private void ApplySearchInternal()
        {
            var filtered = _allCustomers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c =>
                    c.Fullname.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Phone != null && c.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            Customers.Clear();
            foreach (var customer in filtered)
            {
                Customers.Add(customer);
            }
        }

        private void ExecuteAddCustomer(object? parameter)
        {
            var dialog = new AddCustomerDialog();
            var viewModel = new AddCustomerDialogViewModel();
            dialog.DataContext = viewModel;

            if (dialog.ShowDialog() == true && viewModel.DialogResult)
            {
                try
                {
                    var newCustomer = new Customer
                    {
                        Fullname = viewModel.CustomerName,
                        Phone = string.IsNullOrWhiteSpace(viewModel.PhoneNumber) ? null : viewModel.PhoneNumber
                    };

                    _customerService.AddCustomer(newCustomer);
                    _dialogService.ShowSuccess($"Customer '{newCustomer.Fullname}' created successfully!");
                    _ = LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error creating customer: {ex.Message}");
                }
            }
        }

        private void ExecuteEditCustomer(object? parameter)
        {
            if (SelectedCustomer == null) return;

            var dialog = new EditCustomerDialog();
            var viewModel = new EditCustomerDialogViewModel(SelectedCustomer);
            dialog.DataContext = viewModel;

            if (dialog.ShowDialog() == true && viewModel.DialogResult)
            {
                try
                {
                    var updatedCustomer = new Customer
                    {
                        CustomerId = viewModel.CustomerId,
                        Fullname = viewModel.CustomerName,
                        Phone = string.IsNullOrWhiteSpace(viewModel.PhoneNumber) ? null : viewModel.PhoneNumber
                    };

                    _customerService.UpdateCustomer(updatedCustomer);
                    _dialogService.ShowSuccess($"Customer '{updatedCustomer.Fullname}' updated successfully!");
                    _ = LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Error updating customer: {ex.Message}");
                }
            }
        }

        private void ExecuteDeleteCustomer(object? parameter)
        {
            if (SelectedCustomer == null) return;

            var confirm = _dialogService.ShowConfirmation(
                $"Are you sure you want to delete customer '{SelectedCustomer.Fullname}'?\n\n" +
                "This action cannot be undone.",
                "Confirm Delete"
            );

            if (confirm)
            {
                try
                {
                    _customerService.DeleteCustomer(SelectedCustomer.CustomerId);
                    _dialogService.ShowSuccess($"Customer '{SelectedCustomer.Fullname}' deleted successfully!");
                    SelectedCustomer = null;
                    _ = LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Cannot delete customer: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
