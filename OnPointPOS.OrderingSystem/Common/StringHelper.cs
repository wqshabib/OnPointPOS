using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ML.Common
{
    public class StringHelper
    {
        public static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        public static Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }


        public static string EncodeTo64(string toEncode)
        {
            return EncodeTo64(toEncode, false);
        }

        public static string EncodeTo64(string toEncode, bool ASCII)
        {
            byte[] toEncodeAsBytes;

            if (ASCII)
            {
                toEncodeAsBytes = System.Text.Encoding.ASCII.GetBytes(toEncode);
            }
            else
            {
                toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            }

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }


        public static string DecodeFrom64(string encodedData)
        {
            return DecodeFrom64(encodedData, string.Empty);
        }

        public static string DecodeFrom64(string encodedData, string strEncoding)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);

            if (strEncoding.ToLower() == "utf-8")
            {
                return System.Text.Encoding.UTF8.GetString(encodedDataAsBytes);
            }
            else
            {
                return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            }
        }


        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }


        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }



        public static string UrlEncode(string strText)
        {
            return HttpUtility.UrlEncode(strText);
        }
    }



}
