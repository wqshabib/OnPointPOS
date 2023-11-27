using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class SubContentController : ApiController
    {
        private List<SubContent> _mSubContents = new List<SubContent>();
        private SubContent _mSubContent = new SubContent();

        public SubContentController()
        {
            _mSubContents.Add(_mSubContent);
        }

        /// <summary>
        /// id = ContentId or ContentTemplateId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContent.Status = RestStatus.ParameterError;
                _mSubContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContent.Status = RestStatus.AuthenticationFailed;
                _mSubContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Prepare
            IQueryable<DB.tContent> contents = null;

            // Check if SubContents
            contents = new DB.ContentRepository().GetSubContentByContentTemplateGuid(new Guid(id));

            // Check if SubContent
            if (!contents.Any())
            {
                contents = new DB.ContentRepository().GetRelevantSubContentByContentGuid(new Guid(id));
            }

            if (!contents.Any())
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (contents.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Populate
            _mSubContents.Clear();
            foreach (DB.tContent content in contents)
            {
                _mSubContent = new SubContent();
                _mSubContent.ContentId = content.ContentGuid.ToString();
                _mSubContent.Name = content.Name;
                _mSubContent.Price = content.Price;
                _mSubContent.ContentSubContentGroupId = content.ContentSubContentGroupGuid.ToString();

                _mSubContent.Status = RestStatus.Success;
                _mSubContent.StatusText = "Success";

                _mSubContents.Add(_mSubContent);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
        }

        /// <summary>
        /// id = ContentTemplateId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mSubContent.Status = RestStatus.FormatError;
                _mSubContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContent.Status = RestStatus.ParameterError;
                _mSubContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }
            Guid guidContentTemplateGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContent.Status = RestStatus.AuthenticationFailed;
                _mSubContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByContentTemplateGuid(guidContentTemplateGuid);
            if (contentTemplate == null)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (contentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            string strXml = string.Format("<xml>{0}</xml>", contentTemplate.XmlContentTemplate).Replace("\r", string.Empty).Replace("\n", string.Empty);

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0) // && provider.FileData.Count == 0)
            {
                _mSubContent.Status = RestStatus.DataMissing;
                _mSubContent.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Ensure Content does not exist.
            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = new DB.tContent();
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    content = contentRepository.GetContentByContentTemplateGuidAndName(guidContentTemplateGuid, strValue);
                    if (content != null)
                    {
                        if (content.SubContent)
                        {
                            _mSubContent.Status = RestStatus.AlreadyExists;
                            _mSubContent.StatusText = "Already Exists";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
                        }
                    }
                }
            }

            // Default values
            content = new DB.tContent();
            content.ContentGuid = Guid.NewGuid();
            content.ContentTemplateGuid = guidContentTemplateGuid;
            content.Description = string.Empty;
            content.Identifier = Guid.NewGuid().ToString();
            content.Active = true;
            content.PublishDateTime = DateTime.Now;
            content.StartDateTime = DateTime.Now;
            content.FreezeDateTime = Convert.ToDateTime("2099-01-01 00:00:00");
            content.TerminationDateTime = Convert.ToDateTime("2099-01-01 00:00:00");
            content.Map = string.Empty;
            content.Price = 0;
            content.DeliveryPrice = 0;
            content.VAT = 0;
            content.SubContent = true;
            content.MaxInCart = -1;
            content.SortOrder = 0;
            content.ContentSubContentGroupGuid = Guid.Empty;
            content.NoOfFreeSubContent = 0;
            content.NoOfFreeSubContentMaxMessage = string.Empty;
            content.OpenNotRedeemable = false;
            content.TimeStamp = DateTime.Now;

            // Handle values
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    content.Name = strValue;
                }
                else if (strKey == "PRICE")
                {   
                    strValue = strValue.Replace('.', ',');
                    content.Price = Convert.ToDecimal(strValue);
                }
                else if (strKey == "SUBCONTENTGROUP")
                {
                    content.ContentSubContentGroupGuid = new Guid(strValue);
                }
                else
                {
                    strXml = ML.Common.XmlHelper.UpdateXml(strXml, strKey, strValue);
                }
            }

            // Handle files... Implement if needed

            // Validate xml
            try
            {
                XmlDocument xmlValidate = new XmlDocument();
                xmlValidate.LoadXml(strXml);
            }
            catch
            {
                _mSubContent.Status = RestStatus.DataError;
                _mSubContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }
            content.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);

            // Save
            if (contentRepository.Save(content) != DB.Repository.Status.Success)
            {
                _mSubContent.Status = RestStatus.GenericError;
                _mSubContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Populate result
            _mSubContent.ContentId = content.ContentGuid.ToString();

            _mSubContent.Status = RestStatus.Success;
            _mSubContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
        }

        /// <summary>
        /// id = SubContentGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="contentTemplateId"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Put(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mSubContent.Status = RestStatus.FormatError;
                _mSubContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContent.Status = RestStatus.ParameterError;
                _mSubContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }
            Guid guidContentGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContent.Status = RestStatus.AuthenticationFailed;
                _mSubContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = contentRepository.GetSubContent(guidContentGuid);
            if (content == null)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            string strXml = string.Format("<xml>{0}</xml>", content.Xml);

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0) // && provider.FileData.Count == 0)
            {
                _mSubContent.Status = RestStatus.DataMissing;
                _mSubContent.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Handle values
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    content.Name = strValue;
                }
                else if (strKey == "PRICE")
                {
                    strValue = strValue.Replace('.', ',');
                    content.Price = Convert.ToDecimal(strValue);
                }
                else if (strKey == "SUBCONTENTGROUP")
                {
                    content.ContentSubContentGroupGuid = new Guid(strValue);
                }
                else
                {
                    strXml = ML.Common.XmlHelper.UpdateXml(strXml, strKey, strValue);
                }
            }

            // Handle files... Implement if needed

            // Validate xml
            try
            {
                XmlDocument xmlValidate = new XmlDocument();
                xmlValidate.LoadXml(strXml);
            }
            catch
            {
                _mSubContent.Status = RestStatus.DataError;
                _mSubContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }
            content.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);

            // Save
            if (contentRepository.Save() != DB.Repository.Status.Success)
            {
                _mSubContent.Status = RestStatus.GenericError;
                _mSubContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Populate result
            _mSubContent.ContentId = content.ContentGuid.ToString();

            _mSubContent.Status = RestStatus.Success;
            _mSubContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
        }

        /// <summary>
        /// id = SubContentGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContent.Status = RestStatus.ParameterError;
                _mSubContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }
            Guid guidContentGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContent.Status = RestStatus.AuthenticationFailed;
                _mSubContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = contentRepository.GetSubContent(guidContentGuid);
            if (content == null)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            if (content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContent.Status = RestStatus.NotExisting;
                _mSubContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            // Delete
            if (new DB.ContentService().DeleteContent(guidContentGuid) != DB.Repository.Status.Success)
            {
                _mSubContent.Status = RestStatus.GenericError;
                _mSubContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
            }

            _mSubContent.Status = RestStatus.Success;
            _mSubContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContents));
        }






    }
}
