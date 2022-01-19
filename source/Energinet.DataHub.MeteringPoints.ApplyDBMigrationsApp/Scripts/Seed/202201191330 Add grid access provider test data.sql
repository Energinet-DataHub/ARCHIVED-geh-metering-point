INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber], [IdentificationType], [Roles])
VALUES ('f0cc5ecf-3401-4714-a1a1-1bfc3ce61f3d', '8100000000016', 'GLN',    'GridAccessProvider')
INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber], [IdentificationType], [Roles])
VALUES ('a117349a-0265-4ce9-9303-a4f84bf0eb9c', '8100000000023', 'GLN',    'GridAccessProvider')
INSERT INTO [dbo].[Actor] ([Id], [IdentificationNumber], [IdentificationType], [Roles])
VALUES ('31e06e54-3473-4f3d-94d3-c91f26dcd092', '8100000000030', 'GLN',    'GridAccessProvider')

INSERT INTO [dbo].[GridAreas] ([Id], [Code], [Name], [PriceAreaCode], [ActorId])
VALUES ('0cf56e44-d021-411c-936d-3979d8b2c49c', '880', 'Kilo', 'DK2', 'f0cc5ecf-3401-4714-a1a1-1bfc3ce61f3d')
INSERT INTO [dbo].[GridAreas] ([Id], [Code], [Name], [PriceAreaCode], [ActorId])
VALUES ('0b923230-9101-40fb-b798-563176faf3ab', '881', 'Lima', 'DK2', 'a117349a-0265-4ce9-9303-a4f84bf0eb9c')
INSERT INTO [dbo].[GridAreas] ([Id], [Code], [Name], [PriceAreaCode], [ActorId])
VALUES ('7ce2d3e1-2fac-49e3-bf46-de75c5f8b714', '882', 'Mike', 'DK2', '31e06e54-3473-4f3d-94d3-c91f26dcd092')