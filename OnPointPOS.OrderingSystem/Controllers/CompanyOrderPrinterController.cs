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
    public class CompanyOrderPrinterController : ApiController
    {
        private List<CompanyOrderPrinter> _mCompanyOrderPrinters = new List<CompanyOrderPrinter>();
        private CompanyOrderPrinter _mCompanyOrderPrinter = new CompanyOrderPrinter();

        public CompanyOrderPrinterController()
        {
            _mCompanyOrderPrinters.Add(_mCompanyOrderPrinter);
        }

        public HttpResponseMessage Post(string secret, string companyId)
        {
            if (!Request.Content.IsFormData())
            {
                _mCompanyOrderPrinter.Status = RestStatus.NotFormData;
                _mCompanyOrderPrinter.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                _mCompanyOrderPrinter.Status = RestStatus.ParameterError;
                _mCompanyOrderPrinter.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mCompanyOrderPrinter.Status = RestStatus.AuthenticationFailed;
                _mCompanyOrderPrinter.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = dic["Search"];

            // Lookup
            IQueryable<DB.tPostal> postals = new DB.PostalService().Search(strSearch, 1);
            if (!postals.Any())
            {
                _mCompanyOrderPrinter.Status = RestStatus.NotExisting;
                _mCompanyOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            DB.tOrderPrinterZipCode orderPrinterZipCode = new DB.OrderPrinterZipCodeRepository().GetByZipCodeAndCompanyGuid(postals.FirstOrDefault().ZipCode, new Guid(companyId));
            if (orderPrinterZipCode == null)
            {
                _mCompanyOrderPrinter.Status = RestStatus.NotExisting;
                _mCompanyOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            // Populate result
            _mCompanyOrderPrinter.OrderPrinterId = orderPrinterZipCode.tOrderPrinter.OrderPrinterGuid.ToString();
            _mCompanyOrderPrinter.Name = orderPrinterZipCode.tOrderPrinter.Name;
            DB.tContentCategory contentCategory = new DB.ContentCategoryRepository().GetByContentCategoryGuid(orderPrinterZipCode.tOrderPrinter.ContentCategoryGuid);
            if(contentCategory != null)
            {
                _mCompanyOrderPrinter.ContentTemplateId = contentCategory.ContentTemplateGuid.ToString();
            }
            _mCompanyOrderPrinter.ContentCategoryId = orderPrinterZipCode.tOrderPrinter.ContentCategoryGuid.ToString();

            // Success
            _mCompanyOrderPrinter.Status = RestStatus.Success;
            _mCompanyOrderPrinter.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = PostalId (Guid) or ZipCode (int)
        /// <returns></returns>
        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mCompanyOrderPrinter.Status = RestStatus.ParameterError;
                _mCompanyOrderPrinter.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mCompanyOrderPrinter.Status = RestStatus.AuthenticationFailed;
                _mCompanyOrderPrinter.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            // Get Postal
            int intZipCode = 0;
            if (ML.Common.Text.IsGuidNotEmpty(id))
            {
                DB.tPostal postal = new DB.PostalRepository().GetPostal(new Guid(id));
                if (postal == null)
                {
                    _mCompanyOrderPrinter.Status = RestStatus.NotExisting;
                    _mCompanyOrderPrinter.StatusText = "Not Existing";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
                }

                intZipCode = postal.ZipCode;
            }
            else
            {
                intZipCode = Convert.ToInt32(id);
            }

            // Get OrderPrinterZipCode
            DB.tOrderPrinterZipCode orderPrinterZipCode = new DB.OrderPrinterZipCodeRepository().GetByZipCodeAndCompanyGuid(intZipCode, new Guid(companyId));
            if (orderPrinterZipCode == null)
            {
                _mCompanyOrderPrinter.Status = RestStatus.NotExisting;
                _mCompanyOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            // Get ContentCategory
            DB.tContentCategory contentCategory = new DB.ContentCategoryRepository().GetByContentCategoryGuid(orderPrinterZipCode.tOrderPrinter.ContentCategoryGuid);
            if (contentCategory == null)
            {
                _mCompanyOrderPrinter.Status = RestStatus.NotExisting;
                _mCompanyOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
            }

            // Populate result
            _mCompanyOrderPrinter.OrderPrinterId = orderPrinterZipCode.tOrderPrinter.OrderPrinterGuid.ToString();
            _mCompanyOrderPrinter.Name = orderPrinterZipCode.tOrderPrinter.Name;
            _mCompanyOrderPrinter.ContentTemplateId = contentCategory.ContentTemplateGuid.ToString();
            _mCompanyOrderPrinter.ContentCategoryId = orderPrinterZipCode.tOrderPrinter.ContentCategoryGuid.ToString();

            // Success
            _mCompanyOrderPrinter.Status = RestStatus.Success;
            _mCompanyOrderPrinter.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinters));
        }

    }
}
