using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ML.Rest2.Controllers
{
    public class EnjoyRegistrationController : ApiController
    {
        public HttpResponseMessage Post(string secret, string companyId)
        {   
            List<EnjoyRegistration> mEnjoyRegistrations = new List<EnjoyRegistration>();
            EnjoyRegistration mEnjoyRegistration = new EnjoyRegistration();

            mEnjoyRegistration.Status = 0;
            mEnjoyRegistration.Status = EnjoyRestStatus.Success;

            mEnjoyRegistrations.Add(mEnjoyRegistration);

            if (!Request.Content.IsFormData())
            {
                mEnjoyRegistration.Status = EnjoyRestStatus.NotFormData;
                mEnjoyRegistration.StatusText = "No FormData";
                //new ML.Email.Email().SendDebug("EnjoyRegistrationController", mEnjoyRegistration.StatusText);
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                mEnjoyRegistration.Status = EnjoyRestStatus.ParameterError;
                mEnjoyRegistration.StatusText = "Parameter Error";
                //new ML.Email.Email().SendDebug("EnjoyRegistrationController", mEnjoyRegistration.StatusText);
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            }
            Guid guidCompanyGuid = new Guid(companyId);

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                mEnjoyRegistration.Status = EnjoyRestStatus.AuthenticationFailed;
                mEnjoyRegistration.StatusText = "Authentication Failed";
                //new ML.Email.Email().SendDebug("EnjoyRegistrationController", mEnjoyRegistration.StatusText);
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strActivationCode = string.IsNullOrEmpty(dic["ActivationCode"]) ? string.Empty : dic["ActivationCode"];

            // Validate
            strPhoneNo = ML.Common.SmsHelper.CleanPhoneNumber(strPhoneNo);
            var data = "strFirstName:"+ strFirstName+ ",strLastName:" + strLastName + ",strEmail:" + strEmail + ",strPhoneNo:" + strPhoneNo + ",strActivationCode:" + strActivationCode + ",";
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = null;
            if (string.IsNullOrEmpty(strPhoneNo))
            {
                data += ", CASE1";
                string strAdditionalCustomerNo = Guid.NewGuid().ToString();
                // Create new customer
                new DB.CustomerService().AddUpdateCustomer(
                    guidCompanyGuid
                    , string.Empty
                    , Customer.Enums.CustomerType.Api
                    , strFirstName
                    , strLastName
                    , strAdditionalCustomerNo
                    , string.Empty
                    , string.Empty
                    , strEmail
                    , string.Empty
                    , string.Empty
                    , string.Empty
                    );
                data += ",strAdditionalCustomerNo:" + strAdditionalCustomerNo;
                // Reget in context
                customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, strAdditionalCustomerNo);

                if (customer == null)
                {
                    mEnjoyRegistration.Status = EnjoyRestStatus.NotExisting;
                    mEnjoyRegistration.StatusText = "Not Existing";
                    //new ML.Email.Email().SendDebug("EnjoyRegistrationController ERROR01", data);
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
                }
            }
            else
            {
                // Lookup customer
                //bool bNewCustomer = false;
                customer = customerRepository.GetByCompanyGuidAndPhoneNo(guidCompanyGuid, strPhoneNo);
                if (customer == null)
                {
                    //bNewCustomer = true;

                    // Create new customer
                    new DB.CustomerService().AddUpdateCustomer(
                        guidCompanyGuid
                        , strPhoneNo
                        , Customer.Enums.CustomerType.Api
                        , strFirstName
                        , strLastName
                        , string.Empty
                        , string.Empty
                        , string.Empty
                        , strEmail
                        , string.Empty
                        , string.Empty
                        , string.Empty
                        );

                    // Reget in context
                    customer = customerRepository.GetByCompanyGuidAndPhoneNo(guidCompanyGuid, strPhoneNo);
                    data += ", CASE2";
                    if (customer == null)
                    {
                        mEnjoyRegistration.Status = EnjoyRestStatus.NotExisting;
                        mEnjoyRegistration.StatusText = "Not Existing";
                        //new ML.Email.Email().SendDebug("EnjoyRegistrationController ERROR02", data);
                        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
                    }
                }
                else
                {
                    // Update customer
                    new DB.CustomerService().AddUpdateCustomer(
                        guidCompanyGuid
                        , strPhoneNo
                        , Customer.Enums.CustomerType.Api
                        , strFirstName
                        , strLastName
                        , string.Empty
                        , string.Empty
                        , string.Empty
                        , strEmail
                        , string.Empty
                        , string.Empty
                        , string.Empty
                        );
                    data += ", CASE3";
                }
            }

            // ActivationCode exists ?
            DB.tActivation activation = new DB.ActivationRepository().GetByCompanyGuidAndCode(guidCompanyGuid, strActivationCode);
            //TEMP STEPS FOR SOME WRONG CODES OF ENJOY 2017
            if(activation == null && strActivationCode.StartsWith("0"))
            {

                if (strActivationCode.StartsWith("00"))
                    strActivationCode = strActivationCode.Replace("00", "");
                else
                    strActivationCode = strActivationCode.Replace("0", "");
                activation = new DB.ActivationRepository().GetByCompanyGuidAndCode(guidCompanyGuid, strActivationCode);
            }
            if (activation == null && strActivationCode.Length < 6)
            {
                if (strActivationCode.Length == 4)
                    strActivationCode = "00" + strActivationCode;
                else if (strActivationCode.Length == 5)
                    strActivationCode = "0" + strActivationCode;
                activation = new DB.ActivationRepository().GetByCompanyGuidAndCode(guidCompanyGuid, strActivationCode);
            }
            if (activation == null)
            {
                data += ", CASE4";

                mEnjoyRegistration.RegistrationStatus = -1;
                //new ML.Email.Email().SendDebug("EnjoyRegistrationController", data);
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            }

            //// ActivationCode belongs to other Customer
            //if (activation.CustomerGuid != null)
            //{
            //    if (activation.CustomerGuid != customer.CustomerGuid)
            //    {
            //        mEnjoyRegistration.RegistrationStatus = -2;
            //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            //    }
            //}

            // Activate
            ML.DB.ActivationService.RedeemStatus redeemStatus = new DB.ActivationService().TryRedeemManualActivation(guidCompanyGuid, customer.CustomerGuid, strActivationCode);
            if (redeemStatus == DB.ActivationService.RedeemStatus.Success)
            {
                data += ", CASE5";
                mEnjoyRegistration.RegistrationStatus = 0;
            }
            else if (redeemStatus == DB.ActivationService.RedeemStatus.ActivationRedeemed)
            {
                data += ", CASE6";
                mEnjoyRegistration.RegistrationStatus = 0;
            }
            else if (redeemStatus == DB.ActivationService.RedeemStatus.ActivationDoesNotExist)
            {
                data += ", CASE7";
                mEnjoyRegistration.RegistrationStatus = 0;
            }
            else
            {
                data += ", CASE8";
                mEnjoyRegistration.RegistrationStatus = -3;
                //new ML.Email.Email().SendDebug("EnjoyRegistrationController", data);
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
            }


            mEnjoyRegistration.Product = activation.tContentCategory.Name;


            //// Send initial link
            //if (!string.IsNullOrEmpty(customer.PhoneNo))
            //{
            //    DB.tSite site = new DB.SiteRepository().GetBySiteGuid(new Guid("4FCC3422-5C54-4354-AF14-530879B726D5"));
            //    if (site != null)
            //    {
            //        ML.Site.Helper.StartLink startLink = ML.Site.Helper.CreateLinks(site.SiteGuid, customer.CustomerGuid);
            //        string strSms = ML.Site.Helper.BuildPushSms(site.SmsPushMessage, startLink.Link, customer.FirstName, customer.LastName);

            //        // Send Sms
            //        DB.SmsHelper.SendSms(
            //            guidCompanyGuid
            //            , site.SiteGuid
            //            , Guid.Empty
            //            , site.Sender
            //            , customer.PhoneNo
            //            , strSms
            //            , DateTime.Now
            //            , customer.CustomerGuid
            //            );
            //    }
            //}


            // Success
            //new ML.Email.Email().SendDebug("EnjoyRegistrationController", data);
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mEnjoyRegistrations));
        }


    }
}
