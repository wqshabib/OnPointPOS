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
    public class TagListController : ApiController
    {
        List<Models.TagList> _mTagLists = new List<Models.TagList>();
        Models.TagList _mTagList = new Models.TagList();

        public TagListController()
        {
            _mTagLists.Add(_mTagList);
        }

        public HttpResponseMessage Get(string token)
        {
            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mTagList.Status = RestStatus.AuthenticationFailed;
                _mTagList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mTagLists));
            }

            // Get TagList
            IQueryable<DB.tTagList> tagLists = new DB.TagListRepository().GetTagLists(usertoken.CompanyGuid);
            if (!tagLists.Any())
            {
                _mTagList.Status = RestStatus.NotExisting;
                _mTagList.StatusText = "NotExisting";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mTagLists));
            }

            // Popultae
            _mTagLists.Clear();
            foreach(DB.tTagList tagList in tagLists)
            {
                TagList mTagList = new TagList();
                mTagList.TagListId = tagList.TagListGuid.ToString();
                mTagList.Name = tagList.Name;
                _mTagLists.Add(mTagList);

                List<Tag> tags = new List<Tag>();
                foreach(DB.tTag tag in tagList.tTag.OrderBy(t => t.Name))
                {
                    Tag mTag = new Tag();
                    mTag.TagId = tag.TagGuid.ToString();
                    mTag.Name = tag.Name;
                    tags.Add(mTag);
                }
                mTagList.Tags = tags;
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mTagLists));
        }


    }
}
