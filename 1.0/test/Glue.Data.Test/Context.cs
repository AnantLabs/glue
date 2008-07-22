using System;
using System.Collections.Generic;
using System.Text;
using Glue.Lib;
using Glue.Data;
using System.Xml;

namespace Glue.Data.Test
{
    public abstract class Context
    {
        public static Context Current = null;
        
        public IDataProvider Provider;
        public abstract void CreateDatabase();

    }

    /// <summary>
    /// SqlSetup
    /// </summary>
    public class SqlContext : Context
    {
        public SqlContext(XmlNode node)
        {
            Provider = new Glue.Data.Providers.Sql.SqlDataProvider(node);
        }

        public override void CreateDatabase()
        {
            Provider.ExecuteNonQuery(Script);
        }

        public string Script = @"
IF EXISTS (SELECT * FROM sysobjects WHERE name='ContactCategory' AND type='U')
DROP TABLE [ContactCategory]

IF EXISTS (SELECT * FROM sysobjects WHERE name='Contact' AND type='U')
DROP TABLE [Contact]

IF EXISTS (SELECT * FROM sysobjects WHERE name='Category' AND type='U')
DROP TABLE [Category]

IF EXISTS (SELECT * FROM sysobjects WHERE name='Customer' AND type='U')
DROP TABLE [Customer]

IF EXISTS (SELECT * FROM sysobjects WHERE name='Country' AND type='U')
DROP TABLE [Country]

IF EXISTS (SELECT * FROM sysobjects WHERE name='Language' AND type='U')
DROP TABLE [Language]

CREATE TABLE [Language] (
  [Code] VarChar(4) NOT NULL,
  [Name] NVarChar(50) NOT NULL,
  CONSTRAINT [PK_Language] PRIMARY KEY CLUSTERED ([Code])
)

CREATE TABLE [Country] (
  [Code] VarChar(4) NOT NULL,
  [Name] NVarChar(50) NOT NULL,
  CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([Code])
)

CREATE TABLE [Customer] (
  [CustomerCode] NVarChar(8) NOT NULL,
  [DisplayName] NVarChar(100) NOT NULL,
  CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([CustomerCode])
)

CREATE TABLE [Category] (
  [CategoryId] Int IDENTITY (1, 1) NOT NULL,
  [CategoryName] NVarChar(100) NOT NULL,
  CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryId])
)

CREATE TABLE [Contact] (
  [ContactId] Int IDENTITY (1, 1) NOT NULL,
  [FirstName] NVarChar(50) NULL,
  [LastName] NVarChar(100) NOT NULL,
  [Email] NVarChar(100) NOT NULL,
  [AddressStreet] NVarChar(100) NULL,
  [AddressCity] NVarChar(100) NULL,
  [AddressZipCode] VarChar(10) NULL,
  [AddressCountryCode] VarChar(4) NULL,
  [CustomerCode] VarChar(8) NULL,
  [LanguageCode] VarChar(4) NULL,
  CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED ([ContactId])
)
CREATE INDEX [IX_LastName] ON [Contact]([LastName])
CREATE INDEX [IX_Email] ON [Contact]([Email])

CREATE TABLE [ContactCategory] (
  [ContactId] Int NOT NULL,
  [CategoryId] Int NOT NULL,
  CONSTRAINT [PK_ContactCategory] PRIMARY KEY CLUSTERED ([ContactId],[CategoryId])
)
";
    }
    
    /// <summary>
    /// MySqlSetup
    /// </summary>
    public class MySqlContext : Context
    {
        public MySqlContext(XmlNode node)
        {
            Provider = new Glue.Data.Providers.MySql.MySqlDataProvider(node);
        }

        public override void CreateDatabase()
        {
            Provider.ExecuteNonQuery(Script);
        }

        public string Script = @"
DROP TABLE IF EXISTS `ContactCategory`;
DROP TABLE IF EXISTS `Contact`;
DROP TABLE IF EXISTS `Category`;
DROP TABLE IF EXISTS `Customer`;
DROP TABLE IF EXISTS `Country`;
DROP TABLE IF EXISTS `Language`;

CREATE TABLE `Language` (
  `Code` VarChar(4) NOT NULL,
  `Name` NVarChar(50) NOT NULL,
  CONSTRAINT `PK_Language` PRIMARY KEY CLUSTERED (`Code`)
);

CREATE TABLE `Country` (
  `Code` VarChar(4) NOT NULL,
  `Name` NVarChar(50) NOT NULL,
  CONSTRAINT `PK_Country` PRIMARY KEY CLUSTERED (`Code`)
);

CREATE TABLE `Customer` (
  `CustomerCode` NVarChar(8) NOT NULL,
  `DisplayName` NVarChar(100) NOT NULL,
  CONSTRAINT `PK_Customer` PRIMARY KEY CLUSTERED (`CustomerCode`)
);

CREATE TABLE `Category` (
  `CategoryId` INT AUTO_INCREMENT,
  `CategoryName` NVarChar(100) NOT NULL,
  CONSTRAINT `PK_Category` PRIMARY KEY CLUSTERED (`CategoryId`)
);

CREATE TABLE `Contact` (
  `ContactId` INT AUTO_INCREMENT NOT NULL,
  `FirstName` NVarChar(50) NULL,
  `LastName` NVarChar(100) NOT NULL,
  `Email` NVarChar(100) NOT NULL,
  `AddressStreet` NVarChar(100) NULL,
  `AddressCity` NVarChar(100) NULL,
  `AddressZipCode` VarChar(10) NULL,
  `AddressCountryCode` VarChar(4) NULL,
  `CustomerCode` VarChar(8) NULL,
  `LanguageCode` VarChar(4) NULL,
  CONSTRAINT `PK_Contact` PRIMARY KEY CLUSTERED (`ContactId`)
);
CREATE INDEX `IX_LastName` ON `Contact`(`LastName`);
CREATE INDEX `IX_Email` ON `Contact`(`Email`);

CREATE TABLE `ContactCategory` (
  `ContactId` Int NOT NULL,
  `CategoryId` Int NOT NULL,
  CONSTRAINT `PK_ContactCategory` PRIMARY KEY CLUSTERED (`ContactId`,`CategoryId`)
);
";
    }

    public class SQLiteContext : Context
    {
        public SQLiteContext(XmlNode node)
        {
            Provider = new Glue.Data.Providers.SQLite.SQLiteDataProvider(node);
        }

        public override void CreateDatabase()
        {
            Provider.ExecuteNonQuery(Script);
        }

        public string Script = @"
DROP TABLE IF EXISTS [ContactCategory];
DROP TABLE IF EXISTS [Contact];
DROP TABLE IF EXISTS [Category];
DROP TABLE IF EXISTS [Customer];
DROP TABLE IF EXISTS [Country];
DROP TABLE IF EXISTS [Language];

CREATE TABLE [Language] (
  [Code] VARCHAR(4) NOT NULL,
  [Name] NVARCHAR(50) NOT NULL,
  CONSTRAINT [PK_Language] PRIMARY KEY ([Code])
);

CREATE TABLE [Country] (
  [Code] VARCHAR(4) NOT NULL,
  [Name] NVARCHAR(50) NOT NULL,
  CONSTRAINT [PK_Country] PRIMARY KEY ([Code])
);

CREATE TABLE [Customer] (
  [CustomerCode] VARCHAR(8) NOT NULL,
  [DisplayName] NVARCHAR(100) NOT NULL,
  CONSTRAINT [PK_Customer] PRIMARY KEY ([CustomerCode])
);

CREATE TABLE [Category] (
  [CategoryId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  [CategoryName] NVARCHAR(100) NOT NULL
);

CREATE TABLE [Contact] (
  [ContactId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  [FirstName] NVARCHAR(50) NULL,
  [LastName] NVARCHAR(100) NOT NULL,
  [Email] VARCHAR(100) NOT NULL,
  [AddressStreet] NVARCHAR(100) NULL,
  [AddressCity] NVARCHAR(100) NULL,
  [AddressZipCode] VARCHAR(10) NULL,
  [AddressCountryCode] VARCHAR(4) NULL,
  [CustomerCode] VARCHAR(8) NULL,
  [LanguageCode] VARCHAR(4) NULL
);
CREATE INDEX [IX_LastName] ON [Contact]([LastName]);
CREATE INDEX [IX_Email] ON [Contact]([Email]);

CREATE TABLE [ContactCategory] (
  [ContactId] Int NOT NULL,
  [CategoryId] Int NOT NULL,
  CONSTRAINT [PK_ContactCategory] PRIMARY KEY ([ContactId],[CategoryId])
);
";
    }

}
