using System;
using System.IO;

namespace Glue.Web
{
	/// <summary>
	/// Helper class for uploaded files.
	/// </summary>
    public class PostedFile
	{
        string _name;
        string _filename;
        string _ct;
        byte[] _data;
        int _offset;
        int _length;
        Stream _stream;

        protected PostedFile(string name, string filename, string contentType, byte[] data, int offset, int length)
		{
            _name = name;
            _filename = filename;
            _ct = contentType;
            _data = data;
            _offset = offset;
            _length = length;
        }
        
        public virtual void SaveAs(string path)
        {
            using (Stream output = new FileStream(path, FileMode.Create))
            {
                if (_data != null && _length > 0)
                    output.Write(_data, _offset, _length);
                output.Flush();
            }
        }
        
        public string SaveUnique(string path)
        {
            path = Glue.Lib.Applet.GetUniqueSequentialFileName(path);
            SaveAs(path);
            return path;
        }
        
        public string SaveUniqueInDir(string directory)
        {
            return SaveUnique(Path.Combine(directory, Path.GetFileName(Name)));
        }
        
        public string Name
        {
            get { return _name; }
        }

        public string FileName
        {
            get { return _filename; }
        }

        public virtual Stream Content
        {
            get 
            { 
                if (_stream == null)
                    _stream = new MemoryStream(_data, _offset, _length, false);
                return _stream; 
            }
        }

        public int ContentLength
        {
            get { return _length; }
        }
        
        public string ContentType
        {
            get { return _ct; }
        }
    }
}
