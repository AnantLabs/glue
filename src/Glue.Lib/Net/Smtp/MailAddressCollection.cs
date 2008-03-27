//
// Glue.Lib.Mail.MailAddressCollection.cs
//
using System;
using System.Text;
using System.Collections;

namespace Glue.Lib.Mail {

    // represents a collection of MailAddress objects
    public class MailAddressCollection : IEnumerable 
    {
	
        protected ArrayList data = new ArrayList();
	
        public MailAddress this[int index] 
        {
            get { return this.Get(index); }
        }

        public int Count 
        { 
            get { return data.Count; } 
        }
	
        public void Add(MailAddress addr) 
        { 
            data.Add(addr); 
        }

        public void Remove(MailAddress addr) 
        { 
            data.Remove(addr); 
        }
        
        public void Clear()
        {
            data.Clear();
        }
        
        public MailAddress Get(int index) 
        { 
            return (MailAddress)data[index]; 
        }

        public IEnumerator GetEnumerator() 
        {
            return data.GetEnumerator();
        }
    
        public override string ToString() 
        {
            return ToString(false);
        }

        public string ToString(bool encode) 
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i <data.Count ; i++) 
            {
                MailAddress addr = this.Get(i);
		
                builder.Append(addr.ToString(encode));
		
                if (i != data.Count - 1)
                    builder.Append(",\r\n  ");
            }

            return builder.ToString(); 
        }

        public static MailAddressCollection Parse( string str ) 
        {
            if (str == null)
                throw new ArgumentNullException("Null is not allowed as an address string");
	    
            MailAddressCollection list = new MailAddressCollection();
	    
            string[] parts = str.Split(',', ';');
	    
            foreach (string part in parts) 
            {
                MailAddress add = MailAddress.Parse(part);
                if (add == null)
                    continue;

                list.Add (add);
            }
	
            return list;
        }
	
    }

}
