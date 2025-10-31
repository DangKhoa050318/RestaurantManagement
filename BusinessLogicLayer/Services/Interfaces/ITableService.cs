using BusinessObjects.Models;
using System.Collections.Generic;
namespace BusinessLogicLayer.Services.Interfaces
{
    public interface ITableService
    {
        List<Table> GetTables();
        Table GetTableById(int id);
        void AddTable(Table table);
        void DeleteTable(int id);
        void UpdateTable(Table table);
    }
}