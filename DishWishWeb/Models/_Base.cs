using System;
using System.Collections.Generic;
using System.Linq;
using DishWishWeb.Services;
using Newtonsoft.Json;

namespace DishWishWeb.Models
{

    public abstract class _Base<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        static string _azureTable;

        static string AzureTable
        {
            get
            {
                if (string.IsNullOrEmpty(_azureTable))
                {
                    T item = (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[0]);
                    //_azureTable = typeof(T).GetProperty("AzureTable").GetValue(item).ToString();
                }
                return _azureTable;
            }
            set { _azureTable = value; }
        }

        public _Base(string azureTable)
        {
            _azureTable = azureTable;
        }

        public static T Get(string id)
        {
            AzureService service = new AzureService(AzureTable);
            var data = service.Get(id);
            T item = JsonConvert.DeserializeObject<T>(data);
            return item;
        }

        public static List<T> Get()
        {
            AzureService service = new AzureService(AzureTable);
            var data = service.Get();
            IEnumerable<T> items = JsonConvert.DeserializeObject<IEnumerable<T>>(data);
            return items.ToList();
        }

        public static List<T> GetByName(string name, string lat, string lng)
        {
            AzureService service = new AzureService(AzureTable);
            var data = service.GetByName(name, lat, lng);
            IEnumerable<T> items = JsonConvert.DeserializeObject<IEnumerable<T>>(data);
            return items.ToList();
        }  

        public void Save()
        {
            AzureService service = new AzureService(AzureTable);
            if (string.IsNullOrEmpty(this.Id))
            {
                var obj = JsonConvert.SerializeObject(this, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                this.Id = service.Post(obj);
            }
            else
            {
                var obj = JsonConvert.SerializeObject(this);
                service.Put(this.Id, obj);
            }
        }
        
        public void Delete()
        {
            AzureService service = new AzureService(AzureTable);
            service.Delete(this.Id);
        }
       
    }
}