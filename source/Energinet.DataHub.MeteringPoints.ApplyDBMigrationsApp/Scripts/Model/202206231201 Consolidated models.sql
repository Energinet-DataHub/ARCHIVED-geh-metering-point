DELETE
FROM SchemaVersions
WHERE ScriptName like 'Energinet.DataHub.MeteringPoints.ApplyDBMigrationsApp.Scripts.Model%';

IF OBJECT_ID(N'dbo.Actor', N'U') IS NULL
    BEGIN
        create table Actor
        (
            Id                   uniqueidentifier not null
                constraint PK_Actor
                    primary key nonclustered,
            RecordId             int identity,
            IdentificationNumber nvarchar(50)     not null,
            IdentificationType   nvarchar(50)     not null,
            Roles                nvarchar(max)    not null
        )
    END
go

IF OBJECT_ID(N'dbo.BusinessProcesses', N'U') IS NULL
    BEGIN
        create table BusinessProcesses
        (
            Id            uniqueidentifier not null
                constraint PK_BusinessProcesses
                    primary key nonclustered,
            RecordId      int identity,
            TransactionId nvarchar(36)     not null,
            ProcessType   nvarchar(50)     not null,
            Status        nvarchar(50)     not null
        )

        create unique clustered index CIX_BusinessProcesses
            on BusinessProcesses (RecordId)
    END
go


IF OBJECT_ID(N'dbo.GridAreas', N'U') IS NULL
    BEGIN
        create table GridAreas
        (
            Id               uniqueidentifier not null
                constraint PK_GridAreas
                    primary key nonclustered,
            RecordId         int identity,
            Code             nvarchar(3)      not null,
            Name             nvarchar(50)     not null,
            PriceAreaCode    nvarchar(5)      not null,
            FullFlexFromDate datetime2,
            ActorId          uniqueidentifier not null
                constraint FK_GridAreas_Actor
                    references Actor
        )
    END
go

IF OBJECT_ID(N'dbo.GridAreaLinks', N'U') IS NULL
    BEGIN
        create table GridAreaLinks
        (
            Id         uniqueidentifier not null
                constraint PK_GridAreaLinks
                    primary key nonclustered,
            RecordId   int identity,
            GridAreaId uniqueidentifier not null
                constraint FK_GridAreaLinks_GridAreas
                    references GridAreas
        )

        create unique clustered index CIX_GridAreaLink
            on GridAreaLinks (RecordId)

        create unique clustered index CIX_GridArea
            on GridAreas (RecordId)

        create unique index CIX_GridAreas_Code
            on GridAreas (Code)
    END
go

IF OBJECT_ID(N'dbo.IncomingMessages', N'U') IS NULL
    BEGIN
        create table IncomingMessages
        (
            RecordId    int identity,
            MessageId   nvarchar(50)  not null,
            MessageType nvarchar(255) not null,
            constraint PK_IncomingMessages
                primary key nonclustered (MessageId, MessageType)
        )
        create unique clustered index CIX_IncomingMessages
            on IncomingMessages (RecordId)
    END
go



IF OBJECT_ID(N'dbo.MessageHubMessages', N'U') IS NULL
    BEGIN
        create table MessageHubMessages
        (
            Id           uniqueidentifier        not null
                constraint PK_MessageHubMessages
                    primary key nonclustered,
            RecordId     int identity,
            Correlation  nvarchar(500)           not null,
            Type         nvarchar(500)           not null,
            Content      nvarchar(max)           not null,
            Date         datetime2               not null,
            Recipient    nvarchar(128)           not null,
            BundleId     nvarchar(50),
            DequeuedDate datetime2,
            GsrnNumber   nvarchar(36) default '' not null
        )

        create unique clustered index CIX_MessageHubMessages
            on MessageHubMessages (RecordId)
    END
go


IF OBJECT_ID(N'dbo.MeteringPoints', N'U') IS NULL
    BEGIN
        create table MeteringPoints
        (
            Id                            uniqueidentifier not null
                constraint PK_MeteringPoints
                    primary key nonclustered,
            RecordId                      int identity,
            GsrnNumber                    nvarchar(36)     not null,
            StreetName                    nvarchar(500)    not null,
            PostCode                      nvarchar(10)     not null,
            CityName                      nvarchar(100)    not null,
            CountryCode                   nvarchar(4)      not null,
            ConnectionState_PhysicalState nvarchar(255)    not null,
            MeteringPointSubType          nvarchar(50),
            MeterReadingOccurrence        nvarchar(50),
            TypeOfMeteringPoint           nvarchar(50)     not null,
            MaximumCurrent                int default 0,
            MaximumPower                  int default 0,
            MeteringGridArea              uniqueidentifier,
            PowerPlant                    nvarchar(255),
            LocationDescription           nvarchar(255),
            ProductType                   nvarchar(255),
            ParentRelatedMeteringPoint    uniqueidentifier,
            UnitType                      nvarchar(255),
            EffectiveDate                 datetime2,
            MeterNumber                   nvarchar(255),
            ConnectionState_EffectiveDate datetime2,
            StreetCode                    nvarchar(4),
            CitySubDivision               nvarchar(34),
            Floor                         nvarchar(4),
            Room                          nvarchar(4),
            BuildingNumber                nvarchar(6),
            MunicipalityCode              int,
            IsActualAddress               bit default 0,
            GeoInfoReference              uniqueidentifier,
            Capacity                      float,
            AssetType                     nvarchar(255),
            SettlementMethod              nvarchar(50),
            ScheduledMeterReadingDate     nvarchar(10),
            ProductionObligation          bit default 0,
            NetSettlementGroup            nvarchar(10),
            DisconnectionType             nvarchar(10),
            ConnectionType                nvarchar(50),
            StartOfSupplyDate             datetime2,
            ToGrid                        uniqueidentifier,
            FromGrid                      uniqueidentifier
        )
    END
