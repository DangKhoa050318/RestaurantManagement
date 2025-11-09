using RestaurantManagementWPF.Helpers;

namespace RestaurantManagementWPF.ViewModels.Models
{
    public class OrderItemViewModel : BaseViewModel
    {
        private int _dishId;
        private string _dishName = string.Empty;
        private int _quantity;
        private decimal _unitPrice;

        public int DishId
        {
            get => _dishId;
            set => SetProperty(ref _dishId, value);
        }

        public string DishName
        {
            get => _dishName;
            set => SetProperty(ref _dishName, value);
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        public decimal Subtotal => Quantity * UnitPrice;
    }
}
