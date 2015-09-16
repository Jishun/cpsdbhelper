CREATE TABLE [dbo].[Table1]
(
    [Id] INT NOT NULL PRIMARY KEY,
    [Name] char(20) NOT NULL,
    [Name1] BIGINT NOT NULL,
    [Name2] binary(20),
    [Name3] bit,
    [Name4] char(20),
    [Name5] datetime,
    [Name6] decimal(20,2),
    [Name7] float,
    [Name8] image,
    [Name9] int,
    [Name0] money,
    [Name11] NCHAR(20),
    [Name12] ntext,
    [Name13] nvarchar(20),
    [Name14] real,
    [Name15] smalldatetime,
    [Name16] smallint,
    [Name17] smallmoney,
    [Name18] text,
    [Name19] timestamp,
    [Name20] tinyint,
    [Name21] uniqueidentifier,
    [Name22] varbinary(20),
    [Name23] varchar(20),
    [Name24] XML,
    [Name25] date,
    [Name26] time,
    [Name27] datetimeoffset,
    [Name28] datetime2,
    --[Name29] hierarchyid,
)
GO
CREATE NONCLUSTERED INDEX [IX_Table2_Name1] ON dbo.Table1(Name)
GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name2] ON dbo.Table1(Name2)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name3] ON dbo.Table1(Name3)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name4] ON dbo.Table1(Name4)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name5] ON dbo.Table1(Name5)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name6] ON dbo.Table1(Name6)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name7] ON dbo.Table1(Name7)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name8] ON dbo.Table1(Name8)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name9] ON dbo.Table1(Name9)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name0] ON dbo.Table1(Name0)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name11] ON dbo.Table1(Name11)
--GO
----CREATE NONCLUSTERED INDEX [IX_Table2_Name12] ON dbo.Table1(Name12)
----GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name13] ON dbo.Table1(Name13)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name14] ON dbo.Table1(Name14)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name15] ON dbo.Table1(Name15)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name16] ON dbo.Table1(Name16)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name17] ON dbo.Table1(Name17)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name18] ON dbo.Table1(Name18)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name19] ON dbo.Table1(Name19)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name20] ON dbo.Table1(Name20)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name21] ON dbo.Table1(Name21)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name22] ON dbo.Table1(Name22)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name23] ON dbo.Table1(Name23)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name24] ON dbo.Table1(Name24)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name25] ON dbo.Table1(Name25)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name26] ON dbo.Table1(Name26)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name27] ON dbo.Table1(Name27)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name28] ON dbo.Table1(Name28)
--GO
--CREATE NONCLUSTERED INDEX [IX_Table2_Name29] ON dbo.Table1(Name29)
--GO
