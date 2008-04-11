using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;
using Glue.Data.Test.Model;

namespace Glue.Data.Test
{
    public class DAL
    {
        public IMappingProvider Provider;
        
        public DAL(IMappingProvider provider)
        {
            Provider = provider;
        }

        public Contact[] ContactList(Filter filter, Order order, Limit limit)
        {
            return (Contact[])Provider.List(typeof(Contact), filter, order, limit);
        }

        public Contact ContactFind(int id)
        {
            return (Contact)Provider.Find(typeof(Contact), id);
        }

        public Contact ContactFind(Filter filter, Order order)
        {
            return (Contact)Provider.FindByFilter(typeof(Contact), filter, order);
        }

        public void ContactSave(Contact contact)
        {
            Provider.Save(contact);
        }

        public void ContactDelete(int id)
        {
            Provider.Delete(typeof(Contact), id);
        }

        public void ContactAddCategory(Contact contact, Category category)
        {
            Provider.AddManyToMany(contact, category, "ContactCategory");
        }

        public void ContactRemoveCategory(Contact contact, Category category)
        {
            Provider.DelManyToMany(contact, category, "ContactCategory");
        }
    }

    public class Session : IDisposable
    {
        IMappingProvider provider;
        System.Data.IDbTransaction transaction;

        public static Session Open(IMappingProvider provider)
        {
            return new Session(provider);
        }

        private Session(IMappingProvider provider)
        {
            this.provider = provider;
            System.Data.IDbConnection connection = provider.CreateConnection();
            //connection.Open();
            //this.transaction = connection.BeginTransaction();
        }

        public void Dispose()
        {
            //this.transaction.Commit();
            //this.transaction.Connection.Close();
            Log.Info("Closing session.");
        }
    }

    [TestFixture]
    public class DataMappingTest
    {
        public static IMappingProvider Provider1 = new Glue.Data.Providers.Sql.SqlMappingProvider(
            "calypso", 
            "glue_data_test", 
            "glue", 
            "glue",
            MappingOptions.PrefixedColumns
            );
        
        public static IMappingProvider Provider2 = new Glue.Data.Providers.MySql.MySqlMappingProvider(
            "calypso",
            "glue_data_test",
            "glue",
            "glue",
            MappingOptions.PrefixedColumns
            );

        public static IMappingProvider Provider = new Glue.Data.Providers.SQLite.SQLiteMappingProvider(
            null,
            "d:/temp/glue_data_test.db3",
            null,
            null,
            MappingOptions.PrefixedColumns
            );

        //public static IMappingProvider Provider = Configuration.Get("dataprovider");

        [Test]
        public static void Test()
        {
            TestPrimitives();
            TestMapping();
            TestEntities();
        }

        [Test]
        public static void TestMapping()
        {
            //(Provider as Glue.Data.Providers.Sql.SqlMappingProvider).GenerateAccessor(typeof(Contact));
        }

        [Test]
        public static void TestPrimitives()
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

        static void InitData()
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
        public static void TestEntities()
        {
            // InitData();
            Contact c = new Contact();
            
            c = Contact.Find(1);
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

            using (Session.Open(Provider))
            {
                for (int i = 1; i <= 100; i++)
                {
                    Category cat = new Category();
                    cat.CategoryName = "Cat" + i;
                    Provider.Insert(cat);
                }
                for (int i = 1; i <= 100; i++)
                {
                    Provider.ExecuteNonQuery("delete from Category where CategoryName=" + Provider.ToSql("Cat" + i));
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
}
