using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Glue.Data;

namespace mp3sql
{
    public class DataContext
    {
        public readonly IDataProvider Provider;

        public DataContext(IDataProvider provider)
        {
            this.Provider = provider;
        }
        
        public Track TrackFindOrNew(string path)
        {
            return Provider.FindByFilter<Track>(Filter.Create("path=@0", path)) ?? new Track(path);
        }

        public void TrackSave(Track track)
        {
            if (track.Id == 0)
                Provider.Insert(track);
            else
                Provider.Update(track);
        }

        public Album AlbumFindOrNew(string name)
        {
            return Provider.FindByFilter<Album>(Filter.Create("name=@0", name)) ?? new Album(name);
        }

        public void AlbumSave(Album album)
        {
            if (album.Id == 0)
                Provider.Insert(album);
            else
                Provider.Update(album);
        }

        public void AddTrackToAlbum(Track track, Album album)
        {
            Provider.AddManyToMany(track, album, "track_album");
        }

        /// <summary>
        /// Returns tracks in an album. Returned as a dictionary (track: int)
        /// where the integer is the position of the track on the album.
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public IDictionary<Track, int?> AlbumGetTracks(Album album)
        {
            IDictionary<Track, int?> list = new Dictionary<Track, int?>();
            IDataReader reader = Provider.ExecuteReader(@"SELECT track_number, id 
                    FROM track_album INNER JOIN track ON id=trackid 
                    WHERE albumid=@0", "@0", album.Id);
            while (reader.Read())
            {
                int? track_number = reader["track_number"] == DBNull.Value? 0: Convert.ToInt32(reader["track_number"]);
                int id = Convert.ToInt32(reader["id"]);
                list[Provider.Find<Track>(id)] = track_number;
            }
            return list;
        }

        public void CreateTables()
        {
            Provider.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS track (
			        id     INTEGER PRIMARY KEY,
			        path   VARCHAR(255) UNIQUE NOT NULL,
			        title  VARCHAR(30),
			        artist VARCHAR(30),
			        year   INTEGER,
			        comment VARCHAR(255)
		        );
                
                CREATE TABLE IF NOT EXISTS album (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(30) UNIQUE NOT NULL
                );
                --DROP TABLE track_album;
                CREATE TABLE IF NOT EXISTS track_album (
                    trackid INTEGER NOT NULL,
                    albumid INTEGER NOT NULL,
                    track_number INTEGER,
                    UNIQUE (trackid, albumid)
                );
            ");
        }
    }
}
