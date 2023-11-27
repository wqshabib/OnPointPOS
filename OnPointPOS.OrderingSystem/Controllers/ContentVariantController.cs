using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ML.Rest2.Controllers
{
    public class ContentVariantController : ApiController
    {
        private List<ContentVariant> _mContentVariants = new List<ContentVariant>();
        private ContentVariant _mContentVariant = new ContentVariant();

        public ContentVariantController()
        {
            _mContentVariants.Add(_mContentVariant);
        }

       /// <summary>
       ///  id = ContentVariantTemplateGuid
       /// </summary>
       /// <param name="secret"></param>
       /// <param name="companyId"></param>
       /// <param name="contentTemplateId"></param>
       /// <returns></returns>
        public HttpResponseMessage Post(string token, string contentId, string contentVariantTemplateId)
        {
            if (!Request.Content.IsFormData())
            {
                _mContentVariant.Status = RestStatus.NotFormData;
                _mContentVariant.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(contentId) && !ML.Common.Text.IsGuidNotEmpty(contentVariantTemplateId))
            {
                _mContentVariant.Status = RestStatus.ParameterError;
                _mContentVariant.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentVariant.Status = RestStatus.AuthenticationFailed;
                _mContentVariant.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            Guid guidContentGuid = new Guid(contentId);
            DB.tContent content = new DB.ContentRepository().GetContent(guidContentGuid);
            if(content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariant.Status = RestStatus.NotExisting;
                _mContentVariant.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            Guid guidContentVariantTemplateGuid = new Guid(contentVariantTemplateId);
            DB.tContentVariantTemplate contentVariantTemplate = new DB.ContentVariantTemplateRepository().GetContentVariantTemplate(guidContentVariantTemplateGuid);
            if (contentVariantTemplate.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariant.Status = RestStatus.NotExisting;
                _mContentVariant.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            if(dic["Price"] == null && dic["DeliveryPrice"] == null)
            {
                _mContentVariant.Status = RestStatus.DataMissing;
                _mContentVariant.StatusText = "Data Missing (Price and DeliveryPrice)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
            }

            // Save
            DB.ContentVariantRepository contentVariantRepository = new DB.ContentVariantRepository();
            DB.tContentVariant contentVariant = contentVariantRepository.GetByContentGuidAndContentVariantTemplateGuid(guidContentGuid, guidContentVariantTemplateGuid);
            if(contentVariant == null)
            {
                contentVariant = new DB.tContentVariant();
                contentVariant.ContentVariantGuid = Guid.NewGuid();
                contentVariant.ContentGuid = guidContentGuid;
                contentVariant.ContentVariantTemplateGuid = guidContentVariantTemplateGuid;
                contentVariant.Price = dic["Price"] == null ? 0 : Convert.ToDecimal(dic["Price"]);
                contentVariant.TimeStamp = DateTime.Now;
                contentVariant.DeliveryPrice = dic["DeliveryPrice"] == null ? 0 : Convert.ToDecimal(dic["DeliveryPrice"]);

                if (contentVariantRepository.Save(contentVariant) != DB.Repository.Status.Success)
                {
                    _mContentVariant.Status = RestStatus.GenericError;
                    _mContentVariant.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
                }
            }
            else
            {
                if (dic["Price"] != null)
                {
                    contentVariant.Price = Convert.ToDecimal(dic["Price"]);
                }
                if (dic["DeliveryPrice"] != null)
                {
                    contentVariant.DeliveryPrice = Convert.ToDecimal(dic["DeliveryPrice"]);
                }

                if (contentVariantRepository.Save() != DB.Repository.Status.Success)
                {
                    _mContentVariant.Status = RestStatus.GenericError;
                    _mContentVariant.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
                }
            }

            // Populate result
            _mContentVariant.ContentVariantId = contentVariant.ContentVariantGuid.ToString();
            _mContentVariant.ContentId = contentVariant.ContentGuid.ToString();
            _mContentVariant.ContentVariantTemplateId = contentVariantTemplate.ContentVariantTemplateGuid.ToString();
            _mContentVariant.Price = contentVariant.Price;
            _mContentVariant.ContentVariantTemplateName = new DB.ContentVariantTemplateRepository().GetContentVariantTemplate(guidContentVariantTemplateGuid).Name;
            _mContentVariant.DeliveryPrice = contentVariant.DeliveryPrice;

            _mContentVariant.Status = RestStatus.Success;
            _mContentVariant.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariants));
        }




    }
}
