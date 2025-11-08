using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RestaurantManagementWPF.Converters
{
    /// <summary>
    /// Converts status string to appropriate color
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "empty" or "available" => new SolidColorBrush(Colors.Green),
                    "using" or "occupied" => new SolidColorBrush(Colors.Orange),
                    "booked" or "reserved" => new SolidColorBrush(Colors.Blue),
                    "maintenance" or "closed" => new SolidColorBrush(Colors.Red),
                    "completed" or "paid" => new SolidColorBrush(Colors.Green),
                    "pending" or "waiting" => new SolidColorBrush(Colors.Orange),
                    "cancelled" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
