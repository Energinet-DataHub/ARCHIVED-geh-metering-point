CREATE TABLE [User]
(
    Id       UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT User_pk
            PRIMARY KEY NONCLUSTERED,
    RecordId INT IDENTITY
)
GO

CREATE UNIQUE INDEX User_RecordId_uindex
    ON [User] (RecordId)
GO


CREATE TABLE UserActor
(
    UserId   UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT UserActor_User_Id_fk
            REFERENCES [User],
    RecordId INT IDENTITY,
    ActorId  UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT UserActor_Actor_Id_fk
            REFERENCES Actor,
    CONSTRAINT UserActor_pk
        PRIMARY KEY NONCLUSTERED (UserId, ActorId)
)
GO

CREATE UNIQUE INDEX UserActor_RecordId_uindex
    ON UserActor (RecordId)
GO
