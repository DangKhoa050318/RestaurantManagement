using BusinessObjects.Models;
using RestaurantManagementWPF.Helpers;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class EditCustomerDialogViewModel : BaseViewModel
    {
        private int _customerId;
        private string _customerName = string.Empty;
        private string _phoneNumber = string.Empty;

        public EditCustomerDialogViewModel(Customer customer)
        {
            CustomerId = customer.CustomerId;
            CustomerName = customer.Fullname;
            PhoneNumber = customer.Phone ?? string.Empty;

            OKCommand = new RelayCommand(ExecuteOK, CanExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public int CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                if (SetProperty(ref _customerName, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
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
            return !string.IsNullOrWhiteSpace(CustomerName);
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
