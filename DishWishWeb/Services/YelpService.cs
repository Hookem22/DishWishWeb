using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DishWishWeb.Models;
using System.Web;

namespace DishWishWeb.Services
{
    public class YelpService
    {
        public YelpService()
        {

        }
        
        static private OAuthBase oAuth = new OAuthBase();
        public static List<Place> Search(string term, string location)
        {
            term = term.Replace(" ", "-");
            location = location.Replace(" ", "-");

            //var url = String.Format("http://api.yelp.com/v2/search?term={0}&location={1}&limit=10&category_filter=food", term, location);
            var url = String.Format("http://api.yelp.com/v2/search?term={0}&location={1}&limit=10", term, location);
            string data = loadYelp(new Uri(url));

            return Deserialize(data);
        }

        public static Place GetPlace(string yelpId)
        {
            var url = String.Format("http://api.yelp.com/v2/business/{0}", yelpId);
            string data = loadYelp(new Uri(url));

            return Deserialize(data)[0];
        }

        private static string loadYelp(Uri uri)
        {
            string url, parameters;
            var signature = oAuth.GenerateSignature(uri,
                                    ConfigurationManager.AppSettings["YelpConsumerKey"],
                                    ConfigurationManager.AppSettings["YelpConsumerSecret"],
                                    ConfigurationManager.AppSettings["YelpToken"],
                                    ConfigurationManager.AppSettings["YelpTokenSecret"],
                                    "GET",
                                    oAuth.GenerateTimeStamp(),
                                    oAuth.GenerateNonce(),
                                    OAuthBase.SignatureTypes.HMACSHA1,
                                    out url,
                                    out parameters
                                    );
            var newURL = string.Format("{0}?{1}&oauth_signature={2}", url, parameters, HttpUtility.UrlEncode(signature));

            return WebService.GetResponse(newURL);
        }

        private static List<Place> Deserialize(string p)
        {
            if (p.Contains("\"businesses\"")) 
                p = p.Substring(p.IndexOf("\"businesses\""));
            List<Place> places = new List<Place>();
            for (int i = 0; i < 5; i++)
            {
                if (!p.Contains("\"id\": \"") || !p.Contains("\"name\": "))
                    break;

                Place place = new Place();
                places.Add(place);

                place.Id = "";
                place.GoogleId = "";

                if (p.Contains("\"name\": \""))
                {
                    p = p.Remove(0, p.IndexOf("\"name\": \"") + 9);
                    place.Name = p.Substring(0, p.IndexOf("\","));
                }
                if (p.Contains("\"url\": \""))
                {
                    p = p.Remove(0, p.IndexOf("\"url\": \"") + 8);
                    place.Website = p.Substring(0, p.IndexOf("\","));
                }
                if (p.Contains("\"id\": \""))
                {
                    p = p.Remove(0, p.IndexOf("\"id\": \"") + 7);
                    place.YelpId = p.Substring(0, p.IndexOf("\","));
                }

                if (p.Contains("\"location\":"))
                {
                    try
                    {
                        p = p.Remove(0, p.IndexOf("\"location\":") + 11);

                        string address, city, state, country;
                        p = p.Remove(0, p.IndexOf("\"city\": \"") + 9);
                        city = p.Substring(0, p.IndexOf("\","));

                        p = p.Remove(0, p.IndexOf("\"country_code\": \"") + 17);
                        country = p.Substring(0, p.IndexOf("\","));

                        p = p.Remove(0, p.IndexOf("\"address\": ") + 10);
                        p = p.Remove(0, p.IndexOf("[") + 1);
                        address = p.Substring(0, p.IndexOf("],"));
                        if (!string.IsNullOrEmpty(address))
                        {
                            address = address.Substring(1);
                            address = address.Substring(0, address.Length - 1);
                        }
                        if (address.Contains("\","))
                            address = address.Substring(0, address.IndexOf("\","));


                        p = p.Remove(0, p.IndexOf("\"state_code\": \"") + 15);
                        state = p.Substring(0, p.IndexOf("\"}"));

                        place.GoogleReferenceId = string.Format("{0},{1},{2},{3}", address, city, state, country);
                    }
                    catch
                    {
                        place.GoogleReferenceId = "";
                    }

                    
                }
            }

            return places;
        }

        public static string GetWebsite(string yelpUrl)
        {
            try
            {
                string yelp = WebService.GetResponse(yelpUrl);

                yelp = yelp.Substring(yelp.IndexOf("biz-website"));
                yelp = yelp.Substring(yelp.IndexOf("<a href") + 5);
                yelp = yelp.Substring(yelp.IndexOf(">") + 1);
                yelpUrl = yelp.Substring(0, yelp.IndexOf("</a>"));
                if (!yelpUrl.Contains("http://"))
                    yelpUrl = "http://" + yelpUrl;
            }
            catch { }

            return yelpUrl;
        }
    }
}