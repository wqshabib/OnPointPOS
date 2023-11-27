using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSSUM.Data;
using POSSUM.Integration;
using POSSUM.Model;

using Newtonsoft.Json;

namespace POSSUM.Handlers
{
    public class ReceiptHandler
    {
        /// <summary>
        /// Save receipt and return the new receiptnumber
        /// </summary>
        /// <param name="receipt"></param>
        /// <returns></returns>
        public void Create(Receipt receipt)
        {

            if (receipt.VatDetails.Count > 0)
                receipt.VatDetail = JsonConvert.SerializeObject(receipt.VatDetails);
            Defaults.ReceiptCounter = Defaults.ReceiptCounter + 1;
            receipt.ReceiptNumber = Defaults.ReceiptCounter;
            new ReceiptRepository(PosState.GetInstance().Context).Create(receipt);

        }

        public void Update(Receipt receipt)
        {

            new ReceiptRepository(PosState.GetInstance().Context).Update(receipt);

        }
        public void Delete(Receipt receipt)
        {
            try
            {
                LogWriter.CheckOutLogWrite("Deleting  receipt data after Control unit failure", receipt.OrderId);
                LogWriter.LogWrite("Deleting  receipt data after Control unit failure");
                using (var  db = PosState.GetInstance().Context)
                {
                   
                    var receiptData = db.Receipt.FirstOrDefault(r => r.ReceiptId == receipt.ReceiptId);
                    if (receiptData != null)
                    {
                        db.Receipt.Remove(receiptData);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }

        }
        public Receipt Get(Guid receiptId)
        {
            try
            {
                return new ReceiptRepository(PosState.GetInstance().Context).Get(receiptId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Receipt();
            }
        }

        internal Receipt GetByOrderId(Guid orderId)
        {
            try
            {
                return new ReceiptRepository(PosState.GetInstance().Context).GetByOrderId(orderId);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Receipt();
            }
        }

        internal Receipt GetByUnitName(string unitName)
        {
            try
            {
                return new ReceiptRepository(PosState.GetInstance().Context).GetByUnitName(unitName);
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Receipt();
            }
        }

        internal Receipt GetLastReceipt()
        {
            try
            {
                return new ReceiptRepository(PosState.GetInstance().Context).GetLastReceipt();
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return new Receipt();
            }
        }

        public Receipt FromDb(Receipt receipt)
        {
            Receipt r = new Receipt
            {
                ReceiptId = receipt.ReceiptId,
                TerminalId = receipt.TerminalId,
                ReceiptNumber = receipt.ReceiptNumber,
                OrderId = receipt.OrderId,
                ReceiptCopies = receipt.ReceiptCopies,
                GrossAmount = receipt.GrossAmount,
                VatAmount = receipt.VatAmount,
                VatDetails = JsonConvert.DeserializeObject<List<VAT>>(receipt.VatDetail),
                PrintDate = receipt.PrintDate,
                ControlUnitName = receipt.ControlUnitName,
                ControlUnitCode = receipt.ControlUnitCode,
                CustomerPaymentReceipt = receipt.CustomerPaymentReceipt,
                MerchantPaymentReceipt = receipt.MerchantPaymentReceipt,
                IsSignature = receipt.IsSignature
            };
            return r;
        }

        public Receipt FromOrderMaster(Order orderMaster, List<VAT> vatAmounts, Guid terminalId, PaymentTransactionStatus creditcardPaymentResult)
        {
            try
            {            
                StringBuilder sb = new StringBuilder();
                string custReceipt = "";
                string mercReceipt = "";
                bool isSignature = false;


                    if (creditcardPaymentResult != null && creditcardPaymentResult.CustomerReceipt != null)
                    {               
                        sb.AppendLine();
                        sb.AppendLine();
                        var customerReceipt = creditcardPaymentResult.CustomerReceipt.ToList();
                        //TODO´: Add this condition on BamboraConnect2TApi class for Offline Payment
                        var i = customerReceipt.IndexOf("ENCRYPTED INFORMATION");
                        if (i > 0)
                            customerReceipt.RemoveRange(i , 4);

                        customerReceipt.ForEach(ro => { sb.AppendLine(ro); });

                        try
                        {

                            int index = creditcardPaymentResult.CustomerReceipt.ToList().FindIndex(x => !string.IsNullOrEmpty(x) && x.StartsWith("SIGN:"));


                            //if (index != -1)
                            //{
                            //    var count = creditcardPaymentResult.CustomerReceipt.Count;
                            //    for (int i = index; i < count; i++)
                            //    {
                            //        creditcardPaymentResult.CustomerReceipt.RemoveAt(index);
                            //    }
                            //}

                        }
                        catch (Exception ex)
                        {
                            LogWriter.LogException(ex);
                        }
                        //TODO: Temprary fix, should not be included IN CASES LIKE RETURN
                        sb.Replace("*** SPARA KVITTOT", "");
                        sb.AppendLine("*** SPARA KVITTOT, KUNDENS KOPIA ");
                        custReceipt = sb.ToString();
                    }
                    if (creditcardPaymentResult != null && creditcardPaymentResult.MerchantReceipt != null)
                    {
                        sb = new StringBuilder();
                        sb.AppendLine();
                        sb.AppendLine();
                        creditcardPaymentResult.MerchantReceipt.ToList().ForEach(ro =>
                        {
                            if (ro.Equals("SIGN:"))
                                sb.AppendLine();
                            sb.AppendLine(ro);
                            if (ro.Equals("SIGN:") || ro.Equals("ID:") || ro.Equals("Kassörens namnteckning") ||
                                ro.Equals("Kassörens namn"))

                            {
                                isSignature = true;
                                //sb.AppendLine();
                                //sb.AppendLine();
                                //sb.AppendLine("..............................");
                            }
                            //if (ro.Equals("GODKÄNNES FÖR DEBITERING AV MITT") || ro.Equals("KONTO ENLIGT OVAN") )
                            //{
                            //    sb.AppendLine();
                            //}

                        });
                        sb.Replace("*** SPARA KVITTOT", "");
                        sb.AppendLine("*** SPARA KVITTOT, BUTIKENS KOPIA ");
                        mercReceipt = sb.ToString();
                    }

                    var r = new Receipt
                    {
                        ReceiptId = Guid.NewGuid(),
                        TerminalNo = Defaults.Terminal.TerminalNo,
                        TerminalId = terminalId,
                        OrderId = orderMaster.Id,
                        ReceiptNumber = 0,
                        ReceiptCopies = 0,
                        GrossAmount = orderMaster.OrderTotal,
                        VatAmount = vatAmounts.Sum(xx => xx.VATTotal),
                        VatDetails = vatAmounts,
                        PrintDate =
                            orderMaster.InvoiceDate.HasValue ? Convert.ToDateTime(orderMaster.InvoiceDate) : DateTime.Now,
                        ControlUnitName = "",
                        ControlUnitCode = "",
                        CustomerPaymentReceipt = custReceipt,
                        MerchantPaymentReceipt = mercReceipt,
                        IsSignature = isSignature
                    };
                        return r;
                }
                catch (Exception ex)
                {
                    LogWriter.LogException(ex);
                    return new Receipt();
                }           
            }
    }

}