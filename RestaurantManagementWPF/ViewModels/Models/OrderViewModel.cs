using System;

namespace RestaurantManagementWPF.ViewModels.Models
{
    public class OrderViewModel : BaseViewModel
    {
        public int OrderId { get; set; }
        public int? TableId { get; set; }
        public string TableName { get; set; } = "N/A";
        public string AreaName { get; set; } = "N/A";
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "Walk-in";
        public DateTime OrderTime { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Status color for UI display
        /// </summary>
        public string StatusColor
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "completed" => "#4CAF50",  // Green
                    "scheduled" => "#FF9800",  // Orange
                    "cancelled" => "#F44336",  // Red
                    _ => "#9E9E9E"            // Gray
                };
            }
        }

        /// <summary>
        /// Formatted payment time (or "Not paid" if null)
        /// </summary>
        public string PaymentTimeDisplay
        {
            get
            {
                return PaymentTime.HasValue 
                    ? PaymentTime.Value.ToString("dd/MM/yyyy HH:mm") 
                    : "Not paid";
            }
        }

        /// <summary>
        /// Duration between order and payment
        /// </summary>
        public string DurationDisplay
        {
            get
            {
                if (!PaymentTime.HasValue) return "-";
                
                var duration = PaymentTime.Value - OrderTime;
                if (duration.TotalHours < 1)
                    return $"{(int)duration.TotalMinutes} min";
                else
                    return $"{(int)duration.TotalHours}h {duration.Minutes}m";
            }
        }
    }
}
