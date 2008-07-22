using System;
using System.Collections;
using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

namespace Glue.Data.Test
{
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
            Context.Current.Provider.Insert(this);
        }
        public void Update()
        {
            Context.Current.Provider.Update(this);
        }
        public void Delete()
        {
            Delete(Id);
        }
        public Category[] CategoryList()
        {
            return (Category[])Context.Current.Provider.ListManyToMany(this, typeof(Category), "ContactCategory");
        }
        public Category[] CategoryList(int index, int count)
        {
            return (Category[])Context.Current.Provider.ListManyToMany(this, typeof(Category), "ContactCategory", null, null, new Limit(index,count));
        }
        public void CategoryAdd(Category category)
        {
            Context.Current.Provider.AddManyToMany(this, category, "ContactCategory");
        }
        public void CategoryDelete(Category category)
        {
            Context.Current.Provider.DelManyToMany(this, category, "ContactCategory");
        }

        public static Contact Find(int Id)                                                
        { 
            return (Contact)Context.Current.Provider.Find(typeof(Contact), Id); 
        }
        public static Contact[] List(Filter filter, Order order, Limit limit)                   
        { 
            return (Contact[])Context.Current.Provider.List(typeof(Contact), filter, order, limit); 
        }
        public static void Delete(int Id)                                                
        { 
            Context.Current.Provider.Delete(typeof(Contact), Id); 
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

        public static Country Find(string code)
        { 
            return (Country)Context.Current.Provider.Find(typeof(Country), code); 
        }
        public static void Save(string code, string name)
        { 
            Context.Current.Provider.Save(new Country(code, name)); 
        }
        public static Country[] List()
        { 
            return (Country[])Context.Current.Provider.List(typeof(Country), null, null, null);
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

        public static Language Find(string code)
        { 
            return (Language)Context.Current.Provider.Find(typeof(Language), code); 
        }
        public static void Save(string code, string name)
        { 
            Context.Current.Provider.Save(new Language(code, name)); 
        }
        public static Language[] List()
        { 
            return (Language[])Context.Current.Provider.List(typeof(Language), null, null, null);
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

        static Category Find(int id)
        {
            return (Category)Context.Current.Provider.Find(typeof(Category), id);
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
            Context.Current.Provider.Insert(this);
        }
        public void Update()
        {
            Context.Current.Provider.Update(this);
        }
        public Order[] ListOrders(Filter filter, Order order, Limit limit)
        {
            return (Order[])Context.Current.Provider.List(typeof(Order), Filter.And("CustomerCode=" + Code, filter), order, limit);
        }
        public Contact[] ListContacts(Filter filter, Order order, Limit limit)
        {
            return (Contact[])Context.Current.Provider.List(typeof(Order), Filter.And("CustomerCode=" + Code, filter), order, limit);
        }

        public static Customer Find(string code)
        {
            return (Customer)Context.Current.Provider.Find(typeof(Customer), code);
        }
        public static Customer[] List(Filter filter, Order order, Limit limit)
        {
            return (Customer[])Context.Current.Provider.List(typeof(Customer), filter, order, limit);
        }
        public static void Delete(string code)
        {
            Context.Current.Provider.Delete(typeof(Customer), code);
        }
    }
}
