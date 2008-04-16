using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Test
{
    public class DataMappingTest
    {
        public IDataProvider Provider;

        [Test]
        public void TestPrimitives()
        {
            string s = "id desc";
            Order o = s;

            Log.Info("Order: " + o);
            Log.Info("Order Contains Id: " + o.Contains("Id"));
            
            Filter f = "Id=1";
            Log.Info("Filter: " + f);
            f = Filter.And(f, "ClientId=2");
            Log.Info("Filter: " + f);

            f = Filter.Create(
                "Name=@0 AND BirthDate=@1 AND UniqId=@2 AND IsActive=@3 AND Int=@4 AND Float=@5 AND Char=@6", 
                "'s Gravesande", 
                DateTime.Now, 
                Guid.NewGuid(), 
                false, 
                32768, 
                3.141592,
                '\'');
            f = Filter.And(f, "BirthDate > " + Filter.ToSql(DateTime.Now));
            f = Filter.Or(f, "UniqId=" + Filter.ToSql(Guid.NewGuid()));
            Log.Info("Filter: " + f);

            Limit l = Limit.Range(300, 350);
            Log.Info("Limit: " + l);
        }

        [Test]
        public void TestInsertEntities()
        {
            Category cat = new Category();
            cat.CategoryName = "Food";
            Provider.Insert(cat);
            cat.CategoryName = "Drinks";
            Provider.Insert(cat);
            cat.CategoryName = "Sports";
            Provider.Insert(cat);

            Language lan = new Language();
            lan.Code = "EN";
            lan.Name = "English";
            Provider.Insert(lan);
            lan.Code = "NL";
            lan.Name = "Nederlands";
            Provider.Insert(lan);
            lan.Code = "DE";
            lan.Name = "Deutsch";
            Provider.Insert(lan);

            Country cnt = new Country();
            cnt.Code = "uk";
            cnt.Name = "United Kingdom";
            Provider.Insert(cnt);
            cnt.Code = "us";
            cnt.Name = "United States";
            Provider.Insert(cnt);
            cnt.Code = "nl";
            cnt.Name = "Netherlands";
            Provider.Insert(cnt);
            cnt.Code = "de";
            cnt.Name = "Germany";
            Provider.Insert(cnt);

            Contact c = new Contact();
            c.FirstName = "Bob";
            c.LastName = "Builder";
            c.Email = "bob@builder";
            c.Language = Language.Find("EN");
            c.Address.City = "London";
            c.Address.Street = "1 Hyde Park";
            c.Address.ZipCode = "11111";
            c.Address.Country = Country.Find("uk");
            Provider.Insert(c);

            c.FirstName = "Rita";
            c.LastName = "Metermaid";
            c.Email = "rita@metermaid";
            c.Language = Language.Find("EN");
            c.Address.City = "Miami";
            c.Address.Street = "Daytona Beach";
            c.Address.ZipCode = "29384";
            c.Address.Country = Country.Find("us");
            Provider.Insert(c);
        }

        [Test]
        public void TestEntities()
        {
            Contact c = Contact.Find(1);
            Log.Info("Contact: " + c.Id + "=" + c.DisplayName);
            c = (Contact)Provider.FindByFilter(typeof(Contact), (Filter)"ContactId=1");
            
            Log.Info("Contact Language: " + c.Language.Code + "[" + c.Language.Name + "]");
            Log.Info("Contact Country: " + c.Address.Country.Code + "[" + c.Address.Country.Name + "]");
            c.Language = "NL";
            c.Address.Country = "UK";
            Log.Info("Contact Country: " + c.Address.Country.Code + "[" + c.Address.Country.Name + "]");
            
            foreach (Category cat in c.CategoryList())
                Log.Info("  Cat: " + cat.CategoryName);
            c = Contact.Find(2);
            Log.Info("Contact: " + c.Id + "=" + c.DisplayName);
            foreach (Category cat in c.CategoryList())
                Log.Info("  Cat: " + cat.CategoryName);

            c.CategoryAdd(1);
            c.CategoryAdd(2);
            c.CategoryAdd(3);
            foreach (Category cat in c.CategoryList())
                Log.Info("  Cat: " + cat.CategoryName);
            foreach (Category cat in c.CategoryList(1,1))
                Log.Info("  Cat: " + cat.CategoryName);
            
            c.CategoryDelete(1);
            c.CategoryDelete(2);
            c.CategoryDelete(3);
            foreach (Category cat in c.CategoryList())
                Log.Info("  Cat: " + cat.CategoryName);

            using (IDataProvider session = Provider.Open(System.Data.IsolationLevel.ReadCommitted))
            {
                for (int i = 1; i <= 100; i++)
                {
                    Category cat = new Category();
                    cat.CategoryName = "Cat" + i;
                    session.Insert(cat);
                }
                for (int i = 1; i <= 100; i++)
                {
                    session.ExecuteNonQuery("delete from Category where CategoryName=@Name", "Name", "Cat" + i);
                }
            }

            c.LastName = "Wok888";
            c.Insert();
            Log.Info("Contact: " + c.Id + "=" + c.DisplayName);
            c.LastName = "Wok888-Updated";
            c.Update();
            Log.Info("Contact: " + c.Id + "=" + c.DisplayName);
            int id = c.Id;
            c.Delete();
            c = Contact.Find(id);
            Log.Info("Contact: '" + c + "' should be empty");
        }
    }
    
    [TestFixture]
    public class SqlDataMappingTest : DataMappingTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-sql");
            Provider = Context.Current.Provider;
        }
    }

    [TestFixture]
    public class MySqlDataMappingTest : DataMappingTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-mysql");
            Provider = Context.Current.Provider;
        }
    }

    [TestFixture]
    public class SQLiteDataMappingTest : DataMappingTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-sqlite");
            Provider = Context.Current.Provider;
        }
    }
}
