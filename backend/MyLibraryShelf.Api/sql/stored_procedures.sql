-----------------------------------------------------------------------
-- sp_CreateShelf
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_CreateShelf', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateShelf;
GO
CREATE PROCEDURE dbo.sp_CreateShelf
    @ShelfName    NVARCHAR(100),
    @PasswordHash NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO dbo.Shelves (ShelfName, PasswordHash, CreatedAt)
        VALUES (@ShelfName, @PasswordHash, SYSUTCDATETIME());

        SELECT ShelfId = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        THROW;
    END CATCH;
END;
GO


-----------------------------------------------------------------------
-- sp_AuthenticateShelf
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_AuthenticateShelf', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AuthenticateShelf;
GO
CREATE PROCEDURE dbo.sp_AuthenticateShelf
    @ShelfName    NVARCHAR(100),
    @PasswordHash NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ShelfId INT;

    SELECT @ShelfId = ShelfId
    FROM dbo.Shelves
    WHERE ShelfName = @ShelfName
      AND PasswordHash = @PasswordHash;

    SELECT ShelfId = @ShelfId;
END;
GO


-----------------------------------------------------------------------
-- sp_AddSearchHistory
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_AddSearchHistory', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddSearchHistory;
GO
CREATE PROCEDURE dbo.sp_AddSearchHistory
    @ShelfId    INT,
    @QueryText  NVARCHAR(400),
    @EntityType NVARCHAR(50),
    @EntityId   INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO dbo.SearchHistory (ShelfId, QueryText, EntityType, EntityId, CreatedAt)
        VALUES (@ShelfId, @QueryText, @EntityType, @EntityId, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        THROW;
    END CATCH;
END;
GO


-----------------------------------------------------------------------
-- sp_AddBook  (includes nested stored procedure call)
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_AddBook', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_AddBook;
GO
CREATE PROCEDURE dbo.sp_AddBook
    @ShelfId         INT,
    @Title           NVARCHAR(200),
    @Author          NVARCHAR(200),
    @Isbn            NVARCHAR(32) = NULL,
    @Type            NVARCHAR(20),
    @DurationMinutes INT = NULL,
    @Narrator        NVARCHAR(200) = NULL,
    @PublishedYear   INT = NULL,
    @Notes           NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Books
        (
            ShelfId,
            Title,
            Author,
            Isbn,
            Type,
            DurationMinutes,
            Narrator,
            PublishedYear,
            Notes,
            CreatedAt
        )
        VALUES
        (
            @ShelfId,
            @Title,
            @Author,
            @Isbn,
            @Type,
            @DurationMinutes,
            @Narrator,
            @PublishedYear,
            @Notes,
            SYSUTCDATETIME()
        );

        DECLARE @NewBookId INT = SCOPE_IDENTITY();

        -- nested stored procedure
        EXEC dbo.sp_AddSearchHistory
            @ShelfId    = @ShelfId,
            @QueryText  = CONCAT('BOOK_ADDED: ', @Title),
            @EntityType = 'Book',
            @EntityId   = @NewBookId;

        COMMIT TRANSACTION;

        SELECT BookId = @NewBookId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT ERROR_MESSAGE() AS ErrorMessage;
        THROW;
    END CATCH;
END;
GO


-----------------------------------------------------------------------
-- sp_GetBooksForShelf  (WITH CTE + COUNT OVER)
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_GetBooksForShelf', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetBooksForShelf;
GO
CREATE PROCEDURE dbo.sp_GetBooksForShelf
    @ShelfId    INT,
    @PageNumber INT = 1,
    @PageSize   INT = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH BookList AS
    (
        SELECT
            BookId,
            ShelfId,
            Title,
            Author,
            Isbn,
            Type,
            PublishedYear,
            CreatedAt,
            COUNT(*) OVER (PARTITION BY ShelfId) AS TotalBooksOnShelf
        FROM dbo.Books
        WHERE ShelfId = @ShelfId
          AND Type <> 'Audiobook'
    )
    SELECT 
        BookId,
        ShelfId,
        Title,
        Author,
        Isbn,
        Type,
        PublishedYear,
        CreatedAt,
        TotalBooksOnShelf
    FROM BookList
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO


-----------------------------------------------------------------------
-- sp_GetAudiobooksForShelf  (WITH CTE + COUNT OVER)
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_GetAudiobooksForShelf', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAudiobooksForShelf;
GO


-----------------------------------------------------------------------
-- sp_RemoveBook
-----------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_RemoveBook', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_RemoveBook;
GO
CREATE PROCEDURE dbo.sp_RemoveBook
    @ShelfId INT,
    @BookId  INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DELETE FROM dbo.Books
        WHERE ShelfId = @ShelfId
          AND BookId = @BookId;

        SELECT RowsAffected = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        THROW;
    END CATCH;
END;
GO
CREATE PROCEDURE dbo.sp_GetAudiobooksForShelf
    @ShelfId    INT,
    @PageNumber INT = 1,
    @PageSize   INT = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH AudioList AS
    (
        SELECT
            BookId,
            ShelfId,
            Title,
            Author,
            Isbn,
            Type,
            DurationMinutes,
            Narrator,
            CreatedAt,
            COUNT(*) OVER (PARTITION BY ShelfId) AS TotalAudiobooksOnShelf
        FROM dbo.Books
        WHERE ShelfId = @ShelfId
          AND Type = 'Audiobook'
    )
    SELECT
        BookId,
        ShelfId,
        Title,
        Author,
        Isbn,
        Type,
        DurationMinutes,
        Narrator,
        CreatedAt,
        TotalAudiobooksOnShelf
    FROM AudioList
    ORDER BY CreatedAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
