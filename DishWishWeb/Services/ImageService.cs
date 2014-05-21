using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace DishWishWeb.Services
{
    public class ImageService
    {
        public string localFolder;
        string localFileName;
        string localFileNameSort;

        int fullImageWidth = 640;
        int fullImageHeight = 1136;

        public ImageService()
        {
            localFolder = Path.Combine(Path.GetTempPath(), "DishWish");
            if (!Directory.Exists(localFolder))
                Directory.CreateDirectory(localFolder);

            localFileName = Path.Combine(localFolder, "tmp.png");
            localFileNameSort = Path.Combine(localFolder, "tmp_{0}.png");
        }

        public string SaveTmpImage(string url, string id)
        {           
            Download(url);
            ResizeImage(id);
            SetTmpImage(id);
            ScaleImage(id);

            return "tmp_" + id;
        }

        public void Crop(string id, double percentCrop)
        {
            SetTmpImage(id);
            CropImage(id, percentCrop);
            SetTmpImage(id);
            ResizeImage(id);
        }

        public void Download(string url)
        {
            Delete(localFileName);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localFileName);
            }
        }


        public void ResizeImage(string id)
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(localFileName))
                {

                    double ratio = (double)fullImageWidth / (double)original.Width;
                    double d_newHeight = (double)original.Height * ratio;
                    int newHeight = (int)d_newHeight;
                    int newWidth = fullImageWidth;

                    if(newHeight > fullImageHeight)
                    {
                        newHeight = fullImageHeight;
                        double d_newWidth = (double)original.Width * ratio;
                        newWidth = (int)d_newWidth;
                    }

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(newWidth, newHeight))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.DrawImage(original, 0, 0, (newWidth), (newHeight));
                            string newFilename = string.Format(localFileNameSort, id);
                            newPic.Save(newFilename, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void ScaleImage(string id)
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(localFileName))
                {
                    double ratio = 640.0 / (double)original.Width;
                    double d_newHeight = (double)original.Height * ratio;
                    int newHeight = (int)d_newHeight;
                    double d_yOffset = (fullImageHeight - d_newHeight) / 2;
                    int yOffset = (int)d_yOffset;

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(fullImageWidth, fullImageHeight))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.Clear(Color.Black);

                            gr.DrawImage(original, new Point(0, yOffset));

                            string newFilename = string.Format(localFileNameSort, id);
                            newPic.Save(newFilename, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void CropImage(string id, double percentCrop)
        {
            Bitmap src = Image.FromFile(localFileName) as Bitmap;

            double offsetX = percentCrop * (double)src.Width;
            double offsetY = (percentCrop * (double)src.Height) / 2;
            double newHeight = (1 - percentCrop) * (double)src.Height;
            double newWidth = (1 - percentCrop) * (double)src.Width;

            Rectangle cropRect = new Rectangle((int)offsetX, (int)offsetY, (int)newWidth, (int)newHeight);

            using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height))
            {
                using (Graphics g = Graphics.FromImage(newPic))
                {
                    g.DrawImage(src, new Rectangle(0, 0, newPic.Width, newPic.Height),
                                     cropRect, GraphicsUnit.Pixel);
                    string newFilename = string.Format(localFileNameSort, id);
                    newPic.Save(newFilename, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void SetTmpImage(string id)
        {
            try
            {
                Delete(localFileName);
                File.Move(string.Format(localFileNameSort, id), localFileName);
            }
            catch { }

        }

        public void Delete(string url)
        {
            try
            {
                if (File.Exists(url))
                    File.Delete(url);
            }
            catch { }
        }

        public void ClearTmpImages()
        {
            try
            {
                for (int i = 0; i <= 10; i++)
                {
                    string url = string.Format(localFileNameSort, i.ToString());
                    if (File.Exists(url))
                        File.Delete(url);
                }
            }
            catch { }

        }

        public void Upload(Image img)
        {

        }
    }
}