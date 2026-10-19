--========================================================================
-- Sistema Gestion de inventarios - Script Creacion Inicial
-- Motor: SQL Server
-- Patrón: Database Per Service (Base de datos por microservicio)
--========================================================================

--========================================================================
--1. Base de datos por microservicio de Productos
--========================================================================

--Se crea la base solo si no existe. Con esto se evita error al re-ejecutar esta parte
IF DB_ID('InventoryProductsDb') IS NULL
    CREATE DATABASE InventoryProductsDb;
GO

USE InventoryProductsDb;
GO

-- -----------------------------------------------------------------------
-- Tabla Categories: Catalogo fijo de categorias. 
-- Evita texto libre inconsistente (ejem: "Electrónica" vs "electronica")
-- -----------------------------------------------------------------------
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100)  NOT NULL,

    CONSTRAINT PK_Categories    PRIMARY KEY (Id),
    CONSTRAINT UQ_Categories_Name   UNIQUE (Name)
);
GO

-- -----------------------------------------------------------------------
-- Tabla Products
-- -----------------------------------------------------------------------
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
                CONSTRAINT DF_Products_IsActive DEFAULT 1,    --Soft Delete
    CreatedAt   DATETIME2       NOT NULL
                CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2       NULL,
    RowVersion  ROWVERSION,

    CONSTRAINT PK_Products PRIMARY KEY (Id),
    -- FK Valida: Categories vive en la misma base. (Mismo dueño)
    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES Categories (Id),
    CONSTRAINT CK_Products_Price    CHECK   (Price >=0),
    CONSTRAINT CK_Products_Stock    CHECK   (Stock >=0)
);
GO

--Indices para los filtros mas usados en el listado
CREATE INDEX IX_Products_Name       ON Products (Name);
CREATE INDEX IX_Products_CategoryId ON Products (CategoryId);
GO

-- Datos iniciales del catalogo (prefijo N= texto Unicode)
INSERT INTO Categories (Name) VALUES
    (N'Electrónica'),
    (N'Alimentos'),
    (N'Ropa'),
    (N'Hogar'),
    (N'Oficina');
GO

--===========================================================
-- 2. Base de datos del microservicio de transacciones
--==========================================================

IF DB_ID('InventoryTransactionsDb')  IS NULL
    CREATE DATABASE InventoryTransactionsDb;
GO

USE InventoryTransactionsDb;
GO

-- ---------------------------------------------------------
-- Tabla Transactions
-- ProductId es una referencia Logica: no hay FK porque Products vive en otra base de datos que pertenece a otro microservicio
--------------------------------------------------------------------
CREATE TABLE Transactions (
    Id               INT IDENTITY(1,1) NOT NULL,
    TransactionDate  DATETIME2         NOT NULL
                     CONSTRAINT DF_Transactions_Date DEFAULT SYSUTCDATETIME(),
    TransactionType  VARCHAR(10)       NOT NULL,
    ProductId        INT               NOT NULL,      -- sin FK (otro servicio)
    Quantity         INT               NOT NULL,
    UnitPrice        DECIMAL(18,2)     NOT NULL,         -- copia del precio del momento
    TotalPrice       AS (Quantity * UnitPrice) PERSISTED,-- calculado por la BD: nunca inconsistente
    Detail           NVARCHAR(500)     NULL,
    CreatedAt        DATETIME2         NOT NULL
                     CONSTRAINT DF_Transactions_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2         NULL,

    CONSTRAINT PK_Transactions PRIMARY KEY (Id),
    CONSTRAINT CK_Transactions_Type
        CHECK (TransactionType IN ('PURCHASE', 'SALE')),
    CONSTRAINT CK_Transactions_Quantity  CHECK (Quantity > 0),   -- el tipo da el signo
    CONSTRAINT CK_Transactions_UnitPrice CHECK (UnitPrice >= 0)
);
GO

-- Índice compuesto: cubre el historial por producto filtrado por fechas
CREATE INDEX IX_Transactions_ProductId_Date
    ON Transactions (ProductId, TransactionDate);
-- Índice para el filtro por tipo de transacción
CREATE INDEX IX_Transactions_Type
    ON Transactions (TransactionType);
GO