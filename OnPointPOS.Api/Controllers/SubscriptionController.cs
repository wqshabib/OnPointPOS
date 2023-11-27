using POSSUM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace POSSUM.Api.Controllers
{

    [System.Web.Http.RoutePrefix("api/Subscription")]
    public class SubscriptionController : BaseAPIController
    {


        string connectionString = "";
        bool nonAhenticated = false;
        public SubscriptionController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }


        [Route("GetSubscription")]
        public bool GetSubscription([FromUri] string companyGuid)
        {
            try
            {
                LogWriter.LogWrite("GetSubscription is calling: connectionString:" + connectionString);
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var compnyGuid = new Guid(companyGuid);
                    var subscription = db.Subscription.FirstOrDefault(usr => usr.SubscriptionId == compnyGuid);
                    if (subscription != null && subscription.status == "active")
                    {
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
