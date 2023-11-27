using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class CustomerContentSearchController : ApiController
    {
        List<dynamic> _mCustomerContents = new List<dynamic>();
        dynamic _dCustomerContent = new ExpandoObject();

        public CustomerContentSearchController()
        {
            _mCustomerContents.Add(_dCustomerContent);
        }

        public HttpResponseMessage Post(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth)
        {
            if (!Request.Content.IsFormData())
            {
                _dCustomerContent.Status = RestStatus.NotFormData;
                _dCustomerContent.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && contentTemplateTypeId < 1 && (customerContentTypeId < 0 || customerContentTypeId > 4) && list < 0 && imageWidth < 1)
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

            //// Get Customer
            //DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            //if (customer == null)
            //{
            //    _dCustomerContent.Status = RestStatus.NotExisting;
            //    _dCustomerContent.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            //}

            // Prepare
            System.Collections.Specialized.NameValueCollection dicSearch = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = string.IsNullOrEmpty(dicSearch["Search"]) ? string.Empty : dicSearch["Search"];

            if (string.IsNullOrEmpty(strSearch))
            {
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
                contents = new DB.ContentRepository().SearchActiveContent(contentTemplate.ContentTemplateGuid, strSearch);
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



            //// Get CustomerContentAvailability
            //List<DB.tCustomerContentAvailability> customerContentAvailabilities = new DB.CustomerContentAvailabilityRepository().GetActiveByCustomerGuidAndContentTemplateGuid(customer.CustomerGuid, contentTemplate.ContentTemplateGuid).ToList();

            //// Get CustomerContent (Redeemed)
            //List<DB.tCustomerContent> customerContents = new DB.CustomerContentRepository().GetByCustomerGuidAndContentType(customer.CustomerGuid, DB.CustomerContentRepository.CustomerContentType.Redeem).ToList();

            //// Get CustomerContentFavorite
            //List<DB.tCustomerContentFavorite> customerContentFavorites = new DB.CustomerContentFavoriteRepository().GetFavorites(customer.CustomerGuid).ToList();

            //// Get CustomerContentOngoing
            //List<DB.tCustomerContentOngoing> customerContentOngoings = new DB.CustomerContentOngoingRepository().GetOngoings(customer.CustomerGuid).ToList();


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
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Empty;
                                }
                            }
                            else
                            {
                                (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = xmlNode.InnerText;
                            }
                        }
                    }


                    //// Customer unique

                    //// Customer Content Availability
                    //dContent.Availability = 0;
                    //foreach (DB.tCustomerContentAvailability customerContentAvailability in customerContentAvailabilities)
                    //{
                    //    if (content.ContentGuid == customerContentAvailability.ContentGuid)
                    //    {
                    //        dContent.Availability = customerContentAvailability.NoOfRedeems;
                    //        customerContentAvailabilities.Remove(customerContentAvailability);
                    //        break;
                    //    }
                    //}

                    //// Customer Content (Redeemed)
                    //dContent.Redeemed = string.Empty;
                    //foreach (DB.tCustomerContent customerContent in customerContents)
                    //{
                    //    if (content.ContentGuid == customerContent.ContentGuid)
                    //    {
                    //        dContent.Redeemed = customerContent.TimeStamp;
                    //        customerContents.Remove(customerContent);
                    //        break;
                    //    }
                    //}

                    //// Customer Content Favorites
                    //dContent.Favorite = 0;
                    //foreach (DB.tCustomerContentFavorite customerContentFavorite in customerContentFavorites)
                    //{
                    //    if (content.ContentGuid == customerContentFavorite.ContentGuid)
                    //    {
                    //        dContent.Favorite = 1;
                    //        customerContentFavorites.Remove(customerContentFavorite);
                    //        break;
                    //    }
                    //}

                    //// Customer Content Ongoing
                    //dContent.Ongoing = string.Empty;
                    //foreach (DB.tCustomerContentOngoing customerContentOngoing in customerContentOngoings)
                    //{
                    //    if (content.ContentGuid == customerContentOngoing.ContentGuid)
                    //    {
                    //        dContent.Ongoing = customerContentOngoing.TimeStamp.ToString();
                    //        customerContentOngoings.Remove(customerContentOngoing);
                    //        break;
                    //    }
                    //}

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

        public HttpResponseMessage Post(string secret, string companyId, int contentTemplateTypeId, int customerContentTypeId, int list, int imageWidth, string customerId)
        {
            if (!Request.Content.IsFormData())
            {
                _dCustomerContent.Status = RestStatus.NotFormData;
                _dCustomerContent.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
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
            if (customer == null)
            {
                _dCustomerContent.Status = RestStatus.NotExisting;
                _dCustomerContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerContents));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dicSearch = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = string.IsNullOrEmpty(dicSearch["Search"]) ? string.Empty : dicSearch["Search"];

            if (string.IsNullOrEmpty(strSearch))
            {
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
                contents = new DB.ContentRepository().SearchActiveContent(contentTemplate.ContentTemplateGuid, strSearch, customer.CustomerGuid);
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
                                else
                                {
                                    (dContent as IDictionary<string, dynamic>)[xmlNode.Name] = string.Empty;
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
                    foreach (DB.tCustomerContentAvailability customerContentAvailability in customerContentAvailabilities)
                    {
                        if (content.ContentGuid == customerContentAvailability.ContentGuid)
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
                        if (content.ContentGuid == customerContent.ContentGuid)
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





    }
}
