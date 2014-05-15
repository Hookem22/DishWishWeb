using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace DishWishWeb.Services
{
    public class AzureService
    {
        HttpClient client;

        string key = "ttjXxmFBCjMSMnxYlgutYahHQHOMXB15";
        public AzureService()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-ZUMO-APPLICATION",key);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
 
        public IEnumerable<string> Get()
        {
            var data = client.GetStringAsync("https://dishwishes.azure-mobile.net/tables/Place").Result;
            var teams = JsonConvert.DeserializeObject<IEnumerable<string>>(data);
            return teams;
        }
 
        public string Get(int id)
        {
            var data = client.GetStringAsync("https://dishwishes.azure-mobile.net/tables/Place?$filter=Id eq " + id).Result;
            var team = JsonConvert.DeserializeObject<IEnumerable<string>>(data);
            return team.ToString(); //.FirstOrDefault();
        }
 
        public void Post(string team)
        {

            var obj = JsonConvert.SerializeObject(team, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var request = new HttpRequestMessage(HttpMethod.Post, "https://dishwishes.azure-mobile.net/tables/Place");
            request.Content = new StringContent(obj, Encoding.UTF8, "application/json");
 
            var data = client.SendAsync(request).Result;
 
            if (!data.IsSuccessStatusCode)
                throw new HttpResponseException(data.StatusCode);
            
            //throw new HttpResponseException(HttpStatusCode.BadRequest);
        }
 
        public void Put(string team)
        {

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), "https://dishwishes.azure-mobile.net/tables/Place/" + team);
            var obj = JsonConvert.SerializeObject(team);
            request.Content = new StringContent(obj, Encoding.UTF8, "application/json");
 
            var data = client.SendAsync(request).Result;
                
            if (!data.IsSuccessStatusCode)
                throw new HttpResponseException(data.StatusCode);
 
            //throw new HttpResponseException(HttpStatusCode.BadRequest);
        }
 
        public void Delete(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://dishwishes.azure-mobile.net/tables/Place/" + id);
            var data = client.SendAsync(request).Result;
 
            if (!data.IsSuccessStatusCode)
                throw new HttpResponseException(data.StatusCode);
        }
    }
}