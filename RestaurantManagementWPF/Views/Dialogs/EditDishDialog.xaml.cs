using System.Windows;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDishDialog.xaml
    /// </summary>
    public partial class EditDishDialog : Window
    {
        public EditDishDialog()
        {
            InitializeComponent();
            DishNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't set DialogResult here - let the Command handle it
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
