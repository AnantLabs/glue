using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NUnit.Framework;
using Glue.Lib;
using Glue.Data;

namespace Glue.Data.Test
{
    [TestFixture]
    public class DataProviderTest
    {
        public static IDataProvider Provider2 = new Glue.Data.Providers.Sql.SqlDataProvider2(
            "localhost",
            "glue_data_test",
            "glue",
            "glue"
            );

        public static IDataProvider Provider3 = new Glue.Data.Providers.MySql.MySqlDataProvider2(
            "calypso",
            "glue_data_test",
            "glue",
            "glue"
            );

        public static IDataProvider Provider = new Glue.Data.Providers.SQLite.SQLiteDataProvider2(
            null,
            "C:/Users/Gertjan/AppData/Local/Temp/glue_data_test.db3",
            null,
            null
            );

        public static void Test()
        {
            IDbCommand command;

            Provider.ExecuteNonQuery(@"
                CREATE TABLE [Customer] (
                  [CustomerCode] VARCHAR(8) NOT NULL,
                  [DisplayName] NVARCHAR(100) NOT NULL,
                  CONSTRAINT [PK_Customer] PRIMARY KEY ([CustomerCode])
                );"
                );
            Provider.ExecuteNonQuery("DELETE FROM Customer");

            using (IDataProvider session = Provider.Open())
            {
                command = session.CreateInsertCommand("Customer", "CustomerCode", "C1", "DisplayName", "Customer-1");
                Log.Debug(command.CommandText);
                session.ExecuteNonQuery(command);
                session.SetParameters(command, "CustomerCode", "C2", "DisplayName", "Customer-2");
                session.ExecuteNonQuery(command);
                session.SetParameters(command, "CustomerCode", "C3", "DisplayName", "Customer-3");
                session.ExecuteNonQuery(command);

                command = session.CreateUpdateCommand("Customer", "CustomerCode=@OldCode", "CustomerCode", "C1A", "DisplayName", "Customer-1A");
                session.SetParameter(command, "OldCode", "C1");
                Log.Debug(command.CommandText);
                session.ExecuteNonQuery(command);
            }

            command = Provider.CreateUpdateCommand("Customer", "CustomerCode=@CustomerCode", "CustomerCode", "C1", "DisplayName", "Customer-1 Again");
            Log.Debug(command.CommandText);
            Provider.ExecuteNonQuery(command);
            
            /*
            command = Provider.CreateReplaceCommand("Customer", "CustomerCode=@CustomerCode", "CustomerCode", "C1", "DisplayName", "Customer-1 Again Again");
            Log.Debug(command.CommandText);
            Provider.ExecuteNonQuery(command);
            */
            command = Provider.CreateSelectCommand("[Customer] C", "C.CustomerCode,C.DisplayName", null, "-C.DisplayName,C.CustomerCode", Limit.Create(1, 1));
            Log.Debug(command.CommandText);
            
            PrettyPrint.Print(Console.Out, Provider.ExecuteReader(command));
            
            PrettyPrint.Print(Console.Out, Provider.ExecuteReader("select * from Customer"));
        }
    }
}
