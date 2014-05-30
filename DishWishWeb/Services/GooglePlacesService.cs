using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DishWishWeb.Models;

namespace DishWishWeb.Services
{
    public class GooglePlacesService
    {
        public GooglePlacesService()
        {

        }

        public static List<Place> GoogleSearchCity(string placeName, string latitude, string longitude)
        {
            return GooglePlaceSearch(placeName, latitude, longitude, "30000", "&types=bakery|bar|cafe|food|night_club|restaurant", 6);
        }

        public static List<Place> GoogleSearchSpecific(string placeName, string latitude, string longitude)
        {
            return GooglePlaceSearch(placeName, latitude, longitude, "1", /*"&types=food"*/ "", 1);
        }

        private static List<Place> GooglePlaceSearch(string placeName, string latitude, string longitude, string radius, string googleType, int numberToReturn)
        {
            placeName = placeName.Replace(" ", "%22");

            string url = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&radius={2}{3}&name={4}&sensor=false&key=AIzaSyA1Viw-vy8_HSZmS02R9MBMoyNsYi5y2ME", latitude, longitude, radius, googleType, placeName);

            string r = WebService.GetResponse(url);

            return Deserialize(r, numberToReturn);
        }

        public static List<string> GoogleAutoComplete(string city)
        {
            city = city.Replace(" ", "%22");
            string url = string.Format("https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=(cities)&sensor=true&key=AIzaSyA1Viw-vy8_HSZmS02R9MBMoyNsYi5y2ME", city);

            string r = WebService.GetResponse(url);

            List<string> places = new List<string>();
            while (r.Contains("\"description\" : \""))
            {
                r = r.Remove(0, r.IndexOf("\"description\" : \"") + 17);
                places.Add(r.Substring(0, r.IndexOf("\",")));
            }

            return places;
        }

        public static Place GetPlace(string googleReferenceId)
        {
            string url = string.Format("https://maps.googleapis.com/maps/api/place/details/json?reference={0}&sensor=true&key=AIzaSyA1Viw-vy8_HSZmS02R9MBMoyNsYi5y2ME", googleReferenceId);

            string r = WebService.GetResponse(url);

            List<Place> place = Deserialize(r, 1);
            if (place.Count > 0)
                return place[0];

            return new Place();
        }

        private static List<Place> Deserialize(string r, int numberToReturn)
        {
            List<Place> places = new List<Place>();
            for (int i = 0; i < numberToReturn; i++)
            {
                if (!r.Contains("\"id\" : \"") || !r.Contains("\"lat\" : "))
                    break;

                //if (p.Contains("\"price_level\" : 1,"))
                //    continue;

                Place place = new Place();
                places.Add(place);

                place.Id = "";

                if (r.Contains("\"lat\" : "))
                {
                    r = r.Remove(0, r.IndexOf("\"lat\" : ") + 8);
                    string lat = r.Substring(0, r.IndexOf(",")).Trim();
                    double d_lat;
                    if (double.TryParse(lat, out d_lat))
                        place.Latitude = d_lat;
                }

                if (r.Contains("\"lng\" : "))
                {
                    r = r.Remove(0, r.IndexOf("\"lng\" : ") + 8);
                    string lng = r.Substring(0, r.IndexOf("}")).Trim();
                    double d_lng;
                    if (double.TryParse(lng, out d_lng))
                        place.Longitude = d_lng;
                }
                if (r.Contains("\"id\" : \""))
                {
                    r = r.Remove(0, r.IndexOf("\"id\" : \"") + 8);
                    place.GoogleId = r.Substring(0, r.IndexOf("\","));
                }
                if (r.Contains("\"name\" : \""))
                {
                    r = r.Remove(0, r.IndexOf("\"name\" : \"") + 10);
                    place.Name = r.Substring(0, r.IndexOf("\","));
                }

                if (r.Contains("\"reference\" : \""))
                {
                    r = r.Remove(0, r.IndexOf("\"reference\" : \"") + 15);
                    place.GoogleReferenceId = r.Substring(0, r.IndexOf("\","));
                }
                if (r.Contains("\"vicinity\" : \""))
                {
                    r = r.Remove(0, r.IndexOf("\"vicinity\" : \"") + 14);
                    place.YelpId = r.Substring(0, r.IndexOf("\""));
                    if (place.YelpId.Contains(","))
                        place.YelpId = place.YelpId.Substring(0, place.YelpId.IndexOf(","));
                }

                if (r.Contains("\"geometry\" : "))
                {
                    r = r.Remove(0, r.IndexOf("\"geometry\" : ") + 10);
                }
                else
                    break;
            }

            return places;
        }

    }
}