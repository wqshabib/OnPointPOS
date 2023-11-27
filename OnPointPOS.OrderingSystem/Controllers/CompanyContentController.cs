
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
using Models;
using ML.Common.Handlers.Serializers;

namespace ML.Rest2.Controllers
{
    [RoutePrefix("api/CompanyContent")]
    public class CompanyContentController : ApiController
    {
        List<CompanyContent> _mCompanyContents = new List<CompanyContent>();
        CompanyContent _mCompanyContent = new CompanyContent();

        public CompanyContentController()
        {
            _mCompanyContents.Add(_mCompanyContent);
        }

        /// <summary>
        /// Get Content details
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = ContentId (Guid)
        /// <returns></returns>
        /// 
        public HttpResponseMessage Get(string secret, string companyId, string id, int imageWidth)
        {
            //return Get(secret, companyId, id, imageWidth, 1, 0);
            return Get(secret, companyId, id, imageWidth, 1);
        }

        //public HttpResponseMessage Get(string secret, string companyId, string id, int imageWidth, int priceType, int includedSubContentOwing)
        public HttpResponseMessage Get(string secret, string companyId, string id, int imageWidth, int priceType)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
            {
                _mCompanyContent.Status = RestStatus.ParameterError;
                _mCompanyContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
            }

            Guid guidCompanyGuid = new Guid(companyId);
            _mCompanyContent.Name = "Margherita ";
            _mCompanyContent.Name = "Margherita ";
            _mCompanyContent.Description = string.Empty;
            _mCompanyContent.Description = string.Empty;
            _mCompanyContent.Price = Convert.ToDecimal(10.0);
            _mCompanyContent.VAT = Convert.ToDecimal(12.0);
            // Handle Price Type



            // Get Variant templates
            StringBuilder sbVariants = new StringBuilder();
            sbVariants.Append(string.Format("<label id=\"price\" data-amount=\"{0}\">", Convert.ToInt32(10).ToString()));
            sbVariants.Append(string.Format("{0} kr", Convert.ToInt32(10).ToString()));
            sbVariants.Append("</label>");
            _mCompanyContent.Variants = sbVariants.ToString();

            Guid contentsubContentGuid = Guid.Parse("3f6c73f1-80b0-49f1-aca6-573e9dace3a7");
            Guid cont = Guid.Parse("3f6c73f1-80b0-49f1-aca6-573e9dace3a7");
            Guid contentSubContentExtendedguid = Guid.Parse("1b3a3780-97bf-498f-b884-594a86187c16");
            Guid contentsub = Guid.Parse("824e52c7-71ed-461a-8596-ae4d68e871e3");
            // Get Sub Contents
            StringBuilder sbSubContents = new StringBuilder();

