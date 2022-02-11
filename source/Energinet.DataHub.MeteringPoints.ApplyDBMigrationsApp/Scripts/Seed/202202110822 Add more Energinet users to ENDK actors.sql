DECLARE @user1 uniqueidentifier;
SET @user1 = N'3DC0084E-2538-4BBF-9B53-B741C1DD0FA2';
DECLARE @user2 uniqueidentifier;
SET @user2 = N'9B7FB5AE-7B75-432A-8131-14C81E39686A';
DECLARE @user3 uniqueidentifier;
SET @user3 = N'92F50660-69EA-46D4-99C9-EAB74132A60A';
DECLARE @user4 uniqueidentifier;
SET @user4 = N'2288C650-6CC2-469E-B37E-6785CC2A6D96';
DECLARE @user5 uniqueidentifier;
SET @user5 = N'9390A5A5-ACBC-4075-B23A-683C53684F3E';
DECLARE @user6 uniqueidentifier;
SET @user6 = N'2251023B-AD98-418F-9803-18F720E4F4EC';
DECLARE @user7 uniqueidentifier;
SET @user7 = N'CCBDB364-2075-4B89-A860-B49DA6FAC6AB';
DECLARE @actor1 uniqueidentifier;
SET @actor1 = N'03603AF3-8F70-4B23-B369-454AEC9FB4F4';
DECLARE @actor2 uniqueidentifier;
SET @actor2 = N'09591F53-B531-4DB5-BB66-A2F52A393D82';
DECLARE @actor3 uniqueidentifier;
SET @actor3 = N'83510249-9F7C-4827-A67C-499A3A94F533';
DECLARE @actor4 uniqueidentifier;
SET @actor4 = N'0E223E42-BED4-4778-A973-8D0AD9813F71';
INSERT INTO [dbo].UserActor (UserId, ActorId)
VALUES (@user1, @actor1),
       (@user2, @actor1),
       (@user3, @actor1),
       (@user4, @actor1),
       (@user5, @actor1),
       (@user6, @actor1),
       (@user7, @actor1),
       (@user1, @actor2),
       (@user4, @actor2),
       (@user6, @actor2),
       (@user1, @actor3),
       (@user4, @actor3),
       (@user6, @actor3),
       (@user1, @actor4),
       (@user4, @actor4),
       (@user6, @actor4);
