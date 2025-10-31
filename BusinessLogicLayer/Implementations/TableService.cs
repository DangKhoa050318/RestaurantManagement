using BusinessObjects.Models;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System; 
namespace Services.Implementations
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

        public TableService()
        {
            _tableRepository = TableRepository.Instance;
        }

        public List<Table> GetTables() => _tableRepository.GetTables();

        public Table GetTableById(int id) => _tableRepository.GetTableById(id);

        public void AddTable(Table table)
        {
            if (table.AreaId <= 0)
                throw new ArgumentException("AreaId is required.");

            if (string.IsNullOrWhiteSpace(table.TableName))
                throw new ArgumentException("Table name cannot be empty.");

            if (string.IsNullOrWhiteSpace(table.Status))
                table.Status = "Available"; // mặc định là bàn trống

            _tableRepository.AddTable(table);
        }

        public void UpdateTable(Table table)
        {
            var existing = _tableRepository.GetTableById(table.TableId);
            if (existing == null)
                throw new Exception("Table not found.");

            // Cập nhật các trường
            existing.AreaId = table.AreaId;
            existing.TableName = table.TableName;
            existing.Status = table.Status;

            _tableRepository.UpdateTable(existing);
        }

        public void DeleteTable(int id)
        {
            try
            {
                // Repository đã có logic kiểm tra Orders, 
                // Service chỉ cần bắt lỗi và ném lên
                _tableRepository.DeleteTable(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Table> GetTablesByArea(int areaId)
        {
            var all = _tableRepository.GetTables();
            return all.Where(t => t.AreaId == areaId).ToList();
        }

        public List<Table> GetTablesByStatus(string status)
        {
            var all = _tableRepository.GetTables();
            return all
                .Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void UpdateTableStatus(int tableId, string newStatus)
        {
            // Validation trạng thái
            var validStatuses = new[] { "Available", "Occupied", "Reserved", "Maintenance" };
            if (!validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid status. Valid options: {string.Join(", ", validStatuses)}");

            var table = _tableRepository.GetTableById(tableId);
            if (table == null)
                throw new Exception("Table not found.");

            table.Status = newStatus;
            _tableRepository.UpdateTable(table);
        }
    }
}