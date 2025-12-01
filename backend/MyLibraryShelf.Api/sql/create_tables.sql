USE MyLibraryShelf;
GO

CREATE TABLE dbo.Shelves
(
    ShelfId      INT IDENTITY(1, 1) PRIMARY KEY,
    ShelfName    NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(512) NOT NULL,
    CreatedAt    DATETIME2(7)  NOT NULL
);
GO

CREATE TABLE dbo.Books
(
    BookId          INT IDENTITY(1, 1) PRIMARY KEY,
    ShelfId         INT             NOT NULL,
    Title           NVARCHAR(200)   NOT NULL,
    Author          NVARCHAR(200)   NOT NULL,
    Isbn            NVARCHAR(32)    NULL,
    Type            NVARCHAR(20)    NOT NULL,
    DurationMinutes INT             NULL,
    Narrator        NVARCHAR(200)   NULL,
    PublishedYear   INT             NULL,
    Notes           NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2(7)    NOT NULL,
    CONSTRAINT FK_Books_Shelves FOREIGN KEY (ShelfId) REFERENCES dbo.Shelves (ShelfId)
);
GO

CREATE TABLE dbo.SearchHistory
(
    HistoryId  INT IDENTITY(1, 1) PRIMARY KEY,
    ShelfId    INT            NOT NULL,
    QueryText  NVARCHAR(400)  NOT NULL,
    EntityType NVARCHAR(50)   NOT NULL,
    EntityId   INT            NULL,
    CreatedAt  DATETIME2(7)   NOT NULL,
    CONSTRAINT FK_SearchHistory_Shelves FOREIGN KEY (ShelfId) REFERENCES dbo.Shelves (ShelfId)
);
GO
