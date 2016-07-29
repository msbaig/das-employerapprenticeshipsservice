﻿CREATE TABLE [dbo].[User]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PireanKey] UNIQUEIDENTIFIER NOT NULL, 
    [Email] NVARCHAR(50) NOT NULL, 
    [FirstName] NVARCHAR(MAX) NULL, 
    [LastName] NVARCHAR(MAX) NULL
)