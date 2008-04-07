/*
 *   IdSharp - A tagging library for .NET
 *   Copyright (C) 2007  Jud White
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License along
 *   with this program; if not, write to the Free Software Foundation, Inc.,
 *   51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Text;
//using IdSharp.Common;

namespace IdSharp.Tagging.ID3v1
{
    public enum ID3v1TagVersion : int
    {
        /// <summary>
        /// ID3v1.0.
        /// </summary>
        ID3v10,
        /// <summary>
        /// ID3v1.1.
        /// </summary>
        ID3v11
    }

    /// <summary>
    /// ID3v1
    /// </summary>
    public class ID3v1// : IID3v1
    {
        #region <<< Private Fields >>>

        private String m_Title;
        private String m_Artist;
        private String m_Album;
        private String m_Year;
        private String m_Comment;
        private Int32 m_TrackNumber;
        private Int32 m_GenreIndex;
        private ID3v1TagVersion m_TagVersion;

        #endregion <<< Private Fields >>>

        #region <<< Constructor >>>

        /// <summary>
        /// Initializes a new instance of the <see cref="ID3v1"/> class.
        /// </summary>
        public ID3v1()
        {
            m_TagVersion = ID3v1TagVersion.ID3v11;
            m_GenreIndex = 12; // Other
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID3v1"/> class.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public ID3v1(String path)
        {
            m_TagVersion = ID3v1TagVersion.ID3v11;
            m_GenreIndex = 12; // Other
            Read(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ID3v1"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public ID3v1(Stream stream)
        {
            m_TagVersion = ID3v1TagVersion.ID3v11;
            m_GenreIndex = 12; // Other
            ReadStream(stream);
        }

        #endregion <<< Constructor >>>

        #region <<< INotifyPropertyChanged Members >>>

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion <<< INotifyPropertyChanged Members >>>

        #region <<< IID3v1 Members >>>

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public String Title
        {
            get
            {
                return m_Title;
            }
            set
            {
                m_Title = ID3v1.GetString(value, 30);
                FirePropertyChanged("Title");
            }
        }

        /// <summary>
        /// Gets or sets the artist.
        /// </summary>
        /// <value>The artist.</value>
        public String Artist
        {
            get
            {
                return m_Artist;
            }
            set
            {
                m_Artist = ID3v1.GetString(value, 30);
                FirePropertyChanged("Artist");
            }
        }

        /// <summary>
        /// Gets or sets the album.
        /// </summary>
        /// <value>The album.</value>
        public String Album
        {
            get
            {
                return m_Album;
            }
            set
            {
                m_Album = ID3v1.GetString(value, 30);
                FirePropertyChanged("Album");
            }
        }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>The year.</value>
        public String Year
        {
            get
            {
                return m_Year;
            }
            set
            {
                m_Year = ID3v1.GetString(value, 4);
                FirePropertyChanged("Year");
            }
        }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public String Comment
        {
            get
            {
                return m_Comment;
            }
            set
            {
                if (m_TagVersion == ID3v1TagVersion.ID3v11)
                    m_Comment = ID3v1.GetString(value, 28);
                else
                    m_Comment = ID3v1.GetString(value, 30);

                FirePropertyChanged("Comment");
            }
        }

        /// <summary>
        /// Gets or sets the track number.
        /// </summary>
        /// <value>The track number.</value>
        public Int32 TrackNumber
        {
            get
            {
                return m_TrackNumber;
            }
            set
            {
                if (value >= 0 && value <= 255)
                {
                    m_TrackNumber = value;
                    if (m_TagVersion == ID3v1TagVersion.ID3v10)
                    {
                        this.TagVersion = ID3v1TagVersion.ID3v11;
                    }
                }
                FirePropertyChanged("TrackNumber");
            }
        }

        /// <summary>
        /// Gets or sets the index of the genre.
        /// </summary>
        /// <value>The index of the genre.</value>
        public Int32 GenreIndex
        {
            get
            {
                return m_GenreIndex;
            }
            set
            {
                if (value >= 0 && value <= 147)
                    m_GenreIndex = value;
                FirePropertyChanged("GenreIndex");
            }
        }

        /// <summary>
        /// Gets or sets the ID3v1 tag version.
        /// </summary>
        /// <value>The ID3v1 tag version.</value>
        public ID3v1TagVersion TagVersion
        {
            get
            { 
                return m_TagVersion; 
            }
            set 
            {
                m_TagVersion = value;
                FirePropertyChanged("TagVersion");
                if (value == ID3v1TagVersion.ID3v11)
                {
                    this.Comment = m_Comment;
                }
            }
        }


        /// <summary>
        /// Reads the ID3v1 tag from the specified path.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public void Read(String path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                this.ReadStream(fileStream);
            }
        }

        /// <summary>
        /// Reads the ID3v1 tag from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void ReadStream(Stream stream)
        {
            if (stream.Length >= 128)
            {
                stream.Seek(-128, SeekOrigin.End);

                if (ID3v1.GetString(stream, 3) == "TAG")
                {
                    this.Title = ID3v1.GetString(stream, 30);
                    this.Artist = ID3v1.GetString(stream, 30);
                    this.Album = ID3v1.GetString(stream, 30);
                    this.Year = ID3v1.GetString(stream, 4);

                    // Comment
                    Byte[] buf = new Byte[30];
                    stream.Read(buf, 0, 30);
                    String comment = ID3v1.GetString(buf);

                    // ID3v1.1
                    if (buf[28] == 0 && buf[29] != 0)
                    {
                        this.Comment = comment.Substring(0, 28).TrimEnd('\0').TrimEnd(' ');
                        this.TrackNumber = buf[29];
                        this.TagVersion = ID3v1TagVersion.ID3v11;
                    }
                    else
                    {
                        this.Comment = comment;
                        this.TrackNumber = 0;
                        this.TagVersion = ID3v1TagVersion.ID3v10;
                    }

                    Int32 genreIndex = stream.ReadByte();
                    if (genreIndex < 0 || genreIndex > 147) genreIndex = 12;

                    this.GenreIndex = genreIndex;
                }
                else
                {
                    this.Reset();
                }
            }
            else
            {
                this.Reset();
            }
        }

        /// <summary>
        /// Resets the properties of the ID3v1 tag to their default values.
        /// </summary>
        public void Reset()
        {
            this.Title = null;
            this.Artist = null;
            this.Album = null;
            this.Year = null;
            this.Comment = null;
            this.TrackNumber = 0;
            this.GenreIndex = 12; /* Other */
            this.TagVersion = ID3v1TagVersion.ID3v11;
        }

        /// <summary>
        /// Saves the ID3v1 tag to the specified path.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public void Save(String path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                fileStream.Seek(0 - ID3v1.GetTagSize(fileStream), SeekOrigin.End);

                Byte[] tag = Encoding.ASCII.GetBytes("TAG");
                Byte[] titleBytes = SafeGetBytes(m_Title);
                Byte[] artistBytes = SafeGetBytes(m_Artist);
                Byte[] albumBytes = SafeGetBytes(m_Album);
                Byte[] yearBytes = SafeGetBytes(m_Year);
                Byte[] commentBytes;

                fileStream.Write(tag, 0, 3);
                WriteBytesPadded(fileStream, titleBytes, 30);
                WriteBytesPadded(fileStream, artistBytes, 30);
                WriteBytesPadded(fileStream, albumBytes, 30);
                WriteBytesPadded(fileStream, yearBytes, 4);

                if (m_TagVersion == ID3v1TagVersion.ID3v11)
                {
                    commentBytes = SafeGetBytes(m_Comment);
                    WriteBytesPadded(fileStream, commentBytes, 28);
                    fileStream.WriteByte(0);
                    fileStream.WriteByte((Byte)m_TrackNumber);
                }
                else
                {
                    commentBytes = SafeGetBytes(m_Comment);
                    WriteBytesPadded(fileStream, commentBytes, 30);
                }

                fileStream.WriteByte((Byte)m_GenreIndex);
            }
        }

        #endregion <<< IID3v1 Members >>>

        #region <<< Private Methods >>>

        private void FirePropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler tmpPropertyChanged = PropertyChanged;
            if (tmpPropertyChanged != null)
            {
                tmpPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion <<< Private Methods >>>

        #region <<< Private Static Methods >>>

        /// <summary>
        /// Writes a specified number of bytes to a stream, padding any missing bytes with 0x00.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="byteArray">The byte array.</param>
        /// <param name="length">The number of bytes to be written.</param>
        private static void WriteBytesPadded(Stream stream, Byte[] byteArray, Int32 length)
        {
            Int32 i;
            for (i = 0; i < length && i < byteArray.Length && byteArray[i] != 0; i++)
            {
                stream.WriteByte(byteArray[i]);
            }
            for (; i < length; i++)
            {
                stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Gets a string from a specified stream using ISO-8859-1 encoding.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the string in bytes.</param>
        private static String GetString(Stream stream, Int32 length)
        {
            Byte[] byteArray = new Byte[length];
            stream.Read(byteArray, 0, length);
            return ID3v1.GetString(byteArray);
        }

        /// <summary>
        /// Gets a string from a specified byte array using ISO-8859-1 encoding.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        private static String GetString(Byte[] byteArray)
        {
            //String returnValue = Utils.ISO88591.GetString(byteArray).TrimEnd('\0').TrimEnd(' ');
            String returnValue = Encoding.GetEncoding(28591).GetString(byteArray);
            return returnValue.TrimEnd('\0').TrimEnd(' ');
        }

        /// <summary>
        /// Gets a trimmed string with a maximum length from a specified string.
        /// </summary>
        /// <param name="value">The original string value.</param>
        /// <param name="maxLength">Maximum length of the string.</param>
        private static String GetString(String value, Int32 maxLength)
        {
            if (value == null)
                return null;

            value = value.Trim();

            if (value.Length > maxLength)
                return value.Substring(0, maxLength).Trim();
            else
                return value;
        }

        private static Byte[] SafeGetBytes(String value)
        {
            if (value == null)
                return new Byte[0];
            else
                //return Utils.ISO88591.GetBytes(value);
                return Encoding.GetEncoding(28591).GetBytes(value);
        }

        private static void ArgumentNotNull(object o, string parameterName)
        {
            if (o == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        #endregion <<< Private Static Methods >>>

        #region <<< Public static methods >>>

        /// <summary>
        /// Gets the ID3v1 tag size from a specified stream.  Returns 128 if an ID3v1 tag exists; otherwise, 0.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static Int32 GetTagSize(Stream stream)
        {
            //Guard.ArgumentNotNull(stream, "stream");
            ArgumentNotNull(stream, "stream");

            Int64 currentPosition = stream.Position;
            try
            {
                if (stream.Length >= 128)
                {
                    stream.Seek(-128, SeekOrigin.End);

                    Byte[] buf = new Byte[3];
                    stream.Read(buf, 0, 3);

                    // Check for 'TAG'
                    if (buf[0] == 0x54 && buf[1] == 0x41 && buf[2] == 0x47)
                    {
                        return 128;
                    }
                }
                return 0;
            }
            finally
            {
                stream.Position = currentPosition;
            }
        }

        /// <summary>
        /// Gets the ID3v1 tag size from a specified path.  Returns 128 if an ID3v1 tag exists; otherwise, 0.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public static Int32 GetTagSize(String path)
        {
            //Guard.ArgumentNotNull(stream, "path");
            ArgumentNotNull(path, "path");

            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return GetTagSize(fileStream);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if an ID3v1 tag exists in the specified stream; otherwise, <c>false</c>.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static Boolean DoesTagExist(Stream stream)
        {
            //Guard.ArgumentNotNull(stream, "stream");
            ArgumentNotNull(stream, "stream");

            return (GetTagSize(stream) != 0);
        }

        /// <summary>
        /// Returns <c>true</c> if an ID3v1 tag exists in the specified path; otherwise, <c>false</c>.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        public static Boolean DoesTagExist(String path)
        {
            //Guard.ArgumentNotNull(stream, "path");
            ArgumentNotNull(path, "path");

            return (GetTagSize(path) != 0);
        }

        /// <summary>
        /// Removes an ID3v1 tag from a specified path.
        /// </summary>
        /// <param name="path">The full path of the file.</param>
        /// <returns><c>true</c> if an ID3v1 tag was removed; otherwise, <c>false</c>.</returns>
        public static Boolean RemoveTag(String path)
        {
            //Guard.ArgumentNotNull(path, "path");
            ArgumentNotNull(path, "path");

            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                if (!DoesTagExist(fileStream))
                    return false;

                fileStream.SetLength(fileStream.Length - 128);
            }

            return true;
        }

        #endregion <<< Public static methods >>>
    }
}