            sbSubContents.Append("<br>");
            sbSubContents.Append("<label>Inkluderade tillval:</label>");
            sbSubContents.Append("<div style=\"display:block; clear:both;\">");
            sbSubContents.Append("<div class=\"checkbox\" style=\"float:left;\"><label>");
            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"1\" data-amount=\"{1}\" owing=\"{1}\" group=\"-1\" onClick=\"UncheckContentSpecial('{2}x{3}');SumSubContent();\" checked>", contentsubContentGuid.ToString(), "0", cont.ToString(), Guid.Empty.ToString()));
            //sbSubContents.Append(string.Format("<input type=\"hidden");
            sbSubContents.Append(string.Format("{0}", "Basilika"));
            sbSubContents.Append("</label>");
            sbSubContents.Append("</div>");
            sbSubContents.Append("</div>");
            sbSubContents.Append("<br><hr>");
            sbSubContents.Append("<div id=\"contentExtended\" >");
            sbSubContents.Append("<label>Extra tillval:</label>");
            sbSubContents.Append("<div class=\"checkbox\"><label>");
            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"excluded_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" data-amount=\"{1}\" group=\"0\" onClick=\"SumSubContent();\" >", contentSubContentExtendedguid.ToString(), Convert.ToInt32(20).ToString()));
            sbSubContents.Append(string.Format("{0}", "Vitloksås(20 kr)"));
            sbSubContents.Append("</label></div>");
            sbSubContents.Append("</div>");
            sbSubContents.Append("<hr>");
            sbSubContents.Append(string.Format("<label class='group-title' data-title='{0}' style=\"padding-top:10px;\">{0}:</label>", "Välj Sås:"));
            sbSubContents.Append("<div class=\"checkbox\"><label>");
            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" mandatory='{2}' >", contentsub.ToString(), "1", "0"));
            sbSubContents.Append(string.Format("{0}", "Kebabsås"));
            sbSubContents.Append("</label></div>");
            sbSubContents.Append("<hr>");
            _mCompanyContent.SubContents = sbSubContents.ToString();



            //_mCompanyContent.ContentCustom = contentCustoms;
            _mCompanyContent.Images = string.Empty;

            // Success
            _mCompanyContent.Status = RestStatus.Success;
            _mCompanyContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        }




        //public HttpResponseMessage GetOld(string secret, string companyId, string id, int imageWidth, int priceType)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(id))
        //    {
        //        _mCompanyContent.Status = RestStatus.ParameterError;
        //        _mCompanyContent.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCompanyContent.Status = RestStatus.AuthenticationFailed;
        //        _mCompanyContent.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        //    }

        //    Guid guidCompanyGuid = new Guid(companyId);

        //    //bool bIncludedSubContentOwing = includedSubContentOwing == 1 ? true : false;



        //    // Get Main Content
        //    DB.tContent content = new DB.ContentRepository().GetContent(new Guid(id));
        //    if (content == null)
        //    {
        //        _mCompanyContent.Status = RestStatus.NotExisting;
        //        _mCompanyContent.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        //    }

        //    bool bIncludedSubContentOwing = false;
        //    try
        //    {
        //        bIncludedSubContentOwing = (bool)content.IncludedSubContentOwing;
        //    }
        //    catch { }


        //    // Ensure content is within company
        //    if (content.tContentTemplate.CompanyGuid != guidCompanyGuid)
        //    {
        //        _mCompanyContent.Status = RestStatus.NotExisting;
        //        _mCompanyContent.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        //    }

        //    _mCompanyContent.Name = ML.Site.Helper.ExtractTag(content.Xml, "Namn", false, true);
        //    _mCompanyContent.Name = string.IsNullOrEmpty(_mCompanyContent.Name) ? content.Name : ML.Site.Helper.ExtractTag(content.Xml, "Namn", false, true);
        //    _mCompanyContent.Description = ML.Site.Helper.ExtractTag(content.Xml, "Beskrivning", false, true);
        //    _mCompanyContent.Description = string.IsNullOrEmpty(_mCompanyContent.Description) ? content.Description : _mCompanyContent.Description;

        //    // Handle Price Type
        //    DB.ContentRepository.PriceType thePriceType = (DB.ContentRepository.PriceType)Enum.Parse(typeof(DB.ContentRepository.PriceType), priceType.ToString());
        //    _mCompanyContent.Price = content.Price;
        //    if (thePriceType == DB.ContentRepository.PriceType.Delivery)
        //    {
        //        _mCompanyContent.Price = content.DeliveryPrice;
        //    }

        //    _mCompanyContent.VAT = content.VAT;

        //    // Get Variant templates
        //    bool bFound = false;
        //    StringBuilder sbVariants = new StringBuilder();
        //    IQueryable<DB.tContentVariantTemplate> contentVariantTemplates = new DB.ContentVariantTemplateRepository().GetByContentTemplateGuid(content.ContentTemplateGuid);
        //    foreach (DB.tContentVariantTemplate contentVariantTemplate in contentVariantTemplates)
        //    {
        //        // Get Variants
        //        foreach (DB.tContentVariant contentVariant in content.tContentVariant)
        //        {
        //            if (contentVariant.ContentVariantTemplateGuid == contentVariantTemplate.ContentVariantTemplateGuid)
        //            {
        //                if (thePriceType == DB.ContentRepository.PriceType.Regular && contentVariant.Price > 0)
        //                {
        //                    sbVariants.Append("<div class=\"radio\" style=\"text-align:left;\" >");
        //                    sbVariants.Append("<label>");
        //                    if (content.Price == contentVariant.Price)
        //                    {
        //                        sbVariants.Append(string.Format("<input type=\"radio\" name=\"variant\" id=\"{0}\" value=\"{0}\" data-amount=\"{1}\" data-name=\"{2}\" checked>", contentVariant.ContentVariantGuid.ToString(), Convert.ToInt32(contentVariant.Price).ToString(), contentVariantTemplate.Name));
        //                    }
        //                    else
        //                    {
        //                        sbVariants.Append(string.Format("<input type=\"radio\" name=\"variant\" id=\"{0}\" value=\"{0}\" data-amount=\"{1}\" data-name=\"{2}\">", contentVariant.ContentVariantGuid.ToString(), Convert.ToInt32(contentVariant.Price).ToString(), contentVariantTemplate.Name));
        //                    }
        //                    sbVariants.Append("<span>");
        //                    sbVariants.Append(string.Format("{0} <strong style='padding-left:10px;'>{1}:-</strong>", contentVariantTemplate.Name, Convert.ToInt32(contentVariant.Price).ToString()));
        //                    sbVariants.Append("</span>");
        //                    sbVariants.Append("</label>");
        //                    sbVariants.Append("</div>");

        //                    bFound = true;
        //                    break;
        //                }
        //                else if (thePriceType == DB.ContentRepository.PriceType.Delivery && contentVariant.DeliveryPrice > 0)
        //                {
        //                    sbVariants.Append("<div class=\"radio\" style=\"text-align:left;\" >");
        //                    sbVariants.Append("<label>");
        //                    if (content.DeliveryPrice == contentVariant.DeliveryPrice)
        //                    {
        //                        sbVariants.Append(string.Format("<input type=\"radio\" name=\"variant\" id=\"{0}\" value=\"{0}\" data-amount=\"{1}\" data-name=\"{2}\" checked>", contentVariant.ContentVariantGuid.ToString(), Convert.ToInt32(contentVariant.DeliveryPrice).ToString(), contentVariantTemplate.Name));
        //                    }
        //                    else
        //                    {
        //                        sbVariants.Append(string.Format("<input type=\"radio\" name=\"variant\" id=\"{0}\" value=\"{0}\" data-amount=\"{1}\" data-name=\"{2}\">", contentVariant.ContentVariantGuid.ToString(), Convert.ToInt32(contentVariant.DeliveryPrice).ToString(), contentVariantTemplate.Name));
        //                    }
        //                    sbVariants.Append("<span>");
        //                    sbVariants.Append(string.Format("{0} {1} kr", contentVariantTemplate.Name, Convert.ToInt32(contentVariant.DeliveryPrice).ToString()));
        //                    sbVariants.Append("</span>");
        //                    sbVariants.Append("</label>");
        //                    sbVariants.Append("</div>");

        //                    bFound = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    if (!bFound)
        //    {
        //        sbVariants.Append(string.Format("<label id=\"price\" data-amount=\"{0}\">", Convert.ToInt32(_mCompanyContent.Price).ToString()));
        //        sbVariants.Append(string.Format("{0} kr", Convert.ToInt32(_mCompanyContent.Price).ToString()));
        //        sbVariants.Append("</label>");
        //    }

        //    _mCompanyContent.Variants = sbVariants.ToString();

        //    // Get Sub Contents
        //    StringBuilder sbSubContents = new StringBuilder();
        //    IQueryable<DB.ContentSubContentExtended> contentSubContentExtendeds = new DB.ContentSubContentRepository().GetSubContentByContentGuid(content.ContentGuid);

        //    sbSubContents.Append("<br>");

        //    // Included
        //    DB.tContentSpecial contentSpecial = new DB.ContentSpecialRepository().GetContentSpecial(guidCompanyGuid, DB.ContentSpecialRepository.ContentSpecialType.IncludedContent);
        //    bFound = false;
        //    foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
        //    {
        //        if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent == 0)
        //        {
        //            if (contentSubContentExtendeds.Any() && !bFound)
        //            {
        //                sbSubContents.Append("<label>Inkluderade tillval:</label>");
        //                bFound = true;
        //            }

        //            sbSubContents.Append("<div style=\"display:block; clear:both;\">");
        //            sbSubContents.Append("<div class=\"checkbox\" style=\"float:left;\"><label>");
        //            Guid guidContentSpecialGuid = Guid.Empty;
        //            if (contentSpecial != null)
        //            {
        //                guidContentSpecialGuid = contentSpecial.ContentSpecialGuid;
        //            }
        //            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"1\" data-amount=\"{1}\" owing=\"{1}\" group=\"-1\" onClick=\"UncheckContentSpecial('{2}x{3}');SumSubContent();\" checked>", contentSubContentExtended.ContentGuid.ToString(), bIncludedSubContentOwing ? Convert.ToInt32(contentSubContentExtended.Price).ToString() : "0", contentSubContentExtended.ContentGuid.ToString(), guidContentSpecialGuid.ToString()));
        //            //sbSubContents.Append(string.Format("<input type=\"hidden");
        //            sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
        //            sbSubContents.Append("</label>");
        //            sbSubContents.Append("</div>");

        //            if (contentSpecial != null)
        //            {
        //                if (contentSpecial.ContentSpecialGuid != Guid.Empty)
        //                {
        //                    sbSubContents.Append("<div class=\"checkbox\" style=\"margin-left:30px; display:inline-block; \"><label>");
        //                    sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"special_{0}\" id=\"{0}\" value=\"{0}\" special=\"1\" data-amount=\"{1}\" owing=\"{1}\" group=\"-2\" onClick=\"CheckContent('{0}', '{2}');SumSubContent();\" >", string.Format("{0}x{1}", contentSubContentExtended.ContentGuid.ToString(), contentSpecial.ContentSpecialGuid.ToString()), Convert.ToInt32(contentSubContentExtended.Price).ToString(), contentSubContentExtended.ContentGuid.ToString()));
        //                    sbSubContents.Append(string.Format("{0} ({1} kr)", contentSpecial.Name, Convert.ToInt32(contentSubContentExtended.Price).ToString()));
        //                    sbSubContents.Append("</label></div>");
        //                }
        //            }

        //            sbSubContents.Append("</div>");
        //        }
        //        else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent > 0)
        //        {
        //            if (contentSubContentExtendeds.Any() && !bFound)
        //            {
        //                if (content.NoOfFreeSubContent == 1)
        //                {
        //                    sbSubContents.Append("<label>Ett tillval inkluderat:</label>");
        //                }
        //                else
        //                {
        //                    sbSubContents.Append(string.Format("<label>{0} tillval inkluderade:</label>", content.NoOfFreeSubContent.ToString()));
        //                }
        //                bFound = true;
        //            }
        //            sbSubContents.Append("<div class=\"checkbox\"><label>");
        //            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"1\" group=\"topping\" onClick=\"HandleMaxSubContent('topping', {1});\" >", contentSubContentExtended.ContentGuid.ToString(), content.NoOfFreeSubContent.ToString()));
        //            sbSubContents.Append(string.Format("{0}", ML.Common.Text.Truncate(contentSubContentExtended.Name, "(", ")")));
        //            sbSubContents.Append("</label></div>");
        //        }
        //    }
        //    if (bFound)
        //    {
        //        sbSubContents.Append("<br><hr>");
        //    }

        //    // Not included
        //    bFound = false;
        //    foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
        //    {
        //        if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent == 0)
        //        {
        //        }
        //        else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid == Guid.Empty && content.NoOfFreeSubContent > 0)
        //        {
        //        }
        //        else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && !contentSubContentExtended.ContentSubContentGroupDefault)
        //        {
        //        }
        //        else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
        //        {
        //        }
        //        else
        //        {
        //            if (!bFound)
        //            {
        //                sbSubContents.Append("<div id=\"contentExtended\" >");
        //                sbSubContents.Append("<label>Extra tillval:</label>");
        //                bFound = true;
        //            }
        //            sbSubContents.Append("<div class=\"checkbox\"><label>");
        //            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"excluded_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" data-amount=\"{1}\" group=\"0\" onClick=\"SumSubContent();\" >", contentSubContentExtended.ContentGuid.ToString(), Convert.ToInt32(contentSubContentExtended.Price).ToString()));

        //            if (contentSubContentExtended.Price == 0)
        //            {
        //                sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
        //            }
        //            else
        //            {
        //                sbSubContents.Append(string.Format("{0} ({1} kr)", contentSubContentExtended.Name, contentSubContentExtended.Price.ToString()));
        //            }
        //            sbSubContents.Append("</label></div>");
        //        }
        //    }
        //    if (bFound)
        //    {
        //        sbSubContents.Append("</div>");
        //        sbSubContents.Append("<hr>");
        //    }

        //    // Grouped
        //    // Lookup Default Group content
        //    bool bContentSubContentGroupDefault = false;
        //    foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
        //    {
        //        if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
        //        {
        //            bContentSubContentGroupDefault = true;
        //        }
        //    }

        //    bFound = false;
        //    foreach (DB.ContentSubContentExtended contentSubContentExtended in contentSubContentExtendeds)
        //    {
        //        if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && !contentSubContentExtended.ContentSubContentGroupDefault)
        //        {
        //            if (!bFound)
        //            {
        //                sbSubContents.Append(string.Format("<label class='group-title' data-title='{0}' style=\"padding-top:10px;\">{0}:</label>", ML.Common.Text.Truncate(contentSubContentExtended.ContentSubContentGroupName, "(", ")")));
        //                bFound = true;
        //            }

        //            sbSubContents.Append("<div class=\"checkbox\"><label>");
        //            //sbSubContents.Append(string.Format("<input type=\"checkbox\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent({1});\" >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString()));
        //            if (contentSubContentExtended.Max > 1)
        //            {
        //                sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" mandatory='{2}' >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString(), contentSubContentExtended.GroupMandatory ? "1" : "0"));
        //            }
        //            else
        //            {
        //                if (bContentSubContentGroupDefault)
        //                {
        //                    sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" disabled='disabled' group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" mandatory='{2}' >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString(), contentSubContentExtended.GroupMandatory ? "1" : "0"));
        //                }
        //                else
        //                {
        //                    sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" onClick=\"HandleMaxSubContent('1', {1});\" mandatory='{2}' >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString(), contentSubContentExtended.GroupMandatory ? "1" : "0"));
        //                }
        //            }
        //            sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
        //            sbSubContents.Append("</label></div>");
        //        }
        //        else if (contentSubContentExtended.Included && contentSubContentExtended.ContentSubContentGroupGuid != Guid.Empty && contentSubContentExtended.ContentSubContentGroupDefault)
        //        {
        //            if (!bFound)
        //            {
        //                sbSubContents.Append(string.Format("<hr><label>{0}</label>", contentSubContentExtended.ContentSubContentGroupName));
        //                bFound = true;
        //            }

        //            sbSubContents.Append("<div class=\"checkbox\"><label>");
        //            sbSubContents.Append(string.Format("<input type=\"checkbox\" name=\"included_{0}\" id=\"{0}\" value=\"{0}\" included=\"0\" group=\"1\" checked onClick=\"HandleMaxSubContent('1', {1});\" mandatory='{2}' >", contentSubContentExtended.ContentGuid.ToString(), contentSubContentExtended.Max.ToString(), contentSubContentExtended.GroupMandatory ? "1" : "0"));
        //            sbSubContents.Append(string.Format("{0}", contentSubContentExtended.Name));
        //            sbSubContents.Append("</label></div>");
        //        }
        //    }
        //    if (bFound)
        //    {
        //        sbSubContents.Append("<hr>");
        //    }

        //    _mCompanyContent.SubContents = sbSubContents.ToString();




        //    // Image(s)
        //    StringBuilder sbImages = new StringBuilder();
        //    //bool bFound = false;
        //    bFound = false;
        //    List<ContentCustom> contentCustoms = new List<ContentCustom>();
        //    XmlDocument xmlDocument = new XmlDocument();
        //    xmlDocument.LoadXml(ML.Common.XmlHelper.AddRootTags(content.Xml));
        //    foreach (XmlNode xmlNode in xmlDocument.SelectSingleNode("root"))
        //    {
        //        ContentCustom contentCustom = new ContentCustom();
        //        contentCustom.Name = xmlNode.Name;
        //        contentCustom.Type = xmlNode.Attributes["type"] == null ? string.Empty : xmlNode.Attributes["type"].InnerText;
        //        if (contentCustom.Type == "image")
        //        {
        //            if (xmlNode.SelectSingleNode("name") != null)
        //            {
        //                string strFileName = xmlNode.SelectSingleNode("name").InnerText;
        //                string strImageGuid = xmlNode.SelectSingleNode("imageguid").InnerText;
        //                ML.ImageLibrary.ImageHelper.ResizeImage(System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~/storage/{0}/contentimagebank/{1}/{2}", new Guid(companyId).ToString(), strImageGuid, strFileName)), Convert.ToInt32(imageWidth));
        //                contentCustom.Value = string.Format("http://{0}/storage/{1}/contentimagebank/{2}/{3}.{4}", ML.Common.Constants.MOBLINK_HOST_NAME, new Guid(companyId).ToString(), strImageGuid, imageWidth.ToString(), ML.Common.FileHelper.ExtractExtension(strFileName));

        //                // Image
        //                sbImages.Append("<div>");
        //                sbImages.Append(string.Format("<img id=\"{0}\" style='display:block;margin:auto;' src=\"{1}\">", contentCustom.Name, contentCustom.Value));
        //                sbImages.Append("</div>");
        //            }
        //            else
        //            {
        //                contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
        //            }
        //            bFound = true;
        //        }
        //        //else
        //        //{
        //        //    contentCustom.Value = xmlNode == null ? string.Empty : xmlNode.InnerText;
        //        //}

        //        if (bFound)
        //        {
        //            contentCustoms.Add(contentCustom);
        //        }
        //    }
        //    //_mCompanyContent.ContentCustom = contentCustoms;
        //    _mCompanyContent.Images = sbImages.ToString();

        //    // Success
        //    _mCompanyContent.Status = RestStatus.Success;
        //    _mCompanyContent.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyContents));
        //}


    }
}
