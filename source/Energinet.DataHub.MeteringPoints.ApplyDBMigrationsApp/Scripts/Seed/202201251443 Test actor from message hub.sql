INSERT INTO [dbo].[Actor] ([Id],[IdentificationNumber],[IdentificationType],[Roles]) VALUES ('a982de1f-3703-4e3c-be56-4e54418a2a59','0808118335003','GLN','Foo')

INSERT INTO [dbo].[GridAreas]([Id],[Code],[Name],[PriceAreaCode],[FullFlexFromDate],[ActorId]) VALUES ('5055e453-79e9-47b1-b138-6a6e90ccebfa','112','Batman Message hub test user','DK1',null,'a982de1f-3703-4e3c-be56-4e54418a2a59')

INSERT INTO [dbo].[GridAreaLinks] ([Id],[GridAreaId]) VALUES ('36a2959f-d0ea-41bc-acb7-e07871563010','5055e453-79e9-47b1-b138-6a6e90ccebfa')
