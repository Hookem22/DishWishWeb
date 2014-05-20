using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DishWishWeb.Services;

namespace DishWishWeb.Models
{
    public class Place : _Base<Place>
    {
        public Place() : base("Place")
        {

        }
        public string Name { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string GoogleId { get; set; }

        public string GoogleReferenceId { get; set; }

        public static List<Place> GoogleSearch(string placeName, string latitude, string longitude)
        {
            return GooglePlacesService.GoogleSearchCity(placeName, latitude, longitude);
        }

        public static List<string> GoogleImages(string placeName, string city)
        {
            return GoogleImageService.GoogleImages(placeName, city);
        }

        public static List<string> GoogleAutoComplete(string city)
        {
            return GooglePlacesService.GoogleAutoComplete(city);
        }
    }
}