using System.Windows;

namespace RestaurantManagementWPF.Views.Dialogs
{
    public partial class AddTableDialog : Window
    {
        public AddTableDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
