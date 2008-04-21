using System;
using System.Collections.Generic;
using System.Text;

using Glue.Data;
using Glue.Data.Mapping;

namespace mp3sql
{
    [Table]
    public class Album
    {
        [AutoKey]
        public int Id;
        public string Name = "";

        public Album()
        {
        }

        public Album(string name)
        {
            Name = name;
        }
    }
}
