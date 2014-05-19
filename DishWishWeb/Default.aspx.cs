using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using DishWishWeb.Models;

namespace DishWishWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {           
            
            
            //List<Place> p = Place.Get();
            //Place p = new Place();
            //p.Name = "Franklin's";
            //p.Save();

            //p.DeleteImages();
            //Image2.ImageUrl = string.Format("http://dishwishes.blob.core.windows.net/places/{0}_2", p.Id);
        }

        [WebMethod]
        public static List<Place> SearchPlaces(string placeName, double latitude, double longitude)
        {
            List<Place> places = Place.GoogleSearch(placeName, latitude.ToString(), longitude.ToString());

            return places;

        }

        [WebMethod]
        public static List<string> GoogleImages(string placeName, string city)
        {
            List<string> places = Place.GoogleImages(placeName, city);

            return places;

        }

        [WebMethod]
        public static List<string> CityAutoComplete(string city)
        {
            List<string> places = Place.GoogleAutoComplete(city);
            return places;
        }
    }
}