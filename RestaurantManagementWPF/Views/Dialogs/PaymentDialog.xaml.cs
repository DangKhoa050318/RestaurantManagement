using System.Windows;

namespace RestaurantManagementWPF.Views.Dialogs
{
    public partial class PaymentDialog : Window
    {
        public PaymentDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
