using System;
using System.Collections;
using System.Collections.Generic;
using Glue.Web;
using Glue.Lib;
using Glue.Data;
using mp3sql;
using IdSharp.Tagging.ID3v1;

namespace mp3web.Controllers
{
    public class TrackController : BaseController
    {
        public GridHelper.Grid Grid;
        public Track Track;

        public TrackController(IRequest request, IResponse response)
            : base(request, response)
        {
        }

        public void List()
        {
            Grid = new GridHelper.Grid(Parameters);
            Grid.TotalCount = App.Provider.Count<Track>(null);
            Grid.List = (IList)App.Provider.List<Track>(null, Grid.Order, Grid.Limit);

            Render("/views/track/list.html");
        }

        public void Edit(int id)
        {
            Track = App.Provider.Find<Track>(id);

            if (IsPostBack)
            {
                Track.Title = Params.Get("title");
                Track.Artist = Params.Get("artist");
                Track.Comment = Params.Get("comment");
                Track.Path = Params.Get("path");

                long year;
                if (long.TryParse(Params.Get("year"), out year))
                    Track.Year = year;

                try
                {
                    App.Provider.Update(Track);
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }
                if (Errors.Count == 0)
                    Redirect("/track/list");
            
            }
            
            Grid = new GridHelper.Grid(Parameters);
            Filter f = new Filter("EXISTS(SELECT trackid FROM track_album WHERE trackid=@0 AND albumid=album.id)", Track.Id);
            Grid.TotalCount = App.Provider.Count<Album>(f);
            Grid.List = (IList)App.Provider.List<Album>(f, Grid.Order, Grid.Limit);
            
            Render("/views/track/edit.html");
        }

        public void New()
        {
            Track = new Track();
            if (IsPostBack)
            {
                Track.Title = Params.Get("title");
                Track.Artist = Params.Get("artist");
                Track.Comment = Params.Get("comment");
                Track.Path = Params.Get("path");
                try
                {
                    App.Provider.Insert(Track);
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }

                new DataContext(App.Provider).AddTrackToAlbum(Track, App.Provider.Find<Album>(1));
                if (Errors.Count == 0)
                    Redirect("/track/list");
            }
            Render("/views/track/edit.html");
        }

        /// <summary>
        /// Delete a track and redirect to the list view.
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            App.Provider.Delete<Track>(id);
            Redirect("/track/list");
        }

        /// <summary>
        /// Display a page to select an album to add the track to.
        /// </summary>
        /// <param name="id"></param>
        public void AddAlbum(int id)
        {
            Track = App.Provider.Find<Track>(id);

            if (IsPostBack)
            {
                try
                {
                    Album a = App.Provider.Find<Album>(Params.Get("album_id"));
                    string track_nr = Params.Get("track_nr");
                    DataContext dc = new DataContext(App.Provider);
                    if (NullConvert.IsNoE(track_nr))
                        dc.AddTrackToAlbum(Track, a);
                    else
                        dc.AddTrackToAlbum(Track, a, Convert.ToInt32(track_nr));
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }

                if (Errors.Count == 0)
                    Redirect("/track/list");
            }

            Grid = new GridHelper.Grid(Parameters);
            Grid.List = (IList)App.Provider.List<Album>(null, Grid.Order, Grid.Limit);
            Grid.TotalCount = App.Provider.Count<Album>(null);

            Render("/views/track/addalbum.html");
        }

        /// <summary>
        /// Add a new track and get the mp3 tag data from an uploaded file.
        /// </summary>
        public void Upload()
        {
            PostedFile file = Request.Files[0];
            ID3v1 tags = new ID3v1(file.Content);
            Track track = new Track(file.FileName);
            track.Assign(tags);


            DataContext context = new DataContext(App.Provider);
            using (context.Provider.Open())
            {
                // Save track to database
                context.TrackSave(track);

                // Save Album to database
                Album album = context.AlbumFindOrNew(tags.Album);
                context.AlbumSave(album);

                // Put track in album
                context.AddTrackToAlbum(track, album);
            }

            Redirect("/track/list");
        }
    }
}
