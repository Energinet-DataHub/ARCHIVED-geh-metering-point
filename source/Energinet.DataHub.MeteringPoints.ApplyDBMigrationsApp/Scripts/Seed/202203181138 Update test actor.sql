-- Updated DDM 7 Actor with ID from t-001 instead of u-001


declare @newActorId varchar = '55878BD4-EF25-4F2E-8BA8-B2F3CCA962E6';
declare @oldActorId varchar = 'CDDF2D12-3F02-4A52-B1C1-C0D4E03262C4';

INSERT INTO [dbo].Actor (Id, IdentificationNumber, IdentificationType, Roles)
VALUES (@newActorId, 8200000002727, 'GLN', 'GridAccessProvider,MeteredDataResponsible');

UPDATE [dbo].UserActor SET ActorId = @newActorId
WHERE ActorId = @oldActorId;

UPDATE [dbo].GridAreas SET ActorId = @newActorId
WHERE ActorId = @oldActorId;

DELETE FROM [dbo].Actor WHERE Id = @oldActorId;
