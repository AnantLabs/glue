using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data.Test
{
    public class DAL
    {
        IMappingProvider Provider;

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
}
