using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class InfoController : ApiController
    {
        private List<Info> _mInfos = new List<Info>();
        private Info _mInfo = new Info();

        public InfoController()
        {
            _mInfos.Add(_mInfo);
        }

        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mInfo.Status = RestStatus.ParameterError;
                _mInfo.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mInfo.Status = RestStatus.AuthenticationFailed;
                _mInfo.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            // Info (s)
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(id));
            _mInfos.Clear();
            if (orderPrinters.Any())
            {
                foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
                {
                    Info mInfo = new Info();
                    mInfo.Name = orderPrinter.Name;
                    mInfo.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
                    mInfo.ContentCategoryId = orderPrinter.ContentCategoryGuid.ToString();
                    mInfo.PhoneNo = orderPrinter.PhoneNo;
                    mInfo.Address = orderPrinter.Address;
                    mInfo.ZipCode = orderPrinter.ZipCode;
                    mInfo.City = orderPrinter.City;
                    //mInfo.Lat = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[0].Trim().Replace('.', ','));
                    //mInfo.Long = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[1].Trim().Replace('.', ','));
                    mInfo.FacebookUrl = orderPrinter.FacebookUrl;
                    mInfo.TwitterUrl = orderPrinter.TwitterUrl;
                    mInfo.InstagramUrl = orderPrinter.InstagramUrl;
                    mInfo.Presentation = orderPrinter.Presentation;
                    mInfo.About = orderPrinter.About;
                    string strPdfPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/pdf/pdf.pdf", usertoken.CompanyGuid.ToString()));
                    if (File.Exists(strPdfPath))
                    {
                        mInfo.PdfUrl = string.Format("http://{0}/storage/{1}/pdf/pdf.pdf", Request.RequestUri.Authority, usertoken.CompanyGuid.ToString());
                    }
                    else
                    {
                        mInfo.PdfUrl = string.Empty;
                    }
                    mInfo.CompanyGuid = orderPrinter.CompanyGuid;
                    mInfo.CorporateGuid = orderPrinter.tCompany.CorporateGuid;

                    _mInfos.Add(mInfo);
                }
            }
            else
            {
                DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByContentCategoryGuid(new Guid(id));
                if (shopConfig != null)
                {
                    Info mInfo = new Info();
                    mInfo.FacebookUrl = string.IsNullOrEmpty(shopConfig.FacebookUrl) ? string.Empty : shopConfig.FacebookUrl;
                    mInfo.TwitterUrl = string.IsNullOrEmpty(shopConfig.TwitterUrl) ? string.Empty : shopConfig.TwitterUrl;
                    mInfo.InstagramUrl = string.IsNullOrEmpty(shopConfig.InstagramUrl) ? string.Empty : shopConfig.InstagramUrl;
                    mInfo.CompanyGuid = shopConfig.CompanyGuid;
                    mInfo.CorporateGuid = shopConfig.tCompany.CorporateGuid;

                    _mInfos.Add(mInfo);
                }
                else
                {
                    _mInfo.Status = RestStatus.NotExisting;
                    _mInfo.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
                }
            }

            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
        }

        // id = OrderPrinterGuid or ContentCategoryGuid
        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mInfo.Status = RestStatus.NotFormData;
                _mInfo.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id))
            {
                _mInfo.Status = RestStatus.ParameterError;
                _mInfo.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mInfo.Status = RestStatus.AuthenticationFailed;
                _mInfo.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strPresentation = string.IsNullOrEmpty(dic["Presentation"]) ? string.Empty : dic["Presentation"];
            string strAbout = string.IsNullOrEmpty(dic["About"]) ? string.Empty : dic["About"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];
            string strFacebookUrl = string.IsNullOrEmpty(dic["FacebookUrl"]) ? string.Empty : dic["FacebookUrl"];
            string strTwitterUrl = string.IsNullOrEmpty(dic["TwitterUrl"]) ? string.Empty : dic["TwitterUrl"];
            string strInstagramUrl = string.IsNullOrEmpty(dic["InstagramUrl"]) ? string.Empty : dic["InstagramUrl"];

            string strLatLng = ML.Geo.GoogleGeocodingAPI.ConvertToLatLng(string.Concat(strAddress, " ", strZipCode, " ", strCity));

            // Update
            DB.OrderPrinterRepository orderPrinterRepository = new DB.OrderPrinterRepository();
            IQueryable<DB.tOrderPrinter> orderPrinters = orderPrinterRepository.GetByContentCategoryGuid(new Guid(id));
            if (orderPrinters.Any())
            {
                _mInfos.Clear();
                foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
                {
                    //if (!string.IsNullOrEmpty(strPhoneNo))
                    //{
                        orderPrinter.PhoneNo = strPhoneNo;
                    //}
                    //if (!string.IsNullOrEmpty(strPresentation))
                    //{
                        orderPrinter.Presentation = strPresentation;
                    //}
                    //if (!string.IsNullOrEmpty(strAbout))
                    //{
                        orderPrinter.About = strAbout;
                    //}
                    //if (!string.IsNullOrEmpty(strAddress))
                    //{
                        orderPrinter.Address = strAddress;
                    //}
                    //if (!string.IsNullOrEmpty(strZipCode))
                    //{
                        orderPrinter.ZipCode = strZipCode;
                    //}
                    //if (!string.IsNullOrEmpty(strCity))
                    //{
                        orderPrinter.City = strCity;
                    //}
                    //if (!string.IsNullOrEmpty(strFacebookUrl))
                    //{
                        orderPrinter.FacebookUrl = strFacebookUrl;
                    //}
                    //if (!string.IsNullOrEmpty(strTwitterUrl))
                    //{
                        orderPrinter.TwitterUrl = strTwitterUrl;
                    //}
                    //if (!string.IsNullOrEmpty(strInstagramUrl))
                    //{
                        orderPrinter.InstagramUrl = strInstagramUrl;
                    //}
                    //if (!string.IsNullOrEmpty(strLatLng))
                    //{
                        orderPrinter.LatLong = strLatLng;
                    //}

                    Info mInfo = new Info();
                    mInfo.Name = orderPrinter.Name;
                    mInfo.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
                    mInfo.ContentCategoryId = orderPrinter.ContentCategoryGuid.ToString();
                    mInfo.PhoneNo = orderPrinter.PhoneNo;
                    mInfo.Address = orderPrinter.Address;
                    mInfo.ZipCode = orderPrinter.ZipCode;
                    mInfo.City = orderPrinter.City;
                    //mInfo.Lat = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[0].Trim().Replace('.', ','));
                    //mInfo.Long = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[1].Trim().Replace('.', ','));
                    mInfo.FacebookUrl = orderPrinter.FacebookUrl;
                    mInfo.TwitterUrl = orderPrinter.TwitterUrl;
                    mInfo.InstagramUrl = orderPrinter.InstagramUrl;
                    mInfo.Presentation = orderPrinter.Presentation;
                    mInfo.About = orderPrinter.About;

                    _mInfos.Add(mInfo);
                }
                orderPrinterRepository.Save();
            }

            // TODO OrderPrinterGuid


            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
        }

        public async Task<HttpResponseMessage> Post(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mInfo.Status = RestStatus.MimeMultipartContent;
                _mInfo.StatusText = "Not MimeMultipartContent";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id))
            {
                _mInfo.Status = RestStatus.ParameterError;
                _mInfo.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mInfo.Status = RestStatus.AuthenticationFailed;
                _mInfo.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }


            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mInfo.Status = RestStatus.DataMissing;
                _mInfo.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }

            // Pdf file path
            string strPdfPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/pdf/pdf.pdf", usertoken.CompanyGuid.ToString()));

            // Pdf folder
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/pdf", usertoken.CompanyGuid.ToString()))))
            {
                Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/pdf", usertoken.CompanyGuid.ToString())));
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Handle file
            foreach (MultipartFileData file in provider.FileData)
            {
                string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                string strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                // Save to disk
                try
                {
                    // Delete existing pdf
                    if (File.Exists(strPdfPath))
                    {
                        File.Delete(strPdfPath);

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    // Move pdf to storage
                    File.Move(file.LocalFileName, strPdfPath);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Delete upload temp file
                    File.Delete(file.LocalFileName);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch
                {
                    _mInfo.Status = RestStatus.GenericError;
                    _mInfo.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
                }
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = provider.FormData;

            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strPresentation = string.IsNullOrEmpty(dic["Presentation"]) ? string.Empty : dic["Presentation"];
            string strAbout = string.IsNullOrEmpty(dic["About"]) ? string.Empty : dic["About"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];
            string strFacebookUrl = string.IsNullOrEmpty(dic["FacebookUrl"]) ? string.Empty : dic["FacebookUrl"];
            string strTwitterUrl = string.IsNullOrEmpty(dic["TwitterUrl"]) ? string.Empty : dic["TwitterUrl"];
            string strInstagramUrl = string.IsNullOrEmpty(dic["InstagramUrl"]) ? string.Empty : dic["InstagramUrl"];

            string strLatLng = ML.Geo.GoogleGeocodingAPI.ConvertToLatLng(string.Concat(strAddress, " ", strZipCode, " ", strCity));

            // Update
            DB.OrderPrinterRepository orderPrinterRepository = new DB.OrderPrinterRepository();
            IQueryable<DB.tOrderPrinter> orderPrinters = orderPrinterRepository.GetByContentCategoryGuid(new Guid(id));
            if (orderPrinters.Any())
            {
                _mInfos.Clear();
                foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
                {
                    if (!string.IsNullOrEmpty(strPhoneNo))
                    {
                        orderPrinter.PhoneNo = strPhoneNo;
                    }
                    if (!string.IsNullOrEmpty(strPresentation))
                    {
                        orderPrinter.Presentation = strPresentation;
                    }
                    if (!string.IsNullOrEmpty(strAbout))
                    {
                        orderPrinter.About = strAbout;
                    }
                    if (!string.IsNullOrEmpty(strAddress))
                    {
                        orderPrinter.Address = strAddress;
                    }
                    if (!string.IsNullOrEmpty(strZipCode))
                    {
                        orderPrinter.ZipCode = strZipCode;
                    }
                    if (!string.IsNullOrEmpty(strCity))
                    {
                        orderPrinter.City = strCity;
                    }
                    if (!string.IsNullOrEmpty(strFacebookUrl))
                    {
                        orderPrinter.FacebookUrl = strFacebookUrl;
                    }
                    if (!string.IsNullOrEmpty(strTwitterUrl))
                    {
                        orderPrinter.TwitterUrl = strTwitterUrl;
                    }
                    if (!string.IsNullOrEmpty(strInstagramUrl))
                    {
                        orderPrinter.InstagramUrl = strInstagramUrl;
                    }
                    if (!string.IsNullOrEmpty(strLatLng))
                    {
                        orderPrinter.LatLong = strLatLng;
                    }

                    Info mInfo = new Info();
                    mInfo.Name = orderPrinter.Name;
                    mInfo.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
                    mInfo.ContentCategoryId = orderPrinter.ContentCategoryGuid.ToString();
                    mInfo.PhoneNo = orderPrinter.PhoneNo;
                    mInfo.Address = orderPrinter.Address;
                    mInfo.ZipCode = orderPrinter.ZipCode;
                    mInfo.City = orderPrinter.City;
                    //mInfo.Lat = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[0].Trim().Replace('.', ','));
                    //mInfo.Long = string.IsNullOrEmpty(orderPrinter.LatLong) ? 0 : Convert.ToDouble(orderPrinter.LatLong.Split(',').ToArray()[1].Trim().Replace('.', ','));
                    mInfo.FacebookUrl = orderPrinter.FacebookUrl;
                    mInfo.TwitterUrl = orderPrinter.TwitterUrl;
                    mInfo.InstagramUrl = orderPrinter.InstagramUrl;
                    mInfo.Presentation = orderPrinter.Presentation;
                    mInfo.About = orderPrinter.About;
                    if (File.Exists(strPdfPath))
                    {
                        mInfo.PdfUrl = string.Format("http://{0}/storage/{1}/pdf/pdf.pdf", Request.RequestUri.Authority, usertoken.CompanyGuid.ToString());
                    }
                    else
                    {
                        mInfo.PdfUrl = string.Empty;
                    }

                    _mInfos.Add(mInfo);
                }
                orderPrinterRepository.Save();
            }

            // TODO OrderPrinterGuid


            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
        }

    }
}
