GO
ALTER TABLE [dbo].[MessageHubMessages]
    ADD 
    BundleId [NVARCHAR](50) NULL,
    DequeuedDate [DATETIME2](7) NULL