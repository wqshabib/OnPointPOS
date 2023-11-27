using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
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
using Models;

namespace ML.Rest2.Controllers
{
    [RoutePrefix("api/CompanyInfo")]
    public class CompanyInfoController : ApiController
    {
        private List<Info> _mInfos = new List<Info>();
        private Info _mInfo = new Info();

        public CompanyInfoController()
        {
            _mInfos.Add(_mInfo);
        }

        public HttpResponseMessage Get(string secret, string companyId)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                _mInfo.Status = RestStatus.ParameterError;
                _mInfo.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
            }


            Guid guidCompanyGuid = Guid.Parse("00000000-0000-0000-0000-000000000000");

            // Info (s)
            _mInfos.Clear();

            Info mInfo = new Info();
            mInfo.Name = "Template";
            mInfo.OrderPrinterId = "c9eb3703-25d9-4ea9-aa0c-8c7160669934";
            mInfo.ContentCategoryId = "7615b011-6a60-4f0f-b9b9-cd2992ae721e";
            mInfo.PhoneNo = "";
            mInfo.Address = "Vallgatan 26";
            mInfo.ZipCode = "41116";
            mInfo.City = "Göteborg";
            mInfo.Lat = Convert.ToDouble(57.7039106);
            mInfo.Long = Convert.ToDouble(11.9663747);
            mInfo.FacebookUrl = "";
            mInfo.TwitterUrl = "";
            mInfo.InstagramUrl = "";
            mInfo.Presentation = "HEJSAN 2";
            mInfo.About = "HEJSAN";
            mInfo.PdfUrl = "";
            mInfo.CompanyGuid = guidCompanyGuid;
            mInfo.CorporateGuid = guidCompanyGuid;
            _mInfos.Add(mInfo);
            _mInfo.Status = RestStatus.Success;
            _mInfo.StatusText = "";
            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mInfos));
        }



    }


}
