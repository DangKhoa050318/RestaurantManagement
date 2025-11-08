namespace RestaurantManagementWPF.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying Area data in UI
    /// </summary>
    public class AreaViewModel
    {
        public int AreaId { get; set; }
        public string? AreaName { get; set; }
        public int TableCount { get; set; }
        public string? Status { get; set; }
    }
}
