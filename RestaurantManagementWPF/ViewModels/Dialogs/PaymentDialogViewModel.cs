using RestaurantManagementWPF.Helpers;
using RestaurantManagementWPF.ViewModels.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RestaurantManagementWPF.ViewModels.Dialogs
{
    public class PaymentDialogViewModel : BaseViewModel
    {
        private string _tableName = string.Empty;
        private string _areaName = string.Empty;
        private decimal _totalAmount;
        private int _itemCount;

        public PaymentDialogViewModel(
            string tableName,
            string areaName,
            ObservableCollection<OrderItemViewModel> orderItems,
            decimal totalAmount)
        {
            TableName = tableName;
            AreaName = areaName;
            TotalAmount = totalAmount;
            ItemCount = orderItems.Count;

            // Copy order items
            OrderItems = new ObservableCollection<OrderItemViewModel>();
            foreach (var item in orderItems)
            {
                OrderItems.Add(item);
            }

            OKCommand = new RelayCommand(ExecuteOK);
            CancelCommand = new RelayCommand(_ => { });
        }

        #region Properties

        public string TableName
        {
            get => _tableName;
            set => SetProperty(ref _tableName, value);
        }

        public string AreaName
        {
            get => _areaName;
            set => SetProperty(ref _areaName, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        public int ItemCount
        {
            get => _itemCount;
            set => SetProperty(ref _itemCount, value);
        }

        public ObservableCollection<OrderItemViewModel> OrderItems { get; set; }

        public bool DialogResult { get; set; }

        #endregion

        #region Commands

        public ICommand OKCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Methods

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
