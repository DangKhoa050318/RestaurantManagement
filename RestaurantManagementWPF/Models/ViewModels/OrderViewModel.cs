namespace RestaurantManagementWPF.Models.ViewModels
{
    /// <summary>
    /// ViewModel for displaying Order data in UI
    /// </summary>
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? TableName { get; set; }
        public string? CustomerName { get; set; }
    }
}
