using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace ML.ImageLibrary
{
    public class ImageManipulation
    {
        public enum AnchorPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }

        //public static Image FixedSize(Image imgPhoto, Size size, Color color, PixelFormat pixelFormat)
        //{
        //    return FixedSize(imgPhoto, size.Width, size.Height, color, pixelFormat);
        //}

        public static Image FixedSize(Image imgPhoto, int Width, int Height, Color color, PixelFormat pixelFormat)
        {
            Bitmap bmPhoto = new Bitmap(Width, Height, pixelFormat);
            bmPhoto.SetResolution(72, 72);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(color);

            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

    //grPhoto.CompositingMode = CompositingMode.SourceCopy;
            //ImageAttributes ... wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            //grPhoto.CompositingMode = CompositingMode.SourceCopy;

            ImageAttributes imgAtt = new ImageAttributes();
            imgAtt.SetWrapMode(WrapMode.TileFlipXY);
            
            grPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, Width, Height), 0, 0, imgPhoto.Width, imgPhoto.Height, GraphicsUnit.Pixel, imgAtt);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height, Color color, PixelFormat pixelFormat, string strRatio)
        {
            string[] str = strRatio.Split(':');
            decimal decWidthRatio = Convert.ToDecimal(str[0]);
            decimal decHeightRatio = Convert.ToDecimal(str[1]);

            decimal decRatio = decWidthRatio / decHeightRatio;
            Height = Convert.ToInt32(Math.Round(Width * decRatio));

            float width = Width;
            float height = Height;

            SolidBrush brush = new SolidBrush(Color.White);

            Bitmap image = new Bitmap(imgPhoto);

            float scale = Math.Min(width / image.Width, height / image.Height);

            var bmp = new Bitmap((int)width, (int)height, pixelFormat);
            bmp.SetResolution(72, 72);

            Graphics grPhoto = Graphics.FromImage(bmp);
            grPhoto.Clear(color);

            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // uncomment for higher quality output
            //graph.InterpolationMode = InterpolationMode.High;
            //graph.CompositingQuality = CompositingQuality.HighQuality;
            //graph.SmoothingMode = SmoothingMode.AntiAlias;

            var scaleWidth = (int)(image.Width * scale);
            var scaleHeight = (int)(image.Height * scale);

            grPhoto.FillRectangle(brush, new RectangleF(0, 0, width, height));
            grPhoto.DrawImage(image, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));

            grPhoto.Dispose();
            return bmp;
        }

        public static Image FixedSize(Image imgPhoto, int Width, int Height, Color color, PixelFormat pixelFormat, bool testVersion)
        {
            float width = Width;
            float height = Height;

            SolidBrush brush = new SolidBrush(Color.White);

            Bitmap image = new Bitmap(imgPhoto);

            float scale = Math.Min(width / image.Width, height / image.Height);

            var bmp = new Bitmap((int)width, (int)height, pixelFormat);
            bmp.SetResolution(72, 72);

            Graphics grPhoto = Graphics.FromImage(bmp);
            grPhoto.Clear(color);

            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // uncomment for higher quality output
            //graph.InterpolationMode = InterpolationMode.High;
            //graph.CompositingQuality = CompositingQuality.HighQuality;
            //graph.SmoothingMode = SmoothingMode.AntiAlias;

            var scaleWidth = (int)(image.Width * scale);
            var scaleHeight = (int)(image.Height * scale);

            grPhoto.FillRectangle(brush, new RectangleF(0, 0, width, height));
            grPhoto.DrawImage(image, new Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));

            grPhoto.Dispose();
            return bmp;
        }


        public static Image Crop(Image imgPhoto, int Width, int Height, AnchorPosition Anchor)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (Anchor)
                {
                    case AnchorPosition.Top:
                        destY = 0;
                        break;
                    case AnchorPosition.Bottom:
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (Anchor)
                {
                    case AnchorPosition.Left:
                        destX = 0;
                        break;
                    case AnchorPosition.Right:
                        destX = (int)(Width - (sourceWidth * nPercent));
                        break;
                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}
