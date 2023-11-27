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
    public class PunchTicketController : ApiController
    {
        List<Models.PunchTicket> _mPunchTickets = new List<Models.PunchTicket>();
        Models.PunchTicket _mPunchTicket = new Models.PunchTicket();

        public PunchTicketController()
        {
            _mPunchTickets.Add(_mPunchTicket);
        }

        public HttpResponseMessage Post(string token, string customerId)
        {
            List<Models.PunchTicketRoot> mPunchTicketRoots = new List<Models.PunchTicketRoot>();
            Models.PunchTicketRoot mPunchTicketRoot = new Models.PunchTicketRoot();
            mPunchTicketRoots.Add(mPunchTicketRoot);

            if (!Request.Content.IsFormData())
            {
                _mPunchTicket.Status = RestStatus.NotFormData;
                _mPunchTicket.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
            {
                _mPunchTicket.Status = RestStatus.ParameterError;
                _mPunchTicket.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mPunchTicket.Status = RestStatus.AuthenticationFailed;
                _mPunchTicket.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            Guid guidContentGuid = string.IsNullOrEmpty(dic["ContentId"]) ? Guid.Empty : new Guid(dic["ContentId"]);
            int intQuantity = string.IsNullOrEmpty(dic["Quantity"]) ? 0 : Convert.ToInt32(dic["Quantity"]);
            DateTime dtExpireDateTime = string.IsNullOrEmpty(dic["ExpireDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["ExpireDateTime"]);
            List<Guid> tagGuids = string.IsNullOrEmpty(dic["Tags"]) ? null : tagGuids = dic["Tags"].Split(',').Select(s => Guid.Parse(s)).ToList();
            string strComment = string.IsNullOrEmpty(dic["Comment"]) ? string.Empty : dic["Comment"];
            string strText = string.IsNullOrEmpty(dic["Text"]) ? string.Empty : dic["Text"].ToString();
            string strOrderNo = string.IsNullOrEmpty(dic["OrderNo"]) ? string.Empty : dic["OrderNo"].ToString();

            // Validate
            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                _mPunchTicket.Status = RestStatus.NotExisting;
                _mPunchTicket.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            DB.tContent content = new DB.ContentRepository().GetContent(guidContentGuid);
            if(content == null)
            {
                _mPunchTicket.Status = RestStatus.NotExisting;
                _mPunchTicket.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }
            if(content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mPunchTicket.Status = RestStatus.NotExisting;
                _mPunchTicket.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            if(intQuantity < 0)
            {
                _mPunchTicket.Status = RestStatus.ParameterError;
                _mPunchTicket.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            if (dtExpireDateTime < DateTime.Now)
            {
                _mPunchTicket.Status = RestStatus.ParameterError;
                _mPunchTicket.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            if (tagGuids.Count == 0)
            {
                _mPunchTicket.Status = RestStatus.ParameterError;
                _mPunchTicket.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            // Create new Punch ticket
            DB.PunchTicketService punchTicketService = new DB.PunchTicketService();
            if (punchTicketService.AddPunchTicket(usertoken.UserGuid, customer.CustomerGuid, guidContentGuid, intQuantity, dtExpireDateTime, tagGuids, strComment, strText, strOrderNo) != DB.Repository.Status.Success)
            {
                _mPunchTicket.Status = RestStatus.GenericError;
                _mPunchTicket.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            // Reget all Punch Tickets
            IQueryable<DB.tPunchTicket> punchTickets = new DB.PunchTicketRepository().GetPunchTickets(customer.CustomerGuid);
            if(!punchTickets.Any())
            {
                _mPunchTicket.Status = RestStatus.GenericError;
                _mPunchTicket.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            IQueryable<DB.tTagList> tagLists = new DB.TagListRepository().GetTagLists(usertoken.CompanyGuid);

            // Populate
            List<TagListHeader> tagListHeaders = new List<TagListHeader>();
            foreach (DB.tTagList tagList in tagLists)
            {
                TagListHeader mTagListHeader = new TagListHeader();
                mTagListHeader.Name = tagList.Name;
                mTagListHeader.TagListId = tagList.TagListGuid.ToString();
                tagListHeaders.Add(mTagListHeader);
            }
            mPunchTicketRoot.TagListHeader = tagListHeaders;

            _mPunchTickets.Clear();
            foreach (DB.tPunchTicket punchTicket in punchTickets)
            {
                PunchTicket mPunchTicket = new PunchTicket();
                mPunchTicket.PunchTicketId = punchTicket.PunchTicketGuid.ToString();
                mPunchTicket.ContentName = punchTicket.tContent.Name;
                mPunchTicket.Quantity = punchTicket.Quantity;
                mPunchTicket.Used = punchTicket.Used;
                mPunchTicket.ExpireDateTime = punchTicket.ExpireDateTime;
                mPunchTicket.TimeStamp = punchTicket.TimeStamp;
                mPunchTicket.Text = punchTicket.Text;
                mPunchTicket.OrderNo = punchTicket.OrderNo;

                List<Tag> mTags = new List<Tag>();
                foreach (DB.tPunchTicketTag punchTicketTag in punchTicket.tPunchTicketTag)
                {
                    Tag mTag = new Tag();
                    mTag.TagListId = punchTicketTag.tTag.TagListGuid.ToString();
                    mTag.TagId = punchTicketTag.TagGuid.ToString();
                    mTag.Name = punchTicketTag.tTag.Name;
                    mTags.Add(mTag);
                }

                mPunchTicket.Tags = mTags;
                _mPunchTickets.Add(mPunchTicket);
            }
            mPunchTicketRoot.PunchTickets = _mPunchTickets;

            // Success
            _mPunchTicket.Status = RestStatus.Success;
            _mPunchTicket.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
        }
        
        public HttpResponseMessage Get(string token, string customerId)
        {
            List<Models.PunchTicketRoot> mPunchTicketRoots = new List<Models.PunchTicketRoot>();
            Models.PunchTicketRoot mPunchTicketRoot = new Models.PunchTicketRoot();
            mPunchTicketRoots.Add(mPunchTicketRoot);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId))
            {
                mPunchTicketRoot.Status = RestStatus.ParameterError;
                mPunchTicketRoot.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mPunchTicketRoot.Status = RestStatus.AuthenticationFailed;
                mPunchTicketRoot.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
            }

            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                mPunchTicketRoot.Status = RestStatus.NotExisting;
                mPunchTicketRoot.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
            }

            // Get PunchTickets
            IQueryable<DB.tPunchTicket> punchTickets = new DB.PunchTicketRepository().GetPunchTickets(customer.CustomerGuid);
            if (!punchTickets.Any())
            {
                mPunchTicketRoot.Status = RestStatus.NotExisting;
                mPunchTicketRoot.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
            }

            IQueryable<DB.tTagList> tagLists = new DB.TagListRepository().GetTagLists(usertoken.CompanyGuid);

            // Populate
             List<TagListHeader> tagListHeader = new List<TagListHeader>();
             foreach (DB.tTagList tagList in tagLists)
             {
                 TagListHeader mTagListHeader = new TagListHeader();
                 mTagListHeader.Name = tagList.Name;
                 mTagListHeader.TagListId = tagList.TagListGuid.ToString();
                 tagListHeader.Add(mTagListHeader);
             }
             mPunchTicketRoot.TagListHeader = tagListHeader;

             _mPunchTickets.Clear();
            foreach(DB.tPunchTicket punchTicket in punchTickets)
            {
                PunchTicket mPunchTicket = new PunchTicket();
                mPunchTicket.PunchTicketId = punchTicket.PunchTicketGuid.ToString();
                mPunchTicket.ContentName = punchTicket.tContent.Name;
                mPunchTicket.Quantity = punchTicket.Quantity;
                mPunchTicket.Used = punchTicket.Used;
                mPunchTicket.ExpireDateTime = punchTicket.ExpireDateTime;
                mPunchTicket.TimeStamp = punchTicket.TimeStamp;
                mPunchTicket.Text = punchTicket.Text;
                mPunchTicket.OrderNo = punchTicket.OrderNo;

                List<Tag> mTags = new List<Tag>();
                foreach (DB.tPunchTicketTag punchTicketTag in punchTicket.tPunchTicketTag)
                {
                    Tag mTag = new Tag();
                    mTag.TagListId = punchTicketTag.tTag.TagListGuid.ToString();
                    mTag.TagId = punchTicketTag.TagGuid.ToString();
                    mTag.Name = ML.Common.Text.Truncate(punchTicketTag.tTag.Name, "(", ")").Replace("(", string.Empty).Replace(")", string.Empty);
                    mTags.Add(mTag);
                }

                mPunchTicket.Tags = mTags;
                _mPunchTickets.Add(mPunchTicket);
            }
            mPunchTicketRoot.PunchTickets = _mPunchTickets;

            // Success
            mPunchTicketRoot.Status = RestStatus.Success;
            mPunchTicketRoot.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketRoots));
        }

        public HttpResponseMessage Post(string token, string customerId, string punchTicketId, int actionType)
        {
            if (!Request.Content.IsFormData())
            {
                _mPunchTicket.Status = RestStatus.NotFormData;
                _mPunchTicket.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(customerId) || !ML.Common.Text.IsGuidNotEmpty(punchTicketId) || actionType < 1 || actionType > 4)
            {
                _mPunchTicket.Status = RestStatus.ParameterError;
                _mPunchTicket.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mPunchTicket.Status = RestStatus.AuthenticationFailed;
                _mPunchTicket.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                _mPunchTicket.Status = RestStatus.NotExisting;
                _mPunchTicket.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            // Get PunchTicket
            DB.tPunchTicket punchTicket = new DB.PunchTicketRepository().GetPunchTicket(new Guid(punchTicketId));
            if (punchTicket == null)
            {
                _mPunchTicket.Status = RestStatus.NotExisting;
                _mPunchTicket.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strComment = string.IsNullOrEmpty(dic["Comment"]) ? string.Empty : dic["Comment"];


            DB.PunchTicketLogRepository.ActionType theActionType = (DB.PunchTicketLogRepository.ActionType)Enum.Parse(typeof(DB.PunchTicketLogRepository.ActionType), actionType.ToString());
            if (theActionType == DB.PunchTicketLogRepository.ActionType.IncrementQuantity)
            {
                DB.Repository.Status status = new DB.PunchTicketService().IncreaseQuantity(punchTicket.PunchTicketGuid, usertoken.UserGuid, strComment);
                if (status != DB.Repository.Status.Success)
                {
                    _mPunchTicket.Status = RestStatus.GenericError;
                    _mPunchTicket.StatusText = "GenericError";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                _mPunchTicket.Quantity = punchTicket.Quantity + 1;
            }
            else if (theActionType == DB.PunchTicketLogRepository.ActionType.DecrementQuantity)
            {
                DB.Repository.Status status = new DB.PunchTicketService().DecreaseQuantity(punchTicket.PunchTicketGuid, usertoken.UserGuid, strComment);
                if (status == DB.Repository.Status.ExceedingLimit)
                {
                    _mPunchTicket.Status = RestStatus.Illegal;
                    _mPunchTicket.StatusText = "Illegal";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                else if (status != DB.Repository.Status.Success)
                {
                    _mPunchTicket.Status = RestStatus.GenericError;
                    _mPunchTicket.StatusText = "GenericError";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                _mPunchTicket.Quantity = punchTicket.Quantity - 1;
            }
            else if (theActionType == DB.PunchTicketLogRepository.ActionType.IncrementUsed)
            {
                DB.Repository.Status status = new DB.PunchTicketService().Use(punchTicket.PunchTicketGuid, usertoken.UserGuid, strComment);
                if (status == DB.Repository.Status.ExceedingLimit)
                {
                    _mPunchTicket.Status = RestStatus.Illegal;
                    _mPunchTicket.StatusText = "Illegal";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                else if (status != DB.Repository.Status.Success)
                {
                    _mPunchTicket.Status = RestStatus.GenericError;
                    _mPunchTicket.StatusText = "GenericError";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                _mPunchTicket.Used = punchTicket.Used + 1;
            }
            else if (theActionType == DB.PunchTicketLogRepository.ActionType.DecrementUsed)
            {
                DB.Repository.Status status = new DB.PunchTicketService().Unuse(punchTicket.PunchTicketGuid, usertoken.UserGuid, strComment);
                if(status == DB.Repository.Status.ExceedingLimit)
                {
                    _mPunchTicket.Status = RestStatus.Illegal;
                    _mPunchTicket.StatusText = "Illegal";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                else if (status != DB.Repository.Status.Success)
                {
                    _mPunchTicket.Status = RestStatus.GenericError;
                    _mPunchTicket.StatusText = "GenericError";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
                }
                _mPunchTicket.Used = punchTicket.Used - 1;
            }


            _mPunchTicket.PunchTicketLogs = null;
            List<string> lst = new DB.PunchTicketLogService().GetPunchTicketLog(customer.CustomerGuid);
            if (lst.Count > 0)
            {
                List<PunchTicketLog> punchTicketLogs = new List<PunchTicketLog>();
                foreach (string str in lst)
                {
                    PunchTicketLog punchTicketLog = new PunchTicketLog();
                    punchTicketLog.Comment = str;
                    punchTicketLogs.Add(punchTicketLog);
                }
                _mPunchTicket.PunchTicketLogs = punchTicketLogs;
            }

            // Success
            _mPunchTicket.Status = RestStatus.Success;
            _mPunchTicket.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPunchTickets));
        }

        public HttpResponseMessage Post(string token)
        {
            List<Models.PunchTicketReport> mPunchTicketReports = new List<Models.PunchTicketReport>();
            Models.PunchTicketReport mPunchTicketReport = new Models.PunchTicketReport();
            mPunchTicketReports.Add(mPunchTicketReport);

            if (!Request.Content.IsFormData())
            {
                mPunchTicketReport.Status = RestStatus.NotFormData;
                mPunchTicketReport.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketReports));
            }

            if (string.IsNullOrEmpty(token))
            {
                mPunchTicketReport.Status = RestStatus.ParameterError;
                mPunchTicketReport.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketReports));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mPunchTicketReport.Status = RestStatus.AuthenticationFailed;
                mPunchTicketReport.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketReports));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            Guid guidContentGuid = string.IsNullOrEmpty(dic["ContentId"]) ? Guid.Empty : new Guid(dic["ContentId"]);
            DateTime dtStartDateTime = string.IsNullOrEmpty(dic["StartDateTime"]) ? DateTime.Now.Date.AddMonths(-1) : Convert.ToDateTime(dic["StartDateTime"]);
            DateTime dtEndDateTime = string.IsNullOrEmpty(dic["EndDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["EndDateTime"]);
            int intQuantity = string.IsNullOrEmpty(dic["Quantity"]) ? -1 : Convert.ToInt32(dic["Quantity"]);
            List<Guid> tagGuids = string.IsNullOrEmpty(dic["Tags"]) ? null : tagGuids = dic["Tags"].Split(',').Select(s => Guid.Parse(s)).ToList();
            DateTime? dtRedeemStartDateTime = string.IsNullOrEmpty(dic["RedeemStartDateTime"]) ? null : (DateTime?)Convert.ToDateTime(dic["RedeemStartDateTime"]);
            DateTime? dtRedeemEndDateTime = string.IsNullOrEmpty(dic["RedeemEndDateTime"]) ? null : (DateTime?)Convert.ToDateTime(dic["RedeemEndDateTime"]);
            string strText = string.IsNullOrEmpty(dic["Text"]) ? string.Empty : dic["Text"].ToString();
            string strOrderNo = string.IsNullOrEmpty(dic["OrderNo"]) ? string.Empty : dic["OrderNo"].ToString();

            // Search
            IQueryable<DB.tPunchTicket> punchTickets = new DB.PunchTicketRepository().Search(usertoken.CompanyGuid, guidContentGuid, intQuantity, dtStartDateTime, dtEndDateTime, tagGuids, dtRedeemStartDateTime, dtRedeemEndDateTime, strText, strOrderNo);
            DB.PunchTicketService.Report report = new DB.PunchTicketService().SearchForReport(usertoken.CompanyGuid, guidContentGuid, intQuantity, dtStartDateTime, dtEndDateTime, tagGuids, dtRedeemStartDateTime, dtRedeemEndDateTime, strText, strOrderNo);
            
            IQueryable<DB.tTagList> tagLists = new DB.TagListRepository().GetTagLists(usertoken.CompanyGuid);

            // Populate
            List<TagListHeader> tagListHeader = new List<TagListHeader>();
            foreach (DB.tTagList tagList in tagLists)
            {
                TagListHeader mTagListHeader = new TagListHeader();
                mTagListHeader.Name = tagList.Name;
                mTagListHeader.TagListId = tagList.TagListGuid.ToString();
                tagListHeader.Add(mTagListHeader);
            }
            mPunchTicketReport.TagListHeader = tagListHeader;




            _mPunchTickets.Clear();
            foreach (DB.tPunchTicket punchTicket in punchTickets)
            {
                PunchTicket mPunchTicket = new PunchTicket();
                mPunchTicket.PunchTicketId = punchTicket.PunchTicketGuid.ToString();
                mPunchTicket.ContentName = punchTicket.tContent.Name;
                mPunchTicket.Quantity = punchTicket.Quantity;
                mPunchTicket.Used = punchTicket.Used;
                mPunchTicket.ExpireDateTime = punchTicket.ExpireDateTime;
                mPunchTicket.TimeStamp = punchTicket.TimeStamp;
                mPunchTicket.Text = punchTicket.Text;
                mPunchTicket.OrderNo = punchTicket.OrderNo;

                mPunchTicket.CustomerId = punchTicket.tCustomer.AdditionalCustomerNo;
                mPunchTicket.CustomerFullName = string.Format("{0} {1} ({2})", punchTicket.tCustomer.FirstName, punchTicket.tCustomer.LastName, punchTicket.tCustomer.PhoneNo);

                

                List<Tag> mTags = new List<Tag>();
                foreach (DB.tPunchTicketTag punchTicketTag in punchTicket.tPunchTicketTag)
                {
                    Tag mTag = new Tag();
                    mTag.TagListId = punchTicketTag.tTag.TagListGuid.ToString();
                    mTag.TagId = punchTicketTag.TagGuid.ToString();
                    //mTag.Name = punchTicketTag.tTag.Name;
                    mTag.Name = ML.Common.Text.Truncate(punchTicketTag.tTag.Name, "(", ")").Replace("(", string.Empty).Replace(")", string.Empty);
                    mTags.Add(mTag);
                }

                mPunchTicket.Tags = mTags;
                _mPunchTickets.Add(mPunchTicket);
            }
            mPunchTicketReport.PunchTickets = _mPunchTickets;



            PunchTicketdUsedFactor punchTicketdUsedFactor = new PunchTicketdUsedFactor();
            punchTicketdUsedFactor.Count = report.Count;
            punchTicketdUsedFactor.Used = report.Used;
            punchTicketdUsedFactor.UsedFactor = report.UsedFactor;
            mPunchTicketReport.PunchTicketUsedFactor = punchTicketdUsedFactor;
            mPunchTicketReport.PunchTicketUsedFactor = punchTicketdUsedFactor;
            
            // Success
            mPunchTicketReport.Status = RestStatus.Success;
            mPunchTicketReport.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mPunchTicketReports));
        }





        
    }
}
