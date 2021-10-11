ALTER TABLE [dbo].[MessageHubMessages]
ADD
    [Date] [DATETIME2](7) NOT NULL,
    [Recipient] NVARCHAR(128) NOT NULL
