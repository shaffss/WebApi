CREATE DATABASE myDB;
GO
USE myDB;
GO

CREATE TABLE Product (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    Name NTEXT NOT NULL
);
CREATE TABLE Currency (
    Currency_Code NCHAR(3) PRIMARY KEY,
    Currency_Name NVARCHAR(50) NOT NULL
);
CREATE TABLE Price (
    PriceID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    Price_Value DECIMAL(19, 4) NOT NULL,
    Currency_Code NCHAR(3) NOT NULL,
    CONSTRAINT FK_Product FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
    CONSTRAINT FK_Currency FOREIGN KEY (Currency_Code) REFERENCES Currency(Currency_Code)
);

INSERT INTO Product (Name)
VALUES 
('Eco-Friendly Water Bottle'), 
('Smartphone Pro Max 12'), 
('Wireless Noise-Cancelling Headphones'), 
('4K Ultra HD TV 65-inch'), 
('Gaming Laptop RTX 3080'), 
('Organic Green Tea'), 
('Adjustable Standing Desk'), 
('Bluetooth Fitness Tracker'), 
('Coffee Maker with Grinder'), 
('Portable Solar Charger'), 
('LED Desk Lamp with USB Charging'), 
('Electric Toothbrush with Timer'), 
('VR Headset with Controllers'), 
('Premium Leather Wallet'), 
('Noise-Reducing Earplugs'), 
('Smart Home Security Camera'), 
('Personal Blender for Smoothies'), 
('Yoga Mat with Carry Strap'), 
('Herbal Essential Oil Diffuser'), 
('Travel Backpack with Laptop Compartment');

INSERT INTO Currency (Currency_Code, Currency_Name)
VALUES 
('USD', 'US Dollar'), 
('EUR', 'Euro'), 
('JPY', 'Japanese Yen');

INSERT INTO Price (ProductID, Price_Value, Currency_Code)
VALUES 
(1, 24.99, 'USD'), 
(1, 22.99, 'EUR'), 
(2, 999.99, 'USD'), 
(2, 949.99, 'EUR'), 
(3, 299.99, 'USD'), 
(4, 799.99, 'USD'), 
(4, 749.99, 'EUR'), 
(5, 1499.99, 'USD'), 
(6, 12.99, 'USD'), 
(7, 349.99, 'USD'), 
(8, 79.99, 'USD'), 
(9, 129.99, 'USD'), 
(10, 39.99, 'USD'), 
(11, 49.99, 'USD'), 
(12, 89.99, 'USD'), 
(13, 399.99, 'USD'), 
(13, 369.99, 'EUR'), 
(14, 59.99, 'USD'), 
(15, 9.99, 'USD'), 
(16, 199.99, 'USD'), 
(17, 29.99, 'USD'), 
(18, 24.99, 'USD'), 
(19, 29.99, 'USD'), 
(20, 79.99, 'USD'), 
(20, 69.99, 'EUR');

SELECT 
    p.ProductID, 
    p.Name, 
    pr.Price_Value, 
    c.Currency_Code, 
    c.Currency_Name
FROM 
    Price pr
JOIN 
    Product p ON pr.ProductID = p.ProductID
JOIN 
    Currency c ON pr.Currency_Code = c.Currency_Code;
GO

CREATE PROCEDURE GetProductByID
    @ProductID INT = NULL
AS
BEGIN
	IF @ProductID IS NULL
	BEGIN
		SELECT
			p.ProductID,
			p.Name AS ProductName,
			pr.Price_Value,
			c.Currency_Code,
			c.Currency_Name
		FROM 
			Product p
		JOIN 
			Price pr ON p.ProductID = pr.ProductID
		JOIN 
			Currency c ON pr.Currency_Code = c.Currency_Code;
	END
	ELSE
	BEGIN
		SELECT 
			p.ProductID,
			p.Name AS ProductName,
			pr.Price_Value,
			c.Currency_Code,
			c.Currency_Name
		FROM 
			Product p
		JOIN 
			Price pr ON p.ProductID = pr.ProductID
		JOIN 
			Currency c ON pr.Currency_Code = c.Currency_Code
		WHERE 
			p.ProductID = @ProductID;
	END;
END;
GO

CREATE PROCEDURE InsertMultipleProducts
    @ProductData NVARCHAR(MAX)
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        DECLARE @ParsedData TABLE (
            ProductName NVARCHAR(MAX),
            Price_Value DECIMAL(19,4),
            Currency_Code NCHAR(3),
            Currency_Name NVARCHAR(50)
        );

        INSERT INTO @ParsedData (ProductName, Price_Value, Currency_Code, Currency_Name)
        SELECT ProductName, Price_Value, Currency_Code, Currency_Name
        FROM OPENJSON(@ProductData)
        WITH (
            ProductName NVARCHAR(MAX) '$.ProductName',
            Price_Value DECIMAL(19,4) '$.Price_Value',
            Currency_Code NCHAR(3) '$.Currency_Code',
            Currency_Name NVARCHAR(50) '$.Currency_Name'
        );

        INSERT INTO Currency (Currency_Code, Currency_Name)
        SELECT DISTINCT Currency_Code, ISNULL(Currency_Name, 'Unknown Currency')
        FROM @ParsedData pd
        WHERE NOT EXISTS (
            SELECT 1 FROM Currency c WHERE c.Currency_Code = pd.Currency_Code
        );

        DECLARE @NewProductID INT;
        DECLARE cur CURSOR FOR
        SELECT ProductName, Price_Value, Currency_Code
        FROM @ParsedData;

        DECLARE @ProductName NVARCHAR(MAX);
        DECLARE @Price_Value DECIMAL(19,4);
        DECLARE @Currency_Code NCHAR(3);

        OPEN cur;
        FETCH NEXT FROM cur INTO @ProductName, @Price_Value, @Currency_Code;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO Product (Name)
            VALUES (@ProductName);

            SET @NewProductID = SCOPE_IDENTITY();

            INSERT INTO Price (ProductID, Price_Value, Currency_Code)
            VALUES (@NewProductID, @Price_Value, @Currency_Code);

            FETCH NEXT FROM cur INTO @ProductName, @Price_Value, @Currency_Code;
        END;

        CLOSE cur;
        DEALLOCATE cur;

        COMMIT TRANSACTION;

        PRINT 'Products and prices inserted successfully.';

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO
