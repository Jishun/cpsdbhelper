CREATE TABLE [dbo].[Table2]
(
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Name] nvarchar(20),
    Descript INT
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Table2_Name] ON dbo.Table2(Name, Descript)
