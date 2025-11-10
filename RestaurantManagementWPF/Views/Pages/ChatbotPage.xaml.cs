using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RestaurantManagementWPF.ViewModels;

namespace RestaurantManagementWPF.Views.Pages
{
    public partial class ChatbotPage : Page
    {
        public ChatbotPage()
        {
            InitializeComponent();
            
            // Auto scroll to bottom when new messages arrive
            if (DataContext is ChatbotViewModel viewModel)
            {
                viewModel.Messages.CollectionChanged += (s, e) =>
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        ChatScrollViewer.ScrollToBottom();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                };
            }
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            // Send message when Enter is pressed
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                var viewModel = DataContext as ChatbotViewModel;
                if (viewModel?.SendMessageCommand?.CanExecute(null) == true)
                {
                    viewModel.SendMessageCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}