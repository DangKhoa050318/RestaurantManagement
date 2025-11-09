using RestaurantManagementWPF.Helpers;
using System.Windows.Media;

namespace RestaurantManagementWPF.ViewModels.Models
{
    public class TableViewModel : BaseViewModel
    {
        private int _tableId;
        private string _tableName = string.Empty;
        private string _status = "Empty";
        private int? _areaId;
        private string _areaName = string.Empty;

        public int TableId
        {
            get => _tableId;
            set => SetProperty(ref _tableId, value);
        }

        public string TableName
        {
            get => _tableName;
            set => SetProperty(ref _tableName, value);
        }

        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public int? AreaId
        {
            get => _areaId;
            set => SetProperty(ref _areaId, value);
        }

        public string AreaName
        {
            get => _areaName;
            set => SetProperty(ref _areaName, value);
        }

        /// <summary>
        /// Color based on table status
        /// Empty = Light Blue, Using = Light Green, Booked = Light Orange, Maintenance = Light Red
        /// </summary>
        public SolidColorBrush StatusColor
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "empty" => new SolidColorBrush(Color.FromRgb(144, 202, 249)),    // Light Blue #90CAF9
                    "using" => new SolidColorBrush(Color.FromRgb(165, 214, 167)),     // Light Green #A5D6A7
                    "booked" => new SolidColorBrush(Color.FromRgb(255, 204, 128)),    // Light Orange #FFCC80
                    "maintenance" => new SolidColorBrush(Color.FromRgb(239, 154, 154)), // Light Red #EF9A9A
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
        }
    }
}
