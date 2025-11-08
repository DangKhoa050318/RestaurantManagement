using System;
using System.Windows;
using RestaurantManagementWPF.ViewModels;

namespace RestaurantManagementWPF.Views
{
    /// <summary>
    /// Interaction logic for AdminShellWindow.xaml
    /// </summary>
    public partial class AdminShellWindow : Window
    {
        public AdminShellWindow()
        {
            InitializeComponent();

            // Connect Frame to ViewModel
            if (DataContext is AdminShellViewModel viewModel)
            {
                viewModel.MainFrame = MainFrame;
                
                // Navigate to Dashboard by default
                Loaded += (s, e) =>
                {
                    viewModel.NavigateCommand.Execute("Dashboard");
                };
            }
        }
    }
}
