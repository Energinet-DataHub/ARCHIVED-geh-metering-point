ALTER TABLE [dbo].[PostOfficeMessages]
ADD
    [Date] [datetime2](7) NOT NULL,
    [Recipient] NVARCHAR(128) NOT NULL
