USE master;  
ALTER DATABASE RestaurantMiniManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;  
DROP DATABASE RestaurantMiniManagementDB;  

create database RestaurantMiniManagementDB;
go

use RestaurantMiniManagementDB;
go

CREATE TABLE customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    Fullname NVARCHAR(100) NOT NULL,
    Phone VARCHAR(20) UNIQUE,
    Email VARCHAR(50) UNIQUE
);
GO

CREATE TABLE tables (
    TableId INT IDENTITY(1,1) PRIMARY KEY,
    TypeTable VARCHAR(10) NOT NULL 
        CHECK (TypeTable IN ('VIP', 'Normal')),
    Capacity INT NOT NULL CHECK (Capacity > 0),
    Status VARCHAR(20) NOT NULL 
        CHECK (Status IN ('Empty', 'Booked', 'Using', 'Maintenance'))
);
GO

CREATE TABLE categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL  -- Khai vị, Món chính, Tráng miệng, Đồ uống, ...
);
GO

CREATE TABLE foods (
    FoodId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NULL,
    Name NVARCHAR(150) NOT NULL,
    Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    Description NVARCHAR(250),
    ImgURL VARCHAR(MAX) NULL,
    FOREIGN KEY (CategoryId)
        REFERENCES categories(CategoryId)
        ON DELETE SET NULL ON UPDATE CASCADE
);
GO

CREATE TABLE orders (
    OrderId INT IDENTITY(1,1) PRIMARY KEY,
    TableId INT NULL,
    CustomerId INT NULL,
    OrderTime DATETIME NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(15) NOT NULL DEFAULT 'Scheduled'
        CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled')),
    TotalAmount DECIMAL(10,2) NOT NULL DEFAULT 0
        CHECK (TotalAmount >= 0),

    FOREIGN KEY (TableId)
        REFERENCES tables(TableId)
        ON DELETE SET NULL ON UPDATE CASCADE,

    FOREIGN KEY (CustomerId)
        REFERENCES customers(CustomerId)
        ON DELETE SET NULL ON UPDATE CASCADE
);
GO

CREATE TABLE orderdetails (
    OrderDetailId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice >= 0),

    FOREIGN KEY (OrderId)
        REFERENCES orders(OrderId)
        ON DELETE CASCADE ON UPDATE CASCADE,

    FOREIGN KEY (FoodId)
        REFERENCES foods(FoodId)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- TRIGGER: Tự động cập nhật tổng tiền đơn hàng
-- ===========================================
CREATE TRIGGER trg_UpdateOrderTotal
ON orderdetails
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE o
    SET TotalAmount = ISNULL((
        SELECT SUM(od.Quantity * od.UnitPrice)
        FROM orderdetails od
        WHERE od.OrderId = o.OrderId
    ), 0)
    FROM orders o
    WHERE o.OrderId IN (
        SELECT DISTINCT OrderId FROM inserted
        UNION
        SELECT DISTINCT OrderId FROM deleted
    );
END;
GO
-- ngừng sử dụng trigger của toàn bộ database
DISABLE TRIGGER ALL ON DATABASE;
-- khi cần sử dụng lại trigger 
ENABLE TRIGGER ALL ON DATABASE;

-- =============== INSERT DATA ====================

INSERT INTO customers (Fullname, Phone, Email)
VALUES
(N'Nguyễn Văn A', '0912345678', 'nguyenvana@gmail.com'),
(N'Lê Thị B', '0987654321', 'lethib@gmail.com'),
(N'Trần Minh C', '0909090909', 'tranminhc@gmail.com'),
(N'Phạm Thu D', '0977777777', 'phamthud@gmail.com'),
(N'Đỗ Quốc E', '0933333333', 'doquoe@gmail.com');
GO

INSERT INTO tables (TypeTable, Capacity, Status)
VALUES
('VIP', 6, 'Empty'),
('VIP', 8, 'Booked'),
('Normal', 4, 'Using'),
('Normal', 2, 'Empty'),
('Normal', 4, 'Maintenance');
GO

INSERT INTO categories (Name) VALUES 
(N'Khai vị'), (N'Món chính'), (N'Tráng miệng'), (N'Đồ uống');

INSERT INTO foods (CategoryId, Name, Price, Description, ImgURL)
VALUES
(1, N'Súp gà ngô non', 35000, N'Món khai vị nhẹ nhàng', NULL),
(1, N'Gỏi cuốn tôm thịt', 40000, N'Món khai vị phổ biến', NULL),
(2, N'Cơm chiên hải sản', 65000, N'Món chính đậm đà hương vị biển', NULL),
(2, N'Mì xào bò', 55000, N'Mì xào thịt bò thơm ngon', NULL),
(2, N'Lẩu thái hải sản', 150000, N'Món đặc biệt cho bàn VIP', NULL),
(3, N'Bánh flan', 25000, N'Món tráng miệng mát lạnh', NULL),
(3, N'Chè khúc bạch', 30000, N'Món ngọt dễ ăn', NULL),
(4, N'Trà đào cam sả', 30000, N'Nước uống giải khát', NULL),
(4, N'Nước ép dưa hấu', 25000, N'Tươi mát và tốt cho sức khỏe', NULL),
(2, N'Tôm hùm nướng phô mai', 450000, N'Món đặc biệt cao cấp', NULL);
GO

INSERT INTO orders (TableId, CustomerId, OrderTime, Status)
VALUES
(1, 1, GETDATE(), 'Scheduled'),
(2, 2, DATEADD(HOUR, -2, GETDATE()), 'Completed'),
(3, 3, DATEADD(DAY, -1, GETDATE()), 'Completed'),
(4, 4, DATEADD(HOUR, -1, GETDATE()), 'Scheduled'),
(1, 5, DATEADD(DAY, -3, GETDATE()), 'Cancelled');
GO

-- INSERT DATA: orderdetails
-- Đơn hàng 1 (Nguyễn Văn A)
INSERT INTO orderdetails (OrderId, FoodId, Quantity, UnitPrice)
VALUES
(1, 1, 2, 35000),
(1, 3, 1, 65000),
(1, 8, 2, 30000);

-- Đơn hàng 2 (Lê Thị B)
INSERT INTO orderdetails (OrderId, FoodId, Quantity, UnitPrice)
VALUES
(2, 2, 1, 40000),
(2, 4, 1, 55000),
(2, 6, 2, 25000),
(2, 8, 2, 30000);

-- Đơn hàng 3 (Trần Minh C)
INSERT INTO orderdetails (OrderId, FoodId, Quantity, UnitPrice)
VALUES
(3, 3, 2, 65000),
(3, 9, 1, 25000),
(3, 7, 1, 30000);

-- Đơn hàng 4 (Phạm Thu D)
INSERT INTO orderdetails (OrderId, FoodId, Quantity, UnitPrice)
VALUES
(4, 5, 1, 150000),
(4, 10, 1, 450000),
(4, 8, 2, 30000);

-- Đơn hàng 5 (Đỗ Quốc E)
INSERT INTO orderdetails (OrderId, FoodId, Quantity, UnitPrice)
VALUES
(5, 1, 1, 35000),
(5, 2, 1, 40000);
GO

select * from customers;
select * from tables;
select * from categories;
select * from foods;
select * from orders;
select * from orderdetails;

drop table foods;
drop table orders;