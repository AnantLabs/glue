using System;
using System.Collections;
using System.Collections.Generic;

using Glue.Web;
using Glue.Lib;
using mp3sql;

namespace mp3web.Controllers
{
    public class AlbumController : BaseController
    {
        public GridHelper.Grid Grid;
        public Album Album;
        public List<Tup<long?, Track>> Tracks;

        public AlbumController(IRequest request, IResponse response)
            : base(request, response)
        {
        }

        public void List()
        {
            Grid = new GridHelper.Grid(Parameters);
            Grid.List = (IList)App.Provider.List<Album>(null, Grid.Order, Grid.Limit);
            Grid.TotalCount = App.Provider.Count<Album>(null);
            
            Render("/views/album/list.html");
        }

        public void Edit(int id)
        {
            Album = App.Provider.Find<Album>(id);

            if (IsPostBack)
            {
                Album.Name = Params.Get("name");
                try
                {
                    App.Provider.Update(Album);
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }
                if (Errors.Count == 0)
                    Redirect("/album/list");
            }

            Tracks = new DataContext(App.Provider).AlbumGetTracks(Album);
            Render("/views/album/edit.html");
        }

        public void New()
        {
            Album = new Album();
            if (IsPostBack)
            {
                Album.Name = Params.Get("name");
                try
                {
                    App.Provider.Insert(Album);
                }
                catch (Exception e)
                {
                    Errors.Add(e);
                }
                if (Errors.Count == 0)
                    Redirect("/album/list");
            }
            Render("/views/album/edit.html");
        }

        /// <summary>
        /// Delete an album and redirect to the list view.
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            App.Provider.Delete<Album>(id);
            Redirect("/album/list");
        }

        public void RemoveTrack(int id)
        {
            int trackId = Convert.ToInt32(Params["track"]);

            Album a = App.Provider.Find<Album>(id);
            Track t = App.Provider.Find<Track>(trackId);

            App.Provider.DelManyToMany(a, t, "track_album");
            Redirect("/album/edit/" + id);
        }
    }
}
