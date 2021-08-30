ALTER TABLE [dbo].[MeteringPoints]
    ADD
    StreetCode [nvarchar](4) NULL,
    CitySubDivision [nvarchar](34) NULL,
    Floor [nvarchar](4) NULL,
    Room [nvarchar](4) NULL,
    BuildingNumber [nvarchar](6) NULL,
    MunicipalityCode [int] NULL