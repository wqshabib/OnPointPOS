using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using Models;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace ML.Rest2.Controllers
{
    public class CompanySimpleContentController : ApiController
    {
        private List<SimpleContentList> _mSimpleContentLists = new List<SimpleContentList>();
        private SimpleContentList _mSimpleContentList = new SimpleContentList();

        public CompanySimpleContentController()
        {
            _mSimpleContentLists.Add(_mSimpleContentList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// id = ContentCategoryGuid or ContentParentCategoryGuid
        /// <param name="imageWidth"></param>
        /// <returns></returns>

        public HttpResponseMessage Get(string secret, string companyId, string id, int includeCategories, int imageWidth)
        {
            return Get(secret, companyId, id, includeCategories, imageWidth, 1);
        }

        public HttpResponseMessage GetOld(string secret, string companyId, string id, int includeCategories, int imageWidth, int priceType)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id) && (includeCategories == 0 || includeCategories == 1))
            {
                _mSimpleContentList.Status = RestStatus.ParameterError;
                _mSimpleContentList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
            }

            Guid guidCompanyGuid = new Guid(companyId);
            bool bIncludeCategories = Convert.ToBoolean(includeCategories);

            var json = "";
            var path = @"D:\Luqon IT\POSSUM\POSSUM\POSSUM.OrderingSystem\Files\CompanySimpleContent.json";
            path = @"D:\Luqon IT\POSSUM\POSSUM\POSSUM.OrderingSystem\Files\CompanySimpleContent.txt";

            path = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se\storages\JsonFiles\CompanySimpleContent.json";

            using (StreamReader r = new StreamReader(path))
            {
                json = r.ReadToEnd().ToString();


            }

            //var jst = Regex.Replace(json, @"\r\n?|\n|", "").Replace("\\", "");
            //var bbb = json.Replace(@"\", "-");
            //json = jst;
            ////var response = Request.CreateResponse(HttpStatusCode.OK);
            ////response.Content = new StringContent(json);
            ////return response;
            //var ss= ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(json));
            //return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(json));

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(json);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;

        }
         
        public HttpResponseMessage Get(string secret, string companyId, string id, int includeCategories, int imageWidth, int priceType)
        {
            _mSimpleContentLists.Clear();


            _mSimpleContentList = new SimpleContentList();
            _mSimpleContentList.ContentCategoryName = "Mat";
            _mSimpleContentList.ContentCategoryDescription = string.Empty;
            _mSimpleContentList.OrderPrinterAvailabilityType = 0;
            _mSimpleContentList.Available = true;
            _mSimpleContentList.Orderable = true;

            _mSimpleContentLists.Add(_mSimpleContentList);

            List<SimpleContent> mSimpleContents = new List<SimpleContent>();
            mSimpleContents.Add(PopulateSimpleContentList1());
            mSimpleContents.Add(PopulateSimpleContentList2());
            mSimpleContents.Add(PopulateSimpleContentList3());
            _mSimpleContentList.SimpleContent = mSimpleContents;


            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
        }



         

        private SimpleContent PopulateSimpleContentList1()
        {
            SimpleContent simpleContent = new SimpleContent();
            simpleContent.ContentId = "808fe6c6-cf2e-43a9-80c7-158120eb1dd0";
            simpleContent.Name = "Margherita ";
            simpleContent.Description = string.Empty;
            simpleContent.Price = Convert.ToDecimal(10.0);
            simpleContent.IncludedSubContent = "Basilika";
            simpleContent.Scale = 0;
            simpleContent.Available = true;
            simpleContent.Orderable = true;
            simpleContent.Identifier = "margherita";
            simpleContent.DiscountAmount = Convert.ToDecimal(0.0);
            simpleContent.StockQty = 0;

            List<ContentCustom> contentCustom = new List<ContentCustom>();
            contentCustom.Add(new ContentCustom
            {
                Name = "Bild",
                Type = "image",
                Value = string.Empty,
            });
            contentCustom.Add(new ContentCustom
            {
                Name = "Name",
                Type = string.Empty
            });


            simpleContent.ContentCustom = contentCustom;

            List<SimpleContentVariant> simpleContentVariants = new List<SimpleContentVariant>();
            SimpleContentVariant mSimpleContentVariant = new SimpleContentVariant();
            simpleContentVariants.Add(mSimpleContentVariant);
            simpleContent.SimpleContentVariant = simpleContentVariants;

            return simpleContent;
        }

        private SimpleContent PopulateSimpleContentList2()
        {
            SimpleContent simpleContent = new SimpleContent();
            simpleContent.ContentId = "13ffd8c7-0e87-4b61-975f-4fedd495d4c8";
            simpleContent.Name = "Mixa egen sallad";
            simpleContent.Description = "sallad with multiple options";
            simpleContent.Price = Convert.ToDecimal(79.0);
            simpleContent.IncludedSubContent = "";
            simpleContent.Scale = 2;
            simpleContent.Available = true;
            simpleContent.Orderable = true;
            simpleContent.Identifier = "mixa-egen-sallad";
            simpleContent.DiscountAmount = Convert.ToDecimal(0.0);
            simpleContent.StockQty = 0;

            List<ContentCustom> contentCustom = new List<ContentCustom>();
            contentCustom.Add(new ContentCustom
            {
                Name = "Bild",
                Type = "image",
                Value = string.Empty,
            });
            contentCustom.Add(new ContentCustom
            {
                Name = "Name",
                Type = string.Empty
            });


            simpleContent.ContentCustom = contentCustom;

            List<SimpleContentVariant> simpleContentVariants = new List<SimpleContentVariant>();
            simpleContentVariants.Add(new SimpleContentVariant
            {
                Name = "Family",
                Price=Convert.ToDecimal(79.0)
            });
            simpleContentVariants.Add(new SimpleContentVariant
            {
                Name = "Regular",
                Price = Convert.ToDecimal(79.0)
            });

            simpleContent.SimpleContentVariant = simpleContentVariants;

            return simpleContent;
        }

         
        private SimpleContent PopulateSimpleContentList3()
        {
            SimpleContent simpleContent = new SimpleContent();
            simpleContent.ContentId = "1b4ad6b1-b715-4c60-814d-ac80185aba9d";
            simpleContent.Name = "Tonfisksallad ";
            simpleContent.Description = string.Empty;
            simpleContent.Price = Convert.ToDecimal(120.0);
            simpleContent.IncludedSubContent = "";
            simpleContent.Scale = 0;
            simpleContent.Available = true;
            simpleContent.Orderable = true;
            simpleContent.Identifier = "tonfisksallad";
            simpleContent.DiscountAmount = Convert.ToDecimal(0.0);
            simpleContent.StockQty = 0;

            List<ContentCustom> contentCustom = new List<ContentCustom>();
            contentCustom.Add(new ContentCustom
            {
                Name = "Bild",
                Type = "image",
                Value = string.Empty,
            });
            contentCustom.Add(new ContentCustom
            {
                Name = "Name",
                Type = string.Empty
            });


            simpleContent.ContentCustom = contentCustom;

            List<SimpleContentVariant> simpleContentVariants = new List<SimpleContentVariant>();
            SimpleContentVariant mSimpleContentVariant = new SimpleContentVariant();
            simpleContentVariants.Add(mSimpleContentVariant);
            simpleContent.SimpleContentVariant = simpleContentVariants;

            return simpleContent;
        }




    }
}
