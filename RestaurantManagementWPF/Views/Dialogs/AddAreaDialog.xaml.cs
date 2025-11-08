using System.Windows;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for AddAreaDialog.xaml
    /// </summary>
    public partial class AddAreaDialog : Window
    {
        public AddAreaDialog()
        {
            InitializeComponent();
            
            // Focus on area name textbox
            Loaded += (s, e) => AreaNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddAreaDialogViewModel vm)
            {
                // Execute OK command first to set DialogResult
                if (vm.OKCommand.CanExecute(null))
                {
                    vm.OKCommand.Execute(null);
                    
                    // Now check DialogResult
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
