using System.Windows;

namespace RestaurantManagementWPF.Views.Dialogs
{
    public partial class EditCustomerDialog : Window
    {
        public EditCustomerDialog()
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
