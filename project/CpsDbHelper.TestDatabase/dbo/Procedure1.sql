CREATE PROCEDURE [dbo].[Procedure1]
    @param1 int = 0,
    @param2 xml
AS
    SELECT @param1, @param2
RETURN 0
