using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Web.Pdf
{
    public class Ocr
    {
        public static MemoryStream Create(long intInvoiceID, string strCompanyName, DateTime dtInvoiceTimeStamp, DateTime dtExpireDateTime, string strCustomerReference, string strOcr, long intOrderID, string strPartnerReference, string strOrderText, List<OrderLineViewModel> orderLines, string strInvoiceTitle, string strInvoiceText, decimal decTotal, decimal decVATAmount, decimal netTotal, string strReminderText, string strFullName, string strAdressLine1, string strAdressLine2, DateTime dtExpireDate, decimal decReminderFee, string FakturaReference, string filePath, Outlet oultet, string BankAccountNo, string PaymentReceiver)
        {
            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\Names.txt");
            //string source = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ocr.pdf");

            string source = string.Empty;
            try
            {
                source = filePath + "\\invoice.pdf";// System.IO.Path.Combine(Environment.CurrentDirectory, @"\ocr.pdf");
                //source = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\Pdf\invoice.pdf");
            }
            catch { }




            HiQPdf.PdfDocument pdf = HiQPdf.PdfDocument.FromFile(source);
            pdf.SerialNumber = "7aWEvL2J-i6GEj5+M-n5TVw93N-3M3czdre-1M3e3MPc-38PU1NTU";
            HiQPdf.PdfPage page = pdf.Pages[0];

            //// create the true type fonts that can be used in document
            //System.Drawing.Font sysFont = new System.Drawing.Font("Times New Roman", 10, System.Drawing.GraphicsUnit.Point);
            //HiQPdf.PdfFont pdfFont = pdf.CreateFont(sysFont);
            //HiQPdf.PdfFont pdfFontEmbed = pdf.CreateFont(sysFont, true);
            //Monospace821 BT
            // create a standard Helvetica Type 1 font that can be used in document text
            HiQPdf.PdfFont fontLarge = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontLarge.Size = 25;

            HiQPdf.PdfFont fontMedium = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontMedium.Size = 15;

            HiQPdf.PdfFont fontStandard = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontStandard.Size = 11;
            HiQPdf.PdfFont fontStandard2 = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontStandard2.Size = 7;
            HiQPdf.PdfFont fontheading = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.HelveticaBold);
            fontheading.Size = 9;
            HiQPdf.PdfFont fontSystem = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontSystem.Size = 7;
            // System.Drawing.FontFamily fm = new System.Drawing.FontFamily("Monospace821 BT");

            System.Drawing.Font numericfont = new System.Drawing.Font("Monospace821 BT", 11);


            // Company Name
            //HiQPdf.PdfText companyName = new HiQPdf.PdfText(50, 30, strCompanyName, fontLarge);
            //titleTextAtLocation.ForeColor = System.Drawing.Color.Black;
            //HiQPdf.PdfLayoutInfo textLayoutInfo = page.Layout(companyName);
            if (File.Exists(filePath + "\\" + strCompanyName + ".png"))
            {
                // var uriSource = new Uri(Defaults.CompanyInfo.Logo);

                //logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));
                page.Layout(new HiQPdf.PdfImage(50, 25, 100, filePath + "\\" + strCompanyName + ".png"));

            }
            else
            {
                page.Layout(new HiQPdf.PdfText(50, 25, strCompanyName, fontLarge));
            }

            // Invoice label
            // page.Layout(new HiQPdf.PdfText(317, 25, "Fakturanummer", fontMedium));

            // Invoice date label
            page.Layout(new HiQPdf.PdfText(317, 45, strInvoiceTitle, fontSystem));

            // Invoice date
            //   page.Layout(new HiQPdf.PdfText(317, 49, dtInvoiceTimeStamp.ToShortDateString(), fontStandard));

            // Invoice number label
            // page.Layout(new HiQPdf.PdfText(474, 40, "Fakturanummer/OCR", fontSystem));

            // Invoice Ocr
            page.Layout(new HiQPdf.PdfText(478, 48, strOcr, fontStandard));

            string month = dtExpireDateTime.Month < 10 ? "0" + dtExpireDateTime.Month : dtExpireDateTime.Month.ToString();
            string day = dtExpireDateTime.Day < 10 ? "0" + dtExpireDateTime.Day : dtExpireDateTime.Day.ToString();
            // Invoice Expire
            page.Layout(new HiQPdf.PdfText(317, 75, dtExpireDateTime.Year + "-" + month + "-" + day, fontStandard));

            page.Layout(new HiQPdf.PdfText(350, 120, @"Mottagare", fontSystem));

            // Customer Reference  (Er referens))
            if (string.IsNullOrEmpty(strCustomerReference))
                strCustomerReference = " ";
            page.Layout(new HiQPdf.PdfText(350, 131, strCustomerReference, fontSystem));

            // Invoice Id (Projektnr)
            //Refrence code
            page.Layout(new HiQPdf.PdfText(53, 120, @"Avsändare", fontSystem));

            page.Layout(new HiQPdf.PdfText(53, 131, oultet.Name, fontStandard2));
            page.Layout(new HiQPdf.PdfText(53, 142, oultet.Address1 + ",", fontStandard2));
            page.Layout(new HiQPdf.PdfText(53, 153, oultet.PostalCode + ", " + oultet.City, fontStandard2));
            // Order id (Beställningsnummer))
            //page.Layout(new HiQPdf.PdfText(53, 179, intOrderID.ToString(), fontStandard));

            page.Layout(new HiQPdf.PdfText(53, 175, @"Referensnummer", fontSystem));

            // Partner Reference (Vår referens)
            if (string.IsNullOrEmpty(strPartnerReference))
                strPartnerReference = " ";
            page.Layout(new HiQPdf.PdfText(53, 185, strPartnerReference, fontStandard2));

            // Order text (Specifikation)
            // page.Layout(new HiQPdf.PdfText(60, 180, intInvoiceID.ToString(), fontStandard));

            // Orderitems
            //page.Layout(new HiQPdf.PdfText(160, 228, UI.Report_Quantity, fontheading));
            //page.Layout(new HiQPdf.PdfText(230, 228, UI.PlaceOrder_UnitPrice+"/"+UI.PlaceOrder_Unit, fontheading));
            //page.Layout(new HiQPdf.PdfText(360, 228, UI.Global_VAT , fontheading));
            int yPos = 0;
            int i = 0;
            var lines = orderLines.Take(15);
            //foreach (var orderLine in lines)
            //{

            //}
            int index = 0;
            for (index = 0; index < lines.Count(); index++)
            {
                var orderLine = lines.ElementAt(index);

                if (i > 15)
                {
                    break;
                }

                page.Layout(new HiQPdf.PdfText(60, 245 + yPos, orderLine.ItemName, fontStandard));
                page.Layout(new HiQPdf.PdfText(275, 245 + yPos, Math.Round(orderLine.Quantity, 2).ToString(), numericfont));
                string unitPrice = Text.AlignNumber(orderLine.UnitPrice, decTotal);
                page.Layout(new HiQPdf.PdfText(340, 245 + yPos, unitPrice, numericfont));
                string vat = Text.AlignNumber(orderLine.VatAmount(), decTotal);
                vat = vat + " (" + Math.Round(orderLine.TaxPercent, 0) + "%)";
                page.Layout(new HiQPdf.PdfText(400, 245 + yPos, vat, numericfont));
                //string gamount = Text.AlignNumber(orderLine.GrossTotal, decTotal);
                string gamount = Text.AlignNumber(orderLine.Quantity * orderLine.UnitPrice, decTotal);
                page.Layout(new HiQPdf.PdfText(520, 245 + yPos, gamount, numericfont));
                i++;
                yPos += 15;

                if (orderLine.DiscountedUnitPrice != orderLine.UnitPrice)
                {
                    page.Layout(new HiQPdf.PdfText(70, 245 + yPos, "Discount", fontStandard));
                    page.Layout(new HiQPdf.PdfText(275, 245 + yPos, "", numericfont));
                    unitPrice = "";// Text.AlignNumber(orderLine.DiscountedUnitPrice, decTotal);
                    page.Layout(new HiQPdf.PdfText(340, 245 + yPos, unitPrice, numericfont));
                    vat = "";
                    //vat = vat + " (" + Math.Round(orderLine.TaxPercent, 0) + "%)";
                    page.Layout(new HiQPdf.PdfText(400, 245 + yPos, vat, numericfont));
                    gamount = Text.AlignNumber(orderLine.GrossTotal - (orderLine.Quantity * orderLine.UnitPrice), decTotal);
                    page.Layout(new HiQPdf.PdfText(520, 245 + yPos, gamount, numericfont));
                    i++;
                    yPos += 15;
                }
            }

            decimal linetotal = lines.Sum(s => s.GrossTotal);
            decimal lineVatAmount = lines.Sum(s => s.VatAmount());
            decimal lineNetTotal = linetotal - lineVatAmount;
            //decimal decVATPercent = lineVatAmount / linetotal * 100;

            // Amount including VAT
            //if (decVATPercent == 0)
            //{
            var totalofVATless = lines.Where(v => v.TaxPercent == 0).Sum(s => s.GrossAmountDiscounted());
            page.Layout(new HiQPdf.PdfText(90, 478, Math.Round(totalofVATless, 2).ToString(), numericfont));
            // page.Layout(new HiQPdf.PdfText(200, 478, Text.FormatNumber(0, 2), fontStandard));
            //}
            //else
            //{
            // page.Layout(new HiQPdf.PdfText(90, 478, Text.FormatNumber(0, 2), fontStandard));
            var totalofVATSale = lines.Where(v => v.TaxPercent > 0).Sum(s => s.GrossAmountDiscounted());
            page.Layout(new HiQPdf.PdfText(200, 478, Math.Round(totalofVATSale - lineVatAmount, 2).ToString(), numericfont));
            // }

            // VAT Amount
            //   decimal decVATAmount = decTotal * decVATPercent / 100;
            page.Layout(new HiQPdf.PdfText(290, 478, Math.Round(lineVatAmount, 2).ToString(), numericfont));

            // VAT %
            //   page.Layout(new HiQPdf.PdfText(350, 468, " ", fontStandard));
            //  page.Layout(new HiQPdf.PdfText(355, 478, Text.FormatNumber(decVATPercent, 2), fontStandard));

            // Totals
            page.Layout(new HiQPdf.PdfText(520, 478, Math.Round(linetotal, 2).ToString(), numericfont));


            // Invoice
            //  page.Layout(new HiQPdf.PdfText(60, 400, strInvoiceTitle, fontStandard));

            //  page.Layout(new HiQPdf.PdfText(60, 415, strInvoiceText, fontStandard));

            // Reminder
            if (decReminderFee == 0)
            {
                if (string.IsNullOrEmpty(strReminderText))
                    strReminderText = " ";
                page.Layout(new HiQPdf.PdfText(60, 440, strReminderText, fontStandard));
            }
            else if (decReminderFee > 0)
            {
                decTotal += decReminderFee;
                page.Layout(new HiQPdf.PdfText(60, 440, "Påminnelseavgift", fontStandard));
                page.Layout(new HiQPdf.PdfText(480, 440, decReminderFee.ToString(), fontStandard));
            }

            page.Layout(new HiQPdf.PdfText(60, 500, oultet.Name, fontSystem));
            page.Layout(new HiQPdf.PdfText(60, 510, oultet.Address1, fontSystem));
            page.Layout(new HiQPdf.PdfText(60, 520, oultet.PostalCode + " " + oultet.City, fontSystem));

            page.Layout(new HiQPdf.PdfText(60 + 100, 500, "Telefon:", fontSystem));
            page.Layout(new HiQPdf.PdfText(60 + 135, 500, oultet.Phone, fontSystem));

            page.Layout(new HiQPdf.PdfText(60 + 350, 500, "Godkänd för F-Skatt", fontSystem));
            page.Layout(new HiQPdf.PdfText(60 + 350, 510, "Organisationsnummer", fontSystem));
            page.Layout(new HiQPdf.PdfText(60 + 350 + 75, 510, oultet.OrgNo, fontSystem));

            page.Layout(new HiQPdf.PdfText(60 + 350, 520, "Vat-Nummer", fontSystem));
            page.Layout(new HiQPdf.PdfText(60 + 350 + 75, 520, oultet.TaxDescription, fontSystem));

            /*
            // Legals
            page.Layout(new HiQPdf.PdfText(55, 500, "Vi innehar F-skattsedel. Dröjsmålsränta debiteras efter förfallodagen.", fontSystem));

            page.Layout(new HiQPdf.PdfText(55, 520, "Postadress", fontSystem));
            page.Layout(new HiQPdf.PdfText(55, 530, Defaults.CompanyInfo.Address1, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 75, 520, "Telefon", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 75, 530, Defaults.CompanyInfo.Phone, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 150, 520, "E-mail", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 150, 530, Defaults.CompanyInfo.Email, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 225, 520, "Internet", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 225, 530, Defaults.CompanyInfo.URL, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 300, 520, "Organisationsnr", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 300, 530, Defaults.CompanyInfo.OrgNo, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 375, 520, "Momsreg.nr", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 375, 530, Defaults.CompanyInfo.TaxDescription, fontSystem));

            page.Layout(new HiQPdf.PdfText(55 + 450, 520, "Bankgiro", fontSystem));
            page.Layout(new HiQPdf.PdfText(55 + 450, 530, Defaults.CompanyInfo.BankAccountNo, fontSystem));
            */
            // Sender (Consumer)
            var start = 660;
            page.Layout(new HiQPdf.PdfText(350, start, strFullName, fontStandard2));
            if (!string.IsNullOrEmpty(strAdressLine1))
            {
                start = start + 12;
                page.Layout(new HiQPdf.PdfText(350, start, strAdressLine1, fontStandard2));
            }
            if (!string.IsNullOrEmpty(strAdressLine2))
            {
                start = start + 12;
                page.Layout(new HiQPdf.PdfText(350, start, strAdressLine2, fontStandard2));
            }

            // OCR Info
            //   page.Layout(new HiQPdf.PdfText(380, 620, string.Concat("OCR-/referensnummer: ", strOcr), fontStandard));

            // Expiredate
            //  page.Layout(new HiQPdf.PdfText(380, 640, string.Concat("Förfallodag: ", dtExpireDateTime.Year + "-" + dtExpireDateTime.Month + "-" + dtExpireDateTime.Day), fontStandard));


            // Bankgiro
            page.Layout(new HiQPdf.PdfText(60, 693, BankAccountNo, fontStandard2));

            // Betalningsmottagare
            page.Layout(new HiQPdf.PdfText(60, 671, PaymentReceiver, fontStandard2));


            // Left "#"
            page.Layout(new HiQPdf.PdfText(40, 770, "#", fontStandard));

            // OCR
            page.Layout(new HiQPdf.PdfText(150, 770, strOcr, fontStandard));

            // Middle "#"
            page.Layout(new HiQPdf.PdfText(210, 770, "#", fontStandard));

            // Kronor
            page.Layout(new HiQPdf.PdfText(250, 770, Text.FormatNumber(Math.Floor(decTotal)), numericfont));

            // Öre
            page.Layout(new HiQPdf.PdfText(302, 770, Text.FormatNumber((decTotal - Math.Floor(decTotal)) * 100).PadLeft(2, '0'), numericfont));

            // Amount checkdigit  CalculateMod10CheckDigit(decTotal)
            page.Layout(new HiQPdf.PdfText(337, 770, "0", fontStandard));

            //  ">"
            page.Layout(new HiQPdf.PdfText(350, 770, ">", fontStandard));

            //  BG # 41 # // 41 = BG statiskt
            //  page.Layout(new HiQPdf.PdfText(500, 775, "4337937#41#", fontStandard));

            if (orderLines.Count > index)
            {
                // int pageStart = 15;
                int pagesize = 50;
                int TotalRecord = orderLines.Count;
                int pageNo = 2;
                for (int pageStart = index; pageStart < TotalRecord;)
                {
                    var oline = orderLines.Skip(pageStart).Take(pagesize).ToList();

                    pdf.AddDocument(WritePage2(oline, pageNo, filePath));
                    pageNo++;
                    pageStart = pageStart + pagesize;
                }
            }

            MemoryStream ms = new MemoryStream();
            pdf.WriteToStream(ms);

            // Cleanup
            pdf.Close();
            return ms;
        }

        public static HiQPdf.PdfDocument WritePage2(List<OrderLineViewModel> orderLines, int pageNo, string filePath)
        {

            string source = string.Empty;
            try
            {
                source = filePath + "\\invoicepage2.pdf";// System.IO.Path.Combine(Environment.CurrentDirectory, @"\ocr.pdf");
                //source = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\Pdf\invoice.pdf");
            }
            catch { }

            if (string.IsNullOrEmpty(source))
            {
                source = System.IO.Path.Combine(Environment.CurrentDirectory, @"\invoicepage2.pdf");
            }

            HiQPdf.PdfDocument pdf = HiQPdf.PdfDocument.FromFile(source);
            pdf.SerialNumber = "7aWEvL2J-i6GEj5+M-n5TVw93N-3M3czdre-1M3e3MPc-38PU1NTU";
            HiQPdf.PdfPage page = pdf.Pages[0];

            HiQPdf.PdfFont fontStandard = pdf.CreateStandardFont(HiQPdf.PdfStandardFont.Helvetica);
            fontStandard.Size = 11;
            System.Drawing.Font numericfont = new System.Drawing.Font("Monospace821 BT", 11);

            page.Layout(new HiQPdf.PdfText(45, 8, pageNo.ToString(), fontStandard));
            int yPos = 0;
            decimal grossTotal = orderLines.Sum(g => g.GrossTotal);
            var totalVat = orderLines.Sum(ol => ol.VatAmount());

            decimal netTotal = grossTotal - totalVat;

            decimal decTotal = grossTotal;
            decimal decVATPercent = totalVat / decTotal * 100;//

            foreach (var orderLine in orderLines)
            {
                page.Layout(new HiQPdf.PdfText(50, 60 + yPos, orderLine.ItemName, fontStandard));
                page.Layout(new HiQPdf.PdfText(265, 60 + yPos, Math.Round(orderLine.Quantity, 2).ToString(), numericfont));
                string unitPrice = Text.AlignNumber(orderLine.UnitPrice, decTotal);
                page.Layout(new HiQPdf.PdfText(330, 60 + yPos, unitPrice, numericfont));
                string vat = Text.AlignNumber(orderLine.VatAmount(), decTotal);
                vat = vat + " (" + Math.Round(orderLine.TaxPercent, 0) + "%)";
                page.Layout(new HiQPdf.PdfText(390, 60 + yPos, vat, numericfont));
                string gamount = Text.AlignNumber(orderLine.GrossTotal, decTotal);
                page.Layout(new HiQPdf.PdfText(510, 60 + yPos, gamount, numericfont));

                yPos += 15;
            }

            // Invoice
            //  page.Layout(new HiQPdf.PdfText(60, 400, strInvoiceTitle, fontStandard));

            //  page.Layout(new HiQPdf.PdfText(60, 415, strInvoiceText, fontStandard));

            // Reminder
            //if (decReminderFee == 0)
            //{
            //    if (string.IsNullOrEmpty(strReminderText))
            //        strReminderText = " ";
            //    page.Layout(new HiQPdf.PdfText(60, 440, strReminderText, fontStandard));
            //}
            //else if (decReminderFee > 0)
            //{
            //    decTotal += decReminderFee;
            //    page.Layout(new HiQPdf.PdfText(60, 440, "Påminnelseavgift", fontStandard));
            //    page.Layout(new HiQPdf.PdfText(480, 440, decReminderFee.ToString(), fontStandard));
            //}
            // decimal decVATPercent = decVATAmount / decTotal * 100;

            // Amount including VAT

            page.Layout(new HiQPdf.PdfText(70, 818, Math.Round(decTotal, 2).ToString(), numericfont));
            page.Layout(new HiQPdf.PdfText(190, 818, Math.Round(netTotal, 2).ToString(), numericfont));
            // VAT Amount
            decimal decVATAmount = decTotal * decVATPercent / 100;
            page.Layout(new HiQPdf.PdfText(280, 818, Math.Round(decVATAmount, 2).ToString(), numericfont));

            // VAT %
            //  page.Layout(new HiQPdf.PdfText(340, 810, " ", fontStandard));
            // page.Layout(new HiQPdf.PdfText(345, 818, Text.FormatNumber(decVATPercent, 2), fontStandard));

            // Totals
            page.Layout(new HiQPdf.PdfText(510, 818, Math.Round(decTotal, 2).ToString(), numericfont));

            //MemoryStream ms = new MemoryStream();
            //pdf.WriteToStream(ms);

            //// Cleanup
            //pdf.Close();

            return pdf;
        }
        private static string CalculateMod10CheckDigit(decimal decAmount)
        {
            string number = decAmount.ToString().Replace(",", string.Empty);

            var sum = 0;
            var alt = true;
            var digits = number.ToCharArray();
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                var curDigit = (digits[i] - 48);
                if (alt)
                {
                    curDigit *= 2;
                    if (curDigit > 9)
                        curDigit -= 9;
                }
                sum += curDigit;
                alt = !alt;
            }
            if ((sum % 10) == 0)
            {
                return "0";
            }
            return (10 - (sum % 10)).ToString();
        }




    }
    public class Text
    {
        public static bool IsNumeric(object Expression)
        {
            bool bNum;
            double retNum;
            bNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return bNum;
        }

        public static bool IsDateTime(object Expression)
        {
            try
            {
                if (Expression == null)
                {
                    return false;
                }
                DateTime dt = Convert.ToDateTime(Expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsBoolean(object Expression)
        {
            try
            {
                int intBool = Convert.ToInt32(Expression);
                bool b = Convert.ToBoolean(intBool);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuid(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuidNotEmpty(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                if (guid != Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool StartsWithGuidNotEmpty(object Expression, string strDelimter)
        {
            try
            {
                string[] str = Expression.ToString().Split(Convert.ToChar(strDelimter));

                Guid guid = new Guid(str[0]);
                if (guid != Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsGuidEmpty(object Expression)
        {
            try
            {
                Guid guid = new Guid(Expression.ToString());
                if (guid == Guid.Empty)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static string ParseText(object Expression)
        {
            return Expression == null ? string.Empty : Expression.ToString();
        }

        public static bool AlgorithmMatch(string strAlgorithm, string strText)
        {
            try
            {
                string[] strSplitAlgorithm = strAlgorithm.Split(Convert.ToChar(" "));
                string[] strSplitText = strText.Split(Convert.ToChar(" "));

                if (strSplitAlgorithm.Length != strSplitText.Length)
                {
                    return false;
                }

                for (int i = 0; i < strSplitAlgorithm.Length; i++)
                {
                    if (strSplitAlgorithm[i] == "#" && !Text.IsNumeric(strSplitText[i]))
                    {
                        return false;
                    }
                    else if (strSplitAlgorithm[i] == "$" && Text.IsNumeric(strSplitText[i]))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static string AlgorithmRevice(string strText)
        {
            if (string.IsNullOrEmpty(strText))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            string[] strSplitText = strText.Split(Convert.ToChar(" "));

            for (int i = 0; i < strSplitText.Length; i++)
            {
                if (Text.IsNumeric(strSplitText[i]))
                {
                    sb.Append("#");
                    if (i < strSplitText.Length - 1)
                    {
                        sb.Append(" ");
                    }
                }
                else if (!Text.IsNumeric(strSplitText[i]))
                {
                    sb.Append("$");
                    if (i < strSplitText.Length - 1)
                    {
                        sb.Append(" ");
                    }
                }
            }

            return sb.ToString();
        }

        //public static bool IsEmptyOrGuidEmpty(object Expression)
        //{
        //    try
        //    {
        //        Guid guid = new Guid(Expression.ToString());
        //        if (guid != Guid.Empty)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static string ReplaceEx(string strOriginal, string strPattern, string strReplacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = strOriginal.ToUpper();
            string upperPattern = strPattern.ToUpper();
            int inc = (strOriginal.Length / strPattern.Length) *
                      (strReplacement.Length - strPattern.Length);
            char[] chars = new char[strOriginal.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = strOriginal[i];
                for (int i = 0; i < strReplacement.Length; ++i)
                    chars[count++] = strReplacement[i];
                position0 = position1 + strPattern.Length;
            }
            if (position0 == 0) return strOriginal;
            for (int i = position0; i < strOriginal.Length; ++i)
                chars[count++] = strOriginal[i];
            return new string(chars, 0, count);
        }

        public static string TruncateWords(string strText, int intLengthThreshold, int intNewWordLength, string strTruncatedNewWordSuffix)
        {
            if (strText.Length < intLengthThreshold)
            {
                return strText;
            }

            string[] words = strText.Split(' ');

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > intNewWordLength + 1)
                {
                    sb.Append(words[i].Substring(0, intNewWordLength));

                    if (!string.IsNullOrEmpty(strTruncatedNewWordSuffix))
                    {
                        sb.Append(strTruncatedNewWordSuffix);
                    }
                }
                else
                {
                    sb.Append(words[i]);
                }

                sb.Append(" ");
            }

            return sb.ToString();
        }

        public static string FormatNumber(decimal decNumber)
        {
            return FormatNumber(decNumber, 0);
        }
        public static string AlignNumber(decimal decNumber, decimal totalAmount)
        {
            int maxlength = Math.Round(totalAmount, 2).ToString().Length;
            decimal result = Math.Round(decNumber, 2);
            string returnvalue = result.ToString();
            var _lenth = result.ToString().Length;
            int lenDiff = maxlength - _lenth;
            if (lenDiff >= 1)
            {
                string space = " ";
                for (int i = 0; i < lenDiff; i++)
                    space = space + " ";
                returnvalue = space + result.ToString();
            }
            return returnvalue;
        }

        public static string FormatNumber(decimal decNumber, int intNoOfDecimals)
        {
            string strNumber = string.Empty;

            switch (intNoOfDecimals)
            {
                case 0:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0");
                    break;
                case 1:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.#");
                    break;
                case 2:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.##");
                    break;
                case 3:
                    strNumber = Math.Round(decNumber, intNoOfDecimals).ToString("# ### ##0.###");
                    break;
                default:
                    strNumber = decNumber.ToString();
                    break;
            }

            return strNumber.Trim();
        }

        public static string Truncate(string strText, string strStart, string strEnd)
        {
            int intStartPos = strText.IndexOf(strStart, 0);
            if (intStartPos == -1)
            {
                return strText;
            }
            int intEndPos = strText.IndexOf(strEnd, intStartPos);
            if (intEndPos == -1)
            {
                return strText;
            }
            return ReplaceEx(strText, strText.Substring(intStartPos, intEndPos - intStartPos + 1), string.Empty).Trim();
        }

        public static string DateTimeToFileFormat(DateTime dt)
        {
            string strDt = dt.ToString();
            strDt = ReplaceEx(strDt, ":", "_");

            return strDt;
        }

        public static string AnyDateTimeToYearAndMonth(object dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            return string.Concat(dt.Year.ToString(), "-", dt.Month.ToString().PadLeft(2, '0'));
        }
        public static string AnyDateTimeToYearAndMonthAndDay(object dateTime)
        {
            DateTime dt = Convert.ToDateTime(dateTime);
            return string.Concat(dt.Year.ToString(), "-", dt.Month.ToString().PadLeft(2, '0'), "-", dt.Day.ToString().PadLeft(2, '0'));
        }


        //public static string DateTimeTotFileFormat(DateTime dt)
        //{
        //    string strDt = dt.ToString();
        //    strDt = ReplaceEx(strDt, ":", "_");

        //    return strDt;
        //}

        public static string MakeUrlFriendly(string strText)
        {
            string strClean = strText;
            strClean = strClean.Replace(" ", string.Empty);
            strClean = strClean.Replace('å', 'a');
            strClean = strClean.Replace('ä', 'a');
            strClean = strClean.Replace('ö', 'o');
            strClean = strClean.Replace('Å', 'A');
            strClean = strClean.Replace('Ä', 'A');
            strClean = strClean.Replace('Ö', 'O');
            strClean = strClean.Replace('é', 'e');
            return strClean;
        }

        public static string FriendlyDateTime(DateTime dt)
        {
            return string.Concat(dt.ToShortDateString(), " ", dt.ToShortTimeString());
        }





    }
}
