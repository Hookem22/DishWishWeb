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


        public static List<Place> GoogleSearch(string placeName, string latitude, string longitude)
        {
            List<Place> places = GooglePlacesService.GoogleSearchCity(placeName, latitude, longitude);
            if(placeName.Length > 3)
            {
                places.AddRange(Place.GetByName(placeName, latitude, longitude));
            }

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

                string tmpUrl = "tmp_" + i.ToString();
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
            service.Download(string.Format("{0}tmp_{1}", blob.container, id));
            service.Crop(percentCrop);

            blob.CreateBlob("tmp_" + id);
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
                string tmpBlobName = "tmp_" + i.ToString();
                if (blob.Exists(tmpBlobName))
                {
                    service.Download(string.Format("{0}{1}", blob.container, tmpBlobName));
                    blob.CreateBlob(string.Format("{0}_{1}", Id, sortOrder[i].ToString()));
                }
            }
            blob.ClearTmpBlobs();
        }
    }
}