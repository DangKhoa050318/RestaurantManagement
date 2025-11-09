using System.Windows;

namespace RestaurantManagementWPF.Views.Dialogs
{
    public partial class AddCustomerDialog : Window
    {
        public AddCustomerDialog()
        {
            InitializeComponent();
            CustomerNameTextBox.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
