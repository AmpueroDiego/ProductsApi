-- ============================================================
-- ProductsDb schema + seed data
-- Run as-is in SSMS or with:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -i db/schema.sql
-- ============================================================

IF DB_ID('ProductsDb') IS NULL
BEGIN
    CREATE DATABASE ProductsDb;
END
GO

USE ProductsDb;
GO

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

CREATE TABLE dbo.Products
(
    ProductId   INT             NOT NULL PRIMARY KEY,
    Name        NVARCHAR(100)   NOT NULL,
    Status      INT             NOT NULL,
    Stock       INT             NOT NULL,
    Description NVARCHAR(500)   NULL,
    Price       DECIMAL(18,2)   NOT NULL
);
GO

INSERT INTO dbo.Products (ProductId, Name, Status, Stock, Description, Price)
VALUES
    (1, N'Teclado mecánico',  1, 50,  N'Switches rojos, retroiluminado', 120.00),
    (2, N'Mouse inalámbrico',  1, 200, N'6 botones, 1600 DPI',            45.50),
    (3, N'Monitor 24"',        0, 0,   N'Sin stock por el momento',       210.99);
GO
