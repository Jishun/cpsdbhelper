CREATE TABLE [dbo].[Table1]
(
    [Id] INT NOT NULL PRIMARY KEY,
    [Name] varchar(20)
)
GO
CREATE NONCLUSTERED INDEX [IX_Table2_Name] ON dbo.Table1(Name)
GO
