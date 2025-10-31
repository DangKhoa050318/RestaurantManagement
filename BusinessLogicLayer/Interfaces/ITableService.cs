using BusinessObjects.Models;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface ITableService
    {
        List<Table> GetTables();
        Table GetTableById(int id);
        void AddTable(Table table);
        void UpdateTable(Table table);
        void DeleteTable(int id);
        List<Table> GetTablesByArea(int areaId);
        List<Table> GetTablesByStatus(string status);
        void UpdateTableStatus(int tableId, string newStatus);
    }
}