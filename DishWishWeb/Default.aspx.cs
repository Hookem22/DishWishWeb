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

        }

        [WebMethod]
        public static List<Place> SearchPlaces(string placeName, string city, double latitude, double longitude)
        {
            return Place.GoogleSearch(placeName, city, latitude.ToString(), longitude.ToString());
        }

        [WebMethod]
        public static List<string> GoogleImages(string placeName, string city)
        {
            return Place.GoogleImages(placeName, city);
        }

        [WebMethod]
        public static string GetWebsite(string yelpUrl)
        {
            return Place.GetWebsite(yelpUrl);
        }

        [WebMethod]
        public static List<string> GetWebsiteImages(string url)
        {
            return Place.GetWebsiteImages(url);
        }

        [WebMethod]
        public static List<string> DownloadImages(List<string> urls)
        {
            return Place.DownloadImages(urls);
        }

        [WebMethod]
        public static void CropImage(string id, double percentCrop)
        {
            Place.CropImage(id, percentCrop);
        }

        [WebMethod]
        public static Place SavePlace(Place place, List<int> sortOrder)
        {
            place.Save(sortOrder);
            return place;
        }

        [WebMethod]
        public static List<string> CityAutoComplete(string city)
        {
            return Place.GoogleAutoComplete(city);
        }
    }
}