//
// Glue.Lib.Mail.MailAddress.cs
//
using System;
using System.Text;

namespace Glue.Lib.Mail {

    // Reperesents a mail address
    public class MailAddress 
    {
	
        protected string user;
        protected string host;
        protected string name;
	
        public string User 
        {
            get { return user; }
            set { user = value; }
        }

        public string Host 
        {
            get { return host; }
            set { host = value; }
        }

        public string Name 
        {
            get { return name; }
            set { name = value; }
        }

        public string Address 
        {
            get 
            { 
                return String.Format( "{0}@{1}" , user , host ); 
            }
            set 
            {
                string[] parts = value.Split( new char[] { '@' } );
		
                if (parts.Length != 2)
                    throw new FormatException( "Invalid e-mail address: '" + value + "'.");
	
                user = parts[ 0 ];
                host = parts[ 1 ];
            }
        }

        public static MailAddress Parse( string str ) 
        {
            if (str == null || str.Trim () == "")
                return null;

            MailAddress addr = new MailAddress();
            string address = null;
            string nameString = null;
            string[] parts = str.Split( new char[] { ' ' } );
	    
            // find the address: xxx@xx.xxx
            // and put to gether all the parts
            // before the address as nameString
            foreach( string part in parts ) 
            {
                if( part.IndexOf( '@' ) > 0 ) 
                {
                    address = part;
                    break;
                }

                nameString = nameString + part + " ";
            }

            if (address == null)
                throw new FormatException( "Invalid e-mail address: '" + str + "'.");
	    
            address = address.Trim('<', '>', '(', ')', '\r', '\n');
	    
            addr.Address = address;
	    
            if( nameString != null ) 
            {
                addr.Name = nameString.Trim(' ', '"');
                addr.Name = ( addr.Name.Length == 0 ? null : addr.Name ); 
            }
	    
            return addr;
        } 
    
    
        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool encode)
        {
            if (name == null) 
                return string.Format( "<{0}>" , this.Address);
            else 
                if (MailUtil.NeedEncoding(name))
                    return string.Format("=?utf-8?Q?\"{0}\"?= <{1}>", MailUtil.QPEncodeToString(Encoding.UTF8.GetBytes(this.Name)), this.Address);                else                    return string.Format( "\"{0}\" <{1}>" , this.Name, this.Address);
        }
    }

}
