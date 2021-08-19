CREATE TABLE [dbo].[QueuedInternalCommands](
    [Id] [uniqueidentifier] NOT NULL,
    [RecordId] [int] IDENTITY(1,1) NOT NULL,
    [Type] [nvarchar](255) NOT NULL,
    [Data] [varbinary](max) NOT NULL,
    [ScheduleDate] [datetime2](1) NULL,
    [DispatchedDate] [datetime2](1) NULL,
    [SequenceId] [bigint] NULL,
    [ProcessedDate] [datetime2](1) NULL,
    [CreationDate] [datetime2](7) NOT NULL,
    [CorrelationId] [nvarchar](50) NULL,
    CONSTRAINT [PK_InternalCommandQueue] PRIMARY KEY NONCLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
    CONSTRAINT [UC_InternalCommandQueue_Id] UNIQUE CLUSTERED
(
[RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]