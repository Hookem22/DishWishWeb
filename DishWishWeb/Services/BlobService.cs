using System;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DishWishWeb.Services
{
    public class BlobService
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        string _containerName;

        public string container;

        public BlobService(string containerName)
        {
            _containerName = containerName;
            container = string.Format(@"http://dishwishes.blob.core.windows.net/{0}/", _containerName);
        }


        public void CreateBlob(string blobName)
        {
            try
            {
                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container. 
                CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

                // Create or overwrite the "myblob" blob with contents from a local file.
                using (var fileStream = System.IO.File.OpenRead(new ImageService().currentFile))
                {
                    blockBlob.UploadFromStream(fileStream);
                }

            }
            catch (Exception ex)
            {
                string exc = ex.ToString();
            }
        }

        public void DeleteBlob(string blobName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

            // Retrieve reference to a blob named "myblob.txt".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            try
            {
                // Delete the blob.
                blockBlob.Delete();
            }
            catch { }
        }

        public void ClearTmpBlobs()
        {
            for (int i = 0; i <= 10; i++)
            {
                DeleteBlob("tmp_" + i.ToString());
            }
        }
    }
}