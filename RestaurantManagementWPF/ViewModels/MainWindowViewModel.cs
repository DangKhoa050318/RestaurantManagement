namespace RestaurantManagementWPF.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _title = "Restaurant Management System";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}
