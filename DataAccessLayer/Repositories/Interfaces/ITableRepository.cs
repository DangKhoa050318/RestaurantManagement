using BusinessObjects.Models;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface ITableRepository
    {
        List<Table> GetTables();
        Table GetTableById(int id);
        void AddTable(Table table);
        void UpdateTable(Table table);
        void DeleteTable(int id);
    }
}
