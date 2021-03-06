﻿using System;
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
        const int fullImageWidth = 640 / 2;
        const int fullImageHeight = (1136 - 200) / 2;
        const int menuImageWidth = fullImageWidth * 2;
        const int menuImageHeight = 1136 - 100;

        public string currentFile;
        string tempFile;

        public ImageService(bool isPdf = false)
        {
            if(isPdf)
                currentFile = Path.Combine(tempFolder, "current.pdf");
            else
                currentFile = Path.Combine(tempFolder, "current.png");
            tempFile = Path.Combine(tempFolder, "tmp.png");
        }

        private string tempFolder { get { return Path.Combine(Path.GetTempPath(), "DishWish"); } }

        private void DirectoryInit()
        {
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            //else
            //{
            //    try
            //    {
            //        DirectoryInfo folder = new DirectoryInfo(tempFolder);
            //        foreach (FileInfo file in folder.GetFiles())
            //        {
            //            file.Delete();
            //        }
            //    }
            //    catch { }
            //}
        }
        
        public void SaveTmpImage(string url)
        {
            DirectoryInit();
            Download(url);
            ResizeImage();
            SetCurrentImage();
            ScaleImage();
            SetCurrentImage();
        }

        public void Crop(double percentCrop)
        {
            CropImage(percentCrop);
            SetCurrentImage();
            ResizeImage();
            SetCurrentImage();
        }

        public void SaveMenu(string url)
        {
            if (url.Contains("awesomescreenshot.com"))
                url = GetMenuImageFromAwesomeScreenshot(url);

            Download(url);
            ResizeImageForWidth();
            SetCurrentImage();
            ScaleMenuImage();
            SetCurrentImage();
            ToJpg();
        }

        public void Download(string url)
        {
            Delete(currentFile);

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, currentFile);
            }
        }

        public void ToJpg()
        {
            try
            {
                using (System.Drawing.Image img = System.Drawing.Image.FromFile(currentFile))
                {
                    img.Save(tempFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                SetCurrentImage();

            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        //public void DownloadLocal(string url)
        //{
        //    Delete(currentFile);
        //    using (WebClient client = new WebClient())
        //    {
        //        client.DownloadFile(Path.Combine(tempFolder, url), currentFile);
        //    }
        //}

        public void ResizeImage()
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(currentFile))
                {

                    double ratio = (double)fullImageWidth / (double)original.Width;
                    double d_newHeight = (double)original.Height * ratio;
                    int newHeight = (int)d_newHeight;
                    int newWidth = fullImageWidth;

                    //if(newHeight > fullImageHeight)
                    //{
                    //    newHeight = fullImageHeight;
                    //    double d_newWidth = (double)original.Width * ratio;
                    //    newWidth = (int)d_newWidth;
                    //}

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(newWidth, newHeight))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.DrawImage(original, 0, 0, (newWidth), (newHeight));
                            newPic.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void ScaleImage()
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(currentFile))
                {
                    double ratio = fullImageWidth / (double)original.Width;
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

                            newPic.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void CropImage(double percentCrop)
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(currentFile))
                {
                    double offsetX;
                    double offsetY;
                    double newHeight;
                    double newWidth;

                    if (percentCrop < .5)
                    {
                        offsetX = percentCrop * (double)original.Width;
                        offsetY = (percentCrop * (double)original.Height) / 2;
                        newHeight = (1 - percentCrop) * (double)original.Height;
                        newWidth = (1 - percentCrop) * (double)original.Width;
                    }
                    else
                    {
                        percentCrop = 1 - percentCrop;

                        offsetX = 0;
                        offsetY = (percentCrop * (double)original.Height) / 2;
                        newHeight = (1 - percentCrop) * (double)original.Height;
                        newWidth = (1 - percentCrop) * (double)original.Width;
                    }


                    Rectangle cropRect = new Rectangle((int)offsetX, (int)offsetY, (int)newWidth, (int)newHeight);

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(cropRect.Width, cropRect.Height))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.DrawImage(original, new Rectangle(0, 0, newPic.Width, newPic.Height),
                                             cropRect, GraphicsUnit.Pixel);

                            newPic.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
            
        }

        public void ResizeImageForWidth()
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(currentFile))
                {
                    double ratio = (double)menuImageWidth / (double)original.Width;
                    double d_newHeight = (double)original.Height * ratio;
                    int newHeight = (int)d_newHeight;
                    int newWidth = menuImageWidth;

                    using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(newWidth, newHeight))
                    {
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                        {
                            gr.DrawImage(original, 0, 0, (newWidth), (newHeight));
                            newPic.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }

        public void ScaleMenuImage()
        {
            try
            {
                using (System.Drawing.Image original = System.Drawing.Image.FromFile(currentFile))
                {
                    int ht = original.Height < menuImageHeight ? menuImageHeight : original.Height;
                    using (Bitmap originalBitmap = new Bitmap(original))
                    {
                        using (System.Drawing.Bitmap newPic = new System.Drawing.Bitmap(menuImageWidth, ht))
                        {
                            using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(newPic))
                            {
                                Color color = originalBitmap.GetPixel(originalBitmap.Width / 2, originalBitmap.Height - 5);
                                if (color == null)
                                    color = Color.White;

                                gr.Clear(color);

                                gr.DrawImage(original, new Point(0, 0));

                                newPic.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string desc = ex.ToString();
            }
        }


        private void SetCurrentImage()
        {
            try
            {
                Delete(currentFile);
                File.Move(tempFile, currentFile);
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

        public string GetMenuImageFromAwesomeScreenshot(string url)
        {
            string response = WebService.GetResponse(url);
            string menuImg = "";

            if (response.Contains("<img id=\"screenshot\""))
            {
                response = response.Substring(response.IndexOf("<img id=\"screenshot\""));
                response = response.Substring(response.IndexOf("src") + 5);

                menuImg = response.Remove(response.IndexOf("\""));
            }
            return menuImg;

        }

    }
}