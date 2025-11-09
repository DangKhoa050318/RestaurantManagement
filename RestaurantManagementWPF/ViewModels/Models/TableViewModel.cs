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
        /// Empty = Green, Using = Red, Reserved = Orange
        /// </summary>
        public SolidColorBrush StatusColor
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "empty" => new SolidColorBrush(Color.FromRgb(200, 255, 200)), // Light Green
                    "using" => new SolidColorBrush(Color.FromRgb(255, 200, 200)), // Light Red
                    "reserved" => new SolidColorBrush(Color.FromRgb(255, 230, 200)), // Light Orange
                    _ => new SolidColorBrush(Colors.White)
                };
            }
        }
    }
}
