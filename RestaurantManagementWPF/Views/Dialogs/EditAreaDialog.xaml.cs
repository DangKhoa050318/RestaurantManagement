using System.Windows;
using BusinessObjects.Models;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAreaDialog.xaml
    /// </summary>
    public partial class EditAreaDialog : Window
    {
        public EditAreaDialog(Area area)
        {
            InitializeComponent();
            
            // Set DataContext with existing area data
            DataContext = new EditAreaDialogViewModel(area);
            
            // Focus on area name textbox
            Loaded += (s, e) => AreaNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditAreaDialogViewModel vm)
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
