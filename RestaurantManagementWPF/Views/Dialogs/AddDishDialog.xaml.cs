using System.Windows;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddDishDialog.xaml
    /// </summary>
    public partial class AddDishDialog : Window
    {
        public AddDishDialog()
        {
            InitializeComponent();
            DishNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't set DialogResult here - let the Command handle it
            // The Command will set ViewModel.DialogResult = true
            // Then we check it and close the dialog
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
