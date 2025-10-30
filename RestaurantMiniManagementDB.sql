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
    Phone VARCHAR(20) UNIQUE
);
GO

CREATE TABLE areas (
	AreaId INT IDENTITY(1,1) PRIMARY KEY,
	AreaName NVARCHAR(50) NOT NULL,
	AreaStatus VARCHAR(20) NOT NULL
		CHECK (AreaStatus IN ('Using', 'Maintenance')) DEFAULT 'Using'
);
GO

CREATE TABLE tables (
    TableId INT IDENTITY(1,1) PRIMARY KEY,
	AreaId INT NOT NULL,
    TableName VARCHAR(10) NOT NULL,
    Status VARCHAR(20) NOT NULL 
        CHECK (Status IN ('Empty', 'Booked', 'Using', 'Maintenance')),
	FOREIGN KEY (AreaId) REFERENCES areas(AreaId) ON DELETE CASCADE ON UPDATE CASCADE
);
GO

CREATE TABLE categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,  -- Khai vị, Món chính, Tráng miệng, Đồ uống, ...
	Description NVARCHAR(250)
);
GO

CREATE TABLE dishs (
    DishId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NULL,
    Name NVARCHAR(150) NOT NULL,
    Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
	UnitOfCalculation VARCHAR(20) NOT NULL,
    Description NVARCHAR(250),
    ImgURL VARCHAR(MAX) NULL,
    FOREIGN KEY (CategoryId)
        REFERENCES categories(CategoryId)
        ON DELETE CASCADE ON UPDATE CASCADE
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
    DishId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice >= 0),

    FOREIGN KEY (OrderId)
        REFERENCES orders(OrderId)
        ON DELETE CASCADE ON UPDATE CASCADE,

    FOREIGN KEY (DishId)
        REFERENCES dishs(DishId)
        ON DELETE CASCADE ON UPDATE CASCADE
);
GO

-- =============== INSERT DATA ====================
INSERT INTO customers (Fullname, Phone)
VALUES
(N'Nguyễn Văn A', '0905123456'),
(N'Trần Thị B', '0912345678'),
(N'Lê Văn C', '0987654321'),
(N'Phạm Thị D', '0977123123'),
(N'Hoàng Văn E', '0934567890');
GO

INSERT INTO areas (AreaName, AreaStatus)
VALUES
(N'Khu A - Tầng 1', 'Using'),
(N'Khu B - Tầng 2', 'Using'),
(N'Sảnh VIP', 'Using'),
(N'Khu Sân Vườn', 'Maintenance');
GO

INSERT INTO tables (AreaId, TableName, Status)
VALUES
(1, 'A01', 'Empty'),
(1, 'A02', 'Using'),
(2, 'B01', 'Booked'),
(2, 'B02', 'Empty'),
(3, 'VIP1', 'Using'),
(4, 'G01', 'Maintenance');
GO

INSERT INTO categories (Name, Description)
VALUES
(N'Khai vị', N'Món ăn mở đầu bữa ăn, nhẹ nhàng và kích thích vị giác'),
(N'Món chính', N'Món ăn chính trong bữa ăn'),
(N'Tráng miệng', N'Món ngọt hoặc trái cây sau bữa chính'),
(N'Đồ uống', N'Thức uống kèm theo trong bữa ăn');
GO

INSERT INTO dishs (CategoryId, Name, Price, UnitOfCalculation, Description, ImgURL)
VALUES
(1, N'Gỏi cuốn tôm thịt', 45000, 'phần', N'Món khai vị nhẹ nhàng với tôm và thịt', 'images/goi-cuon.jpg'),
(1, N'Súp hải sản', 55000, 'tô', N'Súp thơm ngon với hải sản tươi', 'images/sup-hai-san.jpg'),

(2, N'Cơm chiên dương châu', 65000, 'đĩa', N'Món cơm chiên truyền thống', 'images/com-chien.jpg'),
(2, N'Lẩu thái chua cay', 180000, 'nồi', N'Lẩu thái hương vị đặc trưng', 'images/lau-thai.jpg'),
(2, N'Bò lúc lắc', 120000, 'phần', N'Thịt bò mềm, sốt tiêu đen', 'images/bo-luc-lac.jpg'),

(3, N'Chè khúc bạch', 40000, 'ly', N'Món tráng miệng ngọt mát', 'images/che-khuc-bach.jpg'),
(3, N'Trà trái cây', 35000, 'ly', N'Trà thanh mát pha cùng trái cây tươi', 'images/tra-trai-cay.jpg'),

(4, N'Coca-Cola', 20000, 'chai', N'Thức uống giải khát phổ biến', 'images/coca.jpg'),
(4, N'Bia Heineken', 35000, 'chai', N'Thức uống dành cho bữa tiệc', 'images/heineken.jpg');
GO

INSERT INTO orders (TableId, CustomerId, OrderTime, Status, TotalAmount)
VALUES
(2, 1, GETDATE(), 'Completed', 250000),
(3, 2, GETDATE(), 'Scheduled', 0),
(5, 3, DATEADD(HOUR, -3, GETDATE()), 'Completed', 350000),
(1, 4, DATEADD(DAY, -1, GETDATE()), 'Cancelled', 0),
(2, 5, DATEADD(HOUR, -5, GETDATE()), 'Completed', 180000);
GO

INSERT INTO orderdetails (OrderId, DishId, Quantity, UnitPrice)
VALUES
-- Đơn hàng 1 (250k)
(1, 3, 2, 65000),     -- Cơm chiên dương châu
(1, 8, 2, 20000),     -- Coca
(1, 7, 1, 35000),     -- Trà trái cây

-- Đơn hàng 3 (350k)
(3, 4, 1, 180000),    -- Lẩu thái
(3, 5, 1, 120000),    -- Bò lúc lắc
(3, 9, 1, 35000),     -- Bia Heineken

-- Đơn hàng 5 (180k)
(5, 2, 1, 55000),     -- Súp hải sản
(5, 3, 1, 65000),     -- Cơm chiên
(5, 6, 1, 40000);     -- Chè khúc bạch
GO

-- ============== SELECT =====================
select * from customers;
select * from areas;
select * from tables;
select * from categories;
select * from dishs;
select * from orders;
select * from orderdetails;

drop table dishs;
drop table orders;
