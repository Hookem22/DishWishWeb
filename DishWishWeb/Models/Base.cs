using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DishWishWeb.Services;

namespace DishWishWeb.Models
{
    public class Base
    {
        public Base()
        {
            

        }

        public string Get()
        {
            AzureService service = new AzureService();
            IEnumerable<string> s = service.Get();

            return s.ToString();
        }
    }
}