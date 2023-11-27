using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Utils.Controller;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils
{
    public class DataSyncController
    {
        public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId)
        {
            return DataSync(EXECUTED_DATETIME, terminalId, ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString, "", "", "");
        }


        public bool DataSync(DateTime EXECUTED_DATETIME, Guid terminalId, string connectionstring, string baseUrl, string apiUser, string apiPassword)
        {
            try
            {
                DateTime LAST_EXECUTED_DATETIME = DateTime.Now.Date;

                var db = new ApplicationDbContext(connectionstring);



                DataServiceClient serviceClient = new DataServiceClient(baseUrl, apiUser, apiPassword,true);

                //Get updated products list                
                var productData = serviceClient.GetProducts(terminalId, LAST_EXECUTED_DATETIME, EXECUTED_DATETIME);

                DataProductController productController = new DataProductController(productData, db);
                bool resProduct = productController.UpdateProduct();

                productController.UploadProducts(LAST_EXECUTED_DATETIME, baseUrl, apiUser, apiPassword);                
               
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }

        }
    }

}
