using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web;
using System.Web.Http;

namespace ML.Rest2.Controllers
{   
    public class AuthorizationController : ApiController
    {
        // Login User
        //[HttpPost]
        //[ActionName("authorization/login")]
        public HttpResponseMessage Post(string login)
        {
            List<User> mUsers = new List<User>();
            User mUser = new User();
            mUsers.Add(mUser);

            if (!Request.Content.IsFormData())
            {
                mUser.Status = RestStatus.NotFormData;
                mUser.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUsers));
            }

            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            if (string.IsNullOrEmpty(dic["UserName"]) || string.IsNullOrEmpty(dic["Password"]))
            {
                mUser.Status = RestStatus.ParameterError;
                mUser.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUsers));
            }

            DB.tUser user = new DB.UserService().Login(dic["userName"], dic["password"]);
            if (user == null)
            {
                mUser.Status = RestStatus.AuthenticationFailed;
                mUser.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUsers));
            }

            // Handle User Token
            DB.tUserToken userToken = DB.UserTokenService.GetUserToken(user.UserGuid);
            if (userToken == null)
            {
                mUser.Status = RestStatus.GenericError;
                mUser.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUsers));
            }

            // TEMP TODO Dynamic
            mUser.ApplicationIDs = new int[] { 1 }; // 1 = LUE
            // END TEMP

            mUser.UserName = user.UserName;
            mUser.FirstName = user.FirstName;
            mUser.LastName = user.LastName;
            //mUser.CompanyGuid = user.CompanyGuid;
            //mUser.Secret = user.tCompany.Password;
            mUser.LastLoggedIn = user.LastLoggedIn;
            mUser.Token = userToken.Token;
            mUser.RoleID = user.RoleID;
            mUser.CompanyName = user.tCompany.Name;
            //mUser.Secret = user.tCompany.Password;

            // Image
            DB.tImage image = new DB.ImageRepository().GetByImageGuid(user.tCompany.CompanyImageGuid);
            if(image != null)
            {
                //mUser.CompanyImagePath = System.Web.HttpContext.Current.Server.MapPath(string.Format("~/storage/{0}/companyimage/{1}/CompanyImage.{2}", user.CompanyGuid.ToString(), user.tCompany.CompanyImageGuid.ToString(), ML.Common.FileHelper.ExtractExtension(image.FileName)));
                //mUser.CompanyImagePath = string.Format("~/storage/{0}/companyimage/{1}/CompanyImage.{2}", user.CompanyGuid.ToString(), user.tCompany.CompanyImageGuid.ToString(), ML.Common.FileHelper.ExtractExtension(image.FileName));
                mUser.CompanyImagePath = string.Format("http://{0}/storage/{1}/companyimage/{2}/CompanyImage.{3}", Request.RequestUri.Authority, user.CompanyGuid.ToString(), user.tCompany.CompanyImageGuid.ToString(), ML.Common.FileHelper.ExtractExtension(image.FileName)); 
            }

            // Shop Config
            DB.tContentTemplate contentTemplate = new DB.tContentTemplate();
            ShopConfig _mShopConfig = new ShopConfig();
            DB.tShopConfig shopConfig = new DB.ShopConfigRepository().GetByCompanyGuid(user.CompanyGuid);
            if (shopConfig != null)
            {
                _mShopConfig.SubContentGroup = shopConfig.ContentSubContentGroup;
                _mShopConfig.ContentVariant = shopConfig.ContentVariant;
                _mShopConfig.FreeSubContent = shopConfig.FreeSubContent;
                _mShopConfig.TakeAway = shopConfig.TakeAway;
                _mShopConfig.TakeAwayPayment = shopConfig.TakeAwayPayment;
                _mShopConfig.Delivery = shopConfig.Delivery;
                _mShopConfig.DeliveryPayment = shopConfig.DeliveryPayment;
                _mShopConfig.OrderPrinter = shopConfig.OrderPrinter;
                _mShopConfig.ShopRegistrationType = shopConfig.ShopRegistrationTypeID;
                _mShopConfig.ShopTemplate = shopConfig.ShopTemplateID;
                _mShopConfig.AppStoreUrl = shopConfig.AppStoreUrl;
                _mShopConfig.GooglePlayUrl = shopConfig.GooglePlayUrl;
                _mShopConfig.Cms = shopConfig.Cms;

                // LUE
                contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(user.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Shop);
                if(contentTemplate != null)
                {
                    IQueryable<DB.tContentCategory> contentCategories = null;
                    if(user.ContentCategoryGuid == Guid.Empty)
                    {
                        contentCategories = new DB.ContentCategoryRepository().GetActiveByContentTemplateGuidAndContentParentCategoryGuid(contentTemplate.ContentTemplateGuid, Guid.Empty);
                        _mShopConfig.ContentCategoryId = contentCategories.Count() == 1 ? contentCategories.FirstOrDefault().ContentCategoryGuid.ToString() : Guid.Empty.ToString();
                        mUser.DefaultContentCategoryId = new Guid(_mShopConfig.ContentCategoryId);
                        _mShopConfig.MultipleMenus = contentCategories.Count() > 1 ? true : false;
                    }
                    else
                    {
                        _mShopConfig.ContentCategoryId = user.ContentCategoryGuid.ToString();
                        mUser.DefaultContentCategoryId = new Guid(_mShopConfig.ContentCategoryId);
                        _mShopConfig.MultipleMenus = false;
                    }
                    _mShopConfig.ContentTemplateId = contentTemplate.ContentTemplateGuid.ToString();
                }
            }

            // MOFR
            contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(user.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Mofr);
            if (contentTemplate != null)
            {
                _mShopConfig.UseOpenNotRedeemable = contentTemplate.UseOpenNotRedeemable;

                // Handle Corporate
                if (user.tCompany.CorporateGuid != Guid.Empty)
                {
                    IQueryable<DB.tCompany> companies = new DB.CompanyRepository().GetByCorporateGuid(user.tCompany.CorporateGuid);
                    if (companies.Count() > 1)
                    {
                        List<Company> lstCompanies = new List<Company>();
                        foreach (DB.tCompany company in companies)
                        {
                            Company mCompany = new Company();
                            mCompany.Name = company.Name;
                            mCompany.CompanyId = company.CompanyGuid.ToString();
                            mCompany.Selected = true;
                            lstCompanies.Add(mCompany);
                        }
                        _mShopConfig.Companies = lstCompanies;
                    }
                }

                // PunchTicket due. TODO: set in ContentTemplate perhaps?
                _mShopConfig.PunchTicket = false;
                IQueryable<DB.tTagList> tagLists = new DB.TagListRepository().GetTagLists(user.CompanyGuid);
                if(tagLists.Any())
                {
                    _mShopConfig.PunchTicket = true;
                }


            }
            // Cont...
            if (user.tCompany.tContentTemplate.FirstOrDefault() != null)
            {
                mUser.DefaultContentTemplateId = user.tCompany.tContentTemplate.FirstOrDefault().ContentTemplateGuid; // TODO Add ContentTemplate Type
                mUser.DefaultContentCategoryId = user.tCompany.tContentTemplate.FirstOrDefault().tContentCategory.Where(cc => cc.ContentParentCategoryGuid == Guid.Empty).FirstOrDefault().ContentCategoryGuid;
            }
            else if (contentTemplate!=null)
            {
                mUser.DefaultContentTemplateId = contentTemplate.ContentTemplateGuid; // TODO Add ContentTemplate Type
                if (contentTemplate.tContentCategory.Count>0)
                { 
                    var cat = contentTemplate.tContentCategory.FirstOrDefault(cc => cc.ContentParentCategoryGuid == Guid.Empty);
                    if(cat!=null)
                        mUser.DefaultContentCategoryId = cat.ContentCategoryGuid;

                }
            }
            mUser.ShopConfig = _mShopConfig;

            mUser.Status = RestStatus.Success;
            mUser.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUsers));
        }



    }
}
