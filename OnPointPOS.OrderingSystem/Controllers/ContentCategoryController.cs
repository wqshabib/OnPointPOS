using ML.Common.Handlers.Serializers;
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
    public class ContentCategoryController : ApiController
    {
        private List<ContentCategory> _mContentCategories = new List<ContentCategory>();
        private ContentCategory _mContentCategory = new ContentCategory();
        
        public ContentCategoryController()
        {
            _mContentCategories.Add(_mContentCategory);
        }

        /// <summary>
        /// id = ContentCategoryId or ContentParentCategoryId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id, int imageWidth)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentCategory.Status = RestStatus.ParameterError;
                _mContentCategory.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentCategory.Status = RestStatus.AuthenticationFailed;
                _mContentCategory.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Prepare
            IQueryable<DB.tContentCategory> contentCategories = null;

            // Check if ContentCategory
            contentCategories = new DB.ContentCategoryRepository().GetContentCategory(new Guid(id));
            if(contentCategories.Any())
            {
                // Check if ContentParentCategory
                if(contentCategories.FirstOrDefault().ContentParentCategoryGuid == Guid.Empty)
                {
                    contentCategories = new DB.ContentCategoryRepository().GetByContentParentCategoryGuid(new Guid(id));
                }
            }

            if (!contentCategories.Any())
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            if (contentCategories.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Populate
            _mContentCategories.Clear();
            foreach (DB.tContentCategory contentCategory in contentCategories)
            {
                _mContentCategory = new ContentCategory();
                _mContentCategory.ContentCategoryId = contentCategory.ContentCategoryGuid.ToString();
                _mContentCategory.Name = contentCategory.Name;
                _mContentCategory.Description = contentCategory.Description;
                _mContentCategory.Identifier = contentCategory.Identifier;
                _mContentCategory.SortOrder = contentCategory.SortOrder;
                _mContentCategory.OrderPrinterAvailabilityTypeID = contentCategory.OrderPrinterAvailabilityTypeID;
                _mContentCategory.Orderable = contentCategory.Orderable;

                List<ML.Rest2.Models.ContentCategoryCustom> contentCategoryCustoms = new List<ContentCategoryCustom>();
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(ML.Common.XmlHelper.AddRootTags(contentCategory.Xml));
                foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("root"))
                {
                    ML.Rest2.Models.ContentCategoryCustom contentCategoryCustom = new ContentCategoryCustom();
                    contentCategoryCustom.Name = xmlNode.Name;
                    contentCategoryCustom.Type = xmlNode.Attributes["type"] == null ? string.Empty : xmlNode.Attributes["type"].InnerText;
                    if (contentCategoryCustom.Type == "image")
                    {
                        //if (xmlNode.SelectSingleNode("name") != null)
                        //{
                        //    string strFileName = xmlNode.SelectSingleNode("name").InnerText;
                        //    string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
                        //    ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", usertoken.CompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                        //    contentCategoryCustom.Value = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, usertoken.CompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                        //}
                        //else
                        //{
                        //    contentCategoryCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                        //}
                    }

                    else
                    {
                        contentCategoryCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                    }

                    contentCategoryCustoms.Add(contentCategoryCustom);
                }

                _mContentCategory.ContentCategoryCustom = contentCategoryCustoms;

                _mContentCategories.Add(_mContentCategory);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
        }

        /// <summary>
        /// id = ContentTemplateGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Post(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mContentCategory.Status = RestStatus.FormatError;
                _mContentCategory.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentCategory.Status = RestStatus.ParameterError;
                _mContentCategory.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentCategory.Status = RestStatus.AuthenticationFailed;
                _mContentCategory.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.tContentCategory contentCategory = new DB.ContentCategoryRepository().GetByContentCategoryGuid(new Guid(id));
            if(contentCategory.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }
            Guid guidContentTemplateGuid = contentCategory.tContentTemplate.ContentTemplateGuid;

            string strXml = string.Format("<xml>{0}</xml>", contentCategory.tContentTemplate.XmlContentTemplate).Replace("\r", string.Empty).Replace("\n", string.Empty);

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mContentCategory.Status = RestStatus.DataMissing;
                _mContentCategory.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Ensure ContentCategory does not exist.
            DB.ContentCategoryRepository contentCategoryRepository = new DB.ContentCategoryRepository();
            contentCategory = new DB.tContentCategory();
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    contentCategory = contentCategoryRepository.GetByContentTemplateGuidAndName(guidContentTemplateGuid, strValue);
                    if (contentCategory != null)
                    {
                        _mContentCategory.Status = RestStatus.AlreadyExists;
                        _mContentCategory.StatusText = "Already Exists";
                        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
                    }
                }
                else if (strKey == "IDENTIFIER")
                {
                    contentCategory = contentCategoryRepository.GetContentCategory(guidContentTemplateGuid, strValue);
                    if (contentCategory != null)
                    {
                        _mContentCategory.Status = RestStatus.AlreadyExists;
                        _mContentCategory.StatusText = "Already Exists";
                        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
                    }
                }
            }

            // Default values
            contentCategory = new DB.tContentCategory();
            contentCategory.ContentCategoryGuid = Guid.NewGuid();
            contentCategory.ContentParentCategoryGuid = new Guid(id);
            contentCategory.ContentTemplateGuid = guidContentTemplateGuid;
            contentCategory.Description = string.Empty;
            contentCategory.Identifier = string.Empty;
            contentCategory.Global = false;
            contentCategory.Private = false;
            contentCategory.StaticFilter = false;
            contentCategory.DefaultFilter = false;
            contentCategory.ContentCategoryLinkGuid = Guid.Empty;
            contentCategory.Active = true;
            contentCategory.Map = string.Empty;
            contentCategory.SortOrder = 0;
            contentCategory.TimeStamp = DateTime.Now;
            contentCategory.OrderPrinterAvailabilityTypeID = 0;
            contentCategory.Orderable = true;

            List<string> contentRelativeCategories = new List<string>();
            string strLinkContentTemplateGuid = string.Empty;
            string strLinkIdentifier = string.Empty;

            // Handle values
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key;
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey.ToUpper() == "NAME")
                {
                    contentCategory.Name = strValue;
                }
                else if (strKey.ToUpper() == "DESCRIPTION")
                {
                    contentCategory.Description = strValue;
                }
                else if (strKey.ToUpper() == "IDENTIFIER")
                {
                    contentCategory.Identifier = strValue;
                }
                else if (strKey.ToUpper() == "GLOBAL")
                {
                    contentCategory.Global = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "PRIVATE")
                {
                    contentCategory.Private = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "STATICFILTER")
                {
                    contentCategory.StaticFilter = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "DEFAULTFILTER")
                {
                    contentCategory.DefaultFilter = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "LINKCONTENTTEMPLATEID")
                {
                    strLinkContentTemplateGuid = strValue;
                }
                else if (strKey.ToUpper() == "LINKIDENTIFIER")
                {
                    strLinkIdentifier = strValue;
                }
                else if (strKey.ToUpper() == "ACTIVE")
                {
                    contentCategory.Active = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "SORTORDER")
                {
                    contentCategory.SortOrder = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "MAP")
                {
                    contentCategory.Map = string.Empty;
                    List<string> lstMap = strValue.Split('|').ToList();
                    foreach (string strMap in lstMap)
                    {
                        string strLatLng = string.Empty;
                        if (ML.Geo.MapHelper.IsLatLong(strMap))
                        {
                            strLatLng = strMap;
                        }
                        else
                        {
                            strLatLng = ML.Geo.GoogleGeocodingAPI.ConvertToLatLng(strMap);
                        }
                        if (!string.IsNullOrEmpty(strLatLng))
                        {
                            if (strMap != lstMap[0])
                            {
                                contentCategory.Map += "|";
                            }
                            contentCategory.Map += strLatLng;
                        }
                    }
                }
                else if (strKey.ToUpper() == "RELATIVECATEGORIES")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    contentRelativeCategories = strValue.Split(',').ToList();
                }
                else if (strKey.ToUpper() == "ORDERPRINTERAVAILABILITYTYPE")
                {
                    contentCategory.OrderPrinterAvailabilityTypeID = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "ORDERABLE")
                {
                    if (strValue == "1" || strValue == "0")
                    {
                        contentCategory.Orderable = strValue == "1" ? true : false;
                    }
                    else
                    {
                        contentCategory.Orderable = Convert.ToBoolean(strValue);
                    }
                }
                // Cont...
                else
                {
                    strXml = ML.Common.XmlHelper.UpdateXml(strXml, strKey, strValue);
                }
            }


            // Handle files
            foreach (MultipartFileData file in provider.FileData)
            {
                string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                string strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                if (ML.Common.XmlHelper.NodeExists(strXml, strContentDispositionName))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file.LocalFileName);

                    // Save to imagebank
                    DB.ImageService imageService = new DB.ImageService();
                    imageService.SaveImage(
                        image
                        , strContentDispositionFileName
                        , DB.ImageService.ImageCategory.ContentCategory
                        , string.Empty, DB.ImageService.Rotation.Rotate0
                        , usertoken.CompanyGuid
                        );

                    // Prepare for content
                    string strNodeTag = ML.Common.XmlHelper.GetNodeAttributeValue(strXml, strContentDispositionName);
                    if (strNodeTag == "image")
                    {
                        strXml = ML.Common.XmlHelper.UpdateXml(strXml, strContentDispositionName, "name", strContentDispositionFileName, false);
                        strXml = ML.Common.XmlHelper.UpdateXml(strXml, strContentDispositionName, "imageguid", imageService.ImageGuid.ToString(), false);
                    }
                }
            }

            // Validate xml
            try
            {
                XmlDocument xmlValidate = new XmlDocument();
                xmlValidate.LoadXml(strXml);
            }
            catch
            {
                _mContentCategory.Status = RestStatus.DataError;
                _mContentCategory.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }
            contentCategory.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);


            string strRelativeCategories = string.Empty;
            foreach (string str in contentRelativeCategories)
            {
                strRelativeCategories += ",";
                strRelativeCategories += str;
            }

            // Attach Content Relative Categories
            new DB.ContentRelativeCategoryRepository().DeleteContentRelativeCategory(contentCategory.ContentCategoryGuid);
            foreach (string strContentRelativeCategory in contentRelativeCategories)
            {
                DB.tContentCategory contentRelativeCategory = new DB.ContentCategoryRepository().GetByContentTemplateGuidAndIdentifier(guidContentTemplateGuid, strContentRelativeCategory);
                if (contentRelativeCategory != null)
                {
                    if (contentRelativeCategory.ContentParentCategoryGuid == Guid.Empty)
                    {
                        //new ML.Content.ContentRelativeCategoryTableAdapters.tContentRelativeCategoryTableAdapter().InsertContentRelativeCategory(contentRelativeCategory.ContentCategoryGuid, guidContentCategoryGuid);
                        //TODO
                        //////////////contentRepository.Ctx.tContentCategory.Attach(contentCategory);
                        //////////////content.tContentCategory.Add(contentCategory);
                    }
                }
            }

            // Save
            if (contentCategoryRepository.Save(contentCategory) != DB.Repository.Status.Success)
            {
                _mContentCategory.Status = RestStatus.GenericError;
                _mContentCategory.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Populate result
            _mContentCategory.ContentCategoryId = contentCategory.ContentCategoryGuid.ToString();
            //_mContentCategory.Name = contentCategory.Name;
            //_mContentCategory.Description = contentCategory.Description;
            //_mContentCategory.Identifier = contentCategory.Identifier;
            //_mContentCategory.SortOrder = contentCategory.SortOrder;

            _mContentCategory.Status = RestStatus.Success;
            _mContentCategory.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
        }

        /// <summary>
        /// id = ContentCategoryId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Put(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mContentCategory.Status = RestStatus.FormatError;
                _mContentCategory.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentCategory.Status = RestStatus.ParameterError;
                _mContentCategory.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentCategory.Status = RestStatus.AuthenticationFailed;
                _mContentCategory.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.ContentCategoryRepository contentCategoryRepository = new DB.ContentCategoryRepository();
            DB.tContentCategory contentCategory = contentCategoryRepository.GetByContentCategoryGuid(new Guid(id));
            if (contentCategory == null)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            if (contentCategory.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }
            Guid guidContentTemplateGuid = contentCategory.tContentTemplate.ContentTemplateGuid;

            string strXml = string.Format("<xml>{0}</xml>", contentCategory.tContentTemplate.XmlContentTemplate).Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (contentCategory.ContentParentCategoryGuid == Guid.Empty)
            {
                _mContentCategory.Status = RestStatus.AccessError;
                _mContentCategory.StatusText = "Access Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mContentCategory.Status = RestStatus.DataMissing;
                _mContentCategory.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            List<string> contentRelativeCategories = new List<string>();
            string strLinkContentTemplateGuid = string.Empty;
            string strLinkIdentifier = string.Empty;

            // Handle values
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key;
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey.ToUpper() == "NAME")
                {
                    contentCategory.Name = strValue;
                }
                else if (strKey.ToUpper() == "DESCRIPTION")
                {
                    contentCategory.Description = strValue;
                }
                else if (strKey.ToUpper() == "IDENTIFIER")
                {
                    contentCategory.Identifier = strValue;
                }
                else if (strKey.ToUpper() == "GLOBAL")
                {
                    contentCategory.Global = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "PRIVATE")
                {
                    contentCategory.Private = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "STATICFILTER")
                {
                    contentCategory.StaticFilter = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "DEFAULTFILTER")
                {
                    contentCategory.DefaultFilter = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "LINKCONTENTTEMPLATEID")
                {
                    strLinkContentTemplateGuid = strValue;
                }
                else if (strKey.ToUpper() == "LINKIDENTIFIER")
                {
                    strLinkIdentifier = strValue;
                }
                else if (strKey.ToUpper() == "ACTIVE")
                {
                    contentCategory.Active = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "SORTORDER")
                {
                    contentCategory.SortOrder = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "MAP")
                {
                    contentCategory.Map = string.Empty;
                    List<string> lstMap = strValue.Split('|').ToList();
                    foreach (string strMap in lstMap)
                    {
                        string strLatLng = string.Empty;
                        if (ML.Geo.MapHelper.IsLatLong(strMap))
                        {
                            strLatLng = strMap;
                        }
                        else
                        {
                            strLatLng = ML.Geo.GoogleGeocodingAPI.ConvertToLatLng(strMap);
                        }
                        if (!string.IsNullOrEmpty(strLatLng))
                        {
                            if (strMap != lstMap[0])
                            {
                                contentCategory.Map += "|";
                            }
                            contentCategory.Map += strLatLng;
                        }
                    }
                }
                else if (strKey.ToUpper() == "RELATIVECATEGORIES")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    contentRelativeCategories = strValue.Split(',').ToList();
                }
                else if (strKey.ToUpper() == "ORDERPRINTERAVAILABILITYTYPE")
                {
                    contentCategory.OrderPrinterAvailabilityTypeID = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "ORDERABLE")
                {
                    if (strValue == "1" || strValue == "0")
                    {
                        contentCategory.Orderable = strValue == "1" ? true : false;
                    }
                    else
                    {
                        contentCategory.Orderable = Convert.ToBoolean(strValue);
                    }
                }
                // Cont...
                else
                {
                    strXml = ML.Common.XmlHelper.UpdateXml(strXml, strKey, strValue);
                }
            }


            // Handle files
            foreach (MultipartFileData file in provider.FileData)
            {
                string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                string strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                if (ML.Common.XmlHelper.NodeExists(strXml, strContentDispositionName))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromFile(file.LocalFileName);

                    // Save to imagebank
                    DB.ImageService imageService = new DB.ImageService();
                    imageService.SaveImage(
                        image
                        , strContentDispositionFileName
                        , DB.ImageService.ImageCategory.ContentCategory
                        , string.Empty, DB.ImageService.Rotation.Rotate0
                        , usertoken.CompanyGuid
                        );

                    // Prepare for content
                    string strNodeTag = ML.Common.XmlHelper.GetNodeAttributeValue(strXml, strContentDispositionName);
                    if (strNodeTag == "image")
                    {
                        strXml = ML.Common.XmlHelper.UpdateXml(strXml, strContentDispositionName, "name", strContentDispositionFileName, false);
                        strXml = ML.Common.XmlHelper.UpdateXml(strXml, strContentDispositionName, "imageguid", imageService.ImageGuid.ToString(), false);
                    }
                }
            }

            // Validate xml
            try
            {
                XmlDocument xmlValidate = new XmlDocument();
                xmlValidate.LoadXml(strXml);
            }
            catch
            {
                _mContentCategory.Status = RestStatus.DataError;
                _mContentCategory.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }
            contentCategory.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);


            string strRelativeCategories = string.Empty;
            foreach (string str in contentRelativeCategories)
            {
                strRelativeCategories += ",";
                strRelativeCategories += str;
            }

            // Attach Content Relative Categories
            new DB.ContentRelativeCategoryRepository().DeleteContentRelativeCategory(contentCategory.ContentCategoryGuid);
            foreach (string strContentRelativeCategory in contentRelativeCategories)
            {
                DB.tContentCategory contentRelativeCategory = new DB.ContentCategoryRepository().GetByContentTemplateGuidAndIdentifier(guidContentTemplateGuid, strContentRelativeCategory);
                if (contentRelativeCategory != null)
                {
                    if (contentRelativeCategory.ContentParentCategoryGuid == Guid.Empty)
                    {
                        //new ML.Content.ContentRelativeCategoryTableAdapters.tContentRelativeCategoryTableAdapter().InsertContentRelativeCategory(contentRelativeCategory.ContentCategoryGuid, guidContentCategoryGuid);
                        //TODO
                        //////////////contentRepository.Ctx.tContentCategory.Attach(contentCategory);
                        //////////////content.tContentCategory.Add(contentCategory);
                    }
                }
            }

            // Save
            if (contentCategoryRepository.Save() != DB.Repository.Status.Success)
            {
                _mContentCategory.Status = RestStatus.GenericError;
                _mContentCategory.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Populate result
            _mContentCategory.ContentCategoryId = contentCategory.ContentCategoryGuid.ToString();
            //_mContentCategory.Name = contentCategory.Name;
            //_mContentCategory.Description = contentCategory.Description;
            //_mContentCategory.Identifier = contentCategory.Identifier;
            //_mContentCategory.SortOrder = contentCategory.SortOrder;

            _mContentCategory.Status = RestStatus.Success;
            _mContentCategory.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
        }

        /// <summary>
        /// id = ContentCategoryId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentCategory.Status = RestStatus.ParameterError;
                _mContentCategory.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentCategory.Status = RestStatus.AuthenticationFailed;
                _mContentCategory.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            DB.ContentCategoryRepository contentCategoryRepository = new DB.ContentCategoryRepository();
            DB.tContentCategory contentCategory = contentCategoryRepository.GetByContentCategoryGuid(new Guid(id));
            if (contentCategory == null)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            if (contentCategory.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentCategory.Status = RestStatus.NotExisting;
                _mContentCategory.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }
            Guid guidContentTemplateGuid = contentCategory.tContentTemplate.ContentTemplateGuid;

            if (contentCategory.ContentParentCategoryGuid == Guid.Empty)
            {
                _mContentCategory.Status = RestStatus.AccessError;
                _mContentCategory.StatusText = "Access Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            // Delete
            if(new DB.ContentCategoryService().DeleteContentCateogry(new Guid(id)) != DB.Repository.Status.Success)
            {
                _mContentCategory.Status = RestStatus.GenericError;
                _mContentCategory.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
            }

            _mContentCategory.Status = RestStatus.Success;
            _mContentCategory.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategories));
        }



    }
}