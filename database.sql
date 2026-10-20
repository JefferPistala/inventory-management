-- Sistema de gestión de inventario: bases de datos y tablas (SQL Server)

-- Base de datos de Products.Api
IF DB_ID('InventoryProductsDb') IS NULL
    CREATE DATABASE InventoryProductsDb;
GO

USE InventoryProductsDb;
GO

-- Catálogo fijo: evita la misma categoría escrita de distintas formas.
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100)  NOT NULL,

    CONSTRAINT PK_Categories    PRIMARY KEY (Id),
    CONSTRAINT UQ_Categories_Name   UNIQUE (Name)
);
GO

CREATE TABLE Products (
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(150)     NOT NULL,
    Description NVARCHAR (500)  NULL,
    CategoryId  INT             NOT NULL,
    ImageUrl    NVARCHAR(500)   NULL,
    Price       DECIMAL(18,2)   NOT NULL,
    Stock       INT             NOT NULL
                CONSTRAINT DF_Products_Stock    DEFAULT 0,
    IsActive    BIT             NOT NULL
                CONSTRAINT DF_Products_IsActive DEFAULT 1,
    CreatedAt   DATETIME2       NOT NULL
                CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2       NULL,
    RowVersion  ROWVERSION,

    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories (Id),
    CONSTRAINT CK_Products_Price    CHECK   (Price >=0),
    CONSTRAINT CK_Products_Stock    CHECK   (Stock >=0)
);
GO

CREATE INDEX IX_Products_Name       ON Products (Name);
CREATE INDEX IX_Products_CategoryId ON Products (CategoryId);
GO

INSERT INTO Categories (Name) VALUES
    (N'Electrónica'),
    (N'Alimentos'),
    (N'Ropa'),
    (N'Hogar'),
    (N'Oficina');
GO

-- Base de datos de Transactions.Api
IF DB_ID('InventoryTransactionsDb')  IS NULL
    CREATE DATABASE InventoryTransactionsDb;
GO

USE InventoryTransactionsDb;
GO

-- ProductId sin FK: Products vive en la base de otro servicio.
CREATE TABLE Transactions (
    Id               INT IDENTITY(1,1) NOT NULL,
    TransactionDate  DATETIME2         NOT NULL
                     CONSTRAINT DF_Transactions_Date DEFAULT SYSUTCDATETIME(),
    TransactionType  VARCHAR(10)       NOT NULL,
    ProductId        INT               NOT NULL,
    Quantity         INT               NOT NULL,
    UnitPrice        DECIMAL(18,2)     NOT NULL,         -- precio del momento
    TotalPrice       AS (Quantity * UnitPrice) PERSISTED,
    Detail           NVARCHAR(500)     NULL,
    CreatedAt        DATETIME2         NOT NULL
                     CONSTRAINT DF_Transactions_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2         NULL,

    CONSTRAINT PK_Transactions PRIMARY KEY (Id),
    CONSTRAINT CK_Transactions_Type
        CHECK (TransactionType IN ('PURCHASE', 'SALE')),
    CONSTRAINT CK_Transactions_Quantity  CHECK (Quantity > 0),   -- el tipo define el signo
    CONSTRAINT CK_Transactions_UnitPrice CHECK (UnitPrice >= 0)
);
GO

CREATE INDEX IX_Transactions_ProductId_Date
    ON Transactions (ProductId, TransactionDate);
CREATE INDEX IX_Transactions_Type
    ON Transactions (TransactionType);
GO