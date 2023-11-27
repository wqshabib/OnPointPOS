using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace POSSUM.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/File")]
    public class FileController : BaseAPIController
    {


        [HttpGet]
        [Route("GetNewFile")]
        public HttpResponseMessage GetNewFile([FromUri] Dates dates)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
                string filePath = ConfigurationManager.AppSettings["StockFilePath"];
                string fileSavePath = ConfigurationManager.AppSettings["StockFileSavePath"];

                var directory = new DirectoryInfo(filePath);
                var newFile = directory.GetFiles().OrderByDescending(f => f.CreationTime).First();
                if (newFile != null && newFile.CreationTime >= Convert.ToDateTime(LAST_EXECUTED_DATETIME))
                {
                    var dest = "";
#if (DEBUG)
                    dest = @"D:\Luqon-IT\POSSUM\POSSUM\POSSUM.Api\storage\";
#else
                 dest = fileSavePath;             
#endif

                    System.IO.File.Copy(newFile.FullName, dest + newFile.Name, true);
                    return Request.CreateResponse(HttpStatusCode.OK, newFile.Name);
                }
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, string.Empty);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, string.Empty);
            }

        }

        [HttpGet]
        [Route("GetLatestFile")]
        public HttpResponseMessage GetLatestFile([FromUri] Dates dates)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            DateTime LAST_EXECUTED_DATETIME = dates.LastExecutedDate;
            string filePath = ConfigurationManager.AppSettings["StockFilePath"];

            var directory = new DirectoryInfo(filePath);
            var newFile = directory.GetFiles().OrderByDescending(f => f.CreationTime).First();
            if (newFile != null && newFile.CreationTime >= Convert.ToDateTime(LAST_EXECUTED_DATETIME))
            {
                var path = newFile.FullName;
                var fileName = newFile.Name;
                //var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                //result.Content = new StreamContent(stream);
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                ////Read the File into a Byte Array.
                byte[] bytes = File.ReadAllBytes(path);

                //Set the Response Content.
                result.Content = new ByteArrayContent(bytes);

                //Set the Response Content Length.
                result.Content.Headers.ContentLength = bytes.LongLength;

                //Set the Content Disposition Header Value and FileName.
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = fileName;

                //Set the File Content Type.
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
            }
            else
                result.StatusCode = HttpStatusCode.NoContent;

            return result;
        }


    }

}
