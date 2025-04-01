CREATE DATABASE HomeBook;
GO
USE HomeBook;
GO

-- Bảng quản trị viên
CREATE TABLE Admin (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
);

-- Bảng khách hàng
CREATE TABLE Customer (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Avatar NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bảng danh mục sách
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL -- Ví dụ: Văn học, Khoa học, Trẻ em, v.v.
);

-- Bảng nhà xuất bản
CREATE TABLE Publisher (
    PublisherID INT IDENTITY(1,1) PRIMARY KEY,
    PublisherName NVARCHAR(255) NOT NULL,
    ContactPerson NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(255)
);

-- Bảng sách (có liên kết với Publisher)
CREATE TABLE Book (
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    BookTitle NVARCHAR(255) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL,
    OrderedQuantity INT DEFAULT 0,
    CategoryID INT FOREIGN KEY REFERENCES Category(CategoryID) ON DELETE CASCADE,
    PublisherID INT FOREIGN KEY REFERENCES Publisher(PublisherID) ON DELETE SET NULL, -- Liên kết với Publisher
    Status NVARCHAR(50) NOT NULL, -- Ví dụ: Còn hàng, Hết hàng
    CoverImageURL NVARCHAR(255)
);

-- Bảng giỏ hàng
CREATE TABLE Cart (
    CartID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID) ON DELETE CASCADE,
    BookID INT FOREIGN KEY REFERENCES Book(BookID) ON DELETE CASCADE,
    Quantity INT NOT NULL
);

-- Bảng đơn đặt hàng
CREATE TABLE [Order] (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID) ON DELETE CASCADE,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending' -- Ví dụ: Đang xử lý, Đã giao, Đã hủy
);

-- Bảng chi tiết đơn hàng
CREATE TABLE OrderDetail (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT FOREIGN KEY REFERENCES [Order](OrderID) ON DELETE CASCADE,
    BookID INT FOREIGN KEY REFERENCES Book(BookID) ON DELETE CASCADE,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL
);

-- Bảng đánh giá sách
CREATE TABLE Review (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID) ON DELETE CASCADE,
    BookID INT FOREIGN KEY REFERENCES Book(BookID) ON DELETE CASCADE,
    Rating INT CHECK (Rating >= 1 AND Rating <= 5), -- Đánh giá từ 1-5 sao
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Bảng thống kê doanh thu
CREATE TABLE Statistic (
    StatisticID INT IDENTITY(1,1) PRIMARY KEY,
    ReportDate DATE NOT NULL,
    TotalOrders INT NOT NULL,
    TotalRevenue DECIMAL(18,2) NOT NULL
);

-- Bảng thanh toán
CREATE TABLE Payment (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT FOREIGN KEY REFERENCES [Order](OrderID) ON DELETE CASCADE,
    PaymentMethod NVARCHAR(50) NOT NULL, -- Ví dụ: Tiền mặt, Thẻ tín dụng, Ví điện tử
    PaymentStatus NVARCHAR(50) DEFAULT 'Pending',
    TransactionDate DATETIME DEFAULT GETDATE()
);

-- Bảng liên hệ
CREATE TABLE Contact (
    ContactID INT IDENTITY(1,1) PRIMARY KEY,
	Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Subject NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);