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

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string GoogleId { get; set; }

        public string GoogleReferenceId { get; set; }

        public static List<Place> GoogleSearch(string placeName, string latitude, string longitude)
        {
            return GooglePlacesService.GoogleSearchCity(placeName, latitude, longitude);
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

        public static List<string> GoogleImages(string placeName, string city)
        {
            return GoogleImageService.GoogleImages(placeName, city);
        }

        public static List<string> GoogleAutoComplete(string city)
        {
            return GooglePlacesService.GoogleAutoComplete(city);
        }
    }
}