using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class CustomerContentController : ApiController
    {
        List<dynamic> _mCustomerContents = new List<dynamic>();
        dynamic _dCustomerContent = new ExpandoObject();

        public CustomerContentController()
        {
            _mCustomerContents.Add(_dCustomerContent);
        }

        public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth, string customerId, string contentId)
        {
            if (Redirection.IsValid(companyId,
                Request.RequestUri.ToString()
                , "CustomerContentController-> public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth, string customerId, string contentId)"
                , @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/" + customerId + "/" + contentId + ""
                )
                )
            {
                APIRedirect apiRedirect = new APIRedirect();
                string url = @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/" + customerId + "/" + contentId + "";
                return apiRedirect.GetRequst(url);
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && contentTemplateTypeId < 1 && (customerContentTypeId < 0 || customerContentTypeId > 4) && list < 0 && imageWidth < 1) //&& string.IsNullOrEmpty(customerId)
            {
                _dCustomerContent.Status = RestStatus.ParameterError;
                _dCustomerContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }
            Guid guidCompanyGuid = new Guid(companyId);
            Guid guidContentGuid = Guid.Empty;
            if(ML.Common.Text.IsGuidNotEmpty(contentId))
            {
                guidContentGuid = new Guid(contentId);
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _dCustomerContent.Status = RestStatus.AuthenticationFailed;
                _dCustomerContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)contentTemplateTypeId;

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(guidCompanyGuid, contentTemplateType);
            if (contentTemplate == null)
            {
                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Get Customer
            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            //if (customer == null)
            //{
            //    _dCustomerContent.Status = RestStatus.NotExisting;
            //    _dCustomerContent.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            //}


            // Get Content
            const int ALL = 0;
            //const int VIEWED = 1;
            //const int REDEEMED = 2;
            //const int NEWEST = 3;
            //const int RANDOM = 4;

            DB.tContent content = null;
            if (customerContentTypeId == ALL)
            {
                //contents = new DB.ContentRepository().GetActiveContent(customer.CustomerGuid, contentTemplate.ContentTemplateGuid);
                if(guidContentGuid != Guid.Empty)
                {
                    content = new DB.ContentRepository().GetContent(guidContentGuid);
                    if(content == null)
                    {
                        content = new DB.ContentRepository().GetContent(contentTemplate.ContentTemplateGuid, contentId);
                        if(content == null)
                        {
                            _dCustomerContent.Status = RestStatus.NotExisting;
                            _dCustomerContent.StatusText = "Not Existing";
                            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
                        }
                    }
                    guidContentGuid = content.ContentGuid;
                }
                else
                {
                    content = new DB.ContentRepository().GetContent(contentTemplate.ContentTemplateGuid, contentId);
                    guidContentGuid = content.ContentGuid;
                }
            }
            //else if (customerContentTypeId == VIEWED)
            //{
            //    //contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 1).ToList();
            //    contents = new DB.ContentRepository().GetActiveContent(customer.CustomerGuid, contentTemplate.ContentTemplateGuid).ToList();
            //}
            //else if (customerContentTypeId == REDEEMED)
            //{
            //    contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 2).ToList();
            //}
            //else if (customerContentTypeId == NEWEST)
            //{
            //    contents = new DB.ContentRepository().GetActiveNewest(contentTemplate.ContentTemplateGuid, 10000);
            //}
            //else if (customerContentTypeId == RANDOM)
            //{
            //    contents = new DB.ContentRepository().GetActiveRandom(contentTemplate.ContentTemplateGuid, 10000);
            //}
            //// Cont...

            if (content == null)
            {
                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Get customer unique data
            DB.tCustomerContentAvailability customerContentAvailability = null;
            DB.tCustomerContent customerContent = null;
            DB.tCustomerContentFavorite customerContentFavorite = null;
            DB.tCustomerContentOngoing customerContentOngoing = null;
            if (customer != null)
            {
                // Get CustomerContentAvailability
                customerContentAvailability = new DB.CustomerContentAvailabilityRepository().GetActiveByCustomerGuidAndContenteGuid(customer.CustomerGuid, guidContentGuid);

                // Get CustomerContent (Redeemed)
                customerContent = new DB.CustomerContentRepository().GetByCustomerGuidAndContentTypeAndContentGuid(customer.CustomerGuid, DB.CustomerContentRepository.CustomerContentType.Redeem, guidContentGuid);

                // Get CustomerContentFavorite
                customerContentFavorite = new DB.CustomerContentFavoriteRepository().GetFavorite(customer.CustomerGuid, guidContentGuid);

                // Get CustomerContentOngoing
                customerContentOngoing = new DB.CustomerContentOngoingRepository().GetOngoing(customer.CustomerGuid, guidContentGuid);
            }


            // Prepare Template
            XmlDocument xmlTemplate = new XmlDocument();
            xmlTemplate.LoadXml(ML.Common.XmlHelper.AddRootTags(contentTemplate.XmlContentTemplate));

            // Get contain-list
            List<string> lst = ML.Common.XmlHelper.ExtractCustomList(xmlTemplate, list);

            // Populate
            _mCustomerContents.Clear();
            if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                //foreach (DB.tContent content in contents)
                //{
                    // Static fields
                    dynamic dContent = new ExpandoObject();
                    dContent.ContentGuid = content.ContentGuid;
                    dContent.Name = content.Name;
                    dContent.Description = content.Description;
                    dContent.Identifier = content.Identifier;
                    dContent.PublishDateTime = content.PublishDateTime.ToString();
                    dContent.TerminationDateTime = content.TerminationDateTime.ToString();
                    dContent.OpenNotRedeemable = content.OpenNotRedeemable;
                    dContent.ContentType = content.tContentType.Identifier;

                    List<Dictionary<string, string>> latLngs = new List<Dictionary<string, string>>();
                    foreach (string latLng in content.Map.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(latLng))
                        {
                            string[] temp = latLng.Split(';');
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Lat", temp[0].Split(',')[0]);
                            dic.Add("Lng", temp[0].Split(',')[1]);
                            latLngs.Add(dic);
                        }
                    }
                    dContent.LatLng = latLngs;

                    // Dynamic fields (all or based on contain-list number)
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
                    foreach (XmlNode xmlNode in xml.SelectSingleNode("root"))
                    {
                        if ((lst.Count > 0 && lst.Contains(xmlNode.Name)) || lst.Count == 0)
                        {
                            if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"] != null)
                            {
                                if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "image")
                                {
                                    string strFileName = xmlNode.SelectSingleNode("name").InnerText;
                                    string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
                                    ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", guidCompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, guidCompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                                }
                                else
                                {
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                }
                            }
                            else
                            {
                                (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                            }
                        }
                    }


                    // Customer unique
                    if (customer != null)
                    {
                        // Customer Content Availability
                        if (customerContentAvailability == null)
                        {
                            dContent.Availability = 0;
                        }
                        else
                        {
                            dContent.Availability = customerContentAvailability.NoOfRedeems;
                        }

                        // Customer Content (Redeemed)
                        if (customerContent == null)
                        {
                            dContent.Redeemed = string.Empty;
                        }
                        else
                        {
                            dContent.Redeemed = customerContent.TimeStamp;
                        }

                        // Customer Content Favorites
                        if (customerContentFavorite == null)
                        {
                            dContent.Favorite = 0;
                        }
                        else
                        {
                            dContent.Favorite = 1;
                        }

                        // Customer Content Ongoing
                        if(customerContentOngoing == null)
                        {
                            dContent.Ongoing = string.Empty;
                        }
                        else
                        {
                            dContent.Ongoing = customerContentOngoing.TimeStamp.ToString();
                        }

                        // ContentPush
                        if(content.tContentPush.Any())
                        {
                            dContent.ContentPush = content.tContentPush.OrderByDescending(cp => cp.PushDateTime).FirstOrDefault().PushDateTime;
                        }
                        else
                        {
                            dContent.ContentPush = string.Empty;
                        }
                    }

                    _mCustomerContents.Add(dContent);

                    // Categories
                    List<Guid> contentCategories = new List<Guid>();
                    foreach (DB.tContentCategory contentCategory in content.tContentCategory.Where(ct => ct.ContentParentCategoryGuid != Guid.Empty))
                    {
                        contentCategories.Add(contentCategory.ContentCategoryGuid);
                    }
                    dContent.Categories = contentCategories;
                //}
            }
            // LOG CUSTOMER VIEW
            if (customer!=null && customer.CustomerGuid!=Guid.Empty)
                new ML.Customer.CustomerContentTableAdapters.tCustomerContentTableAdapter().InsertCustomerContent(Guid.NewGuid(), customer.CustomerGuid, guidContentGuid, (int)DB.CustomerContentRepository.CustomerContentType.View);
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
        }

        public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth, string customerId)
        {
            if (Redirection.IsValid(companyId,
                Request.RequestUri.ToString()
                , "CustomerContentController-> public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth, string customerId)"
                , @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/" + customerId
                )
                )
            {
                APIRedirect apiRedirect = new APIRedirect();
                string url = @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/" + customerId;
                return apiRedirect.GetRequst(url);
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && contentTemplateTypeId < 1 && (customerContentTypeId < 0 || customerContentTypeId > 4) && list < 0 && imageWidth < 1 && string.IsNullOrEmpty(customerId))
            {
                _dCustomerContent.Status = RestStatus.ParameterError;
                _dCustomerContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }
            Guid guidCompanyGuid = new Guid(companyId);

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _dCustomerContent.Status = RestStatus.AuthenticationFailed;
                _dCustomerContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)contentTemplateTypeId;

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(guidCompanyGuid, contentTemplateType);
            if (contentTemplate == null)
            {
                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Get Customer
            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            //TEMP FIX TO SUPPORT CONTENTS FOR BOTH HDPASSET & STJARNKLUBB
            if (customer == null && guidCompanyGuid.Equals(Guid.Parse("D88B1E81-4F87-4D2C-A7B1-41535E50816B")))
            {
                customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(Guid.Parse("2F998381-14F4-471E-8CFD-CB5000D6F2B3"), customerId);
            }
            else if (customer == null && guidCompanyGuid.Equals(Guid.Parse("2F998381-14F4-471E-8CFD-CB5000D6F2B3")))
            {
                customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(Guid.Parse("D88B1E81-4F87-4D2C-A7B1-41535E50816B"), customerId);
            }
            if (customer == null)
            {
                new Email.Email().SendDebug("CustomerContentController",  "guidCompanyGuid:" + guidCompanyGuid.ToString() + ", customerId:" + customerId);

                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Get Content
            const int ALL = 0;
            //const int VIEWED = 1;
            //const int REDEEMED = 2;
            //const int NEWEST = 3;
            //const int RANDOM = 4;

            IQueryable<DB.tContent> contents = null;
            if (customerContentTypeId == ALL)
            {
                contents = new DB.ContentRepository().GetAllActiveContent(customer.CustomerGuid, contentTemplate.ContentTemplateGuid);
            }
            //else if (customerContentTypeId == VIEWED)
            //{
            //    //contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 1).ToList();
            //    contents = new DB.ContentRepository().GetActiveContent(customer.CustomerGuid, contentTemplate.ContentTemplateGuid).ToList();
            //}
            //else if (customerContentTypeId == REDEEMED)
            //{
            //    contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 2).ToList();
            //}
            //else if (customerContentTypeId == NEWEST)
            //{
            //    contents = new DB.ContentRepository().GetActiveNewest(contentTemplate.ContentTemplateGuid, 10000);
            //}
            //else if (customerContentTypeId == RANDOM)
            //{
            //    contents = new DB.ContentRepository().GetActiveRandom(contentTemplate.ContentTemplateGuid, 10000);
            //}
            //// Cont...

            // Handle Corporate Siblings


            //contents = contents.Where(c => c.CorporateSiblingGuid == Guid.Empty || (c.CorporateSiblingGuid != Guid.Empty && c.CorporateSiblingEnabled == true));
            contents = contents.Where(c => c.CorporateSiblingGuid == null || (c.CorporateSiblingGuid != Guid.Empty && c.CorporateSiblingEnabled == true));
            //contents =
            //    from c in contents
            //    where c.CorporateSiblingGuid == null
            //    || (c.CorporateSiblingGuid != Guid.Empty
            //    && (bool)c.CorporateSiblingEnabled)
            //    select c;


            // Get CustomerContentAvailability
            List<DB.tCustomerContentAvailability> customerContentAvailabilities = new DB.CustomerContentAvailabilityRepository().GetActiveByCustomerGuidAndContentTemplateGuid(customer.CustomerGuid, contentTemplate.ContentTemplateGuid).ToList();

            // Get CustomerContent (Redeemed)
            List<DB.tCustomerContent> customerContents = new DB.CustomerContentRepository().GetByCustomerGuidAndContentType(customer.CustomerGuid, DB.CustomerContentRepository.CustomerContentType.Redeem).ToList();

            // Get CustomerContentFavorite
            List<DB.tCustomerContentFavorite> customerContentFavorites = new DB.CustomerContentFavoriteRepository().GetFavorites(customer.CustomerGuid).ToList();

            // Get CustomerContentOngoing
            List<DB.tCustomerContentOngoing> customerContentOngoings = new DB.CustomerContentOngoingRepository().GetOngoings(customer.CustomerGuid).ToList();


            // Prepare Template
            XmlDocument xmlTemplate = new XmlDocument();
            xmlTemplate.LoadXml(ML.Common.XmlHelper.AddRootTags(contentTemplate.XmlContentTemplate));

            // Get contain-list
            List<string> lst = ML.Common.XmlHelper.ExtractCustomList(xmlTemplate, list);

            // Populate
            _mCustomerContents.Clear();
            if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                //Guid guidTmp = new Guid("0E047E88-7D8F-4A0E-BE45-9B9D882E8300");
                foreach (DB.tContent content in contents)
                {
                    // Static fields
                    dynamic dContent = new ExpandoObject();
                    dContent.ContentGuid = content.ContentGuid;
                    dContent.Name = content.Name;
                    dContent.Description = content.Description;
                    dContent.Identifier = content.Identifier;
                    dContent.PublishDateTime = content.PublishDateTime.ToString();
                    dContent.TerminationDateTime = content.TerminationDateTime.ToString();
                    dContent.OpenNotRedeemable = content.OpenNotRedeemable;
                    dContent.ContentType = content.tContentType.Identifier;

                    List<Dictionary<string, string>> latLngs = new List<Dictionary<string, string>>();
                    foreach (string latLng in content.Map.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(latLng))
                        {
                            string[] temp = latLng.Split(';');
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Lat", temp[0].Split(',')[0]);
                            dic.Add("Lng", temp[0].Split(',')[1]);
                            latLngs.Add(dic);
                        }
                    }
                    dContent.LatLng = latLngs;

                    // Dynamic fields (all or based on contain-list number)
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
                    foreach (XmlNode xmlNode in xml.SelectSingleNode("root"))
                    {
                        if ((lst.Count > 0 && lst.Contains(xmlNode.Name)) || lst.Count == 0)
                        {
                            if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"] != null)
                            {
                                if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "image")
                                {
                                    string strFileName = xmlNode.SelectSingleNode("name").InnerText;
                                    string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
                                    ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", guidCompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, guidCompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                                }
                                //else if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "text")
                                //{
                                //    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                //}
                                //else if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "textarea")
                                //{
                                //    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                //}
                                else
                                {
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                }
                            }
                            else
                            {
                                (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                            }
                        }
                    }


                    // Customer unique

                    // Customer Content Availability
                    dContent.Availability = 0;
                    foreach(DB.tCustomerContentAvailability customerContentAvailability in customerContentAvailabilities)
                    {
                        if(content.ContentGuid == customerContentAvailability.ContentGuid)
                        {
                            dContent.Availability = customerContentAvailability.NoOfRedeems;
                            customerContentAvailabilities.Remove(customerContentAvailability);
                            break;
                        }
                    }

                    // Customer Content (Redeemed)
                    dContent.Redeemed = string.Empty;
                    foreach (DB.tCustomerContent customerContent in customerContents)
                    {
                        if(content.ContentGuid == customerContent.ContentGuid)
                        {
                            dContent.Redeemed = customerContent.TimeStamp;
                            customerContents.Remove(customerContent);
                            break;
                        }
                    }

                    // Customer Content Favorites
                    dContent.Favorite = 0;
                    foreach (DB.tCustomerContentFavorite customerContentFavorite in customerContentFavorites)
                    {
                        if (content.ContentGuid == customerContentFavorite.ContentGuid)
                        {
                            dContent.Favorite = 1;
                            customerContentFavorites.Remove(customerContentFavorite);
                            break;
                        }
                    }

                    // Customer Content Ongoing
                    dContent.Ongoing = string.Empty;
                    foreach (DB.tCustomerContentOngoing customerContentOngoing in customerContentOngoings)
                    {
                        if (content.ContentGuid == customerContentOngoing.ContentGuid)
                        {
                            dContent.Ongoing = customerContentOngoing.TimeStamp.ToString();
                            customerContentOngoings.Remove(customerContentOngoing);
                            break;
                        }
                    }

                    // ContentPush
                    if (content.tContentPush.Any())
                    {
                        dContent.ContentPush = content.tContentPush.OrderByDescending(cp => cp.PushDateTime).FirstOrDefault().PushDateTime;
                    }
                    else
                    {
                        dContent.ContentPush = string.Empty;
                    }



                    _mCustomerContents.Add(dContent);

                    // Categories
                    List<Guid> contentCategories = new List<Guid>();
                    foreach (DB.tContentCategory contentCategory in content.tContentCategory.Where(ct => ct.ContentParentCategoryGuid != Guid.Empty))
                    {
                        contentCategories.Add(contentCategory.ContentCategoryGuid);
                    }
                    dContent.Categories = contentCategories;
                }
            }
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
        }

        public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth)
        {
            if (Redirection.IsValid(companyId,
                Request.RequestUri.ToString()
                , "CustomerContentController-> public HttpResponseMessage Get(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth)"
                , @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/"
                )
                )
            {
                APIRedirect apiRedirect = new APIRedirect();
                string url = @"api/customercontent/Get/" + secret + "/" + companyId + "/" + contentTemplateTypeId + "/" + customerContentTypeId + "/" + list + "/" + imageWidth + "/";
                return apiRedirect.GetRequst(url);
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && contentTemplateTypeId < 1 && (customerContentTypeId < 1 || customerContentTypeId > 4) && list < 1 && imageWidth < 1)
            {
                _dCustomerContent.Status = RestStatus.ParameterError;
                _dCustomerContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }
            Guid guidCompanyGuid = new Guid(companyId);

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _dCustomerContent.Status = RestStatus.AuthenticationFailed;
                _dCustomerContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)contentTemplateTypeId;

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(guidCompanyGuid, contentTemplateType);
            if (contentTemplate == null)
            {
                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Get Content
            const int VIEWED = 1;
            const int REDEEMED = 2;
            const int NEWEST = 3;
            const int RANDOM = 4;

            List<DB.tContent> contents = null;
            if (customerContentTypeId == VIEWED)
            {
                contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 1);
            }
            else if (customerContentTypeId == REDEEMED)
            {
                contents = new DB.ContentRepository().GetActiveMostVisited(contentTemplate.ContentTemplateGuid, 2);
            }
            else if (customerContentTypeId == NEWEST)
            {
                contents = new DB.ContentRepository().GetActiveNewest(contentTemplate.ContentTemplateGuid, 10000);
            }
            else if (customerContentTypeId == RANDOM)
            {
                contents = new DB.ContentRepository().GetActiveRandom(contentTemplate.ContentTemplateGuid, 10000);
            }
            else
            {
                 contents = new DB.ContentRepository().GetActiveByContentTemplateGuid(contentTemplate.ContentTemplateGuid).ToList();
            }
            // Cont...

            //contents = contents.Where(c => c.CorporateSiblingGuid == null || (c.CorporateSiblingGuid != Guid.Empty && c.CorporateSiblingEnabled == true));
            
            // Prepare Template
            XmlDocument xmlTemplate = new XmlDocument();
            xmlTemplate.LoadXml(ML.Common.XmlHelper.AddRootTags(contentTemplate.XmlContentTemplate));

            // Get contain-list
            List<string> lst = ML.Common.XmlHelper.ExtractCustomList(xmlTemplate, list);
            
            // Populate
            _mCustomerContents.Clear();
            if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                foreach (DB.tContent content in contents)
                {
                    // Static fields
                    dynamic dContent = new ExpandoObject();
                    dContent.ContentGuid = content.ContentGuid;
                    dContent.Name = content.Name;
                    dContent.Description = content.Description;
                    dContent.Identifier = content.Identifier;
                    dContent.PublishDateTime = content.PublishDateTime.ToString();
                    dContent.TerminationDateTime = content.TerminationDateTime.ToString();
                    dContent.ContentType = content.tContentType.Identifier;

                    List<Dictionary<string, string>> latLngs = new List<Dictionary<string, string>>();
                    foreach (string latLng in content.Map.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(latLng))
                        {
                            string[] temp = latLng.Split(';');
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("Lat", temp[0].Split(',')[0]);
                            dic.Add("Lng", temp[0].Split(',')[1]);
                            latLngs.Add(dic);
                        }
                    }
                    dContent.LatLng = latLngs;
                    
                    // Dynamic fields (all or based on contain-list number)
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
                    foreach (XmlNode xmlNode in xml.SelectSingleNode("root"))
                    {
                        if ((lst.Count > 0 && lst.Contains(xmlNode.Name)) || lst.Count == 0)
                        {
                            if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"] != null)
                            {
                                if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "image")
                                {
                                    string strFileName = xmlNode.SelectSingleNode("name").InnerText;
                                    string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
                                    ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", guidCompanyGuid.ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, guidCompanyGuid.ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));
                                }
                                //else if (xmlTemplate.SelectSingleNode("root").SelectSingleNode(xmlNode.Name).Attributes["type"].Value == "text")
                                //{
                                //    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                //}
                                //else
                                //{
                                //    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Empty;
                                //}
                                else
                                {
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                                }
                            }
                            else
                            {
                                (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                            }
                        }
                    }
                    _mCustomerContents.Add(dContent);

                    // Categories
                    List<Guid> contentCategories = new List<Guid>();
                    foreach(DB.tContentCategory contentCategory in content.tContentCategory.Where(ct => ct.ContentParentCategoryGuid != Guid.Empty))
                    {
                        contentCategories.Add(contentCategory.ContentCategoryGuid);
                    }
                    dContent.Categories = contentCategories;
                } 
            }
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
        }





    }
}
