using System;
using System.Collections;
using System.Collections.Generic;
using Glue.Web;
using Glue.Lib;
using mp3sql;

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
    }
}
