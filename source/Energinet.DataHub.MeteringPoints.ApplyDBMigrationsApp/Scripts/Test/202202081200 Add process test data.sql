INSERT [dbo].[ProcessDetails] ([Id], [ProcessId], [Name], [Sender], [Receiver], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'f7373d59-48ac-41f8-a9c3-19a3f230c32d', N'c9ca4ec1-4f48-44e4-b9fb-4e39185d0173', N'ConfirmCreateAccountingPointCharacteristics', N'5790001330552', N'8200000001409', N'2022-02-08T10:30:07.4727139', NULL, 3)
INSERT [dbo].[ProcessDetails] ([Id], [ProcessId], [Name], [Sender], [Receiver], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'aee455ca-eded-44f8-aecc-dc8562e0e394', N'c9ca4ec1-4f48-44e4-b9fb-4e39185d0173', N'RequestCreateAccountingPointCharacteristics', N'8200000001409', N'5790001330552', N'2022-02-08T10:30:07.4716709', N'2021-09-25T22:00:00.0000000', 2)
INSERT [dbo].[Processes] ([Id], [Name], [MeteringPointGsrn], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'c9ca4ec1-4f48-44e4-b9fb-4e39185d0173', N'BRS-004', N'571313157178361184', N'2022-02-08T10:30:07.4716709', N'2021-09-25T22:00:00.0000000', 1)

INSERT [dbo].[ProcessDetailErrors] ([Id] ,[ProcessDetailId] ,[Code] ,[Description])
VALUES (N'bbb29c85-fb22-4b76-8ee4-9cd67fcb689f', N'278b60a8-5ae6-472b-b2ec-c513cb3cbaaa', N'E01', N'Denied')
INSERT [dbo].[ProcessDetails] ([Id], [ProcessId], [Name], [Sender], [Receiver], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'278b60a8-5ae6-472b-b2ec-c513cb3cbaaa', N'5d07aa08-43be-42cb-843d-1444f0d17753', N'RejectCreateAccountingPointCharacteristics', N'5790001330552', N'8200000001409', N'2022-02-08T10:33:07.4727139', NULL, 3)
INSERT [dbo].[ProcessDetails] ([Id], [ProcessId], [Name], [Sender], [Receiver], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'ce4af156-8179-4f85-8dee-de27522d3036', N'5d07aa08-43be-42cb-843d-1444f0d17753', N'RequestCreateAccountingPointCharacteristics', N'8200000001409', N'5790001330552', N'2022-02-08T10:33:07.4716709', N'2021-09-25T22:00:00.0000000', 2)
INSERT [dbo].[Processes] ([Id], [Name], [MeteringPointGsrn], [CreatedDate], [EffectiveDate], [Status])
VALUES (N'5d07aa08-43be-42cb-843d-1444f0d17753', N'BRS-004', N'571313157178361184', N'2022-02-08T10:33:07.4716709', N'2021-09-25T22:00:00.0000000', 1)