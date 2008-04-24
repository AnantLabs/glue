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

        public void AddTrackToAlbum(Track track, Album album, int track_number)
        {
            AddTrackToAlbum(track, album);
            Provider.ExecuteNonQuery("UPDATE track_album SET track_number=@n WHERE trackid=@tid AND albumid=@aid", 
                "@n", track_number, "@tid", track.Id, "@aid", album.Id);
        }

        /// <summary>
        /// Returns tracks in an album. Returned as a list of pairs (track nr, album)
        /// where track nr is the position of the track on the album.
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public List<Tup<long?, Track>> AlbumGetTracks(Album album)
        {
            List<Tup<long?, Track>> list = new List<Tup<long?, Track>>();
            IDataReader reader = Provider.ExecuteReader(@"SELECT track_number, id 
                    FROM track_album INNER JOIN track ON id=trackid 
                    WHERE albumid=@0 ORDER BY track_number", "@0", album.Id);
            while (reader.Read())
            {
                long? track_number = null;
                if (!reader.IsDBNull(0))
                    track_number = reader.GetInt64(0);
                //long? track_number = reader.GetInt64(0);
                long id = reader.GetInt64(1);
                Track t = Provider.Find<Track>(id);
                list.Add(new Tup<long?, Track>(track_number, t));
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

    /// <summary>
    /// Tuple helper class
    /// </summary>
    public class Tup<T1, T2>
    {
        public T1 t1;
        public T2 t2;

        public Tup(T1 x, T2 y)
        {
            t1 = x;
            t2 = y;
        }
    }
}
