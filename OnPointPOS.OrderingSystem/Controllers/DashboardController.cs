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
    public class DashboardController : ApiController
    {
        List<dynamic> _mDashboards = new List<dynamic>();
        dynamic _dDashboard = new ExpandoObject();

        public DashboardController()
        {
            _mDashboards.Add(_dDashboard);
        }

        public HttpResponseMessage Get(string token, int type)
        {
            return Get(token, type, Guid.Empty.ToString());
        }

        public HttpResponseMessage Get(string token, int type, string contentCategory)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsNumeric(type))
            {
                _dDashboard.Status = RestStatus.ParameterError;
                _dDashboard.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _dDashboard.Status = RestStatus.AuthenticationFailed;
                _dDashboard.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
            }

            // Create
            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)type;
            if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Shop)
            {
                DB.tCompany company = new DB.CompanyRepository().GetByCompanyGuid(usertoken.CompanyGuid);

                // Filter all menues for LUE or MyClub
                if (contentCategory == Guid.Empty.ToString() || company.CorporateGuid == new Guid("72DE85CE-0D2B-49F7-9685-1A16271F9DB6"))
                {
                    // Global totals
                    _dDashboard.CustomerCount = new DB.CustomerRepository().GetByCompanyGuid(usertoken.CompanyGuid, Customer.Enums.CustomerType.Cookie).Count();
                    
                    _dDashboard.ProductCount = new DB.ContentRepository().GetActiveMonetaryMainContentByCompanyGuid(usertoken.CompanyGuid).Count();
                    _dDashboard.OrderCount = new DB.OrderRepository().GetByCompanyGuidAndStatus(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed).Count();
                    try
                    {
                        _dDashboard.OrderTotal = new DB.OrderRepository().GetOrderItemsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed).Sum(oi => oi.Price);
                    }
                    catch
                    {
                        _dDashboard.OrderTotal = 0;
                    }

                    try
                    {
                        _dDashboard.OrderTotalPayment = new DB.PaymentRepository().GetPaymentsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed).Sum(p => p.Amount);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalPayment = 0;
                    }
                    _dDashboard.OrderTotalCash = _dDashboard.OrderTotal - _dDashboard.OrderTotalPayment;

                    // Yesterday totals
                    _dDashboard.OrderCountYesterday = new DB.OrderRepository().GetByCompanyGuidAndStatus(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date).Count();
                    try
                    {
                        _dDashboard.OrderTotalYesterday = new DB.OrderRepository().GetOrderItemsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date,  DateTime.Now.Date).Sum(oi => oi.Price);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalYesterday = 0;
                    }

                    try
                    {
                        _dDashboard.OrderTotalPaymentYesterday = new DB.PaymentRepository().GetPaymentsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date).Sum(p => p.Amount);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalPaymentYesterday = 0;
                    }
                    _dDashboard.OrderTotalCashYesterday = _dDashboard.OrderTotalYesterday - _dDashboard.OrderTotalPaymentYesterday;
                }
                else if (ML.Common.Text.IsGuidNotEmpty(contentCategory))
                {
                    Guid guidContentCategoryGuid = new Guid(contentCategory);
                    _dDashboard.CustomerCount = new DB.CustomerRepository().GetByCompanyGuid(usertoken.CompanyGuid, Customer.Enums.CustomerType.Cookie).Count();

                    _dDashboard.ProductCount = new DB.ContentRepository().GetActiveMonetaryMainContentByCompanyGuid(usertoken.CompanyGuid, guidContentCategoryGuid).Count();
                    _dDashboard.OrderCount = new DB.OrderRepository().GetByCompanyGuidAndStatus(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, guidContentCategoryGuid).Count();
                    try
                    {
                        _dDashboard.OrderTotal = new DB.OrderRepository().GetOrderItemsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, guidContentCategoryGuid).Sum(oi => oi.Price);
                    }
                    catch
                    {
                        _dDashboard.OrderTotal = 0;
                    }

                    try
                    {
                        _dDashboard.OrderTotalPayment = new DB.PaymentRepository().GetPaymentsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, guidContentCategoryGuid).Sum(p => p.Amount);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalPayment = 0;
                    }
                    _dDashboard.OrderTotalCash = _dDashboard.OrderTotal - _dDashboard.OrderTotalPayment;

                    // Yesterday totals
                    _dDashboard.OrderCountYesterday = new DB.OrderRepository().GetByCompanyGuidAndStatus(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date, guidContentCategoryGuid).Count();
                    try
                    {
                        _dDashboard.OrderTotalYesterday = new DB.OrderRepository().GetOrderItemsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date, guidContentCategoryGuid).Sum(oi => oi.Price);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalYesterday = 0;
                    }

                    try
                    {
                        _dDashboard.OrderTotalPaymentYesterday = new DB.PaymentRepository().GetPaymentsByCompanyGuid(usertoken.CompanyGuid, DB.OrderStatusRepository.OrderStatus.Completed, DateTime.Now.AddDays(-1).Date, DateTime.Now.Date, guidContentCategoryGuid).Sum(p => p.Amount);
                    }
                    catch
                    {
                        _dDashboard.OrderTotalPaymentYesterday = 0;
                    }
                    _dDashboard.OrderTotalCashYesterday = _dDashboard.OrderTotalYesterday - _dDashboard.OrderTotalPaymentYesterday;
                }
                else
                {
                    _dDashboard.Status = RestStatus.NotExisting;
                    _dDashboard.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
                }
            }
            else if (contentTemplateType == DB.ContentTemplateRepository.ContentTemplateType.Mofr)
            {
                DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(usertoken.CompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Mofr);
                if (contentTemplate == null)
                {
                    _dDashboard.Status = RestStatus.NotExisting;
                    _dDashboard.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
                }

                IQueryable<DB.tContent> contents = new DB.ContentRepository().GetRelevantByContentTemplateGuid(contentTemplate.ContentTemplateGuid);
                try
                {
                    DB.ContentRepository contentRepository = new DB.ContentRepository();
                    _dDashboard.AllCount = contentRepository.GetByContentListType(contentTemplate.ContentTemplateGuid, DB.ContentRepository.ContentListType.All, contents).Count();
                    _dDashboard.InactiveCount = contentRepository.GetByContentListType(contentTemplate.ContentTemplateGuid, DB.ContentRepository.ContentListType.Inactive, contents).Count();
                    _dDashboard.ActiveCount = contentRepository.GetByContentListType(contentTemplate.ContentTemplateGuid, DB.ContentRepository.ContentListType.Current, contents).Count();
                    _dDashboard.ExpiredCount = contentRepository.GetByContentListType(contentTemplate.ContentTemplateGuid, DB.ContentRepository.ContentListType.Expired, contents).Count();
                    _dDashboard.Pending = contentRepository.GetByContentListType(contentTemplate.ContentTemplateGuid, DB.ContentRepository.ContentListType.Pending, contents).Count();

                    DB.CustomerContentRepository customerContentRepository = new DB.CustomerContentRepository();
                    _dDashboard.ViewCount = customerContentRepository.GetByContentTemplateGuidAndCustomerContentType(contentTemplate.ContentTemplateGuid, DB.CustomerContentRepository.CustomerContentType.View).Count();
                    _dDashboard.RedeemCount = customerContentRepository.GetByContentTemplateGuidAndCustomerContentType(contentTemplate.ContentTemplateGuid, DB.CustomerContentRepository.CustomerContentType.Redeem).Count();

                    _dDashboard.CustomerCount = new DB.CustomerRepository().GetByCompanyGuid(usertoken.CompanyGuid, Customer.Enums.CustomerType.Cookie).Count();
                }
                catch
                {
                    _dDashboard.Status = RestStatus.NotExisting;
                    _dDashboard.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
                }
            }



            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mDashboards));
        }


    }


}
