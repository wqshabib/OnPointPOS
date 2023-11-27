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

namespace ML.Rest2.Controllers
{
    public class PublicUrlController : ApiController
    {
        public HttpResponseMessage Post()
        {
            List<dynamic> mUrls = new List<dynamic>();
            dynamic dUrl = new ExpandoObject();
            mUrls.Add(dUrl);

            if (!Request.Content.IsFormData())
            {
                dUrl.Status = RestStatus.NotFormData;
                dUrl.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUrls));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strUrl = string.IsNullOrEmpty(dic["Url"]) ? string.Empty : dic["Url"];

            // Strip www
            strUrl = strUrl.Replace("www.", string.Empty);

            if(string.IsNullOrEmpty(strUrl))
            {
                dUrl.Status = RestStatus.ParameterError;
                dUrl.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUrls));
            }

            //dummy data return so it will 
            //DB.tCompany company = new DB.CompanyRepository().GetByUrl(strUrl);
            //if (company == null)
            //{
            //    company = new DB.CompanyRepository().GetByIdentifier(strUrl.Substring(0, strUrl.IndexOf('.')));
            //    if (company == null)
            //    {
            //        dUrl.Status = RestStatus.NotExisting;
            //        dUrl.StatusText = "Not Existing";
            //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUrls));
            //    }
            //}
            //dUrl.CompanyId = company.CompanyGuid.ToString();
             
            var cmpGuid = "bf87fffb-0e6e-4812-a95f-53d9a2ed34a0";
            dUrl.CompanyId = cmpGuid.ToString(); 
            // Success
            dUrl.Status = RestStatus.Success;
            dUrl.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mUrls));
        }

    }
}
