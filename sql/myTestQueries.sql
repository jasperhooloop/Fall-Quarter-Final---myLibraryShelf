-- Check whether database exists
SELECT name FROM sys.databases WHERE name = 'MyLibraryShelf';

-- Current database context
SELECT DB_NAME() AS CurrentDatabase;

-- List stored procedures
SELECT name FROM sys.procedures ORDER BY name;

---------
SELECT @@VERSION;
---------

USE MyLibraryShelf;
GO

SELECT name 
FROM sys.procedures 
ORDER BY name;
GO
-----

SELECT name 
FROM sys.tables
ORDER BY name;
