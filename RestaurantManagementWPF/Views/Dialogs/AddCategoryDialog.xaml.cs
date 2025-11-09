using System.Windows;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddCategoryDialog.xaml
    /// </summary>
    public partial class AddCategoryDialog : Window
    {
        public AddCategoryDialog()
        {
            InitializeComponent();
            
            // Focus on category name textbox
            Loaded += (s, e) => CategoryNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddCategoryDialogViewModel vm)
            {
                if (vm.OKCommand.CanExecute(null))
                {
                    vm.OKCommand.Execute(null);
                    
                    if (vm.DialogResult)
                    {
                        DialogResult = true;
                        Close();
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
