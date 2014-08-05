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

        #region Properties

        public string Name { get; set; }

        public int ImageCount { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string GoogleId { get; set; }

        public string GoogleReferenceId { get; set; }

        public string YelpId { get; set; }

        public string Website { get; set; }

        public string Menu { get; set; }

        public string LunchMenu { get; set; }

        public string BrunchMenu { get; set; }

        public string DrinkMenu { get; set; }

        public string HappyHourMenu { get; set; }

        #endregion

        public static List<Place> GoogleSearch(string placeName, string city, string latitude, string longitude)
        {
            List<Place> googlePlaces = GooglePlacesService.GoogleSearchCity(placeName, latitude, longitude);
            List<Place> yelpPlaces = YelpService.Search(placeName, city);
            List<Place> places = new List<Place>();

            foreach(Place p1 in googlePlaces)
            {
                Place p2 = yelpPlaces.Find(p => p.GoogleReferenceId.Contains(p1.YelpId));
                if (p2 == null) {
                    p2 = yelpPlaces.Find(p => p.GoogleReferenceId.Contains(p1.YelpId.Substring(0, 3)));
                    if (p2 != null && p1.Name.Length > 2 && p2.Name.Length > 2 && p1.Name.Substring(0, 3) != p2.Name.Substring(0, 3))
                        p2 = null;
                }
                if(p2 != null)
                {
                    p1.YelpId = p2.YelpId;
                    p1.Website = p2.Website;
                    yelpPlaces.Remove(p2);
                    places.Add(p1);
                }
                else
                {
                    p1.YelpId = "";
                    p1.Website = "";
                }
            }

            foreach(Place p in googlePlaces)
            {
                if (string.IsNullOrEmpty(p.YelpId))
                    places.Add(p);
            }

            places.AddRange(yelpPlaces);

            try
            {
                places.AddRange(Place.GetByName(placeName, latitude, longitude));
            }
            catch (Exception ex) { }

            return places;
        }

        public static Place GetFromId(string id)
        {
            Place yelp = YelpService.GetPlace(id);
            Place google = GooglePlacesService.GetPlace(id);
            return yelp;
        }

        public static List<string> GoogleImages(string placeName, string city)
        {
            List<string> urls = GoogleImageService.GoogleImages(placeName, city);
            System.Threading.Thread.Sleep(200);
            urls.AddRange(GoogleImageService.GoogleImages(placeName + " inside", city));
            System.Threading.Thread.Sleep(200);
            urls.AddRange(GoogleImageService.GoogleImages(placeName + " food", city));
            System.Threading.Thread.Sleep(200);
            urls.AddRange(GoogleImageService.GoogleImages(placeName + " yelp", city));

            return urls;
        }

        public static string GetWebsite(string yelpUrl)
        {
            return YelpService.GetWebsite(yelpUrl);
        }

        public static List<string> GetWebsiteImages(string url)
        {
            return GoogleImageService.WebsiteImages(url);
        }

        public static List<string> GoogleAutoComplete(string city)
        {
            return GooglePlacesService.GoogleAutoComplete(city);
        }

        public static List<string> DownloadImages(List<string> urls)
        {
            BlobService blob = new BlobService("places");

            List<string> blobUrls = new List<string>();
            for(int i = 0; i < urls.Count; i++)
            {
                ImageService service = new ImageService();

                string tmpUrl = "tmp_" + i.ToString() + ".png";
                service.SaveTmpImage(urls[i]);
                blob.CreateBlob(tmpUrl, service.currentFile);
                blobUrls.Add(blob.container + tmpUrl);
            }

            return blobUrls;

        }

        public static void CropImage(string id, double percentCrop)
        {
            BlobService blob = new BlobService("places");
            
            ImageService service = new ImageService();
            service.Download(string.Format("{0}tmp_{1}.png", blob.container, id));
            service.Crop(percentCrop);

            blob.CreateBlob(string.Format("tmp_{0}.png", id), service.currentFile);
        }

        public void Save(List<int> sortOrder)
        {
            Name = Name.Replace("&amp;", "&");            
            ImageCount = sortOrder.Count;    
            
            base.Save();

            SaveMenus();
            base.Save(); //Save new menu names

            SaveImages(sortOrder);
        }

        private void SaveMenus()
        {
            if (!string.IsNullOrEmpty(Menu))
            {
                Menu = SaveMenu(Menu, "Menu");
            }
            if (!string.IsNullOrEmpty(LunchMenu))
            {
                LunchMenu = SaveMenu(LunchMenu, "LunchMenu");
            }
            if (!string.IsNullOrEmpty(BrunchMenu))
            {
                BrunchMenu = SaveMenu(BrunchMenu, "BrunchMenu");
            }
            if (!string.IsNullOrEmpty(DrinkMenu))
            {
                DrinkMenu = SaveMenu(DrinkMenu, "DrinkMenu");
            }
            if (!string.IsNullOrEmpty(HappyHourMenu))
            {
                HappyHourMenu = SaveMenu(HappyHourMenu, "HappyHourMenu");
            }
        }

        private string SaveMenu(string menu, string name)
        {
            if (menu.Contains("dishwishes.blob"))
                return menu;
            
            BlobService blob = new BlobService("places");
            ImageService service;
            string blobName = "";

            if (menu.Contains(".pdf"))
            {
                service = new ImageService(true); 
                service.Download(menu);
                blobName = string.Format("{0}_{1}.pdf", Id, name);
            }
            else
            {
                service = new ImageService();                
                service.SaveMenu(menu);
                blobName = string.Format("{0}_{1}.jpg", Id, name);
            }
           
            blob.CreateBlob(blobName, service.currentFile);
            return string.Format("{0}{1}", blob.container, blobName);
        }

        private void SaveImages(List<int> sortOrder)
        {
            BlobService blob = new BlobService("places");
            ImageService service = new ImageService();
            for (int i = 0, ii = sortOrder.Count; i < ii; i++)
            {
                string tmpBlobName = string.Format("tmp_{0}.png", i.ToString());
                if (blob.Exists(tmpBlobName))
                {
                    service.Download(string.Format("{0}{1}", blob.container, tmpBlobName));
                    service.ToJpg();
                    blob.CreateBlob(string.Format("{0}_{1}.jpg", Id, sortOrder[i].ToString()), service.currentFile);
                }
            }
        }

        public List<string> GetImageSizes()
        {
            List<string> sizes = new List<string>();
            BlobService blob = new BlobService("places");
            for(int i = 0, ii = ImageCount; i < ii; i++)
            {
                sizes.Add(blob.GetBlobSize(string.Format("{0}_{1}.jpg", Id, i)));
            }

            GetMenuImageSize(Menu, sizes);
            GetMenuImageSize(LunchMenu, sizes);
            GetMenuImageSize(BrunchMenu, sizes);
            GetMenuImageSize(DrinkMenu, sizes);
            GetMenuImageSize(HappyHourMenu, sizes);
            
            return sizes;
        }

        static void GetMenuImageSize(string menuName, List<string> sizes)
        {
            BlobService blob = new BlobService("places");
            if (menuName.Contains("dishwishes"))
            {
                if (menuName.Contains("/"))
                    menuName = menuName.Substring(menuName.LastIndexOf("/") + 1);

                sizes.Add(blob.GetBlobSize(menuName));
            }
        }

        public static void DeleteAll()
        {
            BlobService blobService = new BlobService("places");

            List<string> blobs = blobService.GetAllBlobs();
            foreach(string blob in blobs)
            {
                blobService.DeleteBlob(blob);
            }

            List<Place> places = Place.Get();
            foreach(Place place in places)
            {
                place.Delete();
            }
        }
    }
}