using System;
using System.Collections;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Test.Model
{
    public class Scripts
    {
        public static string CreateSQL = @"
CREATE DATABASE [Glue_Data_Test]
GO
USE [Glue_Data_Test]
GO

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
GO
";

        public static string CreateMySql = 
@"
CREATE TABLE `Language` (
  `Code` VarChar(4) NOT NULL,
  `Name` NVarChar(50) NOT NULL,
  CONSTRAINT `PK_Language` PRIMARY KEY CLUSTERED (`Code`)
)

CREATE TABLE `Country` (
  `Code` VarChar(4) NOT NULL,
  `Name` NVarChar(50) NOT NULL,
  CONSTRAINT `PK_Country` PRIMARY KEY CLUSTERED (`Code`)
)

CREATE TABLE `Customer` (
  `CustomerCode` NVarChar(8) NOT NULL,
  `DisplayName` NVarChar(100) NOT NULL,
  CONSTRAINT `PK_Customer` PRIMARY KEY CLUSTERED (`CustomerCode`)
)

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

        public static string CreateSQLite = @"
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

    public class Address
    {
        public string Street;
        public string ZipCode;
        public string City;
        [Column("CountryCode")]
        public Country Country;
    }

    [Table]
    public class Contact
    {
        // Attributes
        [AutoKey]
        [Column("ContactId")]
        public int Id;
        public Customer Customer;
        public string DisplayName { get { return ("" + FirstName + " " + LastName).Trim(); } }
        public string Email;
        public string FirstName;
        public string LastName;
        public Address Address = new Address();
        [Column("LanguageCode")]
        public Language Language;

        // Instance methods
        public void Insert()
        {
            provider.Insert(this);
        }
        public void Update()
        {
            provider.Update(this);
        }
        public void Delete()
        {
            Delete(Id);
        }
        public Category[] CategoryList()
        {
            return (Category[])provider.ListManyToMany(this, typeof(Category));
        }
        public Category[] CategoryList(int index, int count)
        {
            return (Category[])provider.ListManyToMany(this, typeof(Category), null, null, new Limit(index,count));
        }
        public void CategoryAdd(Category category)
        {
            provider.AddManyToMany(this, category);
        }
        public void CategoryDelete(Category category)
        {
            provider.DelManyToMany(this, category);
        }

        // Static methods
        static IMappingProvider provider = DataMappingTest.Provider;

        public static Contact Find(int Id)                                                
        { 
            return (Contact)provider.Find(typeof(Contact), Id); 
        }
        public static Contact[] List(Filter filter, Order order, Limit limit)                   
        { 
            return (Contact[])provider.List(typeof(Contact), filter, order, limit); 
        }
        public static void Delete(int Id)                                                
        { 
            provider.Delete(typeof(Contact), Id); 
        }
    }

    [Table(Cached=true)]
    public class Country
    {
        [Key]
        public string Code;
        public string Name;
        
        public Country()
        {
        }
        private Country(string code, string name)
        {
        }
        public static implicit operator Country(string code)
        {
            return Find(code);
        }
        public override string ToString()
        {
            return Code;
        }

        // Static methods
        static IMappingProvider provider = DataMappingTest.Provider;

        public static Country Find(string code)
        { 
            return (Country)provider.Find(typeof(Country), code); 
        }
        public static void Save(string code, string name)
        { 
            provider.Save(new Country(code, name)); 
        }
        public static Country[] List()
        { 
            return (Country[])provider.List(typeof(Country), null, null, null);
        }
    }
    
    [Table(Cached=true)]
    public class Language
    {
        [Key]
        public string Code;
        public string Name;

        private Language(string code, string name)
        {
            Code = code;
            Name = name;
        }
        public Language()
        {
        }
        public static implicit operator Language(string code)
        {
            return Find(code);
        }
        public override string ToString()
        {
            return Code;
        }

        // Static methods
        static IMappingProvider provider = DataMappingTest.Provider;

        public static Language Find(string code)
        { 
            return (Language)provider.Find(typeof(Language), code); 
        }
        public static void Save(string code, string name)
        { 
            provider.Save(new Language(code, name)); 
        }
        public static Language[] List()
        { 
            return (Language[])provider.List(typeof(Language), null, null, null);
        }
    }
    
    [Table]
    public class Category
    {
        [AutoKey]
        public int CategoryId;
        public string CategoryName;
        
        public static implicit operator Category(int id)
        {
            return Find(id);
        }

        static IMappingProvider provider = DataMappingTest.Provider;

        static Category Find(int id)
        {
            return (Category)provider.Find(typeof(Category), id);
        }
    }

    [Table]
    public class Customer
    {
        [Key]
        [Column("CustomerCode")]
        public string Code;
        public string DisplayName;
        
        // Instance methods
        public void Insert()
        {
            provider.Insert(this);
        }
        public void Update()
        {
            provider.Update(this);
        }
        public Order[] ListOrders(Filter filter, Order order, Limit limit)
        {
            return (Order[])provider.List(typeof(Order), Filter.And("CustomerCode=" + Code, filter), order, limit);
        }
        public Contact[] ListContacts(Filter filter, Order order, Limit limit)
        {
            return (Contact[])provider.List(typeof(Order), Filter.And("CustomerCode=" + Code, filter), order, limit);
        }

        // Static methods
        static IMappingProvider provider = DataMappingTest.Provider;

        public static Customer Find(string code)
        {
            return (Customer)provider.Find(typeof(Customer), code);
        }
        public static Customer[] List(Filter filter, Order order, Limit limit)
        {
            return (Customer[])provider.List(typeof(Customer), filter, order, limit);
        }
        public static void Delete(string code)
        {
            provider.Delete(typeof(Customer), code);
        }
    }
}
