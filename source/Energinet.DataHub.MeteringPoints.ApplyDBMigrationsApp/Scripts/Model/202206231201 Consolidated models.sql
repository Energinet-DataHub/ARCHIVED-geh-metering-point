DELETE
FROM SchemaVersions
WHERE ScriptName like 'Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp.Scripts.Model%';

IF OBJECT_ID(N'dbo.Actor', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[Actor]
        (
            [Id]                   [uniqueidentifier]   NOT NULL,
            [RecordId]             [int] IDENTITY (1,1) NOT NULL,
            [IdentificationNumber] [nvarchar](50)       NOT NULL,
            [IdentificationType]   [nvarchar](50)       NOT NULL,
            [Roles]                [nvarchar](max)      NOT NULL,
            CONSTRAINT [PK_Actor] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.BusinessProcesses', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[BusinessProcesses]
        (
            [Id]            [uniqueidentifier]   NOT NULL,
            [RecordId]      [int] IDENTITY (1,1) NOT NULL,
            [TransactionId] [nvarchar](36)       NOT NULL,
            [ProcessType]   [nvarchar](50)       NOT NULL,
            [Status]        [nvarchar](50)       NOT NULL,
            CONSTRAINT [PK_BusinessProcesses] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.GridAreas', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[GridAreas]
        (
            [Id]               [uniqueidentifier]   NOT NULL,
            [RecordId]         [int] IDENTITY (1,1) NOT NULL,
            [Code]             [nvarchar](3)        NOT NULL,
            [Name]             [nvarchar](50)       NOT NULL,
            [PriceAreaCode]    [nvarchar](5)        NOT NULL,
            [FullFlexFromDate] [datetime2](7)       NULL,
            [ActorId]          [uniqueidentifier]   NOT NULL,
            CONSTRAINT [PK_GridAreas] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
        ALTER TABLE [dbo].[GridAreas]
            WITH CHECK ADD CONSTRAINT [FK_GridAreas_Actor] FOREIGN KEY ([ActorId])
                REFERENCES [dbo].[Actor] ([Id])

        ALTER TABLE [dbo].[GridAreas]
            CHECK CONSTRAINT [FK_GridAreas_Actor]

    END
GO

IF OBJECT_ID(N'dbo.GridAreaLinks', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[GridAreaLinks]
        (
            [Id]         [uniqueidentifier]   NOT NULL,
            [RecordId]   [int] IDENTITY (1,1) NOT NULL,
            [GridAreaId] [uniqueidentifier]   NOT NULL,
            CONSTRAINT [PK_GridAreaLinks] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
        ALTER TABLE [dbo].[GridAreaLinks]
            WITH CHECK ADD CONSTRAINT [FK_GridAreaLinks_GridAreas] FOREIGN KEY ([GridAreaId])
                REFERENCES [dbo].[GridAreas] ([Id])

        ALTER TABLE [dbo].[GridAreaLinks]
            CHECK CONSTRAINT [FK_GridAreaLinks_GridAreas]
    END
GO

IF OBJECT_ID(N'dbo.IncomingMessages', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[IncomingMessages]
        (
            [RecordId]    [int] IDENTITY (1,1) NOT NULL,
            [MessageId]   [nvarchar](50)       NOT NULL,
            [MessageType] [nvarchar](255)      NOT NULL,
            CONSTRAINT [PK_IncomingMessages] PRIMARY KEY NONCLUSTERED
                (
                 [MessageId] ASC,
                 [MessageType] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.MessageHubMessages', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[MessageHubMessages]
        (
            [Id]           [uniqueidentifier]   NOT NULL,
            [RecordId]     [int] IDENTITY (1,1) NOT NULL,
            [Correlation]  [nvarchar](500)      NOT NULL,
            [Type]         [nvarchar](500)      NOT NULL,
            [Content]      [nvarchar](max)      NOT NULL,
            [Date]         [datetime2](7)       NOT NULL,
            [Recipient]    [nvarchar](128)      NOT NULL,
            [BundleId]     [nvarchar](50)       NULL,
            [DequeuedDate] [datetime2](7)       NULL,
            [GsrnNumber]   [nvarchar](36)       NOT NULL,
            CONSTRAINT [PK_MessageHubMessages] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
        ALTER TABLE [dbo].[MessageHubMessages]
            ADD DEFAULT ('') FOR [GsrnNumber]

    END
GO

IF OBJECT_ID(N'dbo.MeteringPoints', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[MeteringPoints]
        (
            [Id]                            [uniqueidentifier]   NOT NULL,
            [RecordId]                      [int] IDENTITY (1,1) NOT NULL,
            [GsrnNumber]                    [nvarchar](36)       NOT NULL,
            [StreetName]                    [nvarchar](500)      NOT NULL,
            [PostCode]                      [nvarchar](10)       NOT NULL,
            [CityName]                      [nvarchar](100)      NOT NULL,
            [CountryCode]                   [nvarchar](4)        NOT NULL,
            [ConnectionState_PhysicalState] [nvarchar](255)      NOT NULL,
            [MeteringPointSubType]          [nvarchar](50)       NULL,
            [MeterReadingOccurrence]        [nvarchar](50)       NULL,
            [TypeOfMeteringPoint]           [nvarchar](50)       NOT NULL,
            [MaximumCurrent]                [int]                NULL,
            [MaximumPower]                  [int]                NULL,
            [MeteringGridArea]              [uniqueidentifier]   NULL,
            [PowerPlant]                    [nvarchar](255)      NULL,
            [LocationDescription]           [nvarchar](255)      NULL,
            [ProductType]                   [nvarchar](255)      NULL,
            [ParentRelatedMeteringPoint]    [uniqueidentifier]   NULL,
            [UnitType]                      [nvarchar](255)      NULL,
            [EffectiveDate]                 [datetime2](7)       NULL,
            [MeterNumber]                   [nvarchar](255)      NULL,
            [ConnectionState_EffectiveDate] [datetime2](7)       NULL,
            [StreetCode]                    [nvarchar](4)        NULL,
            [CitySubDivision]               [nvarchar](34)       NULL,
            [Floor]                         [nvarchar](4)        NULL,
            [Room]                          [nvarchar](4)        NULL,
            [BuildingNumber]                [nvarchar](6)        NULL,
            [MunicipalityCode]              [int]                NULL,
            [IsActualAddress]               [bit]                NULL,
            [GeoInfoReference]              [uniqueidentifier]   NULL,
            [Capacity]                      [float]              NULL,
            [AssetType]                     [nvarchar](255)      NULL,
            [SettlementMethod]              [nvarchar](50)       NULL,
            [ScheduledMeterReadingDate]     [nvarchar](10)       NULL,
            [ProductionObligation]          [bit]                NULL,
            [NetSettlementGroup]            [nvarchar](10)       NULL,
            [DisconnectionType]             [nvarchar](10)       NULL,
            [ConnectionType]                [nvarchar](50)       NULL,
            [StartOfSupplyDate]             [datetime2](7)       NULL,
            [ToGrid]                        [uniqueidentifier]   NULL,
            [FromGrid]                      [uniqueidentifier]   NULL,
            CONSTRAINT [PK_MeteringPoints] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
        ALTER TABLE [dbo].[MeteringPoints]
            ADD DEFAULT ((0)) FOR [MaximumCurrent]

        ALTER TABLE [dbo].[MeteringPoints]
            ADD DEFAULT ((0)) FOR [MaximumPower]

        ALTER TABLE [dbo].[MeteringPoints]
            ADD DEFAULT ((0)) FOR [IsActualAddress]

        ALTER TABLE [dbo].[MeteringPoints]
            ADD DEFAULT ((0)) FOR [ProductionObligation]
    END
GO

IF OBJECT_ID(N'dbo.EnergySuppliers', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[EnergySuppliers]
        (
            [Id]                    [uniqueidentifier]   NOT NULL,
            [RecordId]              [int] IDENTITY (1,1) NOT NULL,
            [StartOfSupplyDate]     [datetime2](7)       NOT NULL,
            [GlnNumber]             [nvarchar](50)       NOT NULL,
            [MarketMeteringPointId] [uniqueidentifier]   NOT NULL,
            CONSTRAINT [PK_EnergySupplier] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
        ALTER TABLE [dbo].[EnergySuppliers]
            WITH CHECK ADD FOREIGN KEY ([MarketMeteringPointId])
                REFERENCES [dbo].[MeteringPoints] ([Id])
    END
GO

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[OutboxMessages]
        (
            [Id]            [uniqueidentifier]   NOT NULL,
            [RecordId]      [int] IDENTITY (1,1) NOT NULL,
            [Type]          [nvarchar](255)      NOT NULL,
            [Data]          [nvarchar](max)      NOT NULL,
            [Category]      [nvarchar](50)       NOT NULL,
            [CreationDate]  [datetime2](7)       NOT NULL,
            [ProcessedDate] [datetime2](7)       NULL,
            [Correlation]   [nvarchar](255)      NOT NULL,
            CONSTRAINT [PK_OutboxMessages] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
        ALTER TABLE [dbo].[OutboxMessages]
            ADD DEFAULT ('None') FOR [Correlation]

    END
GO

IF OBJECT_ID(N'dbo.ProcessDetailErrors', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[ProcessDetailErrors]
        (
            [Id]              [uniqueidentifier] NOT NULL,
            [ProcessDetailId] [uniqueidentifier] NOT NULL,
            [Code]            [nvarchar](50)     NOT NULL,
            [Description]     [nvarchar](max)    NULL,
            CONSTRAINT [PK_ProcessDetailErrors] PRIMARY KEY CLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.ProcessDetails', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[ProcessDetails]
        (
            [Id]            [uniqueidentifier] NOT NULL,
            [ProcessId]     [uniqueidentifier] NOT NULL,
            [Name]          [nvarchar](1024)   NOT NULL,
            [Sender]        [nvarchar](1024)   NULL,
            [Receiver]      [nvarchar](1024)   NULL,
            [CreatedDate]   [datetime2](7)     NOT NULL,
            [EffectiveDate] [datetime2](7)     NULL,
            [Status]        [int]              NULL,
            CONSTRAINT [PK_ProcessDetails] PRIMARY KEY CLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.Processes', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[Processes]
        (
            [Id]                [uniqueidentifier] NOT NULL,
            [Name]              [nvarchar](1024)   NOT NULL,
            [MeteringPointGsrn] [nvarchar](128)    NOT NULL,
            [CreatedDate]       [datetime2](7)     NOT NULL,
            [EffectiveDate]     [datetime2](7)     NULL,
            [Status]            [int]              NULL,
            CONSTRAINT [PK_Processes] PRIMARY KEY CLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.QueuedInternalCommands', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[QueuedInternalCommands]
        (
            [Id]            [uniqueidentifier]   NOT NULL,
            [RecordId]      [int] IDENTITY (1,1) NOT NULL,
            [Type]          [nvarchar](255)      NOT NULL,
            [Data]          [nvarchar](max)      NULL,
            [ScheduleDate]  [datetime2](1)       NULL,
            [ProcessedDate] [datetime2](1)       NULL,
            [CreationDate]  [datetime2](7)       NOT NULL,
            [CorrelationId] [nvarchar](255)      NOT NULL,
            [Error]         [nvarchar](max)      NULL,
            CONSTRAINT [PK_InternalCommandQueue] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY],
            CONSTRAINT [UC_InternalCommandQueue_Id] UNIQUE CLUSTERED
                (
                 [RecordId] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
        ALTER TABLE [dbo].[QueuedInternalCommands]
            ADD DEFAULT ('None') FOR [CorrelationId]
    END
GO

IF OBJECT_ID(N'dbo.SchemaVersions', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[SchemaVersions]
        (
            [Id]         [int] IDENTITY (1,1) NOT NULL,
            [ScriptName] [nvarchar](255)      NOT NULL,
            [Applied]    [datetime]           NOT NULL,
            CONSTRAINT [PK_SchemaVersions_Id] PRIMARY KEY CLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.User', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[User]
        (
            [Id]       [uniqueidentifier]   NOT NULL,
            [RecordId] [int] IDENTITY (1,1) NOT NULL,
            CONSTRAINT [User_pk] PRIMARY KEY NONCLUSTERED
                (
                 [Id] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]
    END
GO

IF OBJECT_ID(N'dbo.UserActor', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[UserActor]
        (
            [UserId]   [uniqueidentifier]   NOT NULL,
            [RecordId] [int] IDENTITY (1,1) NOT NULL,
            [ActorId]  [uniqueidentifier]   NOT NULL,
            CONSTRAINT [UserActor_pk] PRIMARY KEY NONCLUSTERED
                (
                 [UserId] ASC,
                 [ActorId] ASC
                    ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
        ) ON [PRIMARY]

        ALTER TABLE [dbo].[UserActor]
            WITH CHECK ADD CONSTRAINT [UserActor_Actor_Id_fk] FOREIGN KEY ([ActorId])
                REFERENCES [dbo].[Actor] ([Id])

        ALTER TABLE [dbo].[UserActor]
            CHECK CONSTRAINT [UserActor_Actor_Id_fk]

        ALTER TABLE [dbo].[UserActor]
            WITH CHECK ADD CONSTRAINT [UserActor_User_Id_fk] FOREIGN KEY ([UserId])
                REFERENCES [dbo].[User] ([Id])

        ALTER TABLE [dbo].[UserActor]
            CHECK CONSTRAINT [UserActor_User_Id_fk]
    END
GO
