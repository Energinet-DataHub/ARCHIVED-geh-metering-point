BEGIN TRANSACTION

/* Insert all existing B2C u-001 users into the user table */
INSERT INTO [dbo].[User] VALUES ('52d22414-e419-4d46-a5c8-364301e83a78');
INSERT INTO [dbo].[User] VALUES ('85e6cd69-9f48-4754-a37d-f6dbc82527c1');
INSERT INTO [dbo].[User] VALUES ('58a495bf-054b-4c62-87b6-a5cfff59c525');
INSERT INTO [dbo].[User] VALUES ('39a02315-8492-4770-94bd-f82464b9e6d3');
INSERT INTO [dbo].[User] VALUES ('bad13775-7a78-4761-85d4-ba3e595392c6');
INSERT INTO [dbo].[User] VALUES ('d962b04e-9050-43df-b777-f1e20ad0e820');
INSERT INTO [dbo].[User] VALUES ('f4d23572-d35d-407f-a9ab-f5f681583f29');
INSERT INTO [dbo].[User] VALUES ('531b3623-4ea8-42e8-8e8a-a728d5aa4622');
INSERT INTO [dbo].[User] VALUES ('6023930b-db51-425c-98de-a7d514294583');
INSERT INTO [dbo].[User] VALUES ('aaff0dc2-a1ec-40de-acd8-16e9d6f31d3c');
INSERT INTO [dbo].[User] VALUES ('e6cca180-718c-446e-b8bb-e7086ee39c34');
INSERT INTO [dbo].[User] VALUES ('8a36c0cf-5f16-40c5-a7fd-910f0a200209');
INSERT INTO [dbo].[User] VALUES ('afb96804-2447-41f7-ac64-b4219fd1fd8b');
INSERT INTO [dbo].[User] VALUES ('57a79a65-9be2-4f02-8027-5c806fc8dd62');
INSERT INTO [dbo].[User] VALUES ('4756a304-66f3-4a21-85a3-e51bbd016844');
INSERT INTO [dbo].[User] VALUES ('ebbbda78-27d1-434e-8d7a-9ffd9c676c3a');
INSERT INTO [dbo].[User] VALUES ('da7cc5f9-0378-44e7-9945-d566abc97ddb');
INSERT INTO [dbo].[User] VALUES ('bb5bc5b7-699d-43dd-ba84-cb9ff125c4fa');
INSERT INTO [dbo].[User] VALUES ('7edc2020-e784-4aed-8cd8-a254806a21c4');
INSERT INTO [dbo].[User] VALUES ('dc3425d1-e557-489b-959e-3fdcd0db9389');
INSERT INTO [dbo].[User] VALUES ('977d58d7-cdb9-4bac-915b-31378a8deecd');

/* Create a cartesian product based on all the given UserIds and all actors currently in the system.
   Then take those values and insert them into the UserActor table unless they already exist.
 */
INSERT INTO [dbo].[UserActor] (UserId, ActorId)
(SELECT u.Id, a.Id
FROM [dbo].[User] u
CROSS JOIN Actor a
WHERE u.Id IN (
'52d22414-e419-4d46-a5c8-364301e83a78',
'85e6cd69-9f48-4754-a37d-f6dbc82527c1',
'58a495bf-054b-4c62-87b6-a5cfff59c525',
'39a02315-8492-4770-94bd-f82464b9e6d3',
'bad13775-7a78-4761-85d4-ba3e595392c6',
'd962b04e-9050-43df-b777-f1e20ad0e820',
'f4d23572-d35d-407f-a9ab-f5f681583f29',
'531b3623-4ea8-42e8-8e8a-a728d5aa4622',
'6023930b-db51-425c-98de-a7d514294583',
'aaff0dc2-a1ec-40de-acd8-16e9d6f31d3c',
'e6cca180-718c-446e-b8bb-e7086ee39c34',
'8a36c0cf-5f16-40c5-a7fd-910f0a200209',
'afb96804-2447-41f7-ac64-b4219fd1fd8b',
'57a79a65-9be2-4f02-8027-5c806fc8dd62',
'4756a304-66f3-4a21-85a3-e51bbd016844',
'ebbbda78-27d1-434e-8d7a-9ffd9c676c3a',
'da7cc5f9-0378-44e7-9945-d566abc97ddb',
'bb5bc5b7-699d-43dd-ba84-cb9ff125c4fa',
'7edc2020-e784-4aed-8cd8-a254806a21c4',
'dc3425d1-e557-489b-959e-3fdcd0db9389',
'977d58d7-cdb9-4bac-915b-31378a8deecd')
  AND NOT EXISTS (SELECT u.Id FROM [dbo].[UserActor] ua WHERE u.Id = ua.UserId AND a.Id = ua.ActorId))

COMMIT TRANSACTION
