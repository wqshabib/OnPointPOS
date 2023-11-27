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
    public class ContentController : ApiController
    {
        private List<Content> _mContents = new List<Content>();
        private Content _mContent = new Content();

        public ContentController()
        {
            _mContents.Add(_mContent);
        }

        public HttpResponseMessage Get(string token, string id)
        {
            List<ContentList> mContentLists = new List<ContentList>();
            ContentList mContentList = new ContentList();

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mContentList.Status = RestStatus.ParameterError;
                mContentList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentLists));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mContentList.Status = RestStatus.AuthenticationFailed;
                mContentList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentLists));
            }

            // Prepare
            IQueryable<DB.tContent> contents = null;

            // Check if ContentCategory
            contents = new DB.ContentRepository().GetRelevantByContentCategoryGuid(new Guid(id));

            // Check if ContentTemplate
            if (!contents.Any())
            {
                contents = new DB.ContentRepository().GetRelevantByContentTemplateGuid(new Guid(id));
            }

            if (!contents.Any())
            {
                mContentList.Status = RestStatus.NotExisting;
                mContentList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentLists));
            }

            if (contents.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                mContentList.Status = RestStatus.NotExisting;
                mContentList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentLists));
            }

            // Populate
            mContentLists.Clear();
            foreach (DB.tContent content in contents)
            {
                mContentList = new ContentList();
                mContentList.Name = content.Name;
                mContentList.ContentId = content.ContentGuid.ToString();
                mContentLists.Add(mContentList);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentLists));
        }


        /// <summary>
        /// id = ContentId or ContentCategoryId or ContentTemplateId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <param name="imageWidth"></param>
        /// <returns></returns>
        /// 

        public HttpResponseMessage Get(string token, string id, int imageWidth)
        {
            return Get(token, id, imageWidth, 0);
        }

        public HttpResponseMessage Get(string token, string id, int imageWidth, int listType)
        {
            List<dynamic> mResults = new List<dynamic>();
            dynamic mResult = new ExpandoObject();
            mResults.Add(mResult);

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mResult.Status = RestStatus.ParameterError;
                mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mResult.Status = RestStatus.AuthenticationFailed;
                mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            // Prepare
            IQueryable<DB.tContent> contents = null;

            // Check if Content
            contents = new DB.ContentRepository().GetRelevantByContentGuid(new Guid(id));

            // Check if ContentCategory
            if (!contents.Any())
            {
                contents = new DB.ContentRepository().GetRelevantByContentCategoryGuid(new Guid(id));

                // Check if ContentTemplate
                if (!contents.Any())
                {
                    contents = new DB.ContentRepository().GetRelevantByContentTemplateGuid(new Guid(id));
                }
            }

            if (listType > 0)
            {
                contents = new DB.ContentRepository().GetByContentListType(new Guid(id), (DB.ContentRepository.ContentListType)listType, contents);
            }



            if (!contents.Any())
            {
                mResult.Status = RestStatus.NotExisting;
                mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            if (contents.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                mResult.Status = RestStatus.NotExisting;
                mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            // Populate
            _mContents.Clear();
            foreach (DB.tContent content in contents)
            {
                _mContent = new Content();
                _mContent.ContentId = content.ContentGuid.ToString();
                _mContent.Name = content.Name;
                _mContent.Description = content.Description;
                _mContent.Active = content.Active;
                _mContent.PublishDateTime = content.PublishDateTime;
                _mContent.StartDateTime = content.StartDateTime;
                _mContent.FreezeDateTime = content.FreezeDateTime;
                _mContent.TerminationDateTime = content.TerminationDateTime;
                _mContent.Map = content.Map;
                _mContent.Identifier = content.Identifier;
                _mContent.NoOfFreeSubContent = content.NoOfFreeSubContent;
                _mContent.OpenNotRedeemable = content.OpenNotRedeemable;
                _mContent.ContentType = content.tContentType.ContentTypeGuid.ToString();
                _mContent.DiscountPercent = content.DiscountPercent;
                _mContent.DiscountAmount = content.DiscountAmount;
                _mContent.SortOrder = content.SortOrder;
                _mContent.Scale = content.Scale == null ? 0 : (int)content.Scale;

                if(!string.IsNullOrEmpty(content.tContentTemplate.PreviewUrl))
                {
                    string strPreviewUrl = content.tContentTemplate.PreviewUrl;
                    if (strPreviewUrl.IndexOf("[IDENTIFIER]") > -1)
                    {
                        strPreviewUrl = ML.Common.Text.ReplaceEx(strPreviewUrl, "[IDENTIFIER]", content.Identifier);
                    }
                    else if (strPreviewUrl.IndexOf("[GUID]") > -1)
                    {
                        strPreviewUrl = ML.Common.Text.ReplaceEx(strPreviewUrl, "[GUID]", content.ContentGuid.ToString());
                    }

                    if (strPreviewUrl.IndexOf("[BASE64:DATE]") > -1)
                    {
                        strPreviewUrl = ML.Common.Text.ReplaceEx(strPreviewUrl, "[BASE64:DATE]", ML.Common.StringHelper.EncodeTo64(DateTime.Today.ToShortDateString(), true));
                    }
                    List<PreviewUrl> lstPreviewUrl = new List<PreviewUrl>();
                    PreviewUrl previewUrl1 = new PreviewUrl();
                    previewUrl1.Name = content.tContentTemplate.tCompany.Name;
                    previewUrl1.Url = strPreviewUrl;
                    lstPreviewUrl.Add(previewUrl1);

                    // Handle Corporate content sibling
                    if (content.tContentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
                    {
                        if (ML.Common.Text.IsGuidNotEmpty(content.CorporateSiblingGuid))
                        {
                            if(contents.Count() == 1)
                            {
                                IQueryable<DB.tContent> contentSiblings = new DB.ContentRepository().GetByCorporateSiblingGuid((Guid)content.CorporateSiblingGuid);
                                foreach (DB.tContent item in contentSiblings)
                                {
                                    if(item.ContentGuid != content.ContentGuid)
                                    {
                                        string strPreviewUrl2 = item.tContentTemplate.PreviewUrl;
                                        if (strPreviewUrl2.IndexOf("[IDENTIFIER]") > -1)
                                        {
                                            strPreviewUrl2 = ML.Common.Text.ReplaceEx(strPreviewUrl2, "[IDENTIFIER]", item.Identifier);
                                        }
                                        else if (strPreviewUrl2.IndexOf("[GUID]") > -1)
                                        {
                                            strPreviewUrl2 = ML.Common.Text.ReplaceEx(strPreviewUrl2, "[GUID]", item.ContentGuid.ToString());
                                        }

                                        if (strPreviewUrl2.IndexOf("[BASE64:DATE]") > -1)
                                        {
                                            strPreviewUrl2 = ML.Common.Text.ReplaceEx(strPreviewUrl2, "[BASE64:DATE]", ML.Common.StringHelper.EncodeTo64(DateTime.Today.ToShortDateString(), true));
                                        }
                                        PreviewUrl previewUrl2 = new PreviewUrl();
                                        previewUrl2.Name = item.tContentTemplate.tCompany.Name;
                                        previewUrl2.Url = strPreviewUrl2;
                                        lstPreviewUrl.Add(previewUrl2);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    _mContent.PreviewUrl = lstPreviewUrl;
                }
                

                if (content.tContentTemplate.Monetary)
                {
                    _mContent.Price = content.Price;
                    _mContent.DeliveryPrice = content.DeliveryPrice;
                    _mContent.VAT = content.VAT;
                }
                else
                {
                    _mContent.Price = 0;
                    _mContent.DeliveryPrice = 0;
                    _mContent.VAT = 0;
                }

                List<ML.Rest2.Models.ContentCustom> contentCustoms = new List<ContentCustom>();
                XmlDocument xmlDocument = new XmlDocument();
                
                xmlDocument.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
                foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("root"))
                {
                    ML.Rest2.Models.ContentCustom contentCustom = new ContentCustom();
                    contentCustom.Name = xmlNode.Name;
                    contentCustom.Type = xmlNode.Attributes["type"] == null ? string.Empty : xmlNode.Attributes["type"].InnerText;
                    contentCustom.Label = xmlNode.Attributes["label"] == null ? string.Empty : xmlNode.Attributes["label"].InnerText;
                    contentCustom.Attributes = new Dictionary<string, string>();
                    if (contentCustom.Type == "image")
                    {
                        if (xmlNode.SelectSingleNode("name") != null)
                        {
                            string strFileName = xmlNode.SelectSingleNode("name").InnerText;
                            string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
                            ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", usertoken.CompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                            contentCustom.Value = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, usertoken.CompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                        }
                        else
                        {
                            contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                        }
                    }
                    else if (contentCustom.Type == "text")
                    {
                        // <Namn type="text" label="Namn" maxlength="25" ></Namn>
                        contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                        contentCustom.Attributes.Add("maxlength", xmlNode.Attributes["maxlength"] == null ? string.Empty : xmlNode.Attributes["maxlength"].InnerText);
                    }
                    else if (contentCustom.Type == "textarea")
                    {
                        // <Namn type="text" label="Namn" maxlength="25" rows="5" ></Namn>
                        contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                        contentCustom.Attributes.Add("maxlength", xmlNode.Attributes["maxlength"] == null ? string.Empty : xmlNode.Attributes["maxlength"].InnerText);
                        contentCustom.Attributes.Add("rows", xmlNode.Attributes["rows"] == null ? string.Empty : xmlNode.Attributes["rows"].InnerText);
                    }
                    // Cont...
                    else
                    {
                        contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
                    }

                    contentCustoms.Add(contentCustom);
                }


                // Prepare xml document
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));

                // Convert xml to objects   
                List<ExpandoObject> expandoObjects = ML.Common.XmlHelper.ConvertXmlToObjects(xml, true);

                // Populate
                List<dynamic> dContents = new List<dynamic>();
                foreach (dynamic expandoObject in expandoObjects)
                {
                    if (expandoObject.type == "image")
                    {
                        XmlNode xmlNodeImage = xml.SelectSingleNode("root").SelectSingleNode(expandoObject.name);
                        if (xmlNodeImage.SelectSingleNode("name") != null)
                        {
                            string strFileName = xmlNodeImage.SelectSingleNode("name").InnerText;
                            string strImageGuid = xmlNodeImage.SelectSingleNode("imageguid").InnerText;
                            ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", usertoken.CompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                            expandoObject.value = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, usertoken.CompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                        }
                        else
                        {
                            expandoObject.value = string.Empty;
                        }
                    }

                    dContents.Add(expandoObject);
                }

                // Add ContentListType
                _mContent.ContentListType = new DB.ContentService().GetContentListType(content);


                // Handle Corporate Siblings
                if (content.tContentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
                {
                    if (ML.Common.Text.IsGuidNotEmpty(content.CorporateSiblingGuid))
                    {
                        //if (contents.Count() == 1)
                        //{
                            List<Company> lstCompanies = new List<Company>();
                            IQueryable<DB.tContent> contentSiblings = new DB.ContentRepository().GetByCorporateSiblingGuid((Guid)content.CorporateSiblingGuid);

                            foreach (DB.tContent contentSibling in contentSiblings)
                            {
                                Company mCompany = new Company();
                                mCompany.Name = contentSibling.tContentTemplate.tCompany.Name;
                                mCompany.CompanyId = contentSibling.tContentTemplate.CompanyGuid.ToString();
                                mCompany.Selected = (bool)contentSibling.CorporateSiblingEnabled;
                                lstCompanies.Add(mCompany);
                            }
                            _mContent.Companies = lstCompanies;
                        //}
                    }
                }

                // Handle Content Push
                List<ContentPush> lstContentPush = new List<ContentPush>();
                foreach(DB.tContentPush contentPush in content.tContentPush.OrderBy(cp => cp.PushDateTime))
                {
                    ContentPush mContentPush = new ContentPush();
                    mContentPush.ContentPushId = contentPush.ContentPushGuid;
                    mContentPush.Message = contentPush.Message;
                    mContentPush.PushDateTime = contentPush.PushDateTime;
                    lstContentPush.Add(mContentPush);
                }
                _mContent.ContentPush = lstContentPush;

                // Final
                _mContent.ContentCustom = contentCustoms;
                _mContent.ContentCategory = new ContentCategoryHelper().AttachContentCategories(usertoken.CompanyGuid, content, imageWidth);
                _mContent.ContentVariant = new ContentVariantHelper().AttachContentVariants(content);
                _mContent.ContentSubContent = new ContentSubContentHelper().AttachContentSubContents(content);
                _mContent.ContentDynamic = dContents;

                _mContents.Add(_mContent);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
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
                _mContent.Status = RestStatus.FormatError;
                _mContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContent.Status = RestStatus.ParameterError;
                _mContent.StatusText = "Parameter Error 1";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            Guid guidContentTemplateGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContent.Status = RestStatus.AuthenticationFailed;
                _mContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByContentTemplateGuid(guidContentTemplateGuid);
            if (contentTemplate == null)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            
            if (contentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            string strXml = string.Format("<xml>{0}</xml>", contentTemplate.XmlContentTemplate).Replace("\r", string.Empty).Replace("\n", string.Empty);

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mContent.Status = RestStatus.DataMissing;
                _mContent.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Validate template
            XmlDocument xmlTemplate = new XmlDocument();
            try
            {
                xmlTemplate.LoadXml(ML.Common.XmlHelper.AddRootTags(contentTemplate.XmlContentTemplate));
            }
            catch
            {
                _mContent.Status = RestStatus.DataError;
                _mContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Ensure Content does not exist.
            string strName = string.Empty;
            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = new DB.tContent();
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    strName = strValue;
                    content = contentRepository.GetContentByContentTemplateGuidAndName(guidContentTemplateGuid, strValue);
                    if (content != null)
                    {
                        if (!content.SubContent)
                        {
                            //_mContent.Status = RestStatus.AlreadyExists;
                            //_mContent.StatusText = "Already Exists";
                            //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
                else if (strKey == "IDENTIFIER")
                {
                    content = contentRepository.GetContent(guidContentTemplateGuid, strValue);
                    if (content != null)
                    {
                        if (!content.SubContent)
                        {
                            //_mContent.Status = RestStatus.AlreadyExists;
                            //_mContent.StatusText = "Already Exists";
                            //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
            }

            foreach(XmlNode xmlNode in xmlTemplate.SelectSingleNode("root"))
            {
                if (xmlNode.Attributes["issystemname"] != null)
                {
                    if (xmlNode.Attributes["issystemname"].Value == "1")
                    {
                        if (provider.FormData.Get(xmlNode.Name) != null)
                        {
                            strName = provider.FormData.Get(xmlNode.Name);
                            content = contentRepository.GetContentByContentTemplateGuidAndName(guidContentTemplateGuid, provider.FormData.Get(xmlNode.Name));
                            if (content != null)
                            {
                                //_mContent.Status = RestStatus.AlreadyExists;
                                //_mContent.StatusText = "Already Exists";
                                //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                            }
                        }
                    }
                }
            }
            
            // Ensure name is present
            if (string.IsNullOrEmpty(strName))
            {
                _mContent.Status = RestStatus.ParameterError;
                _mContent.StatusText = "Parameter Error 2";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }


            // Default values
            content = new DB.tContent();
            content.ContentGuid = Guid.NewGuid();
            content.ContentTemplateGuid = guidContentTemplateGuid;
            content.Name = strName;
            content.Description = string.Empty;
            content.Identifier = Guid.NewGuid().ToString();

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)contentTemplate.CompanyTypeID;
            if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                content.Active = false;
            }
            else
            {
                content.Active = true;
            }
            
            content.PublishDateTime = DateTime.Now;
            content.StartDateTime = DateTime.Now;
            content.FreezeDateTime = Convert.ToDateTime("2099-01-01 00:00:00");
            content.TerminationDateTime = Convert.ToDateTime("2099-01-01 00:00:00");
            content.Map = string.Empty;
            content.Price = 0;
            content.DeliveryPrice = 0;
            content.VAT = 0;
            content.SubContent = false;
            content.MaxInCart = -1;
            content.SortOrder = 0;
            content.ContentSubContentGroupGuid = Guid.Empty;
            content.NoOfFreeSubContent = 0;
            content.NoOfFreeSubContentMaxMessage = string.Empty;
            content.OpenNotRedeemable = false;
            content.TimeStamp = DateTime.Now;
            content.ContentTypeGuid = Guid.Empty;
            content.DiscountPercent = 0;
            content.DiscountAmount = 0;

            // Handle values
            List<string> contentCategories = new List<string>();
            List<string> availableSubContents = new List<string>();
            List<string> includedSubContents = new List<string>();
            List<string> companies = new List<string>();
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key;
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                //if (strKey.ToUpper() == "NAME")
                //{
                //    content.Name = strValue;
                //}
                if (strKey.ToUpper() == "DESCRIPTION")
                {
                    content.Description = strValue;
                }
                else if (strKey.ToUpper() == "IDENTIFIER")
                {
                    content.Identifier = strValue;
                }
                else if (strKey.ToUpper() == "ACTIVE")
                {
                    content.Active = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "PUBLISHDATETIME")
                {
                    content.PublishDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "STARTDATETIME")
                {
                    content.StartDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "FREEZEDATETIME")
                {
                    content.FreezeDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "TERMINATIONDATETIME")
                {
                    content.TerminationDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "MAP")
                {
                    content.Map = string.Empty;
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
                                content.Map += "|";
                            }
                            content.Map += strLatLng;
                        }
                    }
                }
                else if (strKey.ToUpper() == "PRICE")
                {
                    strValue = strValue.Replace('.', ',');
                    content.Price = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "DELIVERYPRICE")
                {
                    strValue = strValue.Replace('.', ',');
                    content.DeliveryPrice = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "VAT")
                {
                    strValue = strValue.Replace('.', ',');
                    content.VAT = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "SORTORDER")
                {
                    content.SortOrder = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "OPENNOTREDEEMABLE")
                {
                    content.OpenNotRedeemable = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "NOOFFREESUBCONTENT")
                {
                    content.NoOfFreeSubContent = Convert.ToInt32(strValue);
                }
                // Static Cont...
                else if (strKey.ToUpper() == "CATEGORIES")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    contentCategories = strValue.Split(',').ToList();

                    foreach (string strContentCategory in contentCategories)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strContentCategory))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error 3";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }

                    if (contentTemplate.ContentTemplateGuid == new Guid("3C1A0D40-304C-4430-BF51-1D1EAEDD43F7"))
                    {
                        if (!contentCategories.Contains("83AB243A-DE06-4C17-9776-0CC8DE94808E"))
                        {
                            contentCategories.Add("83AB243A-DE06-4C17-9776-0CC8DE94808E");
                        }
                    }
                    //else if(contentTemplate.ContentTemplateGuid == new Guid("C44F3ED8-250D-4481-BD24-0D6A95B58FB2"))
                    //{
                    //}
                }
                else if (strKey.ToUpper() == "AVAILABLESUBCONTENTS")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    availableSubContents = strValue.Split(',').ToList();

                    foreach (string strAvailableSubContent in availableSubContents)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strAvailableSubContent))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error 4";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
                else if (strKey.ToUpper() == "INCLUDEDSUBCONTENTS")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    includedSubContents = strValue.Split(',').ToList();

                    foreach (string strIncludedSubContent in includedSubContents)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strIncludedSubContent))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error 5";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
                else if (strKey.ToUpper() == "CONTENTTYPE")
                {
                    content.ContentTypeGuid = new Guid(strValue);
                }
                else if (strKey.ToUpper() == "DISCOUNTPERCENT")
                {
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace('.', ',');
                        content.DiscountPercent = Convert.ToDecimal(strValue);
                    }
                }
                else if (strKey.ToUpper() == "DISCOUNTAMOUNT")
                {
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace('.', ',');
                        content.DiscountAmount = Convert.ToDecimal(strValue);
                    }
                }
                else if (strKey.ToUpper() == "COMPANIES")
                {
                    if(!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace(" ", string.Empty);
                        companies = strValue.Split(',').ToList();

                        foreach (string strCompany in companies)
                        {
                            if (!ML.Common.Text.IsGuidNotEmpty(strCompany))
                            {
                                _mContent.Status = RestStatus.ParameterError;
                                _mContent.StatusText = "Parameter Error 5";
                                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                            }
                        }
                    }
                }
                else if (strKey.ToUpper() == "SCALE")
                {
                    content.Scale = Convert.ToInt32(strValue);
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
                        , DB.ImageService.ImageCategory.Content
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
                _mContent.Status = RestStatus.DataError;
                _mContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            content.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);

            // Handle Corporate sibling
            content.CorporateSiblingGuid = Guid.Empty;
            content.CorporateSiblingEnabled = false;
            Guid guidCorporateSiblingGuid = Guid.NewGuid();
            DB.tCompany company = new DB.CompanyRepository().GetByCompanyGuid(usertoken.CompanyGuid);
            if (contentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                if (company.tCorporate.tCompany.Count() > 1)
                {
                    if (companies.Count > 0)
                    {
                        content.CorporateSiblingGuid = guidCorporateSiblingGuid;
                        foreach (string strCompany in companies)
                        {
                            Guid guidCompanyGuid = new Guid(strCompany);
                            if (guidCompanyGuid == usertoken.CompanyGuid)
                            {
                                content.CorporateSiblingEnabled = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Handle Identifier
            if (contentTemplate.GenerateIdentifiersFromName)
            {
                content.Identifier = new DB.ContentTemplateService().GenerateIdentifierFromName(content);
            }

            // Save
            if(contentRepository.Save(content) != DB.Repository.Status.Success)
            {
                _mContent.Status = RestStatus.GenericError;
                _mContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Populate result
            _mContent.ContentId = content.ContentGuid.ToString();

            // Connect Content Category
            if (contentCategories.Count > 0)
            {
                DB.ContentRepository contentRepository2 = new DB.ContentRepository();
                DB.tContent content2 = contentRepository2.GetContent(content.ContentGuid);
                if (content2 != null)
                {
                    // Delete old Content Categories
                    foreach (DB.tContentCategory contentCategory in content2.tContentCategory.ToList())
                    {
                        if (contentCategory.ContentParentCategoryGuid != Guid.Empty)
                        {
                            content2.tContentCategory.Remove(contentCategory);
                        }
                    }
                    // Add new Content Categories
                    foreach (string strContentCategoryGuid in contentCategories)
                    {
                        DB.tContentCategory contentCategory = contentRepository2.GetInContextByContentCategoryGuid(new Guid(strContentCategoryGuid));
                        content2.tContentCategory.Add(contentCategory);
                    }
                }

                // Add Parent Content Categories
                IQueryable<DB.tContentCategory> contentCategories2 = contentRepository2.GetInContextByContentTemplateGuidAndContentParentCategoryGuid(guidContentTemplateGuid, Guid.Empty);
                foreach (DB.tContentCategory contentCategory in contentCategories2)
                {
                    content2.tContentCategory.Add(contentCategory);
                }
                contentRepository2.Save();
            }

            if(contentTemplate.Redeemable)
            {
                new DB.TaskService().AddTask(usertoken.CompanyGuid, DB.TaskRepository.Priority.Medium, new DB.TaskCustomerContentAvailability(content.ContentGuid, true));
            }

            // Connect SubContents
            DB.ContentSubContentRepository contentSubContentRepository = new DB.ContentSubContentRepository();
            IQueryable<DB.tContent> contents = new DB.ContentRepository().GetSubContentByContentTemplateGuid(content.ContentTemplateGuid);
            foreach (DB.tContent item in contents)
            {
                bool bFound = false;
                foreach (string strContentGuid in availableSubContents)
                {
                    if (new Guid(strContentGuid) == item.ContentGuid)
                    {
                        DB.tContentSubContent contentSubContent = contentSubContentRepository.GetByContentGuidAndSubContentGuid(content.ContentGuid, item.ContentGuid);
                        if (contentSubContent == null)
                        {
                            DB.tContentSubContent contentSubContentNew = new DB.tContentSubContent();
                            contentSubContentNew.ContentGuid = content.ContentGuid;
                            contentSubContentNew.SubContentGuid = item.ContentGuid;
                            contentSubContentNew.Included = false;
                            if (includedSubContents.Contains(strContentGuid))
                            {
                                contentSubContentNew.Included = true;
                            }
                            contentSubContentRepository.Save(contentSubContentNew);
                        }
                        else
                        {
                            contentSubContent.Included = false;
                            if (includedSubContents.Contains(strContentGuid))
                            {
                                contentSubContent.Included = true;
                            }
                            contentSubContentRepository.Save();
                        }
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    DB.tContentSubContent contentSubContent = contentSubContentRepository.GetByContentGuidAndSubContentGuid(content.ContentGuid, item.ContentGuid);
                    if (contentSubContent != null)
                    {
                        contentSubContentRepository.Delete(contentSubContent);
                        contentSubContentRepository.Save();
                    }
                }
            }



            // Handle Corporate sibling
            if (contentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                if (company.tCorporate.tCompany.Count() > 1)
                {
                    if (companies.Count > 0)
                    {
                        foreach (DB.tCompany company2 in (company.tCorporate.tCompany))
                        {
                            if (company2.CompanyGuid != usertoken.CompanyGuid)
                            {
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
                                            , DB.ImageService.ImageCategory.Content
                                            , string.Empty, DB.ImageService.Rotation.Rotate0
                                            , company2.CompanyGuid
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

                                // Prepare
                                DB.tContentTemplate contentTemplate2 = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(company2.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Mofr);

                                // ContentType
                                Guid guidContentTypeGuid = Guid.Empty;
                                DB.tContentType contentType = new DB.ContentTypeRepository().GetByCompanyGuidAndContentTypeGuid(usertoken.CompanyGuid, content.ContentTypeGuid);
                                if (contentType != null)
                                {
                                    DB.tContentType contentType2 = new DB.ContentTypeRepository().GetByCompanyGuidAndIdentifier(company2.CompanyGuid, contentType.Identifier);
                                    if (contentType2 != null)
                                    {
                                        guidContentTypeGuid = contentType2.ContentTypeGuid;
                                    }
                                }

                                // Create corporate sibling
                                DB.tContent contentSibling = new DB.tContent();
                                contentSibling.ContentGuid = Guid.NewGuid();
                                contentSibling.ContentTemplateGuid = contentTemplate2.ContentTemplateGuid;
                                contentSibling.Name = content.Name;
                                contentSibling.Description = content.Description;
                                contentSibling.Identifier = content.Identifier;
                                contentSibling.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);
                                contentSibling.Active = content.Active;
                                contentSibling.TerminationDateTime = content.TerminationDateTime;
                                contentSibling.TimeLimit = content.TimeLimit;
                                contentSibling.MaxNoOfViews = content.MaxNoOfViews;
                                contentSibling.TimeStamp = content.TimeStamp;
                                contentSibling.PublishDateTime = content.PublishDateTime;
                                contentSibling.MaxNoOfRedeems = content.MaxNoOfRedeems;
                                contentSibling.Map = content.Map;
                                contentSibling.StartDateTime = content.StartDateTime;
                                contentSibling.Price = content.Price;
                                contentSibling.VAT = content.VAT;
                                contentSibling.SubContent = content.SubContent;
                                contentSibling.SortOrder = content.SortOrder;
                                contentSibling.ContentSubContentGroupGuid = Guid.Empty; // N/A
                                contentSibling.ContentSubContentGroupDefault = content.ContentSubContentGroupDefault;
                                contentSibling.NoOfFreeSubContent = content.NoOfFreeSubContent;
                                contentSibling.MaxInCart = content.MaxInCart;
                                contentSibling.FreezeDateTime = content.FreezeDateTime;
                                contentSibling.NoOfFreeSubContentMaxMessage = content.NoOfFreeSubContentMaxMessage;
                                contentSibling.Private = content.Private;
                                contentSibling.OpenNotRedeemable = content.OpenNotRedeemable;
                                contentSibling.DeliveryPrice = content.DeliveryPrice;
                                contentSibling.SitePrice = content.SitePrice;
                                contentSibling.ContentTypeGuid = guidContentTypeGuid;
                                contentSibling.DiscountPercent = content.DiscountPercent;
                                contentSibling.DiscountAmount = content.DiscountAmount;
                                contentSibling.CorporateSiblingGuid = guidCorporateSiblingGuid;
                                contentSibling.CorporateSiblingEnabled = false;

                                foreach (string strCompany in companies)
                                {
                                    Guid guidCompanyGuid = new Guid(strCompany);
                                    if (guidCompanyGuid != usertoken.CompanyGuid)
                                    {
                                        contentSibling.CorporateSiblingEnabled = true;
                                        break;
                                    }
                                }

                                // Save
                                if (contentRepository.Save(contentSibling) != DB.Repository.Status.Success)
                                {
                                    _mContent.Status = RestStatus.GenericError;
                                    _mContent.StatusText = "Generic Error";
                                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                                }

                                // Connect Content Category
                                if (contentCategories.Count > 0)
                                {
                                    // Add
                                    DB.ContentRepository contentRepository2 = new DB.ContentRepository();
                                    DB.tContent content2 = contentRepository2.GetContent(contentSibling.ContentGuid);
                                    if (content2 != null)
                                    {
                                        // Delete old Content Categories
                                        foreach (DB.tContentCategory contentCategory in content2.tContentCategory.ToList())
                                        {
                                            if (contentCategory.ContentParentCategoryGuid != Guid.Empty)
                                            {
                                                content2.tContentCategory.Remove(contentCategory);
                                            }
                                        }
                                        // Add new Content Categories
                                        foreach (string strContentCategoryGuid in contentCategories)
                                        {
                                            DB.tContentCategory contentCategory = contentRepository2.GetInContextByContentCategoryGuid(new Guid(strContentCategoryGuid));
                                            if (contentCategory != null)
                                            {
                                                contentCategory = contentRepository2.GetInContextByContentTemplateGuidAndIdentifier(contentTemplate2.ContentTemplateGuid, contentCategory.Identifier);  //Lookup
                                                if (contentCategory != null)
                                                {
                                                    content2.tContentCategory.Add(contentCategory);
                                                }
                                            }
                                        }
                                    }

                                    // Add Parent Content Categories
                                    IQueryable<DB.tContentCategory> contentCategories2 = contentRepository2.GetInContextByContentTemplateGuidAndContentParentCategoryGuid(guidContentTemplateGuid, Guid.Empty);
                                    foreach (DB.tContentCategory contentCategory in contentCategories2)
                                    {
                                        DB.tContentCategory contentCategory2 = contentRepository2.GetInContextByContentTemplateGuidAndIdentifier(contentTemplate2.ContentTemplateGuid, contentCategory.Identifier);  //Lookup
                                        if (contentCategory2 != null)
                                        {
                                            content2.tContentCategory.Add(contentCategory2);
                                        }
                                    }
                                    contentRepository2.Save();
                                }

                                if (contentTemplate.Redeemable)
                                {
                                    new DB.TaskService().AddTask(company2.CompanyGuid, DB.TaskRepository.Priority.Medium, new DB.TaskCustomerContentAvailability(contentSibling.ContentGuid, true));
                                }
                            }
                        }
                    }
                }
            }



            _mContent.Status = RestStatus.Success;
            _mContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
        }

        /// <summary>
        /// id = ContentGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="contentTemplateId"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Put(string token, string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _mContent.Status = RestStatus.FormatError;
                _mContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContent.Status = RestStatus.ParameterError;
                _mContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            Guid guidContentGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContent.Status = RestStatus.AuthenticationFailed;
                _mContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = contentRepository.GetContent(guidContentGuid);
            if (content == null)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            if (content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            string strXml = string.Format("<xml>{0}</xml>", content.Xml);
            string strXmlSibling = string.Empty;
            if(ML.Common.Text.IsGuidNotEmpty(content.CorporateSiblingGuid))
            {
                IQueryable<DB.tContent> contents2 = contentRepository.GetByCorporateSiblingGuid((Guid)content.CorporateSiblingGuid);
                foreach(DB.tContent item in contents2)
                {
                    if (item.ContentGuid != content.ContentGuid)
                    {
                        strXmlSibling = string.Format("<xml>{0}</xml>", item.Xml);
                        break;
                    }
                }
            }

            // Read data
            string strRootPath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/upload/", usertoken.CompanyGuid.ToString()));
            System.IO.Directory.CreateDirectory(strRootPath);
            var provider = new MultipartFormDataStreamProvider(strRootPath);
            await Request.Content.ReadAsMultipartAsync(provider);

            if (provider.FormData.Count == 0 && provider.FileData.Count == 0)
            {
                _mContent.Status = RestStatus.DataMissing;
                _mContent.StatusText = "Data missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Validate template
            XmlDocument xmlTemplate = new XmlDocument();
            try
            {
                xmlTemplate.LoadXml(ML.Common.XmlHelper.AddRootTags(content.tContentTemplate.XmlContentTemplate));
            }
            catch
            {
                _mContent.Status = RestStatus.DataError;
                _mContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Handle Name
            string strContentNameOrig = content.Name;
            string strName = string.Empty;
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key.ToUpper();
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                if (strKey == "NAME")
                {
                    strName = strValue;
                }
            }

            foreach (XmlNode xmlNode in xmlTemplate.SelectSingleNode("root"))
            {
                if (xmlNode.Attributes["issystemname"] != null)
                {
                    if (xmlNode.Attributes["issystemname"].Value == "1")
                    {
                        if (provider.FormData.Get(xmlNode.Name) != null)
                        {
                            strName = provider.FormData.Get(xmlNode.Name);
                        }
                    }
                }
            }

            //// Ensure name is present
            //if (string.IsNullOrEmpty(strName))
            //{
            //    _mContent.Status = RestStatus.ParameterError;
            //    _mContent.StatusText = "Parameter Error 2";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            //}
            if (!string.IsNullOrEmpty(strName))
            {
                content.Name = strName;
            }

            // Default values
            content.OpenNotRedeemable = false;

            // Handle values
            List<string> contentCategories = new List<string>();
            List<string> availableSubContents = new List<string>();
            List<string> includedSubContents = new List<string>();
            List<string> companies = new List<string>();
            foreach (var key in provider.FormData.AllKeys)
            {
                string strKey = key;
                string strValue = provider.FormData.GetValues(key).FirstOrDefault();

                //if (strKey.ToUpper() == "NAME")
                //{
                //    content.Name = strValue;
                //}
                if (strKey.ToUpper() == "DESCRIPTION")
                {
                    content.Description = strValue;
                }
                else if (strKey.ToUpper() == "IDENTIFIER")
                {
                    content.Identifier = strValue;
                }
                else if (strKey.ToUpper() == "ACTIVE")
                {
                    content.Active = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "PUBLISHDATETIME")
                {
                    content.PublishDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "STARTDATETIME")
                {
                    content.StartDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "FREEZEDATETIME")
                {
                    content.FreezeDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "TERMINATIONDATETIME")
                {
                    content.TerminationDateTime = Convert.ToDateTime(strValue);
                }
                else if (strKey.ToUpper() == "MAP")
                {
                    content.Map = string.Empty;
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
                                content.Map += "|";
                            }
                            content.Map += strLatLng;
                        }
                    }
                }
                else if (strKey.ToUpper() == "PRICE")
                {
                    strValue = strValue.Replace('.', ',');
                    content.Price = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "DELIVERYPRICE")
                {
                    strValue = strValue.Replace('.', ',');
                    content.DeliveryPrice = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "VAT")
                {
                    strValue = strValue.Replace('.', ',');
                    content.VAT = Convert.ToDecimal(strValue);
                }
                else if (strKey.ToUpper() == "SORTORDER")
                {
                    content.SortOrder = Convert.ToInt32(strValue);
                }
                else if (strKey.ToUpper() == "OPENNOTREDEEMABLE")
                {
                    content.OpenNotRedeemable = strValue == "1" ? true : false;
                }
                else if (strKey.ToUpper() == "NOOFFREESUBCONTENT")
                {
                    content.NoOfFreeSubContent = Convert.ToInt32(strValue);
                }
                // Static Cont...
                else if (strKey.ToUpper() == "CATEGORIES")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    contentCategories = strValue.Split(',').ToList();

                    foreach (string strContentCategory in contentCategories)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strContentCategory))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }

                    if (content.tContentTemplate.ContentTemplateGuid == new Guid("3C1A0D40-304C-4430-BF51-1D1EAEDD43F7"))
                    {
                        if (!contentCategories.Contains("83AB243A-DE06-4C17-9776-0CC8DE94808E"))
                        {
                            contentCategories.Add("83AB243A-DE06-4C17-9776-0CC8DE94808E");
                        }
                    }
                }
                else if (strKey.ToUpper() == "AVAILABLESUBCONTENTS")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    availableSubContents = strValue.Split(',').ToList();

                    foreach (string strAvailableSubContent in availableSubContents)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strAvailableSubContent))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
                else if (strKey.ToUpper() == "INCLUDEDSUBCONTENTS")
                {
                    strValue = strValue.Replace(" ", string.Empty);
                    includedSubContents = strValue.Split(',').ToList();

                    foreach (string strIncludedSubContent in includedSubContents)
                    {
                        if (!ML.Common.Text.IsGuidNotEmpty(strIncludedSubContent))
                        {
                            _mContent.Status = RestStatus.ParameterError;
                            _mContent.StatusText = "Parameter Error";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                        }
                    }
                }
                else if (strKey.ToUpper() == "CONTENTTYPE")
                {
                    content.ContentTypeGuid = new Guid(strValue);
                }
                else if (strKey.ToUpper() == "DISCOUNTPERCENT")
                {
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace('.', ',');
                        content.DiscountPercent = Convert.ToDecimal(strValue);
                    }
                }
                else if (strKey.ToUpper() == "DISCOUNTAMOUNT")
                {
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace('.', ',');
                        content.DiscountAmount = Convert.ToDecimal(strValue);
                    }
                }
                else if (strKey.ToUpper() == "COMPANIES")
                {
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        strValue = strValue.Replace(" ", string.Empty);
                        companies = strValue.Split(',').ToList();

                        foreach (string strCompany in companies)
                        {
                            if (!ML.Common.Text.IsGuidNotEmpty(strCompany))
                            {
                                _mContent.Status = RestStatus.ParameterError;
                                _mContent.StatusText = "Parameter Error 5";
                                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                            }
                        }
                    }
                }
                else if (strKey.ToUpper() == "SCALE")
                {
                    content.Scale = Convert.ToInt32(strValue);
                }
                // Cont...
                else
                {
                    strXml = ML.Common.XmlHelper.UpdateXml(strXml, strKey, strValue);
                    strXmlSibling = ML.Common.XmlHelper.UpdateXml(strXmlSibling, strKey, strValue);
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
                        , DB.ImageService.ImageCategory.Content
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
                _mContent.Status = RestStatus.DataError;
                _mContent.StatusText = "Data Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            content.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXml);

            // Handle Corporate sibling
            Guid guidCorporateSiblingGuid = Guid.Empty;
            if(ML.Common.Text.IsGuidNotEmpty(content.CorporateSiblingGuid))
            {
                guidCorporateSiblingGuid = (Guid)content.CorporateSiblingGuid;
            }

            DB.tCompany company = new DB.CompanyRepository().GetByCompanyGuid(usertoken.CompanyGuid);
            if (content.tContentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                if (company.tCorporate.tCompany.Count() > 1)
                {
                    if (companies.Count > 0)
                    {
                        if(!ML.Common.Text.IsGuidNotEmpty(content.CorporateSiblingGuid))
                        {
                            guidCorporateSiblingGuid = Guid.NewGuid();
                            content.CorporateSiblingGuid = guidCorporateSiblingGuid;
                        }
                        
                        content.CorporateSiblingEnabled = false;
                        foreach (string strCompany in companies)
                        {
                            Guid guidCompanyGuid = new Guid(strCompany);
                            if (guidCompanyGuid == usertoken.CompanyGuid)
                            {
                                content.CorporateSiblingEnabled = true;
                                break;
                            }
                        }
                    }
                }
            }

            // Handle Identifier
            if (content.tContentTemplate.GenerateIdentifiersFromName)
            {
                if (strContentNameOrig != content.Name)
                {
                    content.Identifier = new DB.ContentTemplateService().GenerateIdentifierFromName(content);
                }
            }

            // Save
            if (contentRepository.Save() != DB.Repository.Status.Success)
            {
                _mContent.Status = RestStatus.GenericError;
                _mContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Connect Content Category
            if (contentCategories.Count > 0)
            {
                DB.ContentRepository contentRepository2 = new DB.ContentRepository();
                DB.tContent content2 = contentRepository2.GetContent(content.ContentGuid);
                if (content2 != null)
                {
                    // Delete old Content Categories
                    foreach (DB.tContentCategory contentCategory in content2.tContentCategory.ToList())
                    {
                        if (contentCategory.ContentParentCategoryGuid != Guid.Empty)
                        {
                            content2.tContentCategory.Remove(contentCategory);
                        }
                    }
                    // Add new Content Categories
                    foreach (string strContentCategoryGuid in contentCategories)
                    {
                        DB.tContentCategory contentCategory = contentRepository2.GetInContextByContentCategoryGuid(new Guid(strContentCategoryGuid));
                        content2.tContentCategory.Add(contentCategory);
                    }
                }

                // Add Parent Content Categories
                IQueryable<DB.tContentCategory> contentCategories2 = contentRepository2.GetInContextByContentTemplateGuidAndContentParentCategoryGuid(content.tContentTemplate.ContentTemplateGuid, Guid.Empty);
                foreach (DB.tContentCategory contentCategory in contentCategories2)
                {
                    content2.tContentCategory.Add(contentCategory);
                }
                contentRepository2.Save();
            }

            if (content.tContentTemplate.Redeemable)
            {
                new DB.TaskService().AddTask(usertoken.CompanyGuid, DB.TaskRepository.Priority.Medium, new DB.TaskCustomerContentAvailability(content.ContentGuid, true));
            }

            // Connect SubContents
            DB.ContentSubContentRepository contentSubContentRepository = new DB.ContentSubContentRepository();
            IQueryable<DB.tContent> contents = new DB.ContentRepository().GetSubContentByContentTemplateGuid(content.ContentTemplateGuid);
            foreach (DB.tContent item in contents)
            {
                bool bFound = false;
                foreach (string strContentGuid in availableSubContents)
                {
                    if (new Guid(strContentGuid) == item.ContentGuid)
                    {
                        DB.tContentSubContent contentSubContent = contentSubContentRepository.GetByContentGuidAndSubContentGuid(content.ContentGuid, item.ContentGuid);
                        if (contentSubContent == null)
                        {
                            DB.tContentSubContent contentSubContentNew = new DB.tContentSubContent();
                            contentSubContentNew.ContentGuid = content.ContentGuid;
                            contentSubContentNew.SubContentGuid = item.ContentGuid;
                            contentSubContentNew.Included = false;
                            if (includedSubContents.Contains(strContentGuid))
                            {
                                contentSubContentNew.Included = true;
                            }
                            contentSubContentRepository.Save(contentSubContentNew);
                        }
                        else
                        {
                            contentSubContent.Included = false;
                            if (includedSubContents.Contains(strContentGuid))
                            {
                                contentSubContent.Included = true;
                            }
                            contentSubContentRepository.Save();
                        }
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    DB.tContentSubContent contentSubContent = contentSubContentRepository.GetByContentGuidAndSubContentGuid(content.ContentGuid, item.ContentGuid);
                    if (contentSubContent != null)
                    {
                        contentSubContentRepository.Delete(contentSubContent);
                        contentSubContentRepository.Save();
                    }
                }
            }



            // Handle Corporate sibling
            if (content.tContentTemplate.CompanyTypeID == (int)DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                if (company.tCorporate.tCompany.Count() > 1)
                {
                    if (companies.Count > 0)
                    {
                        foreach (DB.tCompany company2 in (company.tCorporate.tCompany))
                        {
                            if (company2.CompanyGuid != usertoken.CompanyGuid)
                            {
                                // Handle files
                                foreach (MultipartFileData file in provider.FileData)
                                {
                                    string strContentDispositionName = file.Headers.ContentDisposition.Name.Replace("\"", string.Empty);
                                    string strContentDispositionFileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                                    if (ML.Common.XmlHelper.NodeExists(strXmlSibling, strContentDispositionName))
                                    {
                                        System.Drawing.Image image = System.Drawing.Image.FromFile(file.LocalFileName);

                                        // Save to imagebank
                                        DB.ImageService imageService = new DB.ImageService();
                                        imageService.SaveImage(
                                            image
                                            , strContentDispositionFileName
                                            , DB.ImageService.ImageCategory.Content
                                            , string.Empty, DB.ImageService.Rotation.Rotate0
                                            , company2.CompanyGuid
                                            );

                                        // Prepare for content
                                        string strNodeTag = ML.Common.XmlHelper.GetNodeAttributeValue(strXmlSibling, strContentDispositionName);
                                        if (strNodeTag == "image")
                                        {
                                            strXmlSibling = ML.Common.XmlHelper.UpdateXml(strXmlSibling, strContentDispositionName, "name", strContentDispositionFileName, false);
                                            strXmlSibling = ML.Common.XmlHelper.UpdateXml(strXmlSibling, strContentDispositionName, "imageguid", imageService.ImageGuid.ToString(), false);
                                        }
                                    }
                                }

                                // Prepare
                                DB.tContentTemplate contentTemplate2 = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(company2.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Mofr);

                                // ContentType
                                Guid guidContentTypeGuid = Guid.Empty;
                                DB.tContentType contentType = new DB.ContentTypeRepository().GetByCompanyGuidAndContentTypeGuid(usertoken.CompanyGuid, content.ContentTypeGuid);
                                if (contentType != null)
                                {
                                    DB.tContentType contentType2 = new DB.ContentTypeRepository().GetByCompanyGuidAndIdentifier(company2.CompanyGuid, contentType.Identifier);
                                    if (contentType2 != null)
                                    {
                                        guidContentTypeGuid = contentType2.ContentTypeGuid;
                                    }
                                }

                                // Update corporate sibling
                                DB.ContentRepository contentRepositorySibling = new DB.ContentRepository();
                                DB.tContent contentSibling = contentRepositorySibling.GetContentByContentTemplateGuidAndCorporateSiblingGuid(contentTemplate2.ContentTemplateGuid, (Guid)content.CorporateSiblingGuid);

                                //contentSibling.ContentGuid = Guid.NewGuid();
                                //contentSibling.ContentTemplateGuid = contentTemplate2.ContentTemplateGuid;
                                contentSibling.Name = content.Name;
                                contentSibling.Description = content.Description;
                                contentSibling.Identifier = content.Identifier;
                                contentSibling.Xml = ML.Common.XmlHelper.RemoveRootXmlNode(strXmlSibling);
                                contentSibling.Active = content.Active;
                                contentSibling.TerminationDateTime = content.TerminationDateTime;
                                contentSibling.TimeLimit = content.TimeLimit;
                                contentSibling.MaxNoOfViews = content.MaxNoOfViews;
                                contentSibling.TimeStamp = content.TimeStamp;
                                contentSibling.PublishDateTime = content.PublishDateTime;
                                contentSibling.MaxNoOfRedeems = content.MaxNoOfRedeems;
                                contentSibling.Map = content.Map;
                                contentSibling.StartDateTime = content.StartDateTime;
                                contentSibling.Price = content.Price;
                                contentSibling.VAT = content.VAT;
                                contentSibling.SubContent = content.SubContent;
                                contentSibling.SortOrder = content.SortOrder;
                                contentSibling.ContentSubContentGroupGuid = Guid.Empty; // N/A
                                contentSibling.ContentSubContentGroupDefault = content.ContentSubContentGroupDefault;
                                contentSibling.NoOfFreeSubContent = content.NoOfFreeSubContent;
                                contentSibling.MaxInCart = content.MaxInCart;
                                contentSibling.FreezeDateTime = content.FreezeDateTime;
                                contentSibling.NoOfFreeSubContentMaxMessage = content.NoOfFreeSubContentMaxMessage;
                                contentSibling.Private = content.Private;
                                contentSibling.OpenNotRedeemable = content.OpenNotRedeemable;
                                contentSibling.DeliveryPrice = content.DeliveryPrice;
                                contentSibling.SitePrice = content.SitePrice;
                                contentSibling.ContentTypeGuid = guidContentTypeGuid;
                                contentSibling.DiscountPercent = content.DiscountPercent;
                                contentSibling.DiscountAmount = content.DiscountAmount;
                                contentSibling.CorporateSiblingGuid = guidCorporateSiblingGuid;
                                contentSibling.CorporateSiblingEnabled = false;

                                foreach (string strCompany in companies)
                                {
                                    Guid guidCompanyGuid = new Guid(strCompany);
                                    if (guidCompanyGuid != usertoken.CompanyGuid)
                                    {
                                        contentSibling.CorporateSiblingEnabled = true;
                                        break;
                                    }
                                }

                                // Save
                                if (contentRepositorySibling.Save() != DB.Repository.Status.Success)
                                {
                                    _mContent.Status = RestStatus.GenericError;
                                    _mContent.StatusText = "Generic Error";
                                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
                                }

                                // Connect Content Category
                                if (contentCategories.Count > 0)
                                {
                                    // Add
                                    DB.ContentRepository contentRepository2 = new DB.ContentRepository();
                                    DB.tContent content2 = contentRepository2.GetContent(contentSibling.ContentGuid);
                                    if (content2 != null)
                                    {
                                        // Delete old Content Categories
                                        foreach (DB.tContentCategory contentCategory in content2.tContentCategory.ToList())
                                        {
                                            if (contentCategory.ContentParentCategoryGuid != Guid.Empty)
                                            {
                                                content2.tContentCategory.Remove(contentCategory);
                                            }
                                        }
                                        // Add new Content Categories
                                        foreach (string strContentCategoryGuid in contentCategories)
                                        {
                                            DB.tContentCategory contentCategory = contentRepository2.GetInContextByContentCategoryGuid(new Guid(strContentCategoryGuid));
                                            if (contentCategory != null)
                                            {
                                                contentCategory = contentRepository2.GetInContextByContentTemplateGuidAndIdentifier(contentTemplate2.ContentTemplateGuid, contentCategory.Identifier);  //Lookup
                                                if (contentCategory != null)
                                                {
                                                    content2.tContentCategory.Add(contentCategory);
                                                }
                                            }
                                        }
                                    }

                                    // Add Parent Content Categories
                                    IQueryable<DB.tContentCategory> contentCategories2 = contentRepository2.GetInContextByContentTemplateGuidAndContentParentCategoryGuid(content.tContentTemplate.ContentTemplateGuid, Guid.Empty);
                                    foreach (DB.tContentCategory contentCategory in contentCategories2)
                                    {
                                        DB.tContentCategory contentCategory2 = contentRepository2.GetInContextByContentTemplateGuidAndIdentifier(contentTemplate2.ContentTemplateGuid, contentCategory.Identifier);  //Lookup
                                        if (contentCategory2 != null)
                                        {
                                            content2.tContentCategory.Add(contentCategory2);
                                        }
                                    }
                                    contentRepository2.Save();
                                }

                                if (contentTemplate2.Redeemable)
                                {
                                    new DB.TaskService().AddTask(company2.CompanyGuid, DB.TaskRepository.Priority.Medium, new DB.TaskCustomerContentAvailability(contentSibling.ContentGuid, true));
                                }
                            }
                        }
                    }
                }
            }



            _mContent.Status = RestStatus.Success;
            _mContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
        }

        /// <summary>
        /// id = ContentGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContent.Status = RestStatus.ParameterError;
                _mContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }
            Guid guidContentGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContent.Status = RestStatus.AuthenticationFailed;
                _mContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = contentRepository.GetContent(guidContentGuid);
            if (content == null)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            if (content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContent.Status = RestStatus.NotExisting;
                _mContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            // Delete
            if (new DB.ContentService().DeleteContent(guidContentGuid) != DB.Repository.Status.Success)
            {
                _mContent.Status = RestStatus.GenericError;
                _mContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
            }

            _mContent.Status = RestStatus.Success;
            _mContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContents));
        }


    }
}
