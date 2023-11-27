using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML.Common
{
    public class FileHelper
    {
        public static string ExtractExtension(string strFileName)
        {
            return strFileName.Substring(strFileName.LastIndexOf(".") + 1).ToLower();
        }

        public static string RemoveExtension(string strFileName)
        {
            return strFileName.Substring(0, strFileName.Length - strFileName.Substring(strFileName.LastIndexOf(".")).Length);
        }

        /// <summary>
        /// Returns the content type based on the given file extension
        /// </summary>
        public static string GetContentType(string fileExtension)
        {
            fileExtension = fileExtension.Substring(0, 1) == "." ? fileExtension : string.Concat(".", fileExtension);

            var mimeTypes = new Dictionary<String, String>
        {
            {".bmp", "image/bmp"},
            {".gif", "image/gif"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".pdf", "application/pdf"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".xls", "application/vnd.ms-excel"},
            {".csv", "text/csv"},
            {".xml", "text/xml"},
            {".txt", "text/plain"},
            {".zip", "application/zip"},
            {".ogg", "application/ogg"},
            {".mp3", "audio/mpeg"},
            {".wma", "audio/x-ms-wma"},
            {".wav", "audio/x-wav"},
            {".wmv", "audio/x-ms-wmv"},
            {".swf", "application/x-shockwave-flash"},
            {".avi", "video/avi"},
            {".mp4", "video/mp4"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".qt", "video/quicktime"}
        };

            // if the file type is not recognized, return "application/octet-stream" so the browser will simply download it
            return mimeTypes.ContainsKey(fileExtension) ? mimeTypes[fileExtension] : "application/octet-stream";
        }

        public static void CollectAndWaitForPendingFinalizers()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


    }
}
