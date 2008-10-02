using System;
using System.Collections;
using System.Collections.Specialized;

namespace Glue.Web
{
	/// <summary>
	/// PostedFileCollection holds a number of PostedFile objects.
	/// See IRequest.Files
	/// </summary>
	public class PostedFileCollection : NameObjectCollectionBase
	{
        internal PostedFileCollection()
		{
        }
        
        internal void Add(PostedFile file)
        {
            BaseAdd(file.Name, file);
        }

        public PostedFile this[int i]
        {
            get { return (PostedFile)BaseGet(i); }
        }
        
        public PostedFile this[string name]
        {
            get { return (PostedFile)BaseGet(name); }
        }
    }
}
