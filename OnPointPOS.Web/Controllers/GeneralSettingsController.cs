using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    public class GeneralSettingsController : MyBaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Update20230504()
        {
            using (var db = GetConnection)
            {
                var model = new AllSettingsViewModel();
                var accountNumber = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.AccountNumber);
                if (accountNumber != null)
                    model.AccountNumber = accountNumber.Value;

                var invoiceReference = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.FakturaReference);
                if (invoiceReference != null)
                    model.InvoiceReference = invoiceReference.Value;

                var paymentReceiver = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PaymentReceiver);
                if (paymentReceiver != null)
                    model.PaymentReceiverName = paymentReceiver.Value;

                model.Email = CurrentUserEmail;

                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Update()
        {
            using (var db = GetConnection)
            {
                var model = new AllSettingsViewModel();
                var accountNumber = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.AccountNumber);
                if (accountNumber != null)
                    model.AccountNumber = accountNumber.Value;

                var invoiceReference = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.FakturaReference);
                if (invoiceReference != null)
                    model.InvoiceReference = invoiceReference.Value;

                var paymentReceiver = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PaymentReceiver);
                if (paymentReceiver != null)
                    model.PaymentReceiverName = paymentReceiver.Value;

                model.Email = CurrentUserEmail;

                var mobileLogo = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PrinterLogo);
                if (mobileLogo != null && !string.IsNullOrEmpty(mobileLogo.Value))
                    model.MobileLogoUrl = ConfigurationManager.AppSettings["BaseWebSitePath"] + "storage/PrinterLogo/" + mobileLogo.Value;
                else
                    model.MobileLogoUrl = "";


                return View(model);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSoftPayData(Guid TerminalId)
        {
            SoftPayViewModel objSoftPayViewModel = new SoftPayViewModel();
            var msg = "";

            try
            {
                using (var db = GetConnection)
                {
                    var SoftPayStatus = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayStatus);
                    if (SoftPayStatus != null && !string.IsNullOrEmpty(SoftPayStatus.Value))
                        objSoftPayViewModel.SoftPayStatus = SoftPayStatus.Value.Equals("1") ? true : false;

                    var SoftPayId = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayId);
                    if (SoftPayId != null && !string.IsNullOrEmpty(SoftPayId.Value))
                        objSoftPayViewModel.SoftPayId = SoftPayId.Value;

                    var SoftPayMerchant = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayMerchant);
                    if (SoftPayMerchant != null && !string.IsNullOrEmpty(SoftPayMerchant.Value))
                    {
                        objSoftPayViewModel.SoftPayMerchant = SoftPayMerchant.Value;
                    }
                    else
                    {
                        var objTerminal = db.Terminal.FirstOrDefault(a => a.Id == TerminalId);
                        var objOutlet = db.Outlet.FirstOrDefault(a => a.Id == objTerminal.OutletId);
                        if (objOutlet != null && !string.IsNullOrEmpty(objOutlet.Name))
                            objSoftPayViewModel.SoftPayMerchant = objOutlet.Name;
                    }

                    var SoftIntegratorCredentials = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftIntegratorCredentials);
                    if (SoftIntegratorCredentials != null && !string.IsNullOrEmpty(SoftIntegratorCredentials.Value))
                    {
                        try
                        {
                            byte[] objDecryptedBytes = StringCipher.decodeBytes(SoftIntegratorCredentials.Value);
                            StringBuilder objStringBuilder = new StringBuilder(objDecryptedBytes.Length * 2);
                            foreach (byte objByte in objDecryptedBytes)
                            {
                                objStringBuilder.AppendFormat("{0:x2}", objByte);
                            }
                            objSoftPayViewModel.IntegratorCredentials = objStringBuilder.ToString();
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    return Json(new { SoftPay = objSoftPayViewModel, Status = 1, Message = msg }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { SoftPay = objSoftPayViewModel, Status = 0, Message = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update20230504(AllSettingsViewModel model)
        {
            var msg = "";
            try
            {
                using (var db = GetConnection)
                {
                    var accountNumber = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.AccountNumber);
                    if (accountNumber != null)
                    {
                        accountNumber.Value = model.AccountNumber;
                        db.SaveChanges();
                    }

                    var invoiceReference = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.FakturaReference);
                    if (invoiceReference != null)
                    {
                        invoiceReference.Value = model.InvoiceReference;
                        db.SaveChanges();
                    }

                    var paymentReceiver = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PaymentReceiver);
                    if (paymentReceiver != null)
                    {
                        paymentReceiver.Value = model.PaymentReceiverName;
                        db.SaveChanges();
                    }

                    CurrentUserEmail = model.Email;

                    msg = "Success" + ":" + Resource.Settings + " " + Resource.saved + " " + Resource.successfully;
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(HttpPostedFileBase fileBase, AllSettingsViewModel model)
        {
            var msg = "";
            var mobileLogoUrl = "";

            try
            {
                var filename = "";
                if (model.File != null)
                {
                    var directoryPath = Server.MapPath("~/storage/PrinterLogo");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileGuid = Guid.NewGuid();
                    filename = string.Concat(fileGuid, Path.GetExtension(model.File.FileName));
                    var savePath = Path.Combine(directoryPath, filename);
                    model.File.SaveAs(savePath);
                }

                using (var db = GetConnection)
                {
                    var accountNumber = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.AccountNumber);
                    if (accountNumber != null)
                    {
                        accountNumber.Value = model.AccountNumber;
                        db.SaveChanges();
                    }

                    var invoiceReference = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.FakturaReference);
                    if (invoiceReference != null)
                    {
                        invoiceReference.Value = model.InvoiceReference;
                        db.SaveChanges();
                    }

                    var paymentReceiver = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PaymentReceiver);
                    if (paymentReceiver != null)
                    {
                        paymentReceiver.Value = model.PaymentReceiverName;
                        db.SaveChanges();
                    }

                    CurrentUserEmail = model.Email;

                    var mobileLogo = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PrinterLogo);
                    if (mobileLogo != null)
                    {
                        if (!string.IsNullOrEmpty(filename))
                            mobileLogo.Value = filename;
                        else
                            mobileLogo.Value = mobileLogo.Value;

                        mobileLogo.Updated = DateTime.Now;

                        db.SaveChanges();
                    }
                    else
                    {
                        int maxSettingId = 0;
                        if (db.Setting.Count() > 0)
                        {
                            maxSettingId = db.Setting.Max(obj => obj.Id);
                            maxSettingId++;
                        }

                        mobileLogo = new Setting();
                        mobileLogo.Id = maxSettingId;
                        mobileLogo.Description = "PrinterLogo";
                        mobileLogo.Code = SettingCode.PrinterLogo;
                        mobileLogo.Type = SettingType.MiscSettings;
                        mobileLogo.TerminalId = Guid.Empty;
                        mobileLogo.OutletId = Guid.Empty;
                        mobileLogo.Sort = 0;
                        mobileLogo.Created = DateTime.Now;
                        mobileLogo.Updated = DateTime.Now;

                        if (!string.IsNullOrEmpty(filename))
                            mobileLogo.Value = filename;
                        else
                            mobileLogo.Value = "";

                        db.Setting.Add(mobileLogo);
                        db.SaveChanges();
                    }

                    mobileLogoUrl = ConfigurationManager.AppSettings["BaseWebSitePath"] + "storage/PrinterLogo/" + mobileLogo.Value;
                    msg = "Success" + ":" + Resource.Settings + " " + Resource.saved + " " + Resource.successfully;
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { MobileLogoUrl = mobileLogoUrl, Message = msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteImage()
        {
            string msg = "";

            try
            {
                using (var db = GetConnection)
                {
                    var mobileLogo = db.Setting.FirstOrDefault(a => a.Code == Model.SettingCode.PrinterLogo);
                    if (mobileLogo != null)
                    {
                        mobileLogo.Value = "";
                        mobileLogo.Updated = DateTime.Now;

                        db.Entry(mobileLogo).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    msg = "Success:Deleted successfully";
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateSoftPayConfiguration(SoftPayViewModel SoftPay, Guid TerminalId)
        {
            var msg = "";

            try
            {
                using (var db = GetConnection)
                {
                    var objTerminal = db.Terminal.FirstOrDefault(a => a.Id == TerminalId);

                    int maxSettingId = 0;
                    if (db.Setting.Count() > 0)
                        maxSettingId = db.Setting.Max(obj => obj.Id);

                    var dbSoftPayStatus = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayStatus);
                    if (dbSoftPayStatus != null)
                    {
                        dbSoftPayStatus.Value = SoftPay.SoftPayStatus ? "1" : "0";
                        dbSoftPayStatus.Updated = DateTime.Now;

                        db.Entry(dbSoftPayStatus).State = EntityState.Modified;
                    }
                    else
                    {
                        maxSettingId++;

                        dbSoftPayStatus = new Setting();
                        dbSoftPayStatus.Id = maxSettingId;
                        dbSoftPayStatus.Description = "SoftPayStatus";
                        dbSoftPayStatus.Value = SoftPay.SoftPayStatus ? "1" : "0";
                        dbSoftPayStatus.Code = SettingCode.SoftPayStatus;
                        dbSoftPayStatus.Type = SettingType.TerminalSettings;
                        dbSoftPayStatus.TerminalId = objTerminal.Id;
                        dbSoftPayStatus.OutletId = objTerminal.OutletId;
                        dbSoftPayStatus.Sort = 0;
                        dbSoftPayStatus.Created = DateTime.Now;
                        dbSoftPayStatus.Updated = DateTime.Now;

                        db.Setting.Add(dbSoftPayStatus);
                    }

                    var dbSoftPayId = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayId);
                    if (dbSoftPayId != null)
                    {
                        dbSoftPayId.Value = !string.IsNullOrEmpty(SoftPay.SoftPayId) ? SoftPay.SoftPayId : "";
                        dbSoftPayId.Updated = DateTime.Now;

                        db.Entry(dbSoftPayId).State = EntityState.Modified;
                    }
                    else
                    {
                        maxSettingId++;

                        dbSoftPayId = new Setting();
                        dbSoftPayId.Id = maxSettingId;
                        dbSoftPayId.Description = "SoftPayId";
                        dbSoftPayId.Value = !string.IsNullOrEmpty(SoftPay.SoftPayId) ? SoftPay.SoftPayId : "";
                        dbSoftPayId.Code = SettingCode.SoftPayId;
                        dbSoftPayId.Type = SettingType.TerminalSettings;
                        dbSoftPayId.TerminalId = objTerminal.Id;
                        dbSoftPayId.OutletId = objTerminal.OutletId;
                        dbSoftPayId.Sort = 0;
                        dbSoftPayId.Created = DateTime.Now;
                        dbSoftPayId.Updated = DateTime.Now;

                        db.Setting.Add(dbSoftPayId);
                    }

                    var dbSoftPayMerchant = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftPayMerchant);
                    if (dbSoftPayMerchant != null)
                    {
                        dbSoftPayMerchant.Value = !string.IsNullOrEmpty(SoftPay.SoftPayMerchant) ? SoftPay.SoftPayMerchant : "";
                        dbSoftPayMerchant.Updated = DateTime.Now;

                        db.Entry(dbSoftPayMerchant).State = EntityState.Modified;
                    }
                    else
                    {
                        maxSettingId++;

                        dbSoftPayMerchant = new Setting();
                        dbSoftPayMerchant.Id = maxSettingId;
                        dbSoftPayMerchant.Description = "SoftPayMerchant";
                        dbSoftPayMerchant.Value = !string.IsNullOrEmpty(SoftPay.SoftPayMerchant) ? SoftPay.SoftPayMerchant : "";
                        dbSoftPayMerchant.Code = SettingCode.SoftPayMerchant;
                        dbSoftPayMerchant.Type = SettingType.TerminalSettings;
                        dbSoftPayMerchant.TerminalId = objTerminal.Id;
                        dbSoftPayMerchant.OutletId = objTerminal.OutletId;
                        dbSoftPayMerchant.Sort = 0;
                        dbSoftPayMerchant.Created = DateTime.Now;
                        dbSoftPayMerchant.Updated = DateTime.Now;

                        db.Setting.Add(dbSoftPayMerchant);
                    }

                    var dbSoftIntegratorCredentials = db.Setting.FirstOrDefault(a => a.TerminalId == TerminalId && a.Code == Model.SettingCode.SoftIntegratorCredentials);
                    if (dbSoftIntegratorCredentials != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(SoftPay.IntegratorCredentials))
                            {
                                byte[] IntegratorCredentialsByte = new byte[SoftPay.IntegratorCredentials.Length / 2];

                                for (int i = 0; i < IntegratorCredentialsByte.Length; i++)
                                {
                                    int index = i * 2;
                                    var objSubString = SoftPay.IntegratorCredentials.Substring(index, 2);
                                    int objParsedValue = Convert.ToInt32(objSubString, 16);

                                    IntegratorCredentialsByte[i] = (byte)objParsedValue;
                                }

                                dbSoftIntegratorCredentials.Value = StringCipher.encodeBytes(IntegratorCredentialsByte);
                            }
                            else
                            {
                                dbSoftIntegratorCredentials.Value = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            dbSoftIntegratorCredentials.Value = "";
                        }

                        dbSoftIntegratorCredentials.Updated = DateTime.Now;

                        db.Entry(dbSoftIntegratorCredentials).State = EntityState.Modified;
                    }
                    else
                    {
                        maxSettingId++;

                        dbSoftIntegratorCredentials = new Setting();
                        dbSoftIntegratorCredentials.Id = maxSettingId;
                        dbSoftIntegratorCredentials.Description = "SoftIntegratorCredentials";
                        dbSoftIntegratorCredentials.Code = SettingCode.SoftIntegratorCredentials;
                        dbSoftIntegratorCredentials.Type = SettingType.TerminalSettings;
                        dbSoftIntegratorCredentials.TerminalId = objTerminal.Id;
                        dbSoftIntegratorCredentials.OutletId = objTerminal.OutletId;
                        dbSoftIntegratorCredentials.Sort = 0;
                        dbSoftIntegratorCredentials.Created = DateTime.Now;
                        dbSoftIntegratorCredentials.Updated = DateTime.Now;

                        try
                        {
                            if (!string.IsNullOrEmpty(SoftPay.IntegratorCredentials))
                            {
                                byte[] IntegratorCredentialsByte = new byte[SoftPay.IntegratorCredentials.Length / 2];

                                for (int i = 0; i < IntegratorCredentialsByte.Length; i++)
                                {
                                    int index = i * 2;
                                    var objSubString = SoftPay.IntegratorCredentials.Substring(index, 2);
                                    int objParsedValue = Convert.ToInt32(objSubString, 16);

                                    IntegratorCredentialsByte[i] = (byte)objParsedValue;
                                }

                                dbSoftIntegratorCredentials.Value = StringCipher.encodeBytes(IntegratorCredentialsByte);
                            }
                            else
                            {
                                dbSoftIntegratorCredentials.Value = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            dbSoftIntegratorCredentials.Value = "";
                        }

                        db.Setting.Add(dbSoftIntegratorCredentials);
                    }

                    db.SaveChanges();

                    msg = "Success" + ":" + Resource.Settings + " " + Resource.saved + " " + Resource.successfully;
                }
            }
            catch (Exception ex)
            {
                msg = "Error:" + ex.Message;
            }

            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}