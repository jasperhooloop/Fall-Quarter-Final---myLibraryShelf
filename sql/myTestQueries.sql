-- 1: Database exists?
SELECT name FROM sys.databases WHERE name = 'MyLibraryShelf';

-- 2: What database am I currently in?
SELECT DB_NAME() AS CurrentDatabase;

-- 3: Stored procedures exist?
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

