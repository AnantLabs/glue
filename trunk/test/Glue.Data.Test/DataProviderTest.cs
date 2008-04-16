using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Test
{
    public class DataProviderTest
    {
        public IDataProvider Provider;

        [Test]
        public void TestSimpleCommand()
        {
            IDbCommand command = Provider.CreateCommand("DELETE FROM Customer");
            Provider.ExecuteNonQuery(command);
        }

        [Test]
        public void TestInsertCommand()
        {
            IDbCommand command = Provider.CreateInsertCommand("Customer", "CustomerCode", "C1", "DisplayName", "Customer-1");
            Log.Debug(command.CommandText);
        }

        [Test]
        public void TestInsertSession()
        {
            using (IDataProvider session = Provider.Open())
            {
                IDbCommand command = session.CreateInsertCommand("Customer", "CustomerCode", "C1", "DisplayName", "Customer-1");
                Log.Debug(command.CommandText);
                session.ExecuteNonQuery(command);
                session.SetParameters(command, "CustomerCode", "C2", "DisplayName", "Customer-2");
                session.ExecuteNonQuery(command);
                session.SetParameters(command, "CustomerCode", "C3", "DisplayName", "Customer-3");
                session.ExecuteNonQuery(command);
            }
        }

        [Test]
        public void TestUpdateCommand()
        {
            IDbCommand command = Provider.CreateUpdateCommand("Customer", "CustomerCode=@OldCode", "CustomerCode", "C1A", "DisplayName", "Customer-1A");
            Provider.SetParameter(command, "OldCode", "C1");
            Log.Debug(command.CommandText);
            Provider.ExecuteNonQuery(command);
            command = Provider.CreateUpdateCommand("Customer", "CustomerCode=@CustomerCode", "CustomerCode", "C1A", "DisplayName", "Customer-1 Again");
            Log.Debug(command.CommandText);
            Provider.ExecuteNonQuery(command);
        }

        [Test]
        public void TestReplaceCommand()
        {
            /*
            command = Provider.CreateReplaceCommand("Customer", "CustomerCode=@CustomerCode", "CustomerCode", "C1A", "DisplayName", "Customer-1 Again Again");
            Log.Debug(command.CommandText);
            Provider.ExecuteNonQuery(command);
            */
        }

        [Test]
        public void TestSelectCommand()
        {
            IDbCommand command = Provider.CreateSelectCommand("Customer", "CustomerCode,DisplayName", null, "-DisplayName,CustomerCode", Limit.Create(1, 1));
            Log.Debug(command.CommandText);
            ObjectDumper.Write(Provider.ExecuteReader(command), 3);
            ObjectDumper.Write(Provider.ExecuteReader("select * from Customer"));
        }
    }

    [TestFixture]
    public class SqlDataProviderTest : DataProviderTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-sql");
            Provider = Context.Current.Provider;
        }
    }

    [TestFixture]
    public class MySqlDataProviderTest : DataProviderTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-mysql");
            Provider = Context.Current.Provider;
        }
    }

    [TestFixture]
    public class SQLiteDataProviderTest : DataProviderTest
    {
        [SetUp]
        public void Setup()
        {
            Context.Current = (Context)Configuration.Get("context-sqlite");
            Provider = Context.Current.Provider;
        }
    }
}
