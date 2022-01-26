INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber], [IdentificationType], [Roles])
VALUES ('90f6f4e5-8073-4842-b33e-c59e9f4a8c3f', '8200000001409', 'GLN',    'GridAccessProvider')

UPDATE [dbo].[GridAreas]
SET [ActorId] = '90f6f4e5-8073-4842-b33e-c59e9f4a8c3f'
WHERE [ActorId] = '158725db-35b5-4740-8ba4-80c616ec9f92'

DELETE FROM [dbo].[Actor]
WHERE [Id] = '158725db-35b5-4740-8ba4-80c616ec9f92'