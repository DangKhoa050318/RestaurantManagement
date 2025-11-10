using System.Windows;
using BusinessObjects.Models;
using RestaurantManagementWPF.ViewModels.Dialogs;

namespace RestaurantManagementWPF.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for EditCategoryDialog.xaml
    /// </summary>
    public partial class EditCategoryDialog : Window
    {
        public EditCategoryDialog(Category category)
        {
            InitializeComponent();
            
            // Set DataContext with existing category data
            DataContext = new EditCategoryDialogViewModel(category);
            
            // Focus on category name textbox
            Loaded += (s, e) => CategoryNameTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditCategoryDialogViewModel vm)
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
