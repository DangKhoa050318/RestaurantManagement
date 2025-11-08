namespace RestaurantManagementWPF.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying Table data in UI
    /// </summary>
    public class TableViewModel
    {
        public int TableId { get; set; }
        public string? TableName { get; set; }
        public int Capacity { get; set; }
        public string? Status { get; set; }
        public string? AreaName { get; set; }
        public int? AreaId { get; set; }
    }
}
