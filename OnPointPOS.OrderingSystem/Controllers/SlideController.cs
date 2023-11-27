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
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class SlideController : ApiController
    {
        private List<dynamic> _mSlides = new List<dynamic>();
        private dynamic _mSlide = new ExpandoObject();

        public SlideController()
        {
            _mSlides.Add(_mSlide);
        }

        public HttpResponseMessage Get(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _mSlide.Status = RestStatus.ParameterError;
                _mSlide.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSlide.Status = RestStatus.AuthenticationFailed;
                _mSlide.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Populate
            IQueryable<DB.tSlide> slides = new DB.SlideRepository().GetSlides(usertoken.CompanyGuid);
            if (!slides.Any())
            {
                _mSlide.Status = RestStatus.NotExisting;
                _mSlide.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            _mSlides.Clear();
            foreach (DB.tSlide slide in slides)
            {
                dynamic mSlide = new ExpandoObject();
                mSlide.SlideId = slide.SlideGuid.ToString();
                mSlide.Title = slide.Title;
                mSlide.Description = slide.Description;
                mSlide.SortOrder = slide.SortOrder;

                _mSlide.ImageUrl = string.Empty;
                DB.tImage image = new DB.ImageRepository().GetByImageGuid(slide.ImageGuid);
                if (image != null)
                {
                    mSlide.ImageUrl = string.Format("http://{0}/storage/{1}/slide/{2}/{3}", Request.RequestUri.Authority, usertoken.CompanyGuid.ToString(), slide.ImageGuid.ToString(), image.FileName);
                }

                _mSlide.Url = slide.Url;

                _mSlides.Add(mSlide);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
        }

        public async Task<HttpResponseMessage> Post(string token)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mSlide.Status = RestStatus.FormatError;
                _mSlide.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            if (string.IsNullOrEmpty(token)) // && !ML.Common.Text.IsGuidNotEmpty(id)
            {
                _mSlide.Status = RestStatus.ParameterError;
                _mSlide.StatusText = "Parameter Error 1";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSlide.Status = RestStatus.AuthenticationFailed;
                _mSlide.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Prepare
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mSlide.Status = RestStatus.DataMissing;
                _mSlide.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            System.Collections.Specialized.NameValueCollection dic = provider.FormData;
            string strTitle = string.IsNullOrEmpty(dic["Title"]) ? string.Empty : dic["Title"];
            string strDescription = string.IsNullOrEmpty(dic["Description"]) ? string.Empty : dic["Description"];
            int intSortOrder = string.IsNullOrEmpty(dic["SortOrder"]) ? 0 : Convert.ToInt32(dic["SortOrder"]);
            string strUrl = string.IsNullOrEmpty(dic["Url"]) ? string.Empty : dic["Url"];

            MultipartFileData file = provider.FileData.FirstOrDefault();

            //string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
            string strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
            System.Drawing.Image image = System.Drawing.Image.FromFile(file.LocalFileName);

            // Save to imagebank
            DB.ImageService imageService = new DB.ImageService();
            imageService.SaveImage(
                image
                , strContentDispositionFileName
                , DB.ImageService.ImageCategory.Slide
                , string.Empty, DB.ImageService.Rotation.Rotate0
                , usertoken.CompanyGuid
                );

            Guid guidSlideGuid = Guid.NewGuid();
            if (new DB.SlideService().Save(usertoken.CompanyGuid, guidSlideGuid, strTitle, strDescription, intSortOrder, imageService.ImageGuid, strUrl) != DB.Repository.Status.Success)
            {
                _mSlide.Status = RestStatus.GenericError;
                _mSlide.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Populate
            _mSlide.SlideId = guidSlideGuid.ToString();
            _mSlide.Title = strTitle;
            _mSlide.Description = strDescription;
            _mSlide.SortOrder = intSortOrder;
            _mSlide.ImageUrl = string.Format("http://{0}/storage/{1}/slide/{2}/{3}", Request.RequestUri.Authority, usertoken.CompanyGuid.ToString(), imageService.ImageGuid.ToString(), strContentDispositionFileName);
            _mSlide.Url = strUrl;

            // Success
            _mSlide.Status = RestStatus.Success;
            _mSlide.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
        }

        public async Task<HttpResponseMessage> Put(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mSlide.Status = RestStatus.FormatError;
                _mSlide.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSlide.Status = RestStatus.ParameterError;
                _mSlide.StatusText = "Parameter Error 1";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSlide.Status = RestStatus.AuthenticationFailed;
                _mSlide.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Prepare
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mSlide.Status = RestStatus.DataMissing;
                _mSlide.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            System.Collections.Specialized.NameValueCollection dic = provider.FormData;
            string strTitle = string.IsNullOrEmpty(dic["Title"]) ? string.Empty : dic["Title"];
            string strDescription = string.IsNullOrEmpty(dic["Description"]) ? string.Empty : dic["Description"];
            int intSortOrder = string.IsNullOrEmpty(dic["SortOrder"]) ? 0 : Convert.ToInt32(dic["SortOrder"]);
            string strUrl = string.IsNullOrEmpty(dic["Url"]) ? string.Empty : dic["Url"];

            MultipartFileData file = provider.FileData.FirstOrDefault();

            string strContentDispositionFileName = string.Empty;
            Guid guidImageGuid = Guid.Empty;
            if (file != null)
            {
                //string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                System.Drawing.Image image = System.Drawing.Image.FromFile(file.LocalFileName);

                // Save to imagebank
                DB.ImageService imageService = new DB.ImageService();
                imageService.SaveImage(
                    image
                    , strContentDispositionFileName
                    , DB.ImageService.ImageCategory.Slide
                    , string.Empty, DB.ImageService.Rotation.Rotate0
                    , usertoken.CompanyGuid
                    );

                guidImageGuid = imageService.ImageGuid;
            }

            Guid guidSlideGuid = new Guid(id);
            DB.tSlide slide = new DB.SlideRepository().GetSlide(guidSlideGuid);
            if (slide != null && guidImageGuid == Guid.Empty)
            {
                guidImageGuid = slide.ImageGuid;
            }

            if (new DB.SlideService().Save(usertoken.CompanyGuid, guidSlideGuid, strTitle, strDescription, intSortOrder, guidImageGuid, strUrl) != DB.Repository.Status.Success)
            {
                _mSlide.Status = RestStatus.GenericError;
                _mSlide.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Populate
            _mSlide.SlideId = guidSlideGuid.ToString();
            _mSlide.Title = strTitle;
            _mSlide.Description = strDescription;
            _mSlide.SortOrder = intSortOrder;

            _mSlide.ImageUrl = string.Empty;
            DB.tImage theImage = new DB.ImageRepository().GetByImageGuid(guidImageGuid);
            if (theImage != null)
            {
                _mSlide.ImageUrl = string.Format("http://{0}/storage/{1}/slide/{2}/{3}", Request.RequestUri.Authority, usertoken.CompanyGuid.ToString(), guidImageGuid.ToString(), theImage.FileName);
            }

            _mSlide.Url = strUrl;

            // Success
            _mSlide.Status = RestStatus.Success;
            _mSlide.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
        }

        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) || !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSlide.Status = RestStatus.ParameterError;
                _mSlide.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSlide.Status = RestStatus.AuthenticationFailed;
                _mSlide.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Delete
            if (new DB.SlideService().Delete(new Guid(id)) != DB.Repository.Status.Success)
            {
                _mSlide.Status = RestStatus.GenericError;
                _mSlide.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
            }

            // Success
            _mSlide.Status = RestStatus.Success;
            _mSlide.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSlides));
        }


    }
}
