using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ML.Rest2.Controllers
{
    public class ContentTemplateController : ApiController
    {
        private List<dynamic> _dContentTemplates = new List<dynamic>();
        private dynamic _dContentTemplate = new ExpandoObject();

        public ContentTemplateController()
        {
            _dContentTemplates.Add(_dContentTemplate);
        }

        public HttpResponseMessage Get(string token, int type)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsNumeric(type))
            {
                _dContentTemplate.Status = RestStatus.ParameterError;
                _dContentTemplate.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentTemplates));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _dContentTemplate.Status = RestStatus.AuthenticationFailed;
                _dContentTemplate.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentTemplates));
            }

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)type;

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(usertoken.CompanyGuid, contentTemplateType);
            if(contentTemplate == null)
            {
                _dContentTemplate.Status = RestStatus.NotExisting;
                _dContentTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentTemplates));
            }

            // Prepare xml document
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ML.Common.XmlHelper.AddRootTags(contentTemplate.XmlContentTemplate));

            // Convert xml to objects   
            List<ExpandoObject> expandoObjects = ML.Common.XmlHelper.ConvertXmlToObjects(xml);

            // Populate Dynamic
            _dContentTemplates.Clear();
            foreach (ExpandoObject expandoObject in expandoObjects)
            {
                _dContentTemplates.Add(expandoObject);
            }

            //// Populate Static
            //_dContentTemplate.UseOpenNotRedeemable = contentTemplate.UseOpenNotRedeemable;

            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentTemplates));
        }






    }
}
