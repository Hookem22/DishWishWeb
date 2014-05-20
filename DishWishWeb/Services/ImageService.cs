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
        string localFileName = @"path\tmp.png";
        string localFileNameSort = @"path\tmp_{0}.png";

        int fullImageWidth = 640;
        int fullImageHeight = 1136;

        public ImageService()
        {

        }

        public string SaveTmpImage(string url, string sortId)
        {
            Download(url);
            ResizeImage(sortId, localFileName);
            SetTmpImage(sortId);
            ScaleImage(sortId);

            return "tmp_" + sortId.ToString();
        }

        public void Crop(string id, double percentCrop)
        {
            SetTmpImage(id);
            CropImage(id, percentCrop);
            ResizeImage(id, @"path\crop.png");
        }

        public void Download(string url)
        {
            Delete(localFileName);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localFileName);
            }
        }


        public void ResizeImage(string sortId, string fromFileName)
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(fromFileName))
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
                            string newFilename = string.Format(localFileNameSort, sortId);
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

        public void ScaleImage(string sortId)
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

                            string newFilename = string.Format(localFileNameSort, sortId);
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
                    string newFilename = @"path\crop.png";
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
                    if (File.Exists(string.Format(localFileNameSort, i.ToString())))
                        File.Delete(string.Format(localFileNameSort, i.ToString()));
                }
            }
            catch { }
        }

        public void Upload(Image img)
        {

        }
    }
}