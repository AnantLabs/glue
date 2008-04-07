using System;
using System.Collections.Generic;
using System.Text;

using Glue.Data;
using Glue.Data.Mapping;

namespace mp3sql
{
    [Table]
    class Track : ActiveRecord
    {
        [Key] 
        public int Id;
        public string Path;
        public string Title;
        public string Artist;
        public Nullable<int> Year;
        public string Comment;
    }
}
