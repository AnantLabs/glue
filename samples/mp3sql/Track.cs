using System;
using System.Collections.Generic;
using System.Text;

using Glue.Lib;
using Glue.Data;
using Glue.Data.Mapping;

using IdSharp.Tagging.ID3v1;

namespace mp3sql
{
    //public class QualityInfo
    //{
    //    public long? Bitrate;
    //    public string Encoding;
    //    public long? Level;
        
    //    public override string ToString()
    //    {
    //        return string.Format("B:{0} L:{1} E:{2}", Bitrate, Level, Encoding);
    //    }
    //}

    [Table]
    public class Track : ActiveRecord
    {
        [AutoKey] 
        public int Id;
        public string Path;
        public string Title;
        public string Artist;
        public long? Year;
        public string Comment;
        //public QualityInfo Quality = new QualityInfo();

        public Track()
        {
        }

        public Track(string path)
        {
            Path = path;
        }

        public void Assign(ID3v1 tags)
        {
            Title = tags.Title;
            Artist = tags.Artist;
            Year = NullConvert.ToInt64("0" + tags.Year, 0);
            Comment = tags.Comment;
        }
    }
}
