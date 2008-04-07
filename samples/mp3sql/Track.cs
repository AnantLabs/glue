using System;
using System.Collections.Generic;
using System.Text;

using Glue.Data;
using Glue.Data.Mapping;

namespace mp3sql
{
    public class QualityInfo
    {
        public long? Bitrate;
        public string Encoding;
        public long? Level;
        
        public override string ToString()
        {
            return string.Format("B:{0} L:{1} E:{2}", Bitrate, Level, Encoding);
        }
    }

    [Table]
    public class Track : ActiveRecord
    {
        [Key] 
        public int Id;
        public string Path;
        public string Title;
        public string Artist;
        public long? Year;
        public string Comment;
        public QualityInfo Quality = new QualityInfo();
    }
}
