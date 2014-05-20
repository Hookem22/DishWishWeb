using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            return GooglePlaceSearch(placeName, latitude, longitude, "30000", "&types=bakery|bar|cafe|food|night_club|restaurant", 5);
        }

        public static List<Place> GoogleSearchSpecific(string placeName, string latitude, string longitude)
        {
            return GooglePlaceSearch(placeName, latitude, longitude, "1", /*"&types=food"*/ "", 1);
        }

        private static List<Place> GooglePlaceSearch(string placeName, string latitude, string longitude, string radius, string googleType, int numberToReturn)
        {
            placeName = placeName.Replace(" ", "%22");

            string url = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0},{1}&radius={2}{3}&name={4}&sensor=false&key=AIzaSyA1Viw-vy8_HSZmS02R9MBMoyNsYi5y2ME", latitude, longitude, radius, googleType, placeName);

            string r = GetResponse(url);

            List<Place> places = new List<Place>();
            for (int i = 0; i < numberToReturn; i++)
            {
                if (!r.Contains("\"id\" : \"") || !r.Contains("\"lat\" : ") || !r.Contains("\"vicinity\" : \""))
                    break;

                string p = r.Remove(0, r.IndexOf("\"lat\" : "));
                p = p.Substring(0, p.IndexOf("\"vicinity\" : \""));

                r = r.Remove(0, r.IndexOf("\"vicinity\" : \"") + 10);
                if (p.Contains("\"price_level\" : 1,"))
                    continue;

                Place place = new Place();
                places.Add(place);

                if (p.Contains("\"lat\" : "))
                {
                    p = p.Remove(0, p.IndexOf("\"lat\" : ") + 8);
                    place.Latitude = p.Substring(0, p.IndexOf(",")).Trim();
                }

                if (p.Contains("\"lng\" : "))
                {
                    p = p.Remove(0, p.IndexOf("\"lng\" : ") + 8);
                    place.Longitude = p.Substring(0, p.IndexOf("}")).Trim();
                }
                if (p.Contains("\"id\" : \""))
                {
                    p = p.Remove(0, p.IndexOf("\"id\" : \"") + 8);
                    place.GoogleId = p.Substring(0, p.IndexOf("\","));
                }
                if (p.Contains("\"name\" : \""))
                {
                    p = p.Remove(0, p.IndexOf("\"name\" : \"") + 10);
                    place.Name = p.Substring(0, p.IndexOf("\","));
                }

                if (p.Contains("\"reference\" : \""))
                {
                    p = p.Remove(0, p.IndexOf("\"reference\" : \"") + 15);
                    place.GoogleReferenceId = p.Substring(0, p.IndexOf("\","));
                }

            }

            return places;
        }

        public static List<string> GoogleAutoComplete(string city)
        {
            city = city.Replace(" ", "%22");
            string url = string.Format("https://maps.googleapis.com/maps/api/place/autocomplete/json?input={0}&types=(cities)&sensor=true&key=AIzaSyA1Viw-vy8_HSZmS02R9MBMoyNsYi5y2ME", city);

            string r = GetResponse(url);

            List<string> places = new List<string>();
            while (r.Contains("\"description\" : \""))
            {
                r = r.Remove(0, r.IndexOf("\"description\" : \"") + 17);
                places.Add(r.Substring(0, r.IndexOf("\",")));
            }

            return places;
        }


        private static string GetResponse(string url)
        {
            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.Timeout = 20000;
            webRequest.Method = "GET";

            var response = webRequest.GetResponse();
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                var resp = reader.ReadToEnd();
                return resp.ToString();
            }
        }
    }
}