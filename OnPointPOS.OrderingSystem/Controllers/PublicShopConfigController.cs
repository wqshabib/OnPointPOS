using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
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
using Models;

namespace ML.Rest2.Controllers
{
    public class PublicShopConfigController : ApiController
    {
        private List<Config.ShopConfig> _mShopConfigList = new List<Config.ShopConfig>();
        private Config.ShopConfig _mShopConfig = new Config.ShopConfig();

        public PublicShopConfigController()
        {
            _mShopConfigList.Add(_mShopConfig);
        }

        public HttpResponseMessage Get(string identifier)
        {
            List<dynamic> dCompanies = new List<dynamic>();
            dynamic dCompany = new ExpandoObject();
            dCompanies.Add(dCompany);

            //DB.tCompany company = new DB.CompanyRepository().GetByIdentifier(identifier);
            //if (company == null)
            //{
            //    dCompany.Status = RestStatus.NotExisting;
            //    dCompany.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dCompanies));
            //}

            //dCompany.CompanyId = company.CompanyGuid.ToString();
            var cmpGuid = "bf87fffb-0e6e-4812-a95f-53d9a2ed34a0";
            dCompany.CompanyId = cmpGuid.ToString();
            // Success
            dCompany.Status = RestStatus.Success;
            dCompany.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dCompanies));
        }

        public HttpResponseMessage Get(string companyId, string type) // TODO remove type
        {
            //if (!ML.Common.Text.IsGuidNotEmpty(companyId))
            //{
            //    _mShopConfig.Status = RestStatus.ParameterError;
            //    _mShopConfig.StatusText = "Parameter Error";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
            //}

            Guid guidCompanyGuid = Guid.Empty;
            if (ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                guidCompanyGuid = new Guid(companyId);
            }
            else
            {
                guidCompanyGuid = Guid.Parse("bf87fffb-0e6e-4812-a95f-53d9a2ed34a0");
            }
            // Populate
            _mShopConfig.CompanyId = guidCompanyGuid.ToString();
            _mShopConfig.Secret = "FRQ2RIZ6O0N3XV2C";
            _mShopConfig.ContentTemplateId = "59c2dfc1-b1d7-4d84-84bd-2274a6b69bc8";
            _mShopConfig.ContentCategoryId = "7615b011-6a60-4f0f-b9b9-cd2992ae721e";
            _mShopConfig.MultipleMenus = false;
            _mShopConfig.CompanyName = "Template";


            List<Config.OrderPrinter> mOrderPrinters = new List<Config.OrderPrinter>();


            Config.OrderPrinter mOrderPrinter = new Config.OrderPrinter();
            mOrderPrinter.OrderPrinterId = "c9eb3703-25d9-4ea9-aa0c-8c7160669934";
            mOrderPrinter.Name = "Template";
            mOrderPrinters.Add(mOrderPrinter);

            _mShopConfig.OrderPrinters = mOrderPrinters;
            // Lue Config

            _mShopConfig.TakeAway = true;
            _mShopConfig.CorporateGuid = "e0372026-9422-4500-8cd6-a69d9fed7e63".ToString();
            _mShopConfig.TakeAwayPayment = true;
            _mShopConfig.Delivery = true;
            _mShopConfig.DeliveryPayment = true;
            _mShopConfig.OrderPrinter = true;
            _mShopConfig.ShopRegistrationType = 4;
            _mShopConfig.ShopTemplate = 1;
            _mShopConfig.Menu = false;
            _mShopConfig.ShopListType = 1;
            _mShopConfig.Standalone = true;
            _mShopConfig.Css = "";
            _mShopConfig.DeliveryZipCodePreCheck = true;
            _mShopConfig.OrderCustomerEmail = true;
            _mShopConfig.AppStoreUrl = "";
            _mShopConfig.GooglePlayUrl = "";
            _mShopConfig.ShopType = 1;

            _mShopConfig.Cms = null;
            List<Slide> mSlides = new List<Slide>();
            Slide mSlide = new Slide();
            mSlides.Add(mSlide);
            _mShopConfig.Slides = mSlides;


            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        }




        //public HttpResponseMessage GetOld(string companyId, string type) // TODO remove type
        //{
        //    //if (!ML.Common.Text.IsGuidNotEmpty(companyId))
        //    //{
        //    //    _mShopConfig.Status = RestStatus.ParameterError;
        //    //    _mShopConfig.StatusText = "Parameter Error";
        //    //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //    //}

        //    Guid guidCompanyGuid = Guid.Empty;
        //    if (ML.Common.Text.IsGuidNotEmpty(companyId))
        //    {
        //        guidCompanyGuid = new Guid(companyId);
        //    }
        //    else
        //    {
        //        DB.tCompany company = new DB.CompanyRepository().GetByIdentifier(companyId);
        //        if (company != null)
        //        {
        //            guidCompanyGuid = company.CompanyGuid;
        //        }
        //        else
        //        {
        //            _mShopConfig.Status = RestStatus.NotExisting;
        //            _mShopConfig.StatusText = "Not Existing";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //        }
        //    }

        //    DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(guidCompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Shop);
        //    if (contentTemplate == null)
        //    {
        //        _mShopConfig.Status = RestStatus.NotExisting;
        //        _mShopConfig.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //    }

        //    IQueryable<DB.tContentCategory> contentCategories = new DB.ContentCategoryRepository().GetActiveByContentTemplateGuidAndContentParentCategoryGuid(contentTemplate.ContentTemplateGuid, Guid.Empty);
        //    if (!contentCategories.Any())
        //    {
        //        _mShopConfig.Status = RestStatus.NotExisting;
        //        _mShopConfig.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //    }

        //    //if (contentCategories.FirstOrDefault().tContentTemplate.CompanyGuid != guidCompanyGuid)
        //    //{
        //    //    _mShopConfig.Status = RestStatus.NotExisting;
        //    //    _mShopConfig.StatusText = "Not Existing";
        //    //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //    //}

        //    // Populate
        //    _mShopConfig.CompanyId = guidCompanyGuid.ToString();
        //    _mShopConfig.Secret = contentTemplate.tCompany.Password;
        //    _mShopConfig.ContentTemplateId = contentTemplate.ContentTemplateGuid.ToString();

        //    _mShopConfig.ContentCategoryId = contentCategories.Count() == 1 ? contentCategories.FirstOrDefault().ContentCategoryGuid.ToString() : Guid.Empty.ToString();
        //    _mShopConfig.MultipleMenus = contentCategories.Count() > 1 ? true : false;

        //    _mShopConfig.CompanyName = contentTemplate.tCompany.Name;

        //    // Image
        //    try
        //    {
        //        DB.tImage image = new DB.ImageRepository().GetByImageGuid(contentTemplate.tCompany.CompanyImageGuid);
        //        if (image != null)
        //        {
        //            _mShopConfig.CompanyImagePath = string.Format("http://{0}/storage/{1}/companyimage/{2}/CompanyImage.{3}", Request.RequestUri.Authority, guidCompanyGuid.ToString(), contentTemplate.tCompany.CompanyImageGuid.ToString(), ML.Common.FileHelper.ExtractExtension(image.FileName));
        //        }

        //        var objImage = contentTemplate.tCompany.tImage.OrderByDescending(c => c.TimeStamp).FirstOrDefault(c => c.ImageCategoryID == (int)DB.ImageService.ImageCategory.CompanyBackgroundImage);

        //        image = new DB.ImageRepository().GetByImageGuid(objImage.ImageGuid);
        //        if (image != null)
        //        {
        //            _mShopConfig.CompanyBackgroundImagePath = string.Format("http://{0}/storage/{1}/CompanyBackgroundImage/{2}/CompanyBackgroundImage.{3}", Request.RequestUri.Authority, guidCompanyGuid.ToString(), contentTemplate.tCompany.CompanyImageGuid.ToString(), ML.Common.FileHelper.ExtractExtension(image.FileName));
        //        }
        //    }
        //    catch { }

        //    List<Config.OrderPrinter> mOrderPrinters = new List<Config.OrderPrinter>();
        //    IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByCompanyGuid(guidCompanyGuid);
        //    foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
        //    {
        //        Config.OrderPrinter mOrderPrinter = new Config.OrderPrinter();
        //        mOrderPrinter.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
        //        mOrderPrinter.Name = orderPrinter.Name;
        //        mOrderPrinters.Add(mOrderPrinter);
        //    }
        //    if (mOrderPrinters.Count > 0)
        //    {
        //        _mShopConfig.OrderPrinters = mOrderPrinters;
        //    }

        //    // Lue Config
        //    DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByCompanyGuid(guidCompanyGuid);
        //    if (shopConfig != null)
        //    {
        //        _mShopConfig.TakeAway = shopConfig.TakeAway;
        //        if (shopConfig.tCompany != null)
        //            _mShopConfig.CorporateGuid = shopConfig.tCompany.CorporateGuid.ToString();
        //        _mShopConfig.TakeAwayPayment = shopConfig.TakeAwayPayment;
        //        _mShopConfig.Delivery = shopConfig.Delivery;
        //        _mShopConfig.DeliveryPayment = shopConfig.DeliveryPayment;
        //        _mShopConfig.OrderPrinter = shopConfig.OrderPrinter;
        //        _mShopConfig.ShopRegistrationType = shopConfig.ShopRegistrationTypeID;
        //        _mShopConfig.ShopTemplate = shopConfig.ShopTemplateID;
        //        _mShopConfig.Menu = new DB.ContentRepository().GetActiveMonetaryMainContentByContentTemplateGuid(contentTemplate.ContentTemplateGuid).Count() >= shopConfig.MenuTurnOver ? true : false;
        //        _mShopConfig.ShopListType = shopConfig.ShopListTypeID;
        //        _mShopConfig.Standalone = shopConfig.Standalone;
        //        _mShopConfig.Css = shopConfig.Css;
        //        _mShopConfig.DeliveryZipCodePreCheck = shopConfig.DeliveryZipCodePreCheck;
        //        _mShopConfig.OrderCustomerEmail = shopConfig.OrderCustomerEmail;
        //        _mShopConfig.AppStoreUrl = shopConfig.AppStoreUrl;
        //        _mShopConfig.GooglePlayUrl = shopConfig.GooglePlayUrl;
        //        _mShopConfig.ShopType = (int)shopConfig.ShopTypeID;

        //        if (shopConfig.Standalone)
        //        {
        //            DB.tCms cms = new DB.CmsRepository().GetCms(guidCompanyGuid);
        //            if (cms != null)
        //            {
        //                Cms mCms = new Cms();
        //                mCms.Name = cms.Name;

        //                List<CmsContent> cmsContents = new List<CmsContent>();
        //                foreach (DB.tCmsContent cmsContent in cms.tCmsContent.OrderBy(cc => cc.SortOrder).ThenBy(cc => cc.Name))
        //                {
        //                    CmsContent mCmsContent = new CmsContent();
        //                    mCmsContent.Name = cmsContent.Name;
        //                    mCmsContent.Html = cmsContent.Html;
        //                    mCmsContent.SortOrder = cmsContent.SortOrder;
        //                    mCmsContent.PageId = string.Concat("p", cmsContent.PageId);
        //                    cmsContents.Add(mCmsContent);
        //                }

        //                mCms.CmsContents = cmsContents;
        //                _mShopConfig.Cms = mCms;
        //            }

        //            List<Slide> mSlides = new List<Slide>();
        //            IQueryable<DB.tSlide> slides = new DB.SlideRepository().GetSlides(guidCompanyGuid);
        //            foreach (DB.tSlide slide in slides)
        //            {
        //                Slide mSlide = new Slide();
        //                mSlide.Title = slide.Title;
        //                mSlide.Description = slide.Description;
        //                mSlide.SortOrder = slide.SortOrder;

        //                mSlide.ImageUrl = string.Empty;
        //                DB.tImage image = new DB.ImageRepository().GetByImageGuid(slide.ImageGuid);
        //                if (image != null)
        //                {
        //                    mSlide.ImageUrl = string.Format("http://{0}/storage/{1}/slide/{2}/{3}", Request.RequestUri.Authority, guidCompanyGuid.ToString(), slide.ImageGuid.ToString(), image.FileName);
        //                }

        //                mSlide.Url = slide.Url;

        //                mSlides.Add(mSlide);
        //            }

        //            _mShopConfig.Slides = mSlides;
        //        }



        //    }

        //    // Success
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mShopConfigList));
        //}

    }
}
