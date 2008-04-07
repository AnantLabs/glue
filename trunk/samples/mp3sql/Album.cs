using System;
using System.Collections.Generic;
using System.Text;

using Glue.Data;
using Glue.Data.Mapping;

namespace mp3sql
{
    [Table]
    class Album : ActiveRecord
    {
        [Key]
        public int Id;
        public string Name;
    }
}
