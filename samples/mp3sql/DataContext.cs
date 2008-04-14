using System;
using System.Collections.Generic;
using System.Text;
using Glue.Data;

namespace mp3sql
{
    public class DataContext
    {
        public readonly IMappingProvider Provider;

        public DataContext(IMappingProvider provider)
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
                //track.Insert(provider);
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

                CREATE TABLE IF NOT EXISTS track_album (
                    track_id INTEGER NOT NULL,
                    album_id INTEGER NOT NULL,
                    track_number INTEGER,
                    UNIQUE (track_id, album_id)
                );
            ");
        }
    }
}
