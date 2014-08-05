using System;
using System.Collections.Generic;
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


        public void CreateBlob(string blobName, string currentFile)
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
                using (var fileStream = System.IO.File.OpenRead(currentFile))
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
                blockBlob.Delete();
            }
            catch { }
        }

        public List<string> GetAllBlobs()
        {
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

            IEnumerable<IListBlobItem> blobs = container.ListBlobs();
            List<string> blobList = new List<string>();
            foreach(IListBlobItem blob in blobs)
            {
                blobList.Add(blob.Uri.Segments[blob.Uri.Segments.Length - 1]);
            }

            return blobList;
        }

        public string GetBlobSize(string blobName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.FetchAttributes();

            long size = blockBlob.Properties.Length;

            //String conversion
            string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int mag = (int)Math.Log(size, 1024);
            decimal adjustedSize = (decimal)size / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);

        }

        public void ClearTmpBlobs()
        {
            for (int i = 0; i <= 10; i++)
            {
                DeleteBlob("tmp_" + i.ToString());
            }
        }

        public bool Exists(string blobName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            try
            {
                blockBlob.FetchAttributes();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}