using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ML.ImageLibrary
{
    public class ImageHelper
    {
        public enum JobStatus : int
        {
            Success = 0
            , Error = -1
        }

        //public static JobStatus ResizeImage(XmlNode xmlNodeCommand)
        //{
        //    try
        //    {
        //        System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(xmlNodeCommand.SelectSingleNode("origimage").InnerText);
        //        System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        //        System.Drawing.Image imgTemp = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, Convert.ToInt32(xmlNodeCommand.SelectSingleNode("width").InnerText), Convert.ToInt32(xmlNodeCommand.SelectSingleNode("height").InnerText), color);
        //        imgTemp.Save(xmlNodeCommand.SelectSingleNode("newimage").InnerText);
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }
        //    catch
        //    {
        //        return JobStatus.Error;
        //    }
        //    return JobStatus.Success;
        //}

        public static bool ResizeImage(string strPath, int intWidth)
        {
            return ResizeImage(strPath, intWidth, 0, string.Empty);
        }

        public static bool ResizeImage(string strPath, int intWidth, int intImageCompression)
        {
            return ResizeImage(strPath, intWidth, intImageCompression, string.Empty);
        }

        public static bool ResizeImage(string strPath, int intWidth, int intImageCompression, string strRatio)
        {
            string strNewPath = strPath.Substring(0, strPath.LastIndexOf("\\"));
            string strExt = ML.Common.FileHelper.ExtractExtension(strPath).ToLower();

            //System.Drawing.Imaging.ImageCodecInfo[] encoders = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

            //TEST
            //System.Drawing.Imaging.EncoderParameters parameters = new System.Drawing.Imaging.EncoderParameters(1);
            //parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lCompression); // Compressionvalue
            //parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 40); // Compressionvalue
            //System.Drawing.Imaging.ImageCodecInfo codec = GetEncoderInfo(strExt);
            // END TEST


            long lngCompression = 50L;
            if (intImageCompression > 0)
            {
                lngCompression = (long)intImageCompression;
            }


            if (!File.Exists(string.Format("{0}\\{1}.{2}", strNewPath, intWidth.ToString(), strExt)))
            {
                System.Drawing.Imaging.ImageCodecInfo[] infos = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
                System.Drawing.Imaging.EncoderParameters Params = new System.Drawing.Imaging.EncoderParameters(1);
                System.Drawing.Imaging.ImageCodecInfo info = null;
                System.Drawing.Imaging.PixelFormat pixelFormat = new System.Drawing.Imaging.PixelFormat();

                if (strExt == "bmp")
                {
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                    Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lngCompression);
                    info = infos[0];
                }
                else if (strExt == "jpg")
                {
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format16bppRgb565;
                    Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lngCompression);
                    info = infos[1];
                }
                else if (strExt == "gif")
                {
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lngCompression);
                    info = infos[2];
                }
                else if (strExt == "tif")
                {
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lngCompression);
                    info = infos[3];
                }
                else if (strExt == "png")
                {
                    pixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                    Params.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lngCompression);
                    info = infos[4];
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(strPath);
                    //System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 255, 255, 255);

                    // TEST
                    System.Drawing.Color color = Color.Transparent;
                    // END TEST

                    //System.Drawing.Image imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color, pixelFormat, strRatio);
                    System.Drawing.Image imgNew;
                    if(string.IsNullOrEmpty(strRatio))
                    {
                       imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color, pixelFormat);
                    }
                    else
                    {
                        imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color, pixelFormat, strRatio);
                    }

                    //imgNew.Save(string.Format("{0}\\{1}.{2}", strNewPath, intWidth.ToString(), strExt));
                    //imgNew.Save(string.Format("{0}\\{1}.{2}", strNewPath, intWidth.ToString(), strExt), codec, parameters);
                    imgNew.Save(string.Format("{0}\\{1}.{2}", strNewPath, intWidth.ToString(), strExt), info, Params);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public static void ResizeImage(string strPath, string strNewPath, int intWidth)
        {
            if (File.Exists(strPath))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(strPath);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 255, 255, 255);
                    System.Drawing.Image imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    imgNew.Save(strNewPath);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch { }
            }
        }

        public static void ResizeImage(string strPath, string strNewPath, int intWidth, int intHeight)
        {
            if (File.Exists(strPath))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(strPath);
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(0, 255, 255, 255);
                    System.Drawing.Image imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, intWidth, intHeight, color, System.Drawing.Imaging.PixelFormat.Format32bppArgb, true);
                    imgNew.Save(strNewPath);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch { }
            }
        }

        public static void ResizeImage(string strPath, string strNewPath, ImageItem imageItem)
        {
            if (File.Exists(strPath))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();

                try
                {
                    System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(strPath);
                    System.Drawing.Color color = imageItem.Color;

                    System.Drawing.Image imgNew;
                    if (color == Color.Transparent)
                    {
                        imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, imageItem.Width, imageItem.Height, imageItem.Color, System.Drawing.Imaging.PixelFormat.Format32bppArgb, true);
                    }
                    else
                    {
                        imgNew = ML.ImageLibrary.ImageManipulation.FixedSize(imgOrig, imageItem.Width, imageItem.Height, imageItem.Color, System.Drawing.Imaging.PixelFormat.Format24bppRgb, true);
                    }

                    imgNew.Save(strNewPath);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch { }
            }
        }

        //public static int GetAlteredWidth(int intWidth, string strDeviceID)
        //{
        //    if(strDeviceID.IndexOf("iphone") > -1)
        //    {
        //        return 2 * intWidth;
        //    }

        //    return intWidth;
        //}


        //public static void ResizeImageKeepWidth(string strPath, string strFileName, string strNewFileName, int intWidth, bool bAlpha, long lCompression)
        //{
        //    ResizeImage(strPath, strFileName, strNewFileName, intWidth, 0, bAlpha, lCompression);
        //}

        //public static void ResizeImageKeepHeight(string strPath, string strFileName, string strNewFileName, int intHeight, bool bAlpha, long lCompression)
        //{
        //    ResizeImage(strPath, strFileName, strNewFileName, 0, intHeight, bAlpha, lCompression);
        //}

        //private static void ResizeImage(string strPath, string strFileName, string strNewFileName, int intWidth, int intHeight, bool bAlpha, long lCompression)
        //{
        //    if (File.Exists(string.Format("{0}\\{1}", strPath, strNewFileName)))
        //    {
        //        File.Delete(string.Format("{0}\\{1}", strPath, strNewFileName));
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();

        //    System.Drawing.Image imgOrig = System.Drawing.Image.FromFile(string.Format("{0}\\{1}", strPath, strFileName));
        //    System.Drawing.Color color;
        //    if (bAlpha)
        //    {
        //        color = System.Drawing.Color.FromArgb(0, 255, 255, 255);
        //    }
        //    else
        //    {
        //        color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        //    }

        //    //if (intWidth != 0)
        //    //{
        //    //    System.Drawing.Image imgNew = BLL.ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color);
        //    //    imgNew.Save(string.Format("{0}\\{1}", strPath, strNewFileName));
        //    //}
        //    //else if(intHeight != 0)
        //    //{
        //    //    System.Drawing.Image imgNew = BLL.ImageManipulation.FixedSize(imgOrig, Convert.ToInt32(intHeight / ((float)imgOrig.Height / (float)imgOrig.Width)), intHeight, color);
        //    //    imgNew.Save(string.Format("{0}\\{1}", strPath, strNewFileName));
        //    //}


        //    //System.Drawing.Imaging.ImageCodecInfo.
        //    System.Drawing.Imaging.ImageCodecInfo[] encoders = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

        //    System.Drawing.Imaging.EncoderParameters parameters = new System.Drawing.Imaging.EncoderParameters(1);
        //    parameters.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, lCompression); // Compressionvalue
        //    System.Drawing.Imaging.ImageCodecInfo codec = GetEncoderInfo(FileHelper.ExtractExtension(strFileName));

        //    if (intWidth != 0)
        //    {
        //        System.Drawing.Image imgNew = ImageManipulation.FixedSize(imgOrig, intWidth, Convert.ToInt32(intWidth / ((float)imgOrig.Width / (float)imgOrig.Height)), color);
        //        imgNew.Save(string.Format("{0}\\{1}", strPath, strNewFileName), codec, parameters);
        //    }
        //    else if (intHeight != 0)
        //    {
        //        System.Drawing.Image imgNew = ImageManipulation.FixedSize(imgOrig, Convert.ToInt32(intHeight / ((float)imgOrig.Height / (float)imgOrig.Width)), intHeight, color);
        //        imgNew.Save(string.Format("{0}\\{1}", strPath, strNewFileName), codec, parameters);
        //    }

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //}










    }


}
