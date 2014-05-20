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
        public ImageService()
        {

        }

        public void Download(string url, string blobName)
        {

            string localFilename = string.Format(@"path\{0}.png", blobName);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, localFilename);
            }

            resizeImage(blobName);
        }

        public void resizeImage(string blobName)
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(string.Format(@"path\{0}.png", blobName)))
                {
                    double ratio = 1136.0 / (double)original.Height;

                    int newHeight = 1136;
                    double tempWidth = (double)original.Width * ratio;
                    int newWidth = (int)tempWidth;

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(newWidth, newHeight))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.DrawImage(original, 0, 0, (newWidth), (newHeight));
                            string newFilename = string.Format(@"path\{0}_1.png", blobName); /* Put new file path here */
                            newPic.Save(newFilename, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void cropImage(string blobName, int x, int y)
        {
            Bitmap src = Image.FromFile(string.Format(@"path\{0}_1.png", blobName)) as Bitmap;

            Rectangle cropRect = new Rectangle(x, y, 640, src.Height);

            using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height))
            {
                using (Graphics g = Graphics.FromImage(newPic))
                {
                    g.DrawImage(src, new Rectangle(0, 0, newPic.Width, newPic.Height),
                                     cropRect, GraphicsUnit.Pixel);
                    string newFilename = string.Format(@"path\{0}_2.png", blobName); /* Put new file path here */
                    newPic.Save(newFilename, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        public void Delete(string blobName)
        {
            if (File.Exists(string.Format(@"path\{0}.png", blobName)))
                File.Delete(string.Format(@"path\{0}.png", blobName));
            
            for (int i = 1; i <= 4; i++)
            {
                if (File.Exists(string.Format(@"path\{0}_{1}.png", blobName, i.ToString())))
                    File.Delete(string.Format(@"path\{0}_{1}.png", blobName, i.ToString()));
            }
        }

        public void Upload(Image img)
        {

        }
    }
}