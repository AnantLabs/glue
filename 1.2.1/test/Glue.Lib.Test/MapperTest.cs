using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using NUnit.Framework;
using Glue.Lib;

namespace Glue.Lib.Test
{
    public enum TestEnum
    {
        Cow,
        Cat,
        Dog,
        Pig
    }

    public struct Money
    {
        public readonly double Amount;
        public readonly string Currency;
        public override string ToString()
        {
            return "" + Currency + Amount;
        }
        public Money(double amount)
        {
            Currency = "EUR";
            Amount = amount;
        }
        public Money(string currency, double amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }

    public class MapperTestObject
    {
        public string Name;
        public DateTime BirthDate;
        public int Points;
        public Money Salary;
        public Guid Uniq;
        public TestEnum Kind;

        public MapperTestObject()
        {
        }
        public void List(int number, string name)
        {
            Log.Debug("List(variant#1)(number=" + number + ",name=" + name + ")");
        }
        public void List(int number, string name, Money salary)
        {
            Log.Debug("List(variant#2)(number=" + number + ",name=" + name + ",salary=" + salary + ")");
        }
        public override string ToString()
        {
            return "(name=" + Name + ";birthdate=" + BirthDate + ";points=" + Points + ")";
        }
    }

    [TestFixture]
    public class MapperTest
    {
        [Test]
        public static void Test()
        {
            TestSimple();
            TestProperties();
            TestMethods();
        }

        [Test]
        public static void TestSimple()
        {
            TestDates("nl-NL");
            TestDates("en-US");
            TestDates("");

            NameValueCollection list = new NameValueCollection();
            list["name"] = "Wok";
            list.Add("test", "x");
            list.Add("test", "y");
            list.Add("test", "z");
            list["number"] = "1234";
            list["points"] = "5";
            list["uniq"] = Guid.NewGuid().ToString();
            list.Add("struct.year", "2005");
            list.Add("struct.month", "40");
            list.Add("birthdate", "1980/12/20");
            list.Add("salary.amount", "40");
            list.Add("salary.currency", "$");
            list["kind"] = "Cat";
            
            IDictionary bag = CollectionHelper.ToBag(list);
            Log.Debug(CollectionHelper.ToString(bag));

            object obj = null;
            try 
            {
                obj = Mapper.Create(typeof(MapperTestObject), bag);
            }
            catch (CombinedException e)
            {
                Log.Error(e.ToString());
            }
            Log.Debug("obj = " + obj);
            
            Mapper.Invoke(obj, "List", bag);

            bag.Remove("salary");
            Mapper.Invoke(obj, "List", bag);
            
            bag["salary"] = CollectionHelper.Parse("{amount:40,currency:$}");
            obj = new MapperTestObject();
            Mapper.Assign(obj, bag);

            bag["salary"] = CollectionHelper.Parse("{amount:123}");
            obj = new MapperTestObject();
            Mapper.Assign(obj, bag);

            bag["birthdate"] = "today";
            bag["salary"] = "15404";
            //bag["uniq"] = "44";
            bag["uniq"] = new Money(450);
            try 
            {
                Mapper.Assign(obj, bag);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            bag["salary"] = 123;
            obj = new MapperTestObject();
            Mapper.Assign(obj, bag);
        }

        [Test]
        public static void TestDates(string culture)
        {
            CultureInfo ci = new CultureInfo(culture, false);
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            Log.Info("Culture: " + ci.Name + " (" + ci.DisplayName + ")");
            TestDateParsing(ci, "  7-16-2004 ");
            TestDateParsing(ci, "  7/16/2004 ");
            TestDateParsing(ci, "  16-7-2004 ");
            TestDateParsing(ci, "  16/7/2004 ");
            TestDateParsing(ci, "  2004-7-16");
            TestDateParsing(ci, "  2004/7/16 ");
            TestDateParsing(ci, "  7-16-4    ");
            TestDateParsing(ci, "  7/16/4    ");
            TestDateParsing(ci, "  16-7-4    ");
            TestDateParsing(ci, "  16/7/4    ");
        }

        private static void TestDateParsing(IFormatProvider fi, string s)
        {
            try
            {
                DateTimeFormatInfo dtfi = (DateTimeFormatInfo)fi.GetFormat(typeof(DateTimeFormatInfo));
                string[] patterns = dtfi.GetAllDateTimePatterns();
                DateTime dt = DateTime.ParseExact(s, patterns, dtfi, DateTimeStyles.AllowWhiteSpaces);
                //dt = DateTime.Parse(s, fi, DateTimeStyles.AllowWhiteSpaces);
                Log.Info("  OK: '" + s + "' => " + dt.ToString("dd-MM-yyyy HH:mm"));
            }
            catch (Exception e)
            {
                Log.Info("  ERROR: '" + s + "' : " + e);
            }
        }

        [Test]
        public static void TestProperties()
        {
        }

        [Test]
        public static void TestMethods()
        {
        }

    }
}
