using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
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
    public class CompanyContentDetailsController : ApiController
    {
        List<CompanyContentDetail> _mCompanyContentDetails = new List<CompanyContentDetail>();
        CompanyContentDetail _mCompanyContentDetail = new CompanyContentDetail();

        public CompanyContentDetailsController()
        {
            _mCompanyContentDetails.Add(_mCompanyContentDetail);
        }

        //public HttpResponseMessage Get(string secret, string companyId, string identifier, string type, int imageWidth, int priceType)
        //{
        //    // TODO
        //    return null;
        //}

        /// <summary>
        /// Get Content details
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = ContentId (Guid)
        /// <param name="imageWidth"></param>
        /// <param name="priceType"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string secret, string companyId, string id, int imageWidth, int priceType)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
            {
                _mCompanyContentDetail.Status = RestStatus.ParameterError;
                _mCompanyContentDetail.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContentDetails));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mCompanyContentDetail.Status = RestStatus.AuthenticationFailed;
                _mCompanyContentDetail.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContentDetails));
            }

            Guid guidCompanyGuid = new Guid(companyId);

            // Get Main Content
            DB.tContent content = new DB.ContentRepository().GetContent(new Guid(id));
            if (content == null)
            {
                _mCompanyContentDetail.Status = RestStatus.NotExisting;
                _mCompanyContentDetail.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContentDetails));
            }

            bool bIncludedSubContentOwing = false;
            try
            {
                bIncludedSubContentOwing = (bool)content.IncludedSubContentOwing;
            }
            catch { }


            // Ensure content is within company
            if (content.tContentTemplate.CompanyGuid != guidCompanyGuid)
            {
                _mCompanyContentDetail.Status = RestStatus.NotExisting;
                _mCompanyContentDetail.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContentDetails));
            }

            _mCompanyContentDetail.Name = ML.Site.Helper.ExtractTag(content.Xml, "Namn", false, true);
            _mCompanyContentDetail.Name = string.IsNullOrEmpty(_mCompanyContentDetail.Name) ? content.Name : ML.Site.Helper.ExtractTag(content.Xml, "Namn", false, true);
            _mCompanyContentDetail.Description = ML.Site.Helper.ExtractTag(content.Xml, "Beskrivning", false, true);
            _mCompanyContentDetail.Description = string.IsNullOrEmpty(_mCompanyContentDetail.Description) ? content.Description : _mCompanyContentDetail.Description;

            // Handle Price Type
            DB.ContentRepository.PriceType thePriceType = (DB.ContentRepository.PriceType)Enum.Parse(typeof(DB.ContentRepository.PriceType), priceType.ToString());
            _mCompanyContentDetail.Price = content.Price;
            if (thePriceType == DB.ContentRepository.PriceType.Delivery)
            {
                _mCompanyContentDetail.Price = content.DeliveryPrice == 0 ? content.Price : content.DeliveryPrice;
            }

            _mCompanyContentDetail.VAT = content.VAT;
            _mCompanyContentDetail.NoOfFreeSubContent = content.NoOfFreeSubContent;

            // Get Variant templates
            bool bFound = false;
            List<Models.ContentDetailVariant> contentDetailVariants = new List<ContentDetailVariant>();
            IQueryable<DB.tContentVariantTemplate> contentVariantTemplates = new DB.ContentVariantTemplateRepository().GetByContentTemplateGuid(content.ContentTemplateGuid);
            foreach (DB.tContentVariantTemplate contentVariantTemplate in contentVariantTemplates)
            {
                // Get Variants
                foreach (DB.tContentVariant contentVariant in content.tContentVariant)
                {
                    contentVariant.DeliveryPrice = contentVariant.DeliveryPrice == 0 ? contentVariant.Price : contentVariant.DeliveryPrice;

                    if (contentVariant.ContentVariantTemplateGuid == contentVariantTemplate.ContentVariantTemplateGuid)
                    {
                        if (thePriceType == DB.ContentRepository.PriceType.Regular && contentVariant.Price > 0)
                        {
                            Models.ContentDetailVariant contentDetailVariant = new ContentDetailVariant();
                            contentDetailVariants.Add(contentDetailVariant);
                            contentDetailVariant.ContentVariantId = contentVariant.ContentVariantGuid.ToString();
                            contentDetailVariant.Name = contentVariantTemplate.Name;
                            contentDetailVariant.Price = contentVariant.Price;
                            if (content.Price == contentVariant.Price)
                            {
                                contentDetailVariant.Default = true;
                            }
                            else
                            {
                                contentDetailVariant.Default = false;
                            }

                            bFound = true;
                            break;
                        }
                        else if (thePriceType == DB.ContentRepository.PriceType.Delivery && contentVariant.DeliveryPrice > 0)
                        {
                            Models.ContentDetailVariant contentDetailVariant = new ContentDetailVariant();
                            contentDetailVariants.Add(contentDetailVariant);
                            contentDetailVariant.ContentVariantId = contentVariant.ContentVariantGuid.ToString();
                            contentDetailVariant.Name = contentVariantTemplate.Name;
                            contentDetailVariant.Price = contentVariant.DeliveryPrice;
                            if (content.DeliveryPrice == contentVariant.DeliveryPrice)
                            {
                                contentDetailVariant.Default = true;
                            }
                            else
                            {
                                contentDetailVariant.Default = false;
                            }

                            bFound = true;
                            break;
                        }
                    }
                }
            }

            _mCompanyContentDetail.ContentDetailVariants = contentDetailVariants;

            // Get Sub Contents
            List<Models.SubContentIncluded> subContentIncludeds = new List<SubContentIncluded>();
            IQueryable<DB.ContentSubContentExtended> contentSubContentExtendeds = new DB.ContentSubContentRepository().GetSubContentByContentGuid(content.ContentGuid);
            
            // Included
            DB.tContentSpecial contentSpecial = new DB.ContentSpecialRepository().GetContentSpecial(guidCompanyGuid, DB.ContentSpecialRepository.ContentSpecialType.IncludedContent);
            bFound = false;
            foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
            {
                if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent == 0)
                {
                    if (contentSubContentExtendeds.Any() && !bFound)
                    {
                        bFound = true;
                    }

                    Guid guidContentSpecialGuid = Guid.Empty;
                    if (contentSpecial != null)
                    {
                        guidContentSpecialGuid = contentSpecial.ContentSpecialGuid;
                    }
                    //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"1\" data-amount=\"{1}\" owing=\"{1}\" group=\"-1\" onClick=\"UncheckContentSpecial('{2}x{3}');SumSubContent();\" checked>", contentSubContentExtended.ContentGuid.ToString(), bIncludedSubContentOwing ? Convert.ToInt32(contentSubContentExtended.Price).ToString() : "0", contentSubContentExtended.ContentGuid.ToString(), guidContentSpecialGuid.ToString()));
                    //sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
                    Models.SubContentIncluded contentDetailIncluded = new SubContentIncluded();
                    subContentIncludeds.Add(contentDetailIncluded);
                    contentDetailIncluded.ContentId = contentSubContentExtended.ContentGuid.ToString();
                    contentDetailIncluded.Name = contentSubContentExtended.Name;
                    contentDetailIncluded.Price = contentSubContentExtended.Price;
                    contentDetailIncluded.Owing = bIncludedSubContentOwing ? contentSubContentExtended.Price : 0;
                    contentDetailIncluded.SpecialId = guidContentSpecialGuid.ToString();
                    contentDetailIncluded.NoOfFreeSubContent = false;

                    if (contentSpecial != null)
                    {
                        if (contentSpecial.ContentSpecialGuid != Guid.Empty)
                        {
                            //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"special_{0}\" id=\"{0}\" value=\"{0}\" special=\"1\" data-amount=\"{1}\" owing=\"{1}\" group=\"-2\" onClick=\"CheckContent('{0}', '{2}');SumSubContent();\" >", string.Format("{0}x{1}", contentSubContentExtended.ContentGuid.ToString(), contentSpecial.ContentSpecialGuid.ToString()), Convert.ToInt32(contentSubContentExtended.Price).ToString(), contentSubContentExtended.ContentGuid.ToString()));
                            //sbSubContents.Append(string.Format("{0} ({1} kr)", contentSpecial.Name, Convert.ToInt32(contentSubContentExtended.Price).ToString()));
                            contentDetailIncluded.SpecialName = contentSpecial.Name;
                        }
                    }
                }
                else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent > 0)
                {
                    if (contentSubContentExtendeds.Any() && !bFound)
                    {
                        if (content.NoOfFreeSubContent == 1)
                        {
                            //sbSubContents.Append("<label>Ett tillval inkluderat:</label>");
                        }
                        else
                        {
                            //sbSubContents.Append(string.Format("<label>{0} tillval inkluderade:</label>", content.NoOfFreeSubContent.ToString()));
                        }
                        bFound = true;
                    }
                    //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"1\" group=\"topping\" onClick=\"HandleMaxSubContent('topping', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), content.NoOfFreeSubContent.ToString()));
                    //sbSubContents.Append(string.Format("{0}", ML.Common.Text.Truncate(contentSubContentExtended.Name, "(", ")")));
                    Models.SubContentIncluded contentDetailIncluded = new SubContentIncluded();
                    subContentIncludeds.Add(contentDetailIncluded);
                    contentDetailIncluded.ContentId = contentSubContentExtended.ContentGuid.ToString();
                    contentDetailIncluded.Name = contentSubContentExtended.Name;
                    contentDetailIncluded.Price = 0;
                    contentDetailIncluded.Owing = bIncludedSubContentOwing ? contentSubContentExtended.Price : 0;
                    contentDetailIncluded.NoOfFreeSubContent = true;
                }
            }

            _mCompanyContentDetail.SubContentIncludeds = subContentIncludeds;


            // Excluded
            List<Models.SubContentExcluded> subContentExcludeds = new List<SubContentExcluded>();
            bFound = false;
            foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
            {
                if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent == 0)
                {
                }
                else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent > 0)
                {
                }
                else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && !contentSubContentExtended.ContentSubContentGroupDefault)
                {
                }
                else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
                {
                }
                else
                {
                    if (!bFound)
                    {
                        //sbSubContents.Append("<div id=\"contentExtended\" >");
                        //sbSubContents.Append("<label>Extra tillval:</label>");
                        bFound = true;
                    }
                    Models.SubContentExcluded subContentExcluded = new SubContentExcluded();
                    subContentExcludeds.Add(subContentExcluded);
                    subContentExcluded.ContentId = contentSubContentExtended.ContentGuid.ToString();
                    subContentExcluded.Name = contentSubContentExtended.Name;
                    subContentExcluded.Price = contentSubContentExtended.Price;
                    //sbSubContents.Append("<div class=\"checkbox\"><label>");
                    //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"excluded_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" data-amount=\"{1}\" group=\"0\" onClick=\"SumSubContent();\" >", contentSubContentExtended.ContentGuid.ToString(), Convert.ToInt32(contentSubContentExtended.Price).ToString()));

                    if (contentSubContentExtended.Price == 0)
                    {
                        //sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
                        subContentExcluded.Name = contentSubContentExtended.Name;
                    }
                    else
                    {
                        //sbSubContents.Append(string.Format("{0} ({1} kr)", contentSubContentExtended.Name, contentSubContentExtended.Price.ToString()));
                        subContentExcluded.Name = string.Format("{0} ({1} kr)", contentSubContentExtended.Name, contentSubContentExtended.Price.ToString());
                    }
                }
            }

            _mCompanyContentDetail.SubContentExcludeds = subContentExcludeds;


            // Grouped
            // Lookup Default Group content
            bool bContentSubContentGroupDefault = false;
            foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
            {
                if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
                {
                    bContentSubContentGroupDefault = true;
                }
            }

            List<Models.SubContentGrouped> subContentGroupeds = new List<SubContentGrouped>();
            bFound = false;
            foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
            {
                if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && !contentSubContentExtended.ContentSubContentGroupDefault)
                {
                    if (!bFound)
                    {
                        //sbSubContents.Append(string.Format("<label style=\"padding-top:10px;\">{0}:</label>", ML.Common.Text.Truncate(contentSubContentExtended.ContentSubContentGroupName, "(", ")")));

                        _mCompanyContentDetail.SubContentGroupName = ML.Common.Text.Truncate(contentSubContentExtended.ContentSubContentGroupName, "(", ")");
                        bFound = true;
                    }

                    Models.SubContentGrouped subContentGrouped = new SubContentGrouped();
                    subContentGroupeds.Add(subContentGrouped);
            //        sbSubContents.Append("<div class=\"checkbox\"><label>");
            //        //sbSubContents.Append(string.Format("<input type=\"checkbox\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent({1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
                    subContentGrouped.ContentId = contentSubContentExtended.ContentGuid.ToString();
                    subContentGrouped.Max = contentSubContentExtended.Max;
                    subContentGrouped.Default = false;
                    if (contentSubContentExtended.Max > 1)
                    {
                        //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
                    }
                    else
                    {
                        if (bContentSubContentGroupDefault)
                        {
                            //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" disabled='disabled' group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
                            subContentGrouped.Default = true;
                        }
                        else
                        {
                            //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
                            subContentGrouped.Default = false;
                        }
                    }
                    //sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
                    //sbSubContents.Append("</label></div>");
                    subContentGrouped.Name = contentSubContentExtended.Name;
                    subContentGrouped.Mandatory = contentSubContentExtended.GroupMandatory;
                }
                else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
                {
                    if (!bFound)
                    {
                        //sbSubContents.Append(string.Format("<hr><label>{0}</label>", contentSubContentExtended.ContentSubContentGroupName));
                        _mCompanyContentDetail.SubContentGroupName = contentSubContentExtended.ContentSubContentGroupName;
                        bFound = true;
                    }

                    //sbSubContents.Append("<div class=\"checkbox\"><label>");
                    //sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" checked onClick=\"HandleMaxSubContent('1', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
                    //sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
                    //sbSubContents.Append("</label></div>");
                    Models.SubContentGrouped subContentGrouped = new SubContentGrouped();
                    subContentGroupeds.Add(subContentGrouped);
                    subContentGrouped.ContentId = contentSubContentExtended.ContentGuid.ToString();
                    subContentGrouped.Max = contentSubContentExtended.Max;
                    subContentGrouped.Default = false;
                    subContentGrouped.Name = contentSubContentExtended.Name;
                    subContentGrouped.Mandatory = contentSubContentExtended.GroupMandatory;
                }
            }

            _mCompanyContentDetail.SubContentGroupeds = subContentGroupeds;




            //// Image(s)
            //StringBuilder sbImages = new StringBuilder();
            ////bool bFound = false;
            //bFound = false;
            //List<ML.Rest2.Models.ContentCustom> contentCustoms = new List<ContentCustom>();
            //XmlDocument xmlDocument = new XmlDocument();
            //xmlDocument.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
            //foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("root"))
            //{
            //    ML.Rest2.Models.ContentCustom contentCustom = new ContentCustom();
            //    contentCustom.Name = xmlNode.Name;
            //    contentCustom.Type = xmlNode.Attributes["type"] == null ? string.Empty : xmlNode.Attributes["type"].InnerText;
            //    if (contentCustom.Type == "image")
            //    {
            //        if (xmlNode.SelectSingleNode("name") != null)
            //        {
            //            string strFileName = xmlNode.SelectSingleNode("name").InnerText;
            //            string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
            //            ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", new Guid(companyId).ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
            //            contentCustom.Value = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, new Guid(companyId).ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));

            //            // Image
            //            sbImages.Append("<div>");
            //            sbImages.Append(string.Format("<img id=\"{0}\" style='display:block;margin:auto;' src=\"{1}\">", contentCustom.Name, contentCustom.Value));
            //            sbImages.Append("</div>");
            //        }
            //        else
            //        {
            //            contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
            //        }
            //        bFound = true;
            //    }
            //    //else
            //    //{
            //    //    contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
            //    //}

            //    if (bFound)
            //    {
            //        contentCustoms.Add(contentCustom);
            //    }
            //}
            ////_mCompanyContent.ContentCustom = contentCustoms;
            //_mCompanyContent.Images = sbImages.ToString();

            // Success
            _mCompanyContentDetail.Status = RestStatus.Success;
            _mCompanyContentDetail.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContentDetails));
        }
    }
}
