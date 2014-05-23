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

        public int ImageCount { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string GoogleId { get; set; }

        public string GoogleReferenceId { get; set; }

        public string YelpId { get; set; }

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
                    yelpPlaces.Remove(p2);
                    places.Add(p1);
                }
                else
                {
                    p1.YelpId = "";
                }
            }

            foreach(Place p in googlePlaces)
            {
                if (string.IsNullOrEmpty(p.YelpId))
                    places.Add(p);
            }

            places.AddRange(yelpPlaces);

            //places.AddRange(Place.GetByName(placeName, latitude, longitude));


            return places;
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

        public static List<string> GoogleAutoComplete(string city)
        {
            return GooglePlacesService.GoogleAutoComplete(city);
        }

        public static List<string> DownloadImages(List<string> urls)
        {
            BlobService blob = new BlobService("places");
            blob.ClearTmpBlobs();

            List<string> blobUrls = new List<string>();
            for(int i = 0; i < urls.Count; i++)
            {
                ImageService service = new ImageService();

                string tmpUrl = "tmp_" + i.ToString() + ".png";
                service.SaveTmpImage(urls[i]);
                blob.CreateBlob(tmpUrl);
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

            blob.CreateBlob(string.Format("tmp_{0}.png", id));
        }

        public void Save(List<int> sortOrder)
        {
            Name = Name.Replace("&amp;", "&");            
            ImageCount = sortOrder.Count;
            base.Save();

            BlobService blob = new BlobService("places");
            ImageService service = new ImageService();
            for (int i = 0, ii = sortOrder.Count; i < ii; i++)
            {
                string tmpBlobName = string.Format("tmp_{0}.png", i.ToString());
                if (blob.Exists(tmpBlobName))
                {
                    service.Download(string.Format("{0}{1}", blob.container, tmpBlobName));
                    blob.CreateBlob(string.Format("{0}_{1}.png", Id, sortOrder[i].ToString()));
                }
            }
        }
    }
}