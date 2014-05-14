using System;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DishWish.App_Code
{
    public class BlobService
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        private readonly string placesContainer = "places";

        public BlobService()
        {

        }

        public void CreateBlob(string blobName)
        {
            try
            {
                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container. 
                CloudBlobContainer container = blobClient.GetContainerReference(placesContainer);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                using (var fileStream = System.IO.File.OpenRead(string.Format(@"path\{0}.png", blobName)))
                {
                    blockBlob.UploadFromStream(fileStream);
                }

            }
            catch (Exception ex)
            {
                string exc = ex.ToString();
            }
        }
    }
}