using System.Windows;

namespace RestaurantManagementWPF.Views.Dialogs
{
    public partial class EditTableDialog : Window
    {
        public EditTableDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
