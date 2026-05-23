EFcore database scaffold script

dotnet ef dbcontext scaffold "Server=.;Database=MiniLibraryMgmtSys;User ID=sa;Password=sasa@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f --no-onconfiguring


Mini Lib Mgmt Sys Database Table Creation Script

Run this in your mssql ssms tool query window

```sh

-- USERS TABLE
CREATE TABLE [dbo].[tbl_users] (
    [Id] VARCHAR(50) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Email] VARCHAR(100) NOT NULL UNIQUE,
    [Password] NVARCHAR(255) NOT NULL,
    [Role] VARCHAR(50) NOT NULL DEFAULT 'Member',
    [IsActive] BIT NOT NULL DEFAULT 1,
    [LastLoginDate] DATETIME NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatedBy] VARCHAR(50) NULL,
    [UpdatedBy] VARCHAR(50) NULL,
	[DeleteFlag] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy)
        REFERENCES [dbo].[tbl_users](Id),
    CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (UpdatedBy)
        REFERENCES [dbo].[tbl_users](Id)
);

-- BOOKS TABLE
CREATE TABLE [dbo].[tbl_books] (
    [Id] VARCHAR(50) NOT NULL PRIMARY KEY,
    [Author] NVARCHAR(100) NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Genre] VARCHAR(50),
    [IsAvailable] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatedBy] VARCHAR(50) NULL,
    [UpdatedBy] VARCHAR(50) NULL,
	[DeleteFlag] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT FK_Books_CreatedBy FOREIGN KEY (CreatedBy)
        REFERENCES [dbo].[tbl_users](Id),
    CONSTRAINT FK_Books_UpdatedBy FOREIGN KEY (UpdatedBy)
        REFERENCES [dbo].[tbl_users](Id)
);

-- BORROWED_BOOKS TABLE (unchanged from before)
CREATE TABLE [dbo].[tbl_borrowedBooks] (
    [Id] VARCHAR(50) NOT NULL PRIMARY KEY,
    [UserId] VARCHAR(50) NOT NULL,
    [BookId] VARCHAR(50) NOT NULL,
    [BorrowedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [ReturnedAt] DATETIME NULL,
    [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_BorrowedBooks_Users FOREIGN KEY (UserId)
        REFERENCES [dbo].[tbl_users](Id),
    CONSTRAINT FK_BorrowedBooks_Books FOREIGN KEY (BookId)
        REFERENCES [dbo].[tbl_books](Id)
);


```