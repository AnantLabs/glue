using System;
using NUnit.Framework;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Test
{
    [Table]
    public class Fonds
    {
        [AutoKey]
        public int Id;
        public string Naam;
        public int Status;
        public DateTime ModifiedOn;
        public int AanbiederId;
        public int FondsType;
    }
    
    /// <summary>
	/// Summary description for App.
	/// </summary>
	public class App
	{
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Glue.Lib.Log.Level = Glue.Lib.Level.Info;
            ObjectDumper.Write(Console.Out, new Contact(), 1);
            TestOracle();
            // SqlDataMappingTest test = new SqlDataMappingTest();
            // test.Setup();
            // test.TestEntities();

            // DataProvider
            Tester.Run<SqlDataProviderTest>();
            Tester.Run<MySqlDataProviderTest>();
            Tester.Run<SQLiteDataProviderTest>();

            // DataMapping
            Tester.Run<SqlDataMappingTest>();
            Tester.Run<MySqlDataMappingTest>();
            Tester.Run<SQLiteDataMappingTest>();
        }
        
        static void TestOracle()
        {
            Glue.Data.Providers.Oracle.OracleDataProvider prov = new Glue.Data.Providers.Oracle.OracleDataProvider(
                "ariel", "test01", "test", "cerberos");

            System.Data.OracleClient.OracleConnection con = new System.Data.OracleClient.OracleConnection(
                "Data Source = (DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=ariel)(PORT=1521))(CONNECT_DATA=(SID=test01))); User Id=test; Password=cerberos;"
                );
            con.Open();
            System.Data.OracleClient.OracleCommand cmd = new System.Data.OracleClient.OracleCommand("select * from aanbieder", con);
            using (System.Data.OracleClient.OracleDataReader rdr = cmd.ExecuteReader())
                while (rdr.Read())
                    Console.WriteLine("Id:{0} Name:{1}", rdr[0], rdr[1]);
            //cmd.CommandText = "insert into aanbieder(id,status,modifiedon,naam) values (8,0,sysdate,'Test8')";
            //cmd.ExecuteNonQuery();
            //cmd.CommandText = "select * from aanbieder";
            //using (System.Data.OracleClient.OracleDataReader rdr = cmd.ExecuteReader())
            //    while (rdr.Read())
            //        Console.WriteLine("Id:{0} Name:{1}", rdr[0], rdr[1]);
            cmd.CommandText = "insert into fonds(id,status,modifiedon,AanbiederId,Naam,FondsType) values(FONDS_SEQ.NEXTVAL,0,sysdate,1,'TestFonds2',1) returning id into :p1";
            System.Data.OracleClient.OracleParameter p1 = new System.Data.OracleClient.OracleParameter("p1", System.Data.OracleClient.OracleType.Int32);
            p1.Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.Add(p1);
            cmd.ExecuteNonQuery();
            Console.WriteLine(p1.Value);
            con.Close();


            Fonds fonds = new Fonds();
            fonds.Naam = "Testfonds99";
            fonds.AanbiederId = 1;
            fonds.FondsType = 1;
            fonds.ModifiedOn = DateTime.Now;
            fonds.Status = 0;
            prov.Insert(fonds);
            Console.WriteLine(fonds.Id);
            fonds.Naam = "TOTOTO";
            prov.Update(fonds);
            prov.Delete(fonds);
            
            foreach (Fonds f in prov.List<Fonds>(Filter.Empty, "-MODIFIEDON", Limit.Unlimited))
                Console.WriteLine("Id:{0} Name:{1}", f.Id, f.Naam);
            foreach (Fonds f in prov.List<Fonds>(Filter.Empty, "-MODIFIEDON", Limit.One))
                Console.WriteLine("Id:{0} Name:{1}", f.Id, f.Naam);
            foreach (Fonds f in prov.List<Fonds>(Filter.Empty, "-MODIFIEDON", Limit.Range(0,5)))
                Console.WriteLine("Id:{0} Name:{1}", f.Id, f.Naam);
        }
    }
}
