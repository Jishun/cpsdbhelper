CREATE TABLE [dbo].[Table2]
(
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Name] nvarchar(20),
    [ForId] INT ,
    [ForName] BIGINT ,
    Descript INT,
    CONSTRAINT [FK_T2_T1] FOREIGN KEY ([ForId], [ForName])  REFERENCES dbo.Table1 (Id, Name1)
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Table2_Name] ON dbo.Table2(Name, Descript)
