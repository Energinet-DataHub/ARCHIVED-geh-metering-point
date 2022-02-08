CREATE TABLE [dbo].[Processes](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](10) NOT NULL,
    [CreatedDate] [datetime2](7) NOT NULL,
    [EffectiveDate] [datetime2](7) NULL,
    [Status] [smallint] NULL,
    CONSTRAINT [PK_Processes] PRIMARY KEY CLUSTERED
    (
       [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[ProcessDetails](
    [Id] [uniqueidentifier] NOT NULL,
    [ProcessId] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](1024) NOT NULL,
    [Sender] [nvarchar](1024) NULL,
    [Receiver] [nvarchar](1024) NULL,
    [CreatedDate] [datetime2](7) NOT NULL,
    [EffectiveDate] [datetime2](7) NULL,
    [Status] [smallint] NULL,
    CONSTRAINT [PK_ProcessDetails] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[ProcessDetailErrors](
[Id] [uniqueidentifier] NOT NULL,
[ProcessDetailId] [uniqueidentifier] NOT NULL,
[Code] [nvarchar](50) NOT NULL,
[Description] [nvarchar](max) NULL,
CONSTRAINT [PK_ProcessDetailErrors] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]