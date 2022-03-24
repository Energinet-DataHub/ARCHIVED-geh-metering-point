-- Insert all new users
INSERT INTO dbo.[User] (Id) VALUES
    ('64266670-31B1-4B67-959E-2FE2155E5DAE'),
    ('6543508B-EB89-4F26-8F8E-6C605631668E'),
    ('E095A2AF-ACE0-4A3E-A88F-C9DD63DADAD6'),
    ('DF66BC94-17E4-420A-BF1B-8F8774FE3DD7'),
    ('1E759044-AABF-49B7-B438-2581001BCE32'),
    ('E772C735-E405-4460-B330-D9F3C0007495'),
    ('4951210B-1BE1-4E06-A592-16358A82A04D'),
    ('EAC422F9-2161-4B14-82D8-5495A97FCBE8'),
    ('5283D243-D79B-4CA3-950F-1AC945618328'),
    ('D4D6DB92-1291-4CF6-9512-DA859C7B503E'),
    ('9A621EC8-C76A-4926-BB73-DC7DBBDBFDA8'),
    ('C75FA988-30DE-4EE6-9E9E-FC78313C2C16'),
    ('CA450FE7-CBE4-435C-A049-39203C644F10'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64'),
    ('A4FFA131-6BB8-414D-AE40-DFED8B01FD19'),
    ('C9F20C64-75B6-4219-B7D2-9CC2D8ED6A7E'),
    ('54E61697-3314-4F26-A8D2-5DEA9B679E85'),
    ('0B5D57C9-9419-41F5-B109-6C33E85925BA'),
    ('681E0395-82D9-4947-8843-BB5C3BBA4FED'),
    ('2D85A84D-4D30-444D-878B-013D0B09C942'),
    ('5383221C-3DD1-49C7-A30A-C48493E1C937');




INSERT INTO dbo.[UserActor] (UserId, ActorId) VALUES

-- Assign Energinet users to u-001 Energinet actors
    ('54E61697-3314-4F26-A8D2-5DEA9B679E85','20284DD9-8586-4B9D-87B3-E1F29FAEC8FD'),
    ('54E61697-3314-4F26-A8D2-5DEA9B679E85','A982DE1F-3703-4E3C-BE56-4E54418A2A59'),
    ('54E61697-3314-4F26-A8D2-5DEA9B679E85','B65BF5D4-16ED-415A-BC42-90331D2F8EAF'),
    ('54E61697-3314-4F26-A8D2-5DEA9B679E85','DE716833-4F62-4542-933D-3DF435EB90C7'),
    ('681E0395-82D9-4947-8843-BB5C3BBA4FED','20284DD9-8586-4B9D-87B3-E1F29FAEC8FD'),
    ('681E0395-82D9-4947-8843-BB5C3BBA4FED','A982DE1F-3703-4E3C-BE56-4E54418A2A59'),
    ('681E0395-82D9-4947-8843-BB5C3BBA4FED','B65BF5D4-16ED-415A-BC42-90331D2F8EAF'),
    ('681E0395-82D9-4947-8843-BB5C3BBA4FED','DE716833-4F62-4542-933D-3DF435EB90C7'),
    ('5383221C-3DD1-49C7-A30A-C48493E1C937','20284DD9-8586-4B9D-87B3-E1F29FAEC8FD'),
    ('5383221C-3DD1-49C7-A30A-C48493E1C937','A982DE1F-3703-4E3C-BE56-4E54418A2A59'),
    ('5383221C-3DD1-49C7-A30A-C48493E1C937','B65BF5D4-16ED-415A-BC42-90331D2F8EAF'),
    ('5383221C-3DD1-49C7-A30A-C48493E1C937','DE716833-4F62-4542-933D-3DF435EB90C7'),
    ('A4FFA131-6BB8-414D-AE40-DFED8B01FD19','20284DD9-8586-4B9D-87B3-E1F29FAEC8FD'),
    ('A4FFA131-6BB8-414D-AE40-DFED8B01FD19','A982DE1F-3703-4E3C-BE56-4E54418A2A59'),
    ('A4FFA131-6BB8-414D-AE40-DFED8B01FD19','B65BF5D4-16ED-415A-BC42-90331D2F8EAF'),
    ('A4FFA131-6BB8-414D-AE40-DFED8B01FD19','DE716833-4F62-4542-933D-3DF435EB90C7'),
    ('0B5D57C9-9419-41F5-B109-6C33E85925BA','20284DD9-8586-4B9D-87B3-E1F29FAEC8FD'),
    ('0B5D57C9-9419-41F5-B109-6C33E85925BA','A982DE1F-3703-4E3C-BE56-4E54418A2A59'),
    ('0B5D57C9-9419-41F5-B109-6C33E85925BA','B65BF5D4-16ED-415A-BC42-90331D2F8EAF'),
    ('0B5D57C9-9419-41F5-B109-6C33E85925BA','DE716833-4F62-4542-933D-3DF435EB90C7'),

-- Assign Energinet users to b-002 actors
    ('6543508B-EB89-4F26-8F8E-6C605631668E','09591F53-B531-4DB5-BB66-A2F52A393D82'),
    ('6543508B-EB89-4F26-8F8E-6C605631668E','83510249-9F7C-4827-A67C-499A3A94F533'),
    ('6543508B-EB89-4F26-8F8E-6C605631668E','0E223E42-BED4-4778-A973-8D0AD9813F71'),
    ('D4D6DB92-1291-4CF6-9512-DA859C7B503E','09591F53-B531-4DB5-BB66-A2F52A393D82'),
    ('D4D6DB92-1291-4CF6-9512-DA859C7B503E','83510249-9F7C-4827-A67C-499A3A94F533'),
    ('D4D6DB92-1291-4CF6-9512-DA859C7B503E','0E223E42-BED4-4778-A973-8D0AD9813F71'),

-- Assign Energinet users to t-001 actors
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','1E2CC2E8-670E-40FA-BFAE-BE0FEC70E9AA'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','158725DB-35B5-4740-8BA4-80C616EC9F92'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','A4ABD4AD-4FF3-4E93-A86D-7B164910CA34'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','AF450C03-1937-4EA1-BB66-17B6E4AA51F5'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','B8F00060-E02A-4418-A1BD-963778473238'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','CB2266E1-7ACD-49F7-95AE-858EF7800E39'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','D49BC21D-87C0-426B-BD2D-A097A91DE611'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','F718AC37-9B7F-42DC-943E-BD211EA044FF'),
    ('8C7C3FA7-00F2-4CD7-8208-2D98FF21D07B','0FC7E028-3157-48F2-8744-5C23EFFE857D'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','1E2CC2E8-670E-40FA-BFAE-BE0FEC70E9AA'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','158725DB-35B5-4740-8BA4-80C616EC9F92'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','A4ABD4AD-4FF3-4E93-A86D-7B164910CA34'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','AF450C03-1937-4EA1-BB66-17B6E4AA51F5'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','B8F00060-E02A-4418-A1BD-963778473238'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','CB2266E1-7ACD-49F7-95AE-858EF7800E39'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','D49BC21D-87C0-426B-BD2D-A097A91DE611'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','F718AC37-9B7F-42DC-943E-BD211EA044FF'),
    ('0DCAD398-8179-498B-8DDA-CADB332977D7','0FC7E028-3157-48F2-8744-5C23EFFE857D'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','1E2CC2E8-670E-40FA-BFAE-BE0FEC70E9AA'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','158725DB-35B5-4740-8BA4-80C616EC9F92'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','A4ABD4AD-4FF3-4E93-A86D-7B164910CA34'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','AF450C03-1937-4EA1-BB66-17B6E4AA51F5'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','B8F00060-E02A-4418-A1BD-963778473238'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','CB2266E1-7ACD-49F7-95AE-858EF7800E39'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','D49BC21D-87C0-426B-BD2D-A097A91DE611'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','F718AC37-9B7F-42DC-943E-BD211EA044FF'),
    ('F7DE83B9-8313-4066-8AC5-1C3F49730D64','0FC7E028-3157-48F2-8744-5C23EFFE857D'),

-- Assign external user to b-002 Actors
    ('E772C735-E405-4460-B330-D9F3C0007495','2A0FD613-BB31-455B-9182-0534AC42BA59'),
    ('E772C735-E405-4460-B330-D9F3C0007495','4532D655-B91D-4D6D-8103-1B8471E0BDC6'),
    ('E772C735-E405-4460-B330-D9F3C0007495','61B46919-07E4-4A49-A22A-55708F9EBBF0'),
    ('E772C735-E405-4460-B330-D9F3C0007495','8F7566A0-D540-4789-8739-72484060FBFD'),
    ('E772C735-E405-4460-B330-D9F3C0007495','DDB53B36-9F61-41C4-BEBE-77C06CD33151'),
    ('E772C735-E405-4460-B330-D9F3C0007495','F35E48D3-423A-4507-BCF5-A4268D74FA01'),

-- Assign external user to b-002 Actors
    ('1E759044-AABF-49B7-B438-2581001BCE32','8E6124B1-36F0-43AC-B022-73D9C0FDE334'),
    ('1E759044-AABF-49B7-B438-2581001BCE32','F9B5E62B-3576-4CC6-81B7-CCDEA2010D41');
