using CleanCash_1_1;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO.Ports;
using POSSUM.Data;
using System.Xml.Linq;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Sockets;

namespace POSSUM.Integration.ControlUnits.CleanCash
{
    public class CloudCashControlUnit : IControlUnit
    {
        public CloudCashControlUnit(String connectionString, string orgNumber, string posId)
        {

        }

        public bool Open()
        {
            return true;
        }

        public bool RegisterPOS(string orgNumber, string posId)
        {
            return true;
        }

        public ControlUnitStatus CheckStatus()
        {
            return ControlUnitStatus.OK;
        }

        public ControlUnitResponse SendReceipt(DateTime dateTime, long reciptId, decimal totalAmount, decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false)
        {
            if (string.IsNullOrEmpty(CleanCashConfiguraiton.CleanCashBaseUrl) || string.IsNullOrEmpty(CleanCashConfiguraiton.TaxDescription) || 
                string.IsNullOrEmpty(CleanCashConfiguraiton.CCUData) || string.IsNullOrEmpty(CleanCashConfiguraiton.UniqueIdentification))
            {
                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Unable to Call Clean Cash Tax Description or CCUData or Unique Identification is Missing");
                return new ControlUnitResponse(false, "", "");
            }

            try
            {
                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Calling...");

                HttpResponseMessage response = GetCleanCash(dateTime, reciptId, totalAmount, negativeAmount, vat1, vat2, vat3, vat4, attemptNo, isCopy, isPerforma);

                if (response != null && response.IsSuccessStatusCode)
                {
                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Cloud Clean Cash Response Successfull.");

                    var result = response.Content.ReadAsStringAsync().Result;

                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Cloud Clean Cash Response Successfull Result: " + result);

                    CcuResponse objCcuResponse = JsonConvert.DeserializeObject<CcuResponse>(result);
                    return new ControlUnitResponse(true, objCcuResponse.controlUnitSerial, objCcuResponse.response);
                }
                else
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Cloud Clean Cash Response Failed Result: " + result);

                    return new ControlUnitResponse(false, "", "");
                }
            }
            catch (Exception e)
            {
                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: Exception Generated: " + e.ToString());
                return new ControlUnitResponse(false, "", "");
            }
        }

        private HttpResponseMessage GetCleanCash(DateTime dateTime, long reciptId, decimal totalAmount, decimal negativeAmount, VAT vat1, VAT vat2, VAT vat3, VAT vat4, int attemptNo, bool isCopy = false, bool isPerforma = false)
        {
            try
            {
                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Calling...");

                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: ReceiptId: " + reciptId + "Total Amount: " + totalAmount);

                CcuPost ccuPost = new CcuPost();
                VatRateToSum vatRateToSum = new VatRateToSum();

                if (totalAmount > 0)
                {
                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Normal Order ReceiptId: " + reciptId + "Total Amount: " + totalAmount);

                    double total = getOre((double)totalAmount);

                    ccuPost.brutto = (long)total;
                    ccuPost.refund = false;
                }
                else
                {
                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Return Order ReceiptId: " + reciptId + "Total Amount: " + totalAmount);

                    double total = getOre((double)totalAmount * -1);

                    ccuPost.brutto = (long)total;
                    ccuPost.refund = true;
                }

                vatRateToSum = setVatRateToSum(vatRateToSum, vat1);
                vatRateToSum = setVatRateToSum(vatRateToSum, vat2);
                vatRateToSum = setVatRateToSum(vatRateToSum, vat3);
                vatRateToSum = setVatRateToSum(vatRateToSum, vat4);

                ccuPost.vatRateToSum = vatRateToSum;

                if (!isCopy)
                {
                    ccuPost.printType = "Normal";
                }
                else
                {
                    ccuPost.printType = "Copy";
                }

                ccuPost.receiptNumber = (int)reciptId;

                long validTime = ((DateTimeOffset) dateTime).ToUnixTimeMilliseconds();
                ccuPost.date = validTime;

                string ccuData = StringCipher.decodeSTROnUrl(CleanCashConfiguraiton.CCUData);
                CcuData objCcuData = JsonConvert.DeserializeObject<CcuData>(ccuData);

                using (var httpClient = new HttpClient())
                {
                    String baseUrl = CleanCashConfiguraiton.CleanCashBaseUrl.ToLower();
                    String taxDescription = CleanCashConfiguraiton.TaxDescription.ToLower();
                    String terminalCode = CleanCashConfiguraiton.UniqueIdentification.Replace("-", "");

                    var srv4posUrl = baseUrl + "/" + taxDescription + "/registration/" + terminalCode + "/kd";

                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Final Url" + srv4posUrl);

                    string encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(objCcuData.apiKey), Base64FormattingOptions.None);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedString);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                    string json = JsonConvert.SerializeObject(ccuPost);

                    IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Json: " + json);

                    var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                    stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return httpClient.PostAsync(srv4posUrl, stringContent).Result;
                }
            }
            catch (Exception e)
            {
                IntegrationLogWriter.CloudCleanCashLogWrite("SendReceipt: GetCleanCash: Exception Generated: " + e.ToString());
                return null;
            }
        }

        private double getOre(double value)
        {
            if (value < 0)
            {
                value = value * -1;
            }
            String amount = value.ToString("0.00");
            amount = amount.Replace(".", "").Replace(",", "");
            return long.Parse(amount);
        }

        private VatRateToSum setVatRateToSum(VatRateToSum vatRateToSum, VAT vat)
        {
            if (vat.VATPercent == 0)
            {
                double taxAmount0 = getOre((double) vat.VATTotal);
                vatRateToSum.v0 = (long)taxAmount0;
            }
            else if (vat.VATPercent == 6)
            {
                double taxAmount6 = getOre((double) vat.VATTotal);
                vatRateToSum.v6 = (long)taxAmount6;
            }
            else if (vat.VATPercent == 0)
            {
                double taxAmount12 = getOre((double) vat.VATTotal);
                vatRateToSum.v12 = (long)taxAmount12;
            }
            else
            {
                double taxAmount25 = getOre((double) vat.VATTotal);
                vatRateToSum.v25 = (long)taxAmount25;
            }

            return vatRateToSum;
        }

        public ControlUnitResponse SendReceipt(Model.Receipt receipt, int attemptNo, bool isCopy = false)
        {
            return SendReceipt(receipt.PrintDate, receipt.ReceiptNumber, receipt.GrossAmount, receipt.NegativeAmount, receipt.VatDetails[0], receipt.VatDetails[1], receipt.VatDetails[2], receipt.VatDetails[3], attemptNo, isCopy);
        }

        public bool Dispose()
        {
            return true;
        }

        public bool Close()
        {
            return true;
        }

        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo)
        {
            return SendReceipt(receipt, attemptNo);
        }

        //WAQAS_CHANGES
        public ControlUnitResponse SendReceipt(Receipt receipt, OutletUser user, int attemptNo, bool isCopy = false)
        {
            return SendReceipt(receipt, attemptNo, isCopy);
        }
    }
}
