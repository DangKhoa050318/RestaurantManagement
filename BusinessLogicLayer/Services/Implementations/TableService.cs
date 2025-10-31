using BusinessObjects.Models;
using BusinessLogicLayer.Services.Interfaces;
using DataAccessLayer.Repositories.Implementations;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;

namespace BusinessLogicLayer.Services.Implementations
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;
        public TableService()
        {
            _tableRepository = TableRepository.Instance;
        }

        public Table GetTableById(int id) => _tableRepository.GetTableById(id);
        public List<Table> GetTables() => _tableRepository.GetTables();
        public void AddTable(Table table) => _tableRepository.AddTable(table);
        public void DeleteTable(int id) => _tableRepository.DeleteTable(id);
        public void UpdateTable(Table table) => _tableRepository.UpdateTable(table);
    }
}