using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace DishWishWeb.Services
{
    public class WebService
    {
        public WebService()
        {

        }


        public static string GetResponse(string url)
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