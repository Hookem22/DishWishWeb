using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DishWishWeb.Services
{
    public class GoogleImageService
    {
        public GoogleImageService()
        {

        }

        public static List<string> GoogleImages(string placeName, string city)
        {
            List<string> images = new List<string>();

            try
            {
                placeName = placeName.Replace(" ", "%20").Replace("&amp;", "%26");

                string googleUrl = string.Format("http://www.bing.com/images/search?q={0}%20{1}", placeName, city);
                string html = WebService.GetResponse(googleUrl);
                html = html.Substring(html.IndexOf("<div class=\"imgres\">"));
                html = html.Remove(html.IndexOf("overlayIframe"));

                int i = 0;
                while (html.Contains("imgurl:"))// && i < 20)
                {
                    html = html.Substring(html.IndexOf("imgurl:") + 13);
                    string image = html.Substring(0, html.IndexOf("&quot;"));

                    images.Add(image);
                    i++;
                }
            }
            catch { }

            return images;
        }

        public static List<string> WebsiteImages(string url)
        {
            List<string> images = new List<string>();

            try
            {
                string html = WebService.GetResponse(url);
                html = html.Substring(html.IndexOf("<body"));

                int i = 0;
                while (html.Contains("src=\""))
                {
                    html = html.Substring(html.IndexOf("src=\"") + 5);
                    string image = html.Substring(0, html.IndexOf("\""));

                    if(image.Contains(".png") || image.Contains(".jpg"))
                    {
                        if(!image.Contains("http://") && image.Substring(0, 2) != "//")
                        {
                            image = string.Format("{0}/{1}", url, image);
                        }
                        images.Add(image);
                    }
                }
            }
            catch { }

            return images;
        }
    }
}