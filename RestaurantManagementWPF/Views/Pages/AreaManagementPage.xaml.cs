using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RestaurantManagementWPF.ViewModels;
using RestaurantManagementWPF.ViewModels.Models;

namespace RestaurantManagementWPF.Views.Pages
{
    /// <summary>
    /// Interaction logic for AreaManagementPage.xaml
    /// </summary>
    public partial class AreaManagementPage : Page
    {
        public AreaManagementPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle table card click for multi-select
        /// </summary>
        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as AreaManagementViewModel;
            
            if (viewModel == null || !viewModel.IsSelectionModeEnabled)
            {
                // Selection mode OFF ? Allow normal button interaction
                return;
            }

            // Selection mode ON ? Handle selection and BLOCK all clicks
            if (sender is FrameworkElement element && element.DataContext is TableViewModel table)
            {
                if (viewModel.SelectTableCommand.CanExecute(table))
                {
                    viewModel.SelectTableCommand.Execute(table);
                }
                
                // ? Block ALL events including button clicks
                e.Handled = true;
            }
        }

        /// <summary>
        /// Block button clicks when selection mode is ON
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as AreaManagementViewModel;
            
            // ? When selection mode ON ? Block button execution
            if (viewModel?.IsSelectionModeEnabled == true)
            {
                e.Handled = true;
                // Optional: Show warning
                System.Diagnostics.Debug.WriteLine("?? Buttons disabled in selection mode");
            }
        }
    }
}
