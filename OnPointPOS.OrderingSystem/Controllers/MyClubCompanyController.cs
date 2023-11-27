using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class MyClubCompanyController : ApiController
    {
        // id = corporateId
        public HttpResponseMessage Post(string secret, string id)
        {
            const string GLOBAL_SECRET = "u47dxns9ifh2szy8";

            List<dynamic> mCompanies = new List<dynamic>();
            dynamic mCompany = new ExpandoObject();
            mCompanies.Add(mCompany);


            if (!Request.Content.IsFormData())
            {
                mCompany.Status = RestStatus.NotFormData;
                mCompany.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mCompany.Status = RestStatus.ParameterError;
                mCompany.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            if (secret != GLOBAL_SECRET)
            {
                mCompany.Status = RestStatus.AuthenticationFailed;
                mCompany.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            Guid guidCorporateGuid = new Guid(id);
            DB.tCorporate corporate = new DB.CorporateRepository().GetCorporate(guidCorporateGuid);
            if (corporate == null)
            {
                mCompany.Status = RestStatus.NotExisting;
                mCompany.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            string strOrderEmail = string.IsNullOrEmpty(dic["OrderEmail"]) ? string.Empty : dic["OrderEmail"];

            // Ensure name does not exist
            DB.tCompany companyExisting = new DB.CompanyRepository().GetByCorporateGuidAndName(guidCorporateGuid, strName);
            if (companyExisting != null)
            {
                mCompany.Status = RestStatus.AlreadyExists;
                mCompany.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            //// Ensure identifier does not exist
            //string strIdentifier = string.IsNullOrEmpty(dic["Identifier"]) ? string.Empty : dic["Identifier"];
            //if (!string.IsNullOrEmpty(strIdentifier))
            //{
            //    if(!new DB.CompanyService().IsIdentifierAvailable(strIdentifier, Guid.Empty))
            //    {
            //        mCompany.Status = RestStatus.AlreadyExists;
            //        mCompany.StatusText = "Already Exists";
            //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            //    }
            //}
            string strIdentifier = string.Empty;


            // Prepare new Company
            //string strSecret = ML.Common.CodeGenerator.CreateCode("****************", false, false);
            string strSecret = "8dhxn26dgr9ah3rd";

            // Create Company
            DB.tCompany company = new DB.tCompany();
            company.CompanyGuid = Guid.NewGuid();
            company.ParentCompanyGuid = ML.Common.Constants.MOBLINK_COMPANY_GUID;
            company.Name = strName;
            company.Contact = string.Empty;
            company.ContactPhoneNo = string.Empty;
            company.TechContact = string.Empty;
            company.TechPhoneNo = string.Empty;
            company.Active = true;
            company.TimeStamp = DateTime.Now;
            company.SmsCost = 0;
            company.LicenseID = 1;
            company.LicenseCost = 0;
            company.LicenseStartDate = DateTime.Now;
            company.LicenseEndDate = Convert.ToDateTime("2099-12-31 00:00:00.000");
            company.Password = strSecret;
            company.CorporateGuid = guidCorporateGuid;
            company.PremiumSmsRevenue = 0;
            company.CompanyImageGuid = Guid.Empty;
            company.CustomerInfoLookup = false;
            //company.CompanyNo = identify
            company.PaymentNotificationUrl = "https://member.myclub.se/api/v2/ecommerce/transaction/update/";
            company.CompanyTypeID = 2; //MOFR TODO
            company.PaymentNotificationUser = "moblink";
            company.PaymentNotificationPassword = "4192q32K51p3734788q6RF8Q6S8NnK";
            company.IOsCertificateFileName = string.Empty;
            company.PushAppKeyAndroid = string.Empty;
            company.Identifier = strIdentifier;

            if(new DB.CompanyRepository().Save(company) != DB.Repository.Status.Success)
            {
                mCompany.Status = RestStatus.GenericError;
                mCompany.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            // Grab LogoType (CompanyImage)
            if (!string.IsNullOrEmpty(dic["LogoType"]))
            {
                try
                {
                    DB.ImageService imageService = new DB.ImageService();
                    System.Drawing.Image img = imageService.GetImageFromUrl(dic["LogoType"]);
                    if (img != null)
                    {
                        DB.CompanyRepository companyRepository = new DB.CompanyRepository();
                        DB.tCompany companyUpdate = companyRepository.GetByCompanyGuid(company.CompanyGuid);
                        if (companyUpdate != null)
                        {
                            imageService.SaveImage(img, "Original", DB.ImageService.ImageCategory.CompanyImage, string.Empty, DB.ImageService.Rotation.Rotate0, company.CompanyGuid);

                            companyUpdate.CompanyImageGuid = imageService.ImageGuid;
                            companyRepository.Save();
                        }
                    }
                }
                catch (Exception ex)
                {
                    new ML.Email.Email().SendDebug("ML.Rest2.Controllers.MyClubCompanyController.Post", ex.StackTrace);
                }
            }

            // Handle ShopConfig
            //string strBankgiro = string.IsNullOrEmpty(dic["Bankgiro"]) ? string.Empty : dic["Bankgiro"];
            //string strPostgiro = string.IsNullOrEmpty(dic["Postgiro"]) ? string.Empty : dic["Postgiro"];

            DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByCompanyGuid(company.CompanyGuid);
            if (shopConfig == null)
            {
                shopConfig = new DB.tShopConfig();
                shopConfig.CompanyGuid = company.CompanyGuid;
                shopConfig.ContentSubContentGroup = true;
                shopConfig.ContentVariant = true;
                shopConfig.Css = string.Empty;
                shopConfig.Delivery = false;
                shopConfig.DeliveryPayment = false;
                shopConfig.DeliveryZipCodePreCheck = false;
                shopConfig.FreeSubContent = true;
                shopConfig.MenuTurnOver = 25;
                shopConfig.OrderPrinter = false;
                shopConfig.ShopConfigGuid = Guid.NewGuid();
                shopConfig.ShopListTypeID = (int)DB.ShopListTypeRepository.ShopListType.Cards;
                shopConfig.ShopRegistrationTypeID = (int)DB.ShopRegistrationTypeRepository.ShopRegistrationType.CustomerInfoRequired;
                shopConfig.ShopTemplateID = (int)DB.ShopTemplateRepository.ShopRegistrationType.MyClub;
                shopConfig.Standalone = false;
                shopConfig.TakeAway = false;
                shopConfig.TakeAwayPayment = true;
                shopConfig.TestMode = true;
                shopConfig.DeliveryZipCodePreCheck = false;
                shopConfig.OrderCustomerEmail = true;
                shopConfig.OrderEmail = strOrderEmail;
                shopConfig.AppStoreUrl = string.Empty;
                shopConfig.GooglePlayUrl = string.Empty;
                shopConfig.ShopTypeID = 1;

                new DB.ShopConfigRepository().Save(shopConfig);
            }

            // Initialize CompanyAgreement
            DB.CompanyAgreementRepository companyAgreementRepository = new DB.CompanyAgreementRepository();
            DB.tCompanyAgreement companyAgreement = companyAgreementRepository.GetCompanyAgreement(company.CompanyGuid, DateTime.Now);
            if (companyAgreement == null)
            {
                new DB.CompanyAgreementService().InitializeCompanyAgreement(company.CompanyGuid);
                companyAgreement = companyAgreementRepository.GetCompanyAgreement(company.CompanyGuid, DateTime.Now);
            }
            if (companyAgreement != null)
            {
                companyAgreement.DibsCost = -1;
                companyAgreement.DibsPartnerCost = Convert.ToDecimal(0.5);
                companyAgreement.DibsCustomerCost = Convert.ToDecimal(2.95);
                companyAgreement.DibsPercent = -1;
                companyAgreement.DibsPartnerPercent = Convert.ToDecimal(0.5);
                companyAgreement.DibsCustomerPercent = Convert.ToDecimal(2.95);


                if (!string.IsNullOrEmpty(dic["GiroType"]) && !string.IsNullOrEmpty(dic["Bank"]))
                {
                    if (dic["GiroType"] == "bg")
                    {
                        companyAgreement.Bankgiro = dic["Bank"];
                        companyAgreement.Plusgiro = string.Empty;
                    }
                    else if (dic["GiroType"] == "pg")
                    {
                        companyAgreement.Plusgiro = dic["Bank"];
                        companyAgreement.Bankgiro = string.Empty;
                    }
                }
                companyAgreementRepository.Save();
            }

            // Handle template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(company.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Shop);
            if (contentTemplate == null)
            {
                contentTemplate = new DB.tContentTemplate();
                contentTemplate.ContentTemplateGuid = Guid.NewGuid();
                contentTemplate.CompanyGuid = company.CompanyGuid;
                contentTemplate.Name = "Shop";
                contentTemplate.Description = string.Empty;
                contentTemplate.Identifier = "SHOP";
                contentTemplate.TimeStamp = DateTime.Now;
                contentTemplate.XmlCategoryTemplate = "<Namn></Namn>";
                contentTemplate.XmlContentTemplate = "<Namn></Namn><Beskrivning></Beskrivning><Bild type='image'></Bild>";
                contentTemplate.Monetary = true;
                contentTemplate.Redeemable = false;
                contentTemplate.ActivationRequired = false;
                contentTemplate.GenerateIdentifiersFromName = true;
                contentTemplate.CompanyTypeID = (int)DB.ContentTemplateRepository.ContentTemplateType.Shop;
                contentTemplate.PreviewUrl = string.Empty;

                new DB.ContentTemplateRepository().Save(contentTemplate);
            }

            // Handle ContentCategory (Parent (menu))
            Guid guidContentCategoryGuid = Guid.Empty;
            IQueryable<DB.tContentCategory> contentCategories = new DB.ContentCategoryRepository().GetContentParentCategoriesByTemplateGuid(contentTemplate.ContentTemplateGuid);
            if (!contentCategories.Any())
            {
                DB.tContentCategory contentCategory = new DB.tContentCategory();
                contentCategory.ContentCategoryGuid = Guid.NewGuid();
                guidContentCategoryGuid = contentCategory.ContentCategoryGuid;
                contentCategory.ContentParentCategoryGuid = Guid.Empty;
                contentCategory.ContentTemplateGuid = contentTemplate.ContentTemplateGuid;
                contentCategory.Name = "Menu"; //TODO  Handle multiple menus/restaurants
                contentCategory.Description = string.Empty;
                contentCategory.TimeStamp = DateTime.Now;
                contentCategory.Global = false;
                contentCategory.Private = false;
                contentCategory.StaticFilter = true;
                contentCategory.DefaultFilter = false;
                contentCategory.Xml = string.Empty;
                contentCategory.ContentCategoryLinkGuid = Guid.Empty;
                contentCategory.Active = true;
                contentCategory.Identifier = string.Empty;
                contentCategory.Map = string.Empty;
                contentCategory.SortOrder = 0;

                new DB.ContentCategoryRepository().Save(contentCategory);
            }

            // Populate result
            mCompany.Secret = company.Password;
            mCompany.CompanyId = company.CompanyGuid;

            // Success
            mCompany.Status = RestStatus.Success;
            mCompany.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
        }

        // id = companyId
        public HttpResponseMessage Put(string secret, string id)
        {
            List<dynamic> mCompanies = new List<dynamic>();
            dynamic mCompany = new ExpandoObject();
            mCompanies.Add(mCompany);

            if (!Request.Content.IsFormData())
            {
                mCompany.Status = RestStatus.NotFormData;
                mCompany.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mCompany.Status = RestStatus.ParameterError;
                mCompany.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(id, secret))
            {
                mCompany.Status = RestStatus.AuthenticationFailed;
                mCompany.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            Guid guidCompanyGuid = new Guid(id);

            DB.CompanyRepository companyRepository = new DB.CompanyRepository();
            DB.tCompany company = companyRepository.GetByCompanyGuid(guidCompanyGuid);
            if(company == null)
            {
                mCompany.Status = RestStatus.NotExisting;
                mCompany.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strOrderEmail = string.IsNullOrEmpty(dic["OrderEmail"]) ? string.Empty : dic["OrderEmail"];

            // Ensure identifier does not exist
            string strIdentifier = string.IsNullOrEmpty(dic["Identifier"]) ? string.Empty : dic["Identifier"];
            if (!string.IsNullOrEmpty(strIdentifier))
            {
                if (!new DB.CompanyService().IsIdentifierAvailable(strIdentifier, guidCompanyGuid))
                {
                    mCompany.Status = RestStatus.AlreadyExists;
                    mCompany.StatusText = "Already Exists";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
                }

                company.Identifier = strIdentifier;
            }

            // Handle Company
            if(!string.IsNullOrEmpty(dic["Name"]))
            {
                company.Name = dic["Name"];
            }

            // Grab LogoType (CompanyImage)
            if(!string.IsNullOrEmpty(dic["LogoType"]))
            {
                try
                {
                    DB.ImageService imageService = new DB.ImageService();
                    System.Drawing.Image img = imageService.GetImageFromUrl(dic["LogoType"]);
                    if (img != null)
                    {
                        imageService.SaveImage(img, "Original", DB.ImageService.ImageCategory.CompanyImage, string.Empty, DB.ImageService.Rotation.Rotate0, guidCompanyGuid);
                        company.CompanyImageGuid = imageService.ImageGuid;
                        //new ML.Email.Email().SendDebug("ML.Rest2.Controllers.MyClubCompanyController.Put", "CompanyGuid: " + guidCompanyGuid.ToString() + ", 
                    }
                }
                catch (Exception ex)
                {
                    new ML.Email.Email().SendDebug("ML.Rest2.Controllers.MyClubCompanyController.Put", ex.StackTrace);
                }
            }

            if(companyRepository.Save() != DB.Repository.Status.Success)
            {
                mCompany.Status = RestStatus.GenericError;
                mCompany.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            // Handle ShopConfig
            DB.ShopConfigRepository shopConfigRepository = new DB.ShopConfigRepository();
            DB.tShopConfig shopConfig = shopConfigRepository.GetByCompanyGuid(guidCompanyGuid);
            if (shopConfig == null)
            {
                shopConfig = new DB.tShopConfig();
                shopConfig.CompanyGuid = company.CompanyGuid;
                shopConfig.ContentSubContentGroup = true;
                shopConfig.ContentVariant = true;
                shopConfig.Css = string.Empty;
                shopConfig.Delivery = false;
                shopConfig.DeliveryPayment = false;
                shopConfig.DeliveryZipCodePreCheck = false;
                shopConfig.FreeSubContent = true;
                shopConfig.MenuTurnOver = 25;
                shopConfig.OrderPrinter = false;
                shopConfig.ShopConfigGuid = Guid.NewGuid();
                shopConfig.ShopListTypeID = (int)DB.ShopListTypeRepository.ShopListType.Cards;
                shopConfig.ShopRegistrationTypeID = (int)DB.ShopRegistrationTypeRepository.ShopRegistrationType.CustomerInfoRequired;
                shopConfig.ShopTemplateID = (int)DB.ShopTemplateRepository.ShopRegistrationType.MyClub;
                shopConfig.Standalone = false;
                shopConfig.TakeAway = false;
                shopConfig.TakeAwayPayment = true;
                shopConfig.TestMode = true;
                shopConfig.DeliveryZipCodePreCheck = false;
                shopConfig.OrderCustomerEmail = true;
                shopConfig.OrderEmail = strOrderEmail;
                shopConfig.AppStoreUrl = string.Empty;
                shopConfig.GooglePlayUrl = string.Empty;
                shopConfig.ShopTypeID = 1;
                shopConfig.Cms = true;

                shopConfigRepository.Save(shopConfig);
            }
            else
            {
                shopConfig.OrderEmail = strOrderEmail;
                shopConfig.AppStoreUrl = string.Empty;
                shopConfig.GooglePlayUrl = string.Empty;
                shopConfig.Cms = true;

                shopConfigRepository.Save();
            }

            // Handle CompanyAgreement
            DB.CompanyAgreementRepository companyAgreementRepository = new DB.CompanyAgreementRepository();
            DB.tCompanyAgreement companyAgreement = companyAgreementRepository.GetCompanyAgreement(guidCompanyGuid, DateTime.Now);
            if (companyAgreement == null)
            {
                new DB.CompanyAgreementService().InitializeCompanyAgreement(guidCompanyGuid);
                companyAgreement = companyAgreementRepository.GetCompanyAgreement(guidCompanyGuid, DateTime.Now);
            }
            if (companyAgreement != null)
            {
                if (!string.IsNullOrEmpty(dic["GiroType"]) && !string.IsNullOrEmpty(dic["Bank"]))
                {
                    if (dic["GiroType"] == "bg")
                    {
                        companyAgreement.Bankgiro = dic["Bank"].Trim();
                        companyAgreement.Plusgiro = string.Empty;
                    }
                    else if (dic["GiroType"] == "pg")
                    {
                        companyAgreement.Plusgiro = dic["Bank"].Trim();
                        companyAgreement.Bankgiro = string.Empty;
                    }
                }
                companyAgreementRepository.Save();
            }

            // Success
            mCompany.Status = RestStatus.Success;
            mCompany.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
        }

        // id = companyId
        public HttpResponseMessage Get(string secret, string id)
        {
            List<dynamic> mCompanies = new List<dynamic>();
            dynamic mCompany = new ExpandoObject();
            mCompanies.Add(mCompany);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mCompany.Status = RestStatus.ParameterError;
                mCompany.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(id, secret))
            {
                mCompany.Status = RestStatus.AuthenticationFailed;
                mCompany.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            Guid guidCompanyGuid = new Guid(id);

            DB.tCompany company = new DB.CompanyRepository().GetByCompanyGuid(guidCompanyGuid);
            if (company == null)
            {
                mCompany.Status = RestStatus.NotExisting;
                mCompany.StatusText = "Not Existing (1)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByCompanyGuid(guidCompanyGuid);
            if(shopConfig == null)
            {
                mCompany.Status = RestStatus.NotExisting;
                mCompany.StatusText = "Not Existing (2)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            DB.tCompanyAgreement companyAgreement = new DB.CompanyAgreementRepository().GetCompanyAgreement(guidCompanyGuid, DateTime.Now);
            if(companyAgreement == null)
            {
                mCompany.Status = RestStatus.NotExisting;
                mCompany.StatusText = "Not Existing (3)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
            }

            // Populate
            mCompany.Name = company.Name;
            mCompany.Identifier = company.Identifier;

            mCompany.BankGiro = companyAgreement.Bankgiro;
            mCompany.PlusGiro = companyAgreement.Plusgiro;
            mCompany.TestMode = shopConfig.TestMode;
            // Cont...

            // Success
            mCompany.Status = RestStatus.Success;
            mCompany.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mCompanies));
        }


    }
}