go

IF OBJECT_ID(N'dbo.EnergySuppliers', N'U') IS NULL
    BEGIN
        create table EnergySuppliers
        (
            Id                    uniqueidentifier not null
                constraint PK_EnergySupplier
                    primary key nonclustered,
            RecordId              int identity,
            StartOfSupplyDate     datetime2        not null,
            GlnNumber             nvarchar(50)     not null,
            MarketMeteringPointId uniqueidentifier not null
                references MeteringPoints
        )

        create unique clustered index CIX_EnergySupplier
            on EnergySuppliers (RecordId)

        create unique clustered index CIX_MeteringPoints
            on MeteringPoints (RecordId)

        create unique index CIX_MeteringPoints_GsrnNumber
            on MeteringPoints (GsrnNumber)
    END
go

IF OBJECT_ID(N'dbo.OutboxMessages', N'U') IS NULL
    BEGIN
        create table OutboxMessages
        (
            Id            uniqueidentifier             not null
                constraint PK_OutboxMessages
                    primary key nonclustered,
            RecordId      int identity,
            Type          nvarchar(255)                not null,
            Data          nvarchar(max)                not null,
            Category      nvarchar(50)                 not null,
            CreationDate  datetime2                    not null,
            ProcessedDate datetime2,
            Correlation   nvarchar(255) default 'None' not null
        )

        create unique clustered index CIX_OutboxMessages
            on OutboxMessages (RecordId)

        create index ProcessedOutboxMessagesNonClusteredIndex
            on OutboxMessages (CreationDate, Category) include (Id, Data, Type, ProcessedDate)
            where [ProcessedDate] IS NULL
    END
go



IF OBJECT_ID(N'dbo.ProcessDetailErrors', N'U') IS NULL
    BEGIN
        create table ProcessDetailErrors
        (
            Id              uniqueidentifier not null
                constraint PK_ProcessDetailErrors
                    primary key,
            ProcessDetailId uniqueidentifier not null,
            Code            nvarchar(50)     not null,
            Description     nvarchar(max)
        )
    END
go

IF OBJECT_ID(N'dbo.ProcessDetails', N'U') IS NULL
    BEGIN
        create table ProcessDetails
        (
            Id            uniqueidentifier not null
                constraint PK_ProcessDetails
                    primary key,
            ProcessId     uniqueidentifier not null,
            Name          nvarchar(1024)   not null,
            Sender        nvarchar(1024),
            Receiver      nvarchar(1024),
            CreatedDate   datetime2        not null,
            EffectiveDate datetime2,
            Status        int
        )
    END
go

IF OBJECT_ID(N'dbo.Processes', N'U') IS NULL
    BEGIN
        create table Processes
        (
            Id                uniqueidentifier not null
                constraint PK_Processes
                    primary key,
            Name              nvarchar(1024)   not null,
            MeteringPointGsrn nvarchar(128)    not null,
            CreatedDate       datetime2        not null,
            EffectiveDate     datetime2,
            Status            int
        )
    END
go

IF OBJECT_ID(N'dbo.QueuedInternalCommands', N'U') IS NULL
    BEGIN
        create table QueuedInternalCommands
        (
            Id             uniqueidentifier             not null
                constraint PK_InternalCommandQueue
                    primary key nonclustered,
            RecordId       int identity
                constraint UC_InternalCommandQueue_Id
                    unique clustered,
            Type           nvarchar(255)                not null,
            Data           varbinary(max)               not null,
            ScheduleDate   datetime2(1),
            DispatchedDate datetime2(1),
            SequenceId     bigint,
            ProcessedDate  datetime2(1),
            CreationDate   datetime2                    not null,
            CorrelationId  nvarchar(255) default 'None' not null
        )
    END
go

IF OBJECT_ID(N'dbo.User', N'U') IS NULL
    BEGIN
        create table [User]
        (
            Id       uniqueidentifier not null
                constraint User_pk
                    primary key nonclustered,
            RecordId int identity
        )

        create unique index User_RecordId_uindex
            on [User] (RecordId)
    END
go

IF OBJECT_ID(N'dbo.UserActor', N'U') IS NULL
    BEGIN
        create table UserActor
        (
            UserId   uniqueidentifier not null
                constraint UserActor_User_Id_fk
                    references [User],
            RecordId int identity,
            ActorId  uniqueidentifier not null
                constraint UserActor_Actor_Id_fk
                    references Actor,
            constraint UserActor_pk
                primary key nonclustered (UserId, ActorId)
        )

        create unique index UserActor_RecordId_uindex
            on UserActor (RecordId)
    END
go
