# ?? STATUS VALUES REFERENCE

## Database CHECK Constraints

### Table Status (tables.Status)
Valid values defined in database CHECK constraint:
- `Empty` - Bàn tr?ng, s?n sàng ph?c v?
- `Using` - ?ang có khách s? d?ng
- `Booked` - ?ã ???c ??t tr??c
- `Maintenance` - ?ang b?o trì

### Area Status (areas.AreaStatus)
Valid values:
- `Using` - Khu v?c ?ang ho?t ??ng
- `Maintenance` - Khu v?c ?ang b?o trì

### Order Status (orders.Status)
Valid values:
- `Pending` - ??n hàng ch? x? lý
- `In Progress` - ?ang ch? bi?n
- `Completed` - Hoàn thành
- `Cancelled` - ?ã h?y

---

## Color Mapping (StatusToColorConverter)

### Table Status Colors:
- **Empty** ? ?? Green (#28A745)
- **Using** ? ?? Orange (#FF9800)
- **Booked** ? ?? Blue (#007ACC)
- **Maintenance** ? ?? Red (#DC3545)

### Area Status Colors:
- **Using** ? ?? Green
- **Maintenance** ? ?? Red

### Order Status Colors:
- **Pending** ? ?? Orange
- **In Progress** ? ?? Blue
- **Completed** ? ?? Green
- **Cancelled** ? ?? Red

---

## Usage in Code

### Creating New Table:
```csharp
var table = new Table
{
    TableName = "Table 01",
    Status = "Empty",  // Use "Empty" not "Available"!
    AreaId = areaId
};
```

### Creating New Area:
```csharp
var area = new Area
{
    AreaName = "VIP Zone",
    AreaStatus = "Using"  // Use "Using" not "Active"!
};
```

### Updating Table Status:
```csharp
// Valid transitions
tableService.UpdateTableStatus(tableId, "Empty");
tableService.UpdateTableStatus(tableId, "Using");
tableService.UpdateTableStatus(tableId, "Booked");
tableService.UpdateTableStatus(tableId, "Maintenance");

// INVALID - will throw exception
tableService.UpdateTableStatus(tableId, "Available"); // ?
tableService.UpdateTableStatus(tableId, "Occupied");  // ?
```

---

## Common Mistakes ??

### ? WRONG:
```csharp
Status = "Available"  // Not in CHECK constraint
Status = "Occupied"   // Not in CHECK constraint
Status = "Reserved"   // Not in CHECK constraint
Status = "Active"     // Not in CHECK constraint
```

### ? CORRECT:
```csharp
Status = "Empty"       // ?
Status = "Using"       // ?
Status = "Booked"      // ?
Status = "Maintenance" // ?
```

---

## Database Schema Validation

To verify CHECK constraints in SQL Server:

```sql
-- View CHECK constraints for tables
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
WHERE t.name = 'tables';

-- View CHECK constraints for areas
SELECT 
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
WHERE t.name = 'areas';
```

---

## Files Using Status Values

### Services:
- `TableService.cs` - Validates table status
- `AreaService.cs` - Validates area status
- `OrderService.cs` - Validates order status

### ViewModels:
- `AreaManagementViewModel.cs` - Creates tables with "Empty" status
- `POSViewModel.cs` (future) - Updates table status to "Using"

### Converters:
- `StatusToColorConverter.cs` - Maps status to colors

### Database:
- `RestaurantContext.cs` - Entity configurations
- SQL Server CHECK constraints

---

## Testing Status Values

### Test Table Creation:
```csharp
[Test]
public void CreateTable_WithEmptyStatus_ShouldSucceed()
{
    var table = new Table 
    { 
        TableName = "Test", 
        Status = "Empty", 
        AreaId = 1 
    };
    
    tableService.AddTable(table); // Should succeed
}

[Test]
public void CreateTable_WithAvailableStatus_ShouldFail()
{
    var table = new Table 
    { 
        TableName = "Test", 
        Status = "Available",  // Invalid!
        AreaId = 1 
    };
    
    Assert.Throws<DbUpdateException>(() => 
        tableService.AddTable(table)
    );
}
```

---

## Migration Notes

If you need to change status values in the future:

1. **Update Database**:
```sql
ALTER TABLE tables
DROP CONSTRAINT CK__tables__Status__xxx;

ALTER TABLE tables
ADD CONSTRAINT CK__tables__Status__xxx
CHECK (Status IN ('Empty', 'Using', 'Booked', 'Maintenance', 'NewValue'));
```

2. **Update Code**:
- `TableService.cs` - UpdateTableStatus validStatuses array
- `StatusToColorConverter.cs` - Add new color mapping
- `AreaManagementViewModel.cs` - Update default status if needed

3. **Update Documentation**:
- This file
- FEATURE_ANALYSIS.md
- Any API documentation

---

## Summary

? **Always use these exact values:**
- Table: `Empty`, `Using`, `Booked`, `Maintenance`
- Area: `Using`, `Maintenance`
- Order: `Pending`, `In Progress`, `Completed`, `Cancelled`

? **Never use:**
- `Available`, `Occupied`, `Reserved`, `Active`, `Closed`, etc.

**Reason:** Database has CHECK constraints that will reject invalid values!
