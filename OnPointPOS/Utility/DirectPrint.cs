using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using POSSUM.Handlers;
using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Data;
using POSSUM.Views.PrintOrder;
using System.Configuration;
using Newtonsoft.Json;

namespace POSSUM
{
    public partial class DirectPrint
    {
        private string _printData = "This is the default\rText\r\r\r\n\n\n";
        private string _printerName = "SAM4S GIANT-100";
        private Font _paragraphFont = new Font("Anonymous Pro", 10, FontStyle.Bold);
        private Font _h1Font = new Font("Anonymous Pro", 14, FontStyle.Bold);
        // private Font h2Font = new Font("Anonymous Pro", 12, FontStyle.Bold);
        private PrintModel _printModel;
        private Order _orderMaster;
        private Customer _ordercustomer;
        private bool _isKitchen = true;
        List<Journal> _journalLogs;
        private bool _isCopy;

        ApplicationDbContext db;
        public DirectPrint()
        {
            db = PosState.GetInstance().Context;
        }

        public DirectPrint(string printData)
        {
            _printData = printData;
            db = PosState.GetInstance().Context;
        }

        public string PrinterName => _printerName;

        public void OpenCashDrawer()
        {

            //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

            _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(AsciiControlChars.Escape);
                bw.Write((byte)112);
                bw.Write((byte)0);
                bw.Write((byte)25);
                bw.Write((byte)250);

                Print(_printerName, ms.ToArray());
            }
        }


        public void PrintBill(Guid orderId)
        {
            try
            {

                //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

                _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));

                Console.WriteLine(@"Preparing data for print      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                GetReceiptData(orderId, false);
                VerifyPrinterName();
                Console.WriteLine(PrinterName);

                Console.WriteLine(@"sending for direct print       -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                Print(PrinterName, GetBillDocument());
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public void PrintPerfroma(Order order)
        {
            try
            {

                //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

                _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));

                //Set Print Model data
                _printModel = new PrintModel
                {
                    Footer = Defaults.Outlet.FooterText,
                    Header = string.IsNullOrEmpty(Defaults.Outlet.HeaderText) ? Defaults.Outlet.Name : Defaults.Outlet.HeaderText,
                    CompanyInfo = Defaults.CompanyInfo,
                    TaxDesc = Defaults.Outlet.TaxDescription
                };
                _printModel.ReceiptDate = order.CreationDate;
                _printModel.Cashier = Defaults.User.UserName;
                _printModel.GrandTotal = order.OrderTotal;
                _printModel.OrderMaster = order;
                _printModel.Items = order.OrderLines.ToList();
                //SetVatDetail(order.OrderLines.ToList());
                SetVatDetail(order);

                VerifyPrinterName();

                Console.WriteLine(PrinterName);

                Print(PrinterName, GetBillDocument());
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public void PrintJournal(List<Journal> logs)
        {
            _journalLogs = logs;
            try
            {
                _printModel = new PrintModel
                {
                    Footer = Defaults.Outlet.FooterText,
                    Header = string.IsNullOrEmpty(Defaults.Outlet.HeaderText) ? Defaults.Outlet.Name : Defaults.Outlet.HeaderText,
                    CompanyInfo = Defaults.CompanyInfo,
                    TaxDesc = Defaults.Outlet.TaxDescription
                };

                _printerName = Utilities.GetPrinterById(Defaults.Outlet.KitchenPrinterId);

                Print(PrinterName, GetJournalDocument());
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }

        public void SetPrintData(string printData)
        {
            _printData = printData;
        }

        public void SetPrinterName(string printerName)
        {
            _printerName = printerName;
        }

        public void SetParagraphFont(Font paragraphFont)
        {
            _paragraphFont = paragraphFont;
        }

        public void SetParagraphBoldFont(Font paragraphBoldFont)
        {
        }

        public void SetH1Font(Font h1Font)
        {
            _h1Font = h1Font;
        }

        public void Print(string printData)
        {
            _printData = printData;

            Print();
        }

        public void Print()
        {
            VerifyPrinterName();
            Console.WriteLine(@"BABABABABABABABABABA");
            Console.WriteLine(PrinterName);
            Print(PrinterName, GetReceiptDocument());
        }

        private static void Print(string printerName, byte[] document)
        {
            var documentInfo = new NativeMethods.DOC_INFO_1();
            documentInfo.pDataType = "RAW";
            documentInfo.pDocName = "Receipt";

            var printerHandle = new IntPtr(0);

            if (NativeMethods.OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
            {
                if (NativeMethods.StartDocPrinter(printerHandle, 1, documentInfo))
                {
                    int bytesWritten;
                    byte[] managedData;
                    IntPtr unmanagedData;

                    managedData = document;
                    unmanagedData = Marshal.AllocCoTaskMem(managedData.Length);
                    Marshal.Copy(managedData, 0, unmanagedData, managedData.Length);

                    if (NativeMethods.StartPagePrinter(printerHandle))
                    {
                        NativeMethods.WritePrinter(
                            printerHandle,
                            unmanagedData,
                            managedData.Length,
                            out bytesWritten);
                        NativeMethods.EndPagePrinter(printerHandle);
                    }
                    else
                    {
                        throw new Win32Exception();
                    }

                    Marshal.FreeCoTaskMem(unmanagedData);

                    NativeMethods.EndDocPrinter(printerHandle);
                }
                else
                {
                    throw new Win32Exception();
                }

                NativeMethods.ClosePrinter(printerHandle);
            }
            else
            {
                throw new Win32Exception();
            }
        }


        #region Print BONG

        public void PrintBong(Order order, bool isKitchen)
        {
            try
            {
                LogWriter.LogWrite(new Exception("printbong... calling" + isKitchen));

                if (order != null && order.OrderLines.Count(a => a.Product.Bong == true) == 0)
                {
                    return;
                }

                var ordRepo = new InvoiceHandler();
                _printerName = Utilities.GetPrinterById(Defaults.Outlet.KitchenPrinterId);
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptKitchen), order.Id);
                _isKitchen = isKitchen;
                _orderMaster = order;

                if (Defaults.BongByProduct)
                {
                    var printGroups = order.OrderLines.GroupBy(o => o.PrinterId);
                    foreach (var grp in printGroups)
                    {
                        var itemLines = new List<OrderLine>();
                        var printId = grp.First().PrinterId;
                        foreach (var itm in grp)
                        {
                            itemLines.Add(itm);
                        }
                        var productPrinter = Utilities.GetPrinterByProduct(printId);
                        if (!string.IsNullOrEmpty(productPrinter))
                            _printerName = productPrinter;
                        else
                            _printerName = Utilities.GetPrinterById(Defaults.Outlet.KitchenPrinterId);
                        Print(PrinterName, GetBongDocument(itemLines));
                    }
                }
                else
                {
                    Print(PrinterName, GetBongDocument(order.OrderLines.ToList()));

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        private byte[] GetBongDocument(List<OrderLine> orderDetails)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintBongDocumemt(bw, orderDetails);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }
        private void PrintBongDocumemt(BinaryWriter bw, List<OrderLine> itemLines)
        {
            InvoiceHandler ordRepo = new InvoiceHandler();
            int bongNo = 0;
            int.TryParse(_orderMaster.Bong, out bongNo);
            int dailyBongCounter = 0;
            int.TryParse(_orderMaster.DailyBong, out dailyBongCounter);
            if (bongNo == 0)
            {
                bongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                //if (_orderMaster.Id != default(Guid))
                //   ordRepo.UpdateBongNo(_orderMaster.Id, dailyBongCounter);
            }
            string orderDate = string.Format("{0,-21}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            bw.LargeText(orderDate);
            bw.FeedLines(1);
            bw.NormalFont(Defaults.User.UserCode);
            if (Defaults.BongCounter)
            {
                string BongNo = string.Format("{0,-25}", "BONG#" + bongNo);
                bw.LargeText(BongNo);
            }
            if (Defaults.OrderNoOnBong)
            {
                string orderNo = string.Format("{0,-30}", "ORDER#" + _orderMaster.OrderNoOfDay);
                bw.LargeText(orderNo);
            }
            if (Defaults.DailyBongCounter)
            {
                if (dailyBongCounter == 0)
                {
                    int tmp = ordRepo.GetLastBongNo(out dailyBongCounter);
                }

                string dailyBongNo = string.Format("{0,-36}", "DailyBong#" + dailyBongCounter);
                bw.LargeText(dailyBongNo);
            }

            if (_orderMaster.TableId > 0)
            {
                // bw.FeedLines(1);
                string TableNo = "";

                if (!string.IsNullOrEmpty(_orderMaster.TableName))
                {
                    TableNo = string.Format("{0,-40}", _orderMaster.TableName);
                }
                else
                {
                    if (_orderMaster.SelectedTable != null)
                    {
                        if (_orderMaster.SelectedTable.Name.Contains("Table"))
                        {
                            TableNo = string.Format("{0,-40}", _orderMaster.SelectedTable.Name);
                        }
                        else
                            TableNo = string.Format("{0,-40}", UI.OpenOrder_TableButton + " " + _orderMaster.SelectedTable.Name);
                    }
                    else
                        TableNo = string.Format("{0,-40}", UI.OpenOrder_TableButton + " " + _orderMaster.TableId.ToString());
                }
                bw.LargeText(TableNo);
            }
            if (_orderMaster.CustomerId != null && _orderMaster.CustomerId != default(Guid))
            {

                var customer = ordRepo.GetCustomer(_orderMaster.CustomerId);
                bw.NormalFontBold(customer.Name);

                if (!string.IsNullOrEmpty(customer.Address1))
                {
                    string address = customer.Address1;
                    if (customer.Address1.Length > 26)
                    {
                        string adrs = address.Substring(0, 26);
                        bw.NormalFontBold(adrs);
                        address = "";
                        int counter = 1;
                        foreach (var s in customer.Address1.Skip(26))
                        {

                            if (counter % 20 == 0)
                            {

                                //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                                bw.NormalFontBold(address);
                                address = "";
                            }
                            address = address + s;
                            counter++;
                        }
                        if (!string.IsNullOrEmpty(address))
                        {
                            //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                            bw.NormalFontBold(address);
                        }
                    }
                    else
                    {
                        // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                        bw.NormalFontBold(address);
                    }


                }

                string cityZip = customer.City ?? " " + ", " + customer.ZipCode ?? " ";
                bw.NormalFontBold(cityZip);

                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    bw.NormalFontBold(customer.Phone);

                }
                if (customer.FloorNo > 0)
                    bw.NormalFontCenter(UI.Global_Floor + ": " + customer.FloorNo);
                if (customer.PortCode > 0)
                    bw.NormalFontCenter(UI.Global_PortCode + ": " + customer.PortCode);

                bw.FeedLines(1);

            }

            if (_orderMaster.Customer != null && !string.IsNullOrEmpty(_orderMaster.Customer.Address1))
            {
                string address = _orderMaster.Customer.Address1;
                if (_orderMaster.Customer.Address1.Length > 29)
                {
                    string adrs = address.Substring(0, 29);
                    bw.NormalFontBold(adrs);
                    address = "";
                    int counter = 1;
                    foreach (var s in _orderMaster.Customer.Address1.Skip(29))
                    {

                        if (counter % 20 == 0)
                        {

                            //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                            bw.NormalFontBold(address);
                            address = "";
                        }
                        address = address + s;
                        counter++;
                    }
                    if (!string.IsNullOrEmpty(address))
                    {
                        //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                        bw.NormalFontBold(address);
                    }
                }
                else
                {
                    // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                    bw.NormalFontBold(address);
                }

            }
            if (_orderMaster.Customer != null)
            {
                if (_orderMaster.Customer.CustomerNo != "0")
                    bw.NormalFontBold("LGH: " + _orderMaster.Customer.CustomerNo);//here customer is used as appartment number
                if (_orderMaster.Customer.FloorNo > 0)
                    bw.NormalFontBold(UI.Global_Floor + ": " + _orderMaster.Customer.FloorNo);
                if (_orderMaster.Customer.PortCode > 0)
                    bw.NormalFontBold(UI.Global_PortCode + ": " + _orderMaster.Customer.PortCode);
            }
            //bw.FeedLines(1);

            if (_orderMaster.Status == OrderStatus.ReturnOrder)
            {
                bw.FeedLines(1);
                bw.LargeTextCenter("**** " + UI.Global_Return + " ****");

            }
            else if (_orderMaster.Type == OrderType.TakeAway || _orderMaster.Type == OrderType.TableTakeAwayOrder)
            {
                bw.FeedLines(1);
                bw.LargeTextCenter("**** " + UI.Sales_TakeAwayButton + " ****");

            }
            else if (_orderMaster.Type == OrderType.TakeAwayReturn)
            {
                bw.FeedLines(1);
                bw.LargeTextCenter("**** " + UI.Sales_ReturnOrder + " " + UI.Sales_TakeAwayButton + " * ***");
            }
            var lstDetail = FormatOrderList(itemLines, _orderMaster.Type);//orderMaster.OrderLinesList.ToList()

            if (_isKitchen)
            {
                string orderComment = string.IsNullOrEmpty(_orderMaster.OrderComments) ? "" : _orderMaster.OrderComments;
                if (!string.IsNullOrEmpty(orderComment))
                {
                    string comments = "";
                    if (orderComment.Contains(":"))
                    {
                        string[] str1 = orderComment.Split(':');
                        var strLable = str1[0];
                        if (!string.IsNullOrEmpty(strLable))
                            bw.High(string.Format("{0,-40}", strLable + ":"));
                        comments = str1[1];
                    }
                    else
                        comments = orderComment;
                    if (comments.Length > 37)
                    {
                        var str = SplitComment(comments, 37).ToList();
                        int counter = 0;
                        foreach (var s in str)
                        {
                            bw.High(string.Format("{0,-40}", s));
                            counter++;
                        }
                    }
                    else
                        bw.High(string.Format("{0,-40}", comments));
                }
                // string comments = string.IsNullOrEmpty(orderMaster.Comments) ? "" : orderMaster.Comments;

            }
            if (Defaults.MultiKitchen)
            {
                bw.FeedLines(2);

                bw.LargeTextCenter("**** " + UI.Global_MultipleKitchen + " ****");

            }
            bw.FeedLines(1);
            // bw.LargeText(string.Format("{0,-34}{1,8}", UI.Report_Description, UI.Report_Quantity));

            var detailgroups = lstDetail.GroupBy(grp => new { grp.ItemId, grp.IngredientItems, grp.ItemComments });
            var lstItemsDetails = new List<OrderLine>();
            foreach (var grp in detailgroups)
            {
                var itm = grp.First();
                decimal qty = grp.Sum(s => s.Quantity);

                lstItemsDetails.Add(new OrderLine
                {
                    Id = itm.Id,

                    Product = itm.Product,
                    Quantity = qty,
                    ItemId = itm.ItemId,
                    ItemComments = itm.ItemComments,
                    ItemIdex = itm.ItemIdex,
                    ReceiptItems = itm.ReceiptItems,
                    IngredientItems = itm.IngredientItems
                });
            }
            //  var items = lstItemsDetails.OrderBy(i => i.ItemIdex).ToList();
            bw.NormalFont("..........................................");
            foreach (var item in lstItemsDetails)
            {
                if (item.Product.Bong == false)
                    continue;

                string s = string.Format("{0,-2}{1,-2}{2,-18}", item.CartQty, "x", item.ItemName);
                if (Defaults.BongNormalFont)
                    bw.High(s);
                else
                    bw.LargeText(s);
                //string coments = string.Format("{0,-2}", item.ItemComments);
                //if (!string.IsNullOrEmpty(coments))
                //{
                //    if (Defaults.BongNormalFont)
                //        bw.High(coments);
                //    else
                //        bw.LargeText(coments);
                //}
                if (item.ReceiptItems != null)
                {
                    foreach (var extra in item.ReceiptItems)
                    {
                        string es = string.Format("{0,-22}{1,8}", "    " + extra.Direction + " " + extra.Text, extra.Quantity);
                        bw.NormalFontBold(es);
                    }
                }
                if (item.IngredientItems != null && item.IngredientItems.Count > 0)
                {
                    foreach (var ingridient in item.IngredientItems.OrderBy(o => o.IngredientMode).ToList())
                    {
                        //string direction = "-";
                        //if (ingridient.GrossTotal != 0)
                        //    direction = "+";
                        string es = string.Format("{0,-22}{1,8}", "    " + ingridient.IngredientMode + " " + ingridient.ItemName, ingridient.CartQty);
                        bw.NormalFontBold(es);
                    }
                }
                if (!string.IsNullOrEmpty(item.ItemComments))
                {
                    string comment = string.Format("{0,-38}", item.ItemComments);
                    bw.High(comment);
                }

                bw.NormalFont("..........................................");
            }

            bw.FeedLines(2);
            bw.NormalFontCenter("..........      " + UI.Report_EndOrder + "      ..........");
            bw.Finish();


        }



        public void PrintFoodOrderBong(Order order, bool isKitchen, Customer customer = null)
        {
            try
            {
                LogWriter.LogWrite(new Exception("printbong... calling" + isKitchen));
                if (Defaults.SaleType == SaleType.Restaurant)
                {
                    var ordRepo = new InvoiceHandler();
                    _printerName = Utilities.GetPrinterById(Defaults.Outlet.KitchenPrinterId);
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptKitchen), order.Id);
                    _isKitchen = isKitchen;
                    _orderMaster = order;
                    _ordercustomer = customer;
                    Print(PrinterName, GetBongFoodOrderDocument(order.OrderLines.ToList()));
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private byte[] GetBongFoodOrderDocument(List<OrderLine> orderDetails)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintBongFoodOrderDocumemt(bw, orderDetails);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        private void PrintBongFoodOrderDocumemt(BinaryWriter bw, List<OrderLine> itemLines)
        {
            InvoiceHandler ordRepo = new InvoiceHandler();
            int bongNo = 0;
            int.TryParse(_orderMaster.Bong, out bongNo);
            int dailyBongCounter = 0;
            int.TryParse(_orderMaster.DailyBong, out dailyBongCounter);
            var deliveryDateTime = _orderMaster.DeliveryDate == null ? DateTime.Now : _orderMaster.DeliveryDate;
            var dt = DateTime.Parse(deliveryDateTime.ToString());
            if (bongNo == 0)
            {
                bongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                //if (_orderMaster.Id != default(Guid))
                //   ordRepo.UpdateBongNo(_orderMaster.Id, dailyBongCounter);
            }
            string orderDate = string.Format("{0,-21}", "Order Date:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            bw.High(orderDate);

            // string s = string.Format("{0,-2}{1,-2}{2,-18}", item.CartQty, "x", item.ItemName);
            string deliveryDate = string.Format("{0,-21}", "Delivery Date:" + dt.ToString("yyyy-MM-dd HH:mm:ss"));
            // string deliveryDate = string.Format("{0,-21}","{ 0,-21}"," ","Delivery Date:" +  dt.ToString("yyyy-MM-dd HH:mm:ss"));
            bw.High(deliveryDate);
            //bw.FeedLines(1);
            bw.NormalFont("..........................................");

            if (Defaults.OrderNoOnBong)
            {

                string orderNo = string.Format("{0,-30}", "ORDER#" + _orderMaster.OrderNoOfDay);
                bw.NormalFontBold(orderNo);

            }
            if (Defaults.BongCounter)
            {

                string BongNo = string.Format("{0,-36}", "BONG#" + bongNo);
                bw.LargeText(BongNo);


            }

            if (Defaults.DailyBongCounter)
            {

                if (dailyBongCounter == 0)
                {
                    int tmp = ordRepo.GetLastBongNo(out dailyBongCounter);
                }

                string dailyBongNo = string.Format("{0,-36}", "DailyBong#" + dailyBongCounter);
                bw.LargeText(dailyBongNo);

            }


            if (_ordercustomer != null)
            {

                string name = string.Format("{0,-36}", "Name:" + _ordercustomer.Name);
                bw.LargeText(name);

                string phone = string.Format("{0,-36}", "Phone:" + _ordercustomer.Phone);
                bw.LargeText(phone);

                string address = string.Format("{0,-36}", "Address:" + _ordercustomer.Address1);
                bw.LargeText(address);
            }

            if (_orderMaster.TableId > 0)
                //  var items = lstItemsDetails.OrderBy(i => i.ItemIdex).ToList();
                bw.NormalFont("..........................................");
            var lstDetail = FormatOrderList(itemLines, _orderMaster.Type);//orderMaster.OrderLinesList.ToList()
            var detailgroups = lstDetail.GroupBy(grp => new { grp.ItemId, grp.IngredientItems, grp.ItemComments });
            var lstItemsDetails = new List<OrderLine>();
            foreach (var grp in detailgroups)
            {
                var itm = grp.First();
                decimal qty = grp.Sum(s => s.Quantity);

                lstItemsDetails.Add(new OrderLine
                {
                    Id = itm.Id,

                    Product = itm.Product,
                    Quantity = qty,
                    ItemId = itm.ItemId,
                    ItemComments = itm.ItemComments,
                    ItemIdex = itm.ItemIdex,
                    ReceiptItems = itm.ReceiptItems,
                    IngredientItems = itm.IngredientItems
                });
            }
            //  var items = lstItemsDetails.OrderBy(i => i.ItemIdex).ToList();
            bw.NormalFont("..........................................");
            foreach (var item in lstItemsDetails)
            {
                if (item.Product.Bong == false)
                    continue;

                string s = string.Format("{0,-2}{1,-2}{2,-18}", item.CartQty, "x", item.ItemName);
                if (Defaults.BongNormalFont)
                    bw.High(s);
                else
                    bw.LargeText(s);
                if (item.ReceiptItems != null)
                {
                    foreach (var extra in item.ReceiptItems)
                    {
                        string es = string.Format("{0,-22}{1,8}", "    " + extra.Direction + " " + extra.Text, extra.Quantity);
                        bw.NormalFontBold(es);
                    }
                }
                if (item.IngredientItems != null && item.IngredientItems.Count > 0)
                {
                    foreach (var ingridient in item.IngredientItems.OrderBy(o => o.IngredientMode).ToList())
                    {
                        //string direction = "-";
                        //if (ingridient.GrossTotal != 0)
                        //    direction = "+";
                        string es = string.Format("{0,-22}{1,8}", "    " + ingridient.IngredientMode + " " + ingridient.ItemName, ingridient.CartQty);
                        bw.NormalFontBold(es);
                    }
                }
                if (!string.IsNullOrEmpty(item.ItemComments))
                {
                    string comment = string.Format("{0,-38}", item.ItemComments);
                    bw.High(comment);
                }

                bw.NormalFont("..........................................");
            }

            //bw.NormalFontCenter("..........      " + bongNo + "      ..........");
            bw.FeedLines(2);
            bw.NormalFontCenter("..........      " + UI.Report_EndOrder + "      ..........");
            bw.Finish();
            try
            {
                new InvoiceHandler().UpdateBongNoForFoodOrder(dailyBongCounter);

            }
            catch (Exception e)
            {

            }

        }







        #endregion END Print BONG

        #region Print Receipt
        bool CU_SUCCESS = false;
        public void PrintReceipt(Guid orderId, bool isCopy, bool cusuccess)
        {
            try
            {

                CU_SUCCESS = cusuccess;

                //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

                _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));

                //   getorderbyId();

                Console.WriteLine("Preparing data for print      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Defaults.PerformanceLog.Add("Preparing data for print     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                GetReceiptData(orderId, isCopy);
                VerifyPrinterName();
                Console.WriteLine(PrinterName);

                Console.WriteLine("sending for direct printert       -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Defaults.PerformanceLog.Add("sending for direct printert     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                if (_isMarchantReceipt)//in the case of Marchant receipt
                    Print(PrinterName, GetReceiptDocument());

                Print(PrinterName, GetReceiptDocument());
                if (_printModel.OrderMaster.CustomerId != null && _printModel.OrderMaster.CustomerId != default(Guid) && _printModel.OrderMaster.Customer.HasDeposit == false)
                {
                    Print(PrinterName, GetCustomerReceiptDocument());
                }

                LogWriter.LogWrite(PrinterName + " : Receipt is printed            -> Order ID " + _printModel.OrderMaster.Id  + " Order amount: "+ _printModel.OrderMaster.OrderTotal);
                Defaults.PerformanceLog.Add("Receipt is printed     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                if (_isCopy)
                {
                    ReceiptHandler receiptRepo = new ReceiptHandler();
                    Receipt receipt = _printModel.OrderMaster.Receipt;
                    //LogWriter.LogWrite("Receipt Object for copy receipt printing : " + JsonConvert.SerializeObject(receipt));
                    /* Start control code */
                    using (var cuAction = PosState.GetInstance().ControlUnitAction)
                    {
                        cuAction.ControlUnit.RegisterPOS(Defaults.Outlet.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                        int attempts = 1;
                        var x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1, true); // true for Clean cash debugging
                        while (x == null && attempts < 4)
                        {
                            attempts++;
                            x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts, true); // true for Clean cash debugging
                        }

                        // Assigned a dummy code and unit name to avoid removing this from database
                        if (x == null)
                        {
                            x = new ControlUnitResponse(true, "CONTROL_UNIT_FAILED", "1234567890abcdefghijklmnopqrstuvwxyz");
                        }
                        receipt.ReceiptCopies++;
                    }
                    receiptRepo.Update(receipt);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        public void PrintReceiptForAccountCustomer(Guid orderId, bool isCopy, bool cusuccess)
        {
            try
            {

                CU_SUCCESS = cusuccess;

                //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

                _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));

                //   getorderbyId();

                Console.WriteLine("Preparing data for print      -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Defaults.PerformanceLog.Add("Preparing data for print     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                GetReceiptData(orderId, isCopy);
                VerifyPrinterName();
                Console.WriteLine(PrinterName);

                Console.WriteLine("sending for direct printert       -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Defaults.PerformanceLog.Add("sending for direct printert     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                if (_isMarchantReceipt)//in the case of Marchant receipt
                    Print(PrinterName, GetReceiptDocument());

                Print(PrinterName, GetReceiptDocument());
                if (Defaults.POSMiniPrintForAccountCustomer == "1")
                {
                    if (_printModel.OrderMaster.CustomerId != null && _printModel.OrderMaster.CustomerId != default(Guid) && _printModel.OrderMaster.Customer.HasDeposit == false)
                    {
                        Print(PrinterName, GetCustomerReceiptDocument());
                    }
                }
                Console.WriteLine("Receipt is printed            -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                Defaults.PerformanceLog.Add("Receipt is printed     -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                if (_isCopy)
                {
                    ReceiptHandler receiptRepo = new ReceiptHandler();
                    Receipt receipt = _printModel.OrderMaster.Receipt;

                    /* Start control code */
                    using (var cuAction = PosState.GetInstance().ControlUnitAction)
                    {
                        cuAction.ControlUnit.RegisterPOS(Defaults.Outlet.OrgNo, Defaults.Terminal.TerminalNo.ToString());
                        int attempts = 1;
                        var x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, 1, true); // true for Clean cash debugging
                        while (x == null && attempts < 4)
                        {
                            attempts++;
                            x = cuAction.ControlUnit.SendReceipt(receipt, Defaults.User, attempts, true); // true for Clean cash debugging
                        }

                        // Assigned a dummy code and unit name to avoid removing this from database
                        if (x == null)
                        {
                            x = new ControlUnitResponse(true, "CONTROL_UNIT_FAILED", "1234567890abcdefghijklmnopqrstuvwxyz");
                        }
                        receipt.ReceiptCopies++;
                    }
                    receiptRepo.Update(receipt);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private byte[] GetReceiptDocument()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintReceiptDocuments(bw);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// This is the method we print the receipt the way we want. Note the spaces.
        /// Wasted a lot of paper on this to get it right.
        /// </summary>
        /// <param name="bw"></param>

        private void PrintReceiptDocuments(BinaryWriter bw)
        {
            LogWriter.LogWrite("PrintReceiptDocuments Starts");
            if (Defaults.LogoEnable)
            {
                bw.PrintLogo();
                bw.FeedLines(1);
                bw.NormalFontCenter(_printModel.Header); //Name of Outlet
            }
            else
            {
                bw.LargeTextCenter(_printModel.Header);
                bw.FeedLines(1);
            }



            string address = Defaults.Outlet.Address1;

            if (!string.IsNullOrEmpty(address))
            {
                if (Defaults.Outlet.Address1.Length > 26)
                {
                    string adrs = address.Substring(0, 26);
                    bw.NormalFontCenter(adrs);
                    address = "";
                    int counter = 1;
                    foreach (var s in Defaults.Outlet.Address1.Skip(26))
                    {

                        if (counter % 20 == 0)
                        {

                            //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                            bw.NormalFontCenter(address);
                            address = "";
                        }
                        address = address + s;
                        counter++;
                    }
                    if (!string.IsNullOrEmpty(address))
                    {
                        //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                        bw.NormalFontCenter(address);
                    }
                }
                else
                {
                    // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                    bw.NormalFontCenter(address);
                }
            }
            bw.NormalFontCenter(Defaults.Outlet.PostalCode + " " + Defaults.Outlet.City);
            string orgNo = UI.Report_OrgNo + " " + Defaults.CompanyInfo.OrgNo;
            bw.NormalFontCenter(orgNo);
            string phon = UI.Report_Phone + " " + Defaults.CompanyInfo.Phone;
            bw.NormalFontCenter(phon);

            if (!string.IsNullOrEmpty(Defaults.CompanyInfo.Email) && Defaults.CompanyInfo.Email.Length > 10)
            {
                string email = UI.Global_Email + " " + Defaults.CompanyInfo.Email;
                bw.NormalFontCenter(email);
            }

            if (!string.IsNullOrEmpty(Defaults.CompanyInfo.URL) && Defaults.CompanyInfo.URL.Length > 5)
            {
                string url = UI.Global_URL + " " + Defaults.CompanyInfo.URL;
                bw.NormalFontCenter(url);
            }
            bw.FeedLines(1);
            if (CU_SUCCESS == false)
            {
                bw.NormalFontBoldCenter("***  EJ KVITTO ***"); //return order
                bw.FeedLines(1);
            }

            bool isNewLine = false;
            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                bw.LargeTextCenter(UI.Global_Return); //return order
                isNewLine = true;
            }
            if (_printModel.OrderMaster.Type == OrderType.TakeAway || _printModel.OrderMaster.Type == OrderType.TableTakeAwayOrder)
            {
                bw.LargeTextCenter(UI.Sales_TakeAwayButton); //takeaway order
                isNewLine = true;
            }
            if (_isCopy)
            {
                bw.FeedLines(1);
                bw.LargeTextCenter(UI.Report_ReceiptCopy); //Kopia order
                isNewLine = true;
            }
            if (Defaults.User.TrainingMode)
            {
                bw.LargeTextCenter(UI.Report_Trainingmode); //Training Mode
                isNewLine = true;
            }
            if (_printModel.OrderMaster.CustomerId != null && _printModel.OrderMaster.CustomerId != default(Guid))
            {
                CustomerRepository ordRepo = new CustomerRepository();
                var customer = ordRepo.GetCustomerById(_printModel.OrderMaster.CustomerId);
                bw.NormalFontCenter(customer.Name);

                if (!string.IsNullOrEmpty(customer.Address1))
                {
                    address = customer.Address1;
                    if (customer.Address1.Length > 26)
                    {
                        string adrs = address.Substring(0, 26);
                        bw.NormalFontCenter(adrs);
                        address = "";
                        int counter = 1;
                        foreach (var s in customer.Address1.Skip(26))
                        {

                            if (counter % 20 == 0)
                            {

                                //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                                bw.NormalFontCenter(address);
                                address = "";
                            }
                            address = address + s;
                            counter++;
                        }
                        if (!string.IsNullOrEmpty(address))
                        {
                            //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                            bw.NormalFontCenter(address);
                        }
                    }
                    else
                    {
                        // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                        bw.NormalFontCenter(address);
                    }

                }
                string cityZip = customer.City ?? " " + ", " + customer.ZipCode ?? " ";
                bw.NormalFontCenter(cityZip);
                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    bw.NormalFontCenter(customer.Phone);

                }
                if (customer.FloorNo > 0)
                    bw.NormalFontCenter(UI.Global_Floor + ": " + customer.FloorNo);
                if (customer.PortCode > 0)
                    bw.NormalFontCenter(UI.Global_PortCode + ": " + customer.PortCode);

                isNewLine = true;
            }
            if (isNewLine)
                bw.FeedLines(1);

            if (_isMarchantReceipt)
            {
                bw.NormalFontBoldCenter("Butikens Kvitto");
                bw.FeedLines(1);
            }
            string receiptNo = UI.Global_Receipt + " " + _printModel.ReceiptNo;
            bw.NormalFontBoldCenter(receiptNo);
            //   string date = string.Format("{0,-10}{1,-26}", UI.OrderHistory_Date, printModel.OrderMaster.InvoiceDate);
            bw.NormalFontCenter(Convert.ToDateTime(_orderMaster.InvoiceDate).ToString("yyyy-MM-dd HH:mm:ss"));

            bool productBong = false;
            //string dotLine = "-----------------------------------------";
            // bw.FeedLines(1);
            if (_printModel.HasBalanceValue)
            {
                bw.NormalFontCenter(UI.Transaction_Old_Balance + ": " + _printModel.OldBalance);
                bw.NormalFontCenter(UI.Transaction_New_Balance + ": " + _printModel.NewBalance);
            }
            bw.FeedLines(1);
            bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Global_Articles, " "));
            //  bw.NormalFont(dotLine);
            foreach (var item in _printModel.Items)
            {

                // var idx = InvoiceItems.IndexOf(item) + 1;
                if (item.ItemName.Length > 17)
                {
                    if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                    {
                        if (item.Quantity == 1)
                        {
                            string s = String.Format("{0,-20} {1,10} {2,10}", item.ItemName, "  ", " ");//Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo)
                            bw.NormalFontBold(s);
                        }
                        else
                        {
                            string s = String.Format("{0,-20} {1,10} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");// Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo)
                            bw.NormalFontBold(s);
                        }
                        if (item.ItemDiscount != 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = "Kampanj " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = String.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        bw.NormalFont(item.ItemName);
                        string s = String.Format("{0,-17} {1,13} {2,10}", " ", item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount != 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = "Kampanj " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = String.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                }
                else
                {
                    if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                    {
                        if (item.Quantity == 1)
                        {
                            string s = String.Format("{0,-20} {1,10} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));//Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo)
                            bw.NormalFontBold(s);
                        }
                        else
                        {
                            string s = String.Format("{0,-20} {1,10} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");// Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo)
                            bw.NormalFontBold(s);
                        }
                        if (item.ItemDiscount != 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = "Kampanj " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = String.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        if (item.Quantity != 1)
                        {
                            string s = String.Format("{0,-17} {1,13} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                            if (item.ItemDiscount != 0)
                            {
                                string discountDesc = UI.Sales_Discount;
                                if (item.DiscountType == DiscountType.Offer)
                                    discountDesc = "Kampanj " + discountDesc;
                                if (!string.IsNullOrEmpty(item.DiscountDescription))
                                    discountDesc = item.DiscountDescription;
                                string dis = String.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                                bw.NormalFont(dis);
                            }
                        }
                        else
                        {
                            string s = String.Format("{0,-17} {1,13} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                            if (item.ItemDiscount != 0)
                            {
                                string discountDesc = UI.Sales_Discount;
                                if (item.DiscountType == DiscountType.Offer)
                                    discountDesc = "Kampanj " + discountDesc;
                                if (!string.IsNullOrEmpty(item.DiscountDescription))
                                    discountDesc = item.DiscountDescription;
                                string dis = String.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                                bw.NormalFont(dis);
                            }
                        }
                    }
                }
                //Print ingredients
                if (item.IngredientItems != null && item.IngredientItems.Count > 0)
                {
                    foreach (var _item in item.IngredientItems.OrderBy(o => o.IngredientMode).ToList())
                    {
                        if (_item.Quantity != 1)
                        {
                            string qty = "(" + _item.Quantity + "x" + _item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                            string s = String.Format("{0,-23}{1,8} {2,10}", "  " + _item.IngredientMode + " " + _item.ItemName, Math.Round(_item.Quantity, 0), Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }
                        else
                        {
                            string s = String.Format("{0,-31} {1,10}", "  " + _item.IngredientMode + " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }

                    }
                }
                //print group items
                if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                {
                    if (item.ItemDetails != null)
                    {
                        foreach (var _item in item.ItemDetails)
                        {
                            if (_item.ItemName.Length >= 17)
                            {
                                if (_item.Quantity != 1)
                                {
                                    string qty = "(" + _item.CartQty + "x" + _item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                                    string s = String.Format("{0,-20} {1,10} {2,-8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), qty);
                                    bw.NormalFont(s);
                                }
                                else
                                {
                                    string s = String.Format("{0,-20} {1,10} {2,8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");
                                    bw.NormalFont(s);
                                }
                            }
                            else
                            {
                                if (_item.Quantity != 1)
                                {
                                    string qty = "(" + _item.CartQty + "x" + _item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                                    string s = String.Format("{0,-17} {1,13} {2,-8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), qty);
                                    bw.NormalFont(s);
                                }
                                else
                                {
                                    string s = String.Format("{0,-17} {1,13} {2,8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");
                                    bw.NormalFont(s);
                                }
                            }
                        }
                    }
                }
            }

            //  bw.NormalFont(dotLine);
            bw.FeedLines(1);
            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                string endTotal = String.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalReturnBill + " (SEK)", _printModel.GrandTotal.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);

            }
            else
            {
                string endTotal = String.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalBill + " (SEK)", _printModel.GrandTotal.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);
            }

            if (_printModel.OrderMaster.RoundedAmount != 0)
            {
                string roundedAmount = String.Format("{0,-32}{1,10}", UI.CheckOutOrder_RoundOff + ":", _printModel.OrderMaster.RoundedAmount.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(roundedAmount);
            }

            bw.FeedLines(1);




            // bw.NormalFont(dotLine);
            var lstPayments = _printModel.Payments;

            if (lstPayments != null && lstPayments.Count > 0 && Defaults.SaleType == SaleType.Restaurant)
            {
                var tipAmount = lstPayments.Sum(a => a.TipAmount);
                if (tipAmount > 0)
                {
                    string tipAmountString = String.Format("{0,-32}{1,10}", UI.CheckOutOrder_Tip + " (SEK)", tipAmount.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFont(tipAmountString);
                    var netPayable = _printModel.GrandTotal + tipAmount;
                    string netPayableAmount = String.Format("{0,-32}{1,10}", UI.CheckOutOrder_NetAmount + " (SEK)", netPayable.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFontBold(netPayableAmount);
                    bw.FeedLines(1);
                }
            }

            var discount = _printModel.OrderMaster.OrderLines.Where(a => a.ItemDiscount > 0).Sum(a => a.ItemDiscount);
            if (discount != 0)
            {
                var discountLine = String.Format("{0,-31} {1,10}", "Total rabatt:", Math.Round(discount).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(discountLine);
                bw.FeedLines(1);
            }

            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", "Åter", " "));
            }
            else
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Report_Payment, " "));
            }
            if (remainingAmount > 0)
            {
                string s = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_Remaining, remainingAmount.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(s);

            }

            foreach (var payment in lstPayments)
            {
                // var idx = InvoiceItems.IndexOf(item) + 1;
                if (payment.CashCollected != 0)
                {
                    string s = String.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.CashCollected, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }
                else
                {
                    string s = String.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.PaidAmount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }

            }

            //  bw.NormalFont(dotLine);
            //decimal returncash = lstPayments.Where(pmt => pmt.TypeId == 1 || pmt.TypeId == 7 || pmt.TypeId == 4 || pmt.TypeId == 9 || pmt.TypeId == 10 || pmt.TypeId == 11).Sum(tot => tot.CashChange);
            decimal returncash = lstPayments.Where(pmt => UtilityConstants.ListCashBack.Contains(pmt.TypeId)).Sum(tot => tot.ReturnAmount);
            decimal collectedcash = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.CashCollected);
            decimal collectedcard = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.CashCollected);
            decimal tip = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.TipAmount);
            decimal cardtip = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.TipAmount);
            if (returncash > 0)
            {
                bw.FeedLines(1);



                //if (collectedcash > 0 && printModel.OrderMaster.Type != OrderType.Return)
                //{
                //    string lblCashCollected = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_ReceivedCash, collectedcash.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                //    bw.NormalFont(lblCashCollected);
                //}
                //if (collectedcard > 0 && printModel.OrderMaster.Type != OrderType.Return)
                //{
                //    string lblKortCollected = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_ReceivedCard, collectedcard.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                //    bw.NormalFont(lblKortCollected);
                //}
                string lblreturnCash = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_CashBack + " " + UI.CheckOutOrder_Method_Cash, ((-1) * returncash).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(lblreturnCash);
            }
            bw.FeedLines(1);
            // bw.NormalFont("Moms %     NETTO       MOMS          SUMMA");
            string vatheader = String.Format("{0,-13} {1,-11} {2,-10} {3,2}", UI.Global_VAT + "%", UI.Global_VAT, UI.Report_TotalNet, UI.Global_Total);
            bw.NormalFont(vatheader);
            foreach (var vat in _printModel.VatDetails)
            {
                string s = String.Format("{0,5} {1,12} {2,12} {3,10}", vat.VATPercent.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.VATTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.NetAmount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.Total, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(s);
            }

            //Comments
            //bw.FeedLines(1);
            //bw.NormalFont(string.Format("{0,-40}", "Kvitto Text:"));
            if (Defaults.ShowComments)
            {
                string comments = "";
                if (_printModel.Comment.Contains(":"))
                {
                    string[] str1 = _printModel.Comment.Split(':');
                    var strLable = str1[0];
                    if (!string.IsNullOrEmpty(strLable))
                        bw.NormalFont(string.Format("{0,-40}", strLable + ":"));
                    comments = str1[1];
                }
                else
                    comments = _printModel.Comment;
                if (comments.Length > 37)
                {
                    var str = SplitComment(comments, 37).ToList();
                    int counter = 0;
                    bw.NormalFont(string.Format("{0,-40}", "kvitto Text:"));
                    foreach (var s in str)
                    {

                        bw.NormalFont(string.Format("{0,-40}", s));
                        //bw.NormalFont(string.Format("{0,-5}{1,-30}", "kvitto Test:", s));
                        counter++;
                    }

                    while (counter < 5)
                    {
                        bw.FeedLines(1);
                        counter++;
                    }

                }
                else
                {
                    bw.NormalFont(string.Format("{0,-40}", "kvitto Text:"));
                    bw.NormalFont(string.Format("{0,-40}", comments));
                    //bw.NormalFont(string.Format("{0,-15}{1,-40}", "Kvitto Text:", comments));
                    bw.FeedLines(2);

                }



            }

            if (Defaults.SignatureOnReturnReceipt && _printModel.OrderMaster.Type == OrderType.Return)
            {
                bw.FeedLines(2);
                bw.NormalFont(string.Format("{0,-40}", "Namn: _____________________________ "));
                bw.FeedLines(2);
                bw.NormalFont(string.Format("{0,-40}", "Mob nr: _____________________________ "));
                bw.FeedLines(2);
                bw.NormalFont(string.Format("{0,-40}", "Sign: _____________________________ "));


            }
            //bw.NormalFont("Datum     2016-04-10 20:01");
            if (CU_SUCCESS == false)
            {
                bw.FeedLines(1);
                bw.NormalFontBoldCenter("**  CONTROL UNIT FAILURE **"); //Control Unit Fail
                bw.FeedLines(1);
            }
            else
            {
                bw.FeedLines(1);
                if (!string.IsNullOrEmpty(_printModel.ControlUnitName))
                    bw.NormalFont(String.Format("{0,-14} {1,-20}", UI.Global_ControlUnit, _printModel.ControlUnitName));
            }

            string cashier = string.Format("{0,-15}{1,-20}", UI.Report_Cashier, Defaults.User.UserCode);
            bw.NormalFont(cashier);
            bw.NormalFont(String.Format("{0,-14} {1,-20}", UI.Global_Terminal, Defaults.Terminal.UniqueIdentification));

            if (_printModel.OrderMaster.TableId > 0)
            {
                int floor = 1;
                if (_printModel.OrderMaster.SelectedTable != null)
                {
                    floor = _printModel.OrderMaster.SelectedTable.Floor != null ? _printModel.OrderMaster.SelectedTable.Floor.Id : 1;
                }
                if (floor > 1)
                    _printModel.OrderMaster.TableName = _printModel.OrderMaster.TableName + " (" + floor + ")";

                //new code for table number and bong number print
                string TableNo = String.Format("{0,-14} {1,-20}", UI.OpenOrder_TableButton, _printModel.OrderMaster.TableId);
                TableNo = String.Format("{0,-14} {1,-20}", "Table", _printModel.OrderMaster.TableId);
                bw.NormalFont(TableNo);

                if (!(_printModel.OrderMaster != null && _printModel.OrderMaster.OrderLines.Count(a => a.Product.Bong == true) == 0))
                {
                    if (_printModel.OrderMaster.OrderType != OrderType.Return)
                    {
                        if (Defaults.DailyBongCounter)
                        {
                            if (string.IsNullOrEmpty(_printModel.OrderMaster.DailyBong))
                            {
                                InvoiceHandler ordRepo = new InvoiceHandler();
                                int dailyBongCounter = 0;
                                var tmpbongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                                _printModel.OrderMaster.DailyBong = dailyBongCounter.ToString();
                            }

                            bw.FeedLines(1);
                            string dailyBongNo = String.Format("{0,-14} {1,-14}", "Bong#", _printModel.OrderMaster.DailyBong);
                            bw.LargeText(dailyBongNo);
                        }
                        else if (Defaults.BongCounter)
                        {
                            if (string.IsNullOrEmpty(_printModel.OrderMaster.Bong))
                            {
                                InvoiceHandler ordRepo = new InvoiceHandler();
                                int dailyBongCounter = 0;
                                var tmpbongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                                _printModel.OrderMaster.Bong = tmpbongNo.ToString();
                            }
                            string BongNp = String.Format("{0,-14} {1,-20}", UI.OpenOrder_TableButton, _printModel.OrderMaster.TableName);
                            BongNp = String.Format("{0,-14} {1,-14}", "Bong#", _printModel.OrderMaster.Bong);
                            bw.LargeText(BongNp);
                        }


                    }
                }
            }
            if (_printModel.OrderMaster.TableId == 0)// && !string.IsNullOrEmpty(_printModel.OrderMaster.Bong))
            {
                if (!(_printModel.OrderMaster != null && _printModel.OrderMaster.OrderLines.Count(a => a.Product.Bong == true) == 0))
                {
                    if (_printModel.OrderMaster.OrderType != OrderType.Return)
                    {
                        if (Defaults.DailyBongCounter)
                        {
                            if (string.IsNullOrEmpty(_printModel.OrderMaster.DailyBong))
                            {
                                InvoiceHandler ordRepo = new InvoiceHandler();
                                int dailyBongCounter = 0;
                                var tmpbongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                                _printModel.OrderMaster.DailyBong = dailyBongCounter.ToString();
                            }

                            bw.FeedLines(1);
                            string dailyBongNo = String.Format("{0,-8} {1,-2}", "Bong#", _printModel.OrderMaster.DailyBong);
                            bw.LargeText(dailyBongNo);
                        }
                        else if (Defaults.BongCounter)
                        {
                            if (string.IsNullOrEmpty(_printModel.OrderMaster.Bong))
                            {
                                InvoiceHandler ordRepo = new InvoiceHandler();
                                int dailyBongCounter = 0;
                                var tmpbongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
                                _printModel.OrderMaster.Bong = tmpbongNo.ToString();
                            }
                            string BongNp = String.Format("{0,-14} {1,-20}", UI.OpenOrder_TableButton, _printModel.OrderMaster.Bong);
                            BongNp = String.Format("{0,-8} {1,-2}", "Bong#", _printModel.OrderMaster.Bong);
                            bw.LargeText(BongNp);
                        }
                    }
                }
            }
            bw.FeedLines(1);
            if (_isMarchantReceipt)
            {
                if (!string.IsNullOrEmpty(_printModel.MarchantPaymentReceipt))
                    bw.NormalFont(_printModel.MarchantPaymentReceipt);
                _isMarchantReceipt = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(_printModel.CustomerPaymentReceipt))
                    bw.NormalFont(_printModel.CustomerPaymentReceipt);
            }
            // if (!string.IsNullOrEmpty(Defaults.Outlet.TaxDescription))
            //    bw.NormalFontCenter(Defaults.Outlet.TaxDescription);
            bw.FeedLines(1);

            bw.NormalFontCenter(_printModel.Footer); //Footer
            //bw.NormalFontCenter("Toilet code: 4321"); //Footer
            //bw.FeedLines(1);
            //bw.NormalFontCenter("Wifi Koppsguest code: elvis2015"); //Footer
            //bw.FeedLines(1);
            //bw.NormalFontCenter("SPARA KVITTOT - KUNDENS EX");
            bw.Finish();

            UpdateBongInDb();
            LogWriter.LogWrite("PrintReceiptDocuments Ends");
        }

        public void UpdateBongInDb()
        {
            var orderRepo = new InvoiceHandler();
            int dailyBongCounter = 0;
            var tmp = orderRepo.GetLastBongNo(out dailyBongCounter);
            if (_orderMaster.Id != default(Guid))// && (string.IsNullOrEmpty(_orderMaster.Bong) || _orderMaster.Bong == "0"))
                orderRepo.UpdateBongNo(_orderMaster.Id, dailyBongCounter);
        }

        private void PrintReceiptDocuments1(BinaryWriter bw)
        {
            PrintHeader(bw);

            if (Defaults.Outlet.HasAddress)
            {
                PrintAddress(bw);
            }

            bool isNewLine = false;
            if (_printModel.OrderMaster.Type != OrderType.TableOrder || _printModel.OrderMaster.Type == OrderType.Standard)
            {
                PrintOrderType(_printModel.OrderMaster.Type, bw);
                isNewLine = true;
            }

            if (_isCopy)
            {
                PrintCopyInfo(bw);

                isNewLine = true;
            }

            if (Defaults.User.TrainingMode)
            {
                PrintTrainingMode(bw);

                isNewLine = true;
            }

            if (isNewLine)
            {
                bw.FeedLines(1);
                isNewLine = false;
            }


            PrintReceiptInfo(bw);

            PrintItems(_printModel.Items, bw);

            PrintFooter(bw);

            bw.FeedLines(1);

            PrintPayments(_printModel.Payments, bw);

            PrintVat(_printModel.VatDetails, bw);

            if (!string.IsNullOrEmpty(_printModel.ControlUnitName))
            {
                bw.FeedLines(1);
                PrintControlUnit(bw);
            }

            PrintCashier(bw);

            PrintTable(bw);

            if (_isMarchantReceipt)
            {
                bw.FeedLines(1);

                if (!string.IsNullOrEmpty(_printModel.MarchantPaymentReceipt))
                {
                    bw.NormalFont(_printModel.MarchantPaymentReceipt);
                }

                _isMarchantReceipt = false;
            }
            else if (!string.IsNullOrEmpty(_printModel.CustomerPaymentReceipt))
            {
                string receipt = _printModel.CustomerPaymentReceipt.Trim();

                receipt = string.Format("\r\n{0}\r\n", receipt);

                bw.NormalFont(receipt);
            }

            bw.NormalFontCenter(Defaults.Outlet.FooterText); //Footer

            bw.Finish();
        }

        private byte[] GetCustomerReceiptDocument()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintCustomerReceiptDocuments(bw);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        private void PrintCustomerReceiptDocuments(BinaryWriter bw)
        {
            if (Defaults.LogoEnable)
            {
                bw.PrintLogo();
                bw.FeedLines(1);
                bw.NormalFontCenter(_printModel.Header); //Name of Outlet
            }
            else
            {
                bw.LargeTextCenter(_printModel.Header);
                bw.FeedLines(1);
            }

            string address = Defaults.Outlet.Address1;
            if (!string.IsNullOrEmpty(address))
            {
                if (Defaults.Outlet.Address1.Length > 26)
                {
                    string adrs = address.Substring(0, 26);
                    bw.NormalFontCenter(adrs);
                    address = "";
                    int counter = 1;
                    foreach (var s in Defaults.Outlet.Address1.Skip(26))
                    {
                        if (counter % 20 == 0)
                        {
                            //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                            bw.NormalFontCenter(address);
                            address = "";
                        }
                        address += s;
                        counter++;
                    }
                    if (!string.IsNullOrEmpty(address))
                    {
                        //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                        bw.NormalFontCenter(address);
                    }
                }
                else
                {
                    // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                    bw.NormalFontCenter(address);
                }
            }
            bw.NormalFontCenter(Defaults.Outlet.PostalCode + " " + Defaults.Outlet.City);
            string orgNo = UI.Report_OrgNo + " " + Defaults.CompanyInfo.OrgNo;
            bw.NormalFontCenter(orgNo);
            string phon = UI.Report_Phone + " " + Defaults.CompanyInfo.Phone;
            bw.NormalFontCenter(phon);
            if (!string.IsNullOrEmpty(Defaults.CompanyInfo.Email))
            {
                string email = UI.Global_Email + " " + Defaults.CompanyInfo.Email;
                bw.NormalFontCenter(email);
            }
            bw.FeedLines(1);
            bool isNewLine = false;

            bw.LargeTextCenter(UI.Global_InvoiceBuy); //return order"Faktura Köp"
            isNewLine = true;

            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                bw.LargeTextCenter(UI.Global_Return); //return order
                isNewLine = true;
            }
            if (_printModel.OrderMaster.Type == OrderType.TakeAway || _printModel.OrderMaster.Type == OrderType.TableTakeAwayOrder)
            {
                bw.LargeTextCenter(UI.Sales_TakeAwayButton); //takeaway order
                isNewLine = true;
            }
            if (_isCopy)
            {
                bw.FeedLines(1);
                bw.LargeTextCenter(UI.Report_ReceiptCopy); //Kopia order
                isNewLine = true;
            }
            if (Defaults.User.TrainingMode)
            {
                bw.LargeTextCenter(UI.Report_Trainingmode); //Training Mode
                isNewLine = true;
            }
            if (isNewLine)
                bw.FeedLines(2);

            if (_printModel.OrderMaster.CustomerId != null && _printModel.OrderMaster.CustomerId != default(Guid))
            {
                CustomerRepository ordRepo = new CustomerRepository();
                var customer = ordRepo.GetCustomerById(_printModel.OrderMaster.CustomerId);
                bw.NormalFontCenter(customer.Name);

                if (!string.IsNullOrEmpty(customer.Address1))
                {
                    address = customer.Address1;
                    if (customer.Address1.Length > 26)
                    {
                        string adrs = address.Substring(0, 26);
                        bw.NormalFontCenter(adrs);
                        address = "";
                        int counter = 1;
                        foreach (var s in customer.Address1.Skip(26))
                        {

                            if (counter % 20 == 0)
                            {

                                //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                                bw.NormalFontCenter(address);
                                address = "";
                            }
                            address = address + s;
                            counter++;
                        }
                        if (!string.IsNullOrEmpty(address))
                        {
                            //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                            bw.NormalFontCenter(address);
                        }
                    }
                    else
                    {
                        // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                        bw.NormalFontCenter(address);
                    }

                }
                string cityZip = customer.City ?? " " + ", " + customer.ZipCode ?? " ";
                bw.NormalFontCenter(cityZip);
                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    bw.NormalFontCenter(customer.Phone);

                }
                if (customer.FloorNo > 0)
                    bw.NormalFontCenter(UI.Global_Floor + ": " + customer.FloorNo);
                if (customer.PortCode > 0)
                    bw.NormalFontCenter(UI.Global_PortCode + ": " + customer.PortCode);

                isNewLine = true;

                bw.FeedLines(1);
            }
            //if (isNewLine)
            //    bw.FeedLines(2);

            if (_isMarchantReceipt)
            {
                bw.NormalFontBoldCenter(UI.Global_StoreReceipt);
                bw.FeedLines(1);
            }
            string receiptNo = UI.Global_Receipt + " " + _printModel.ReceiptNo;
            bw.NormalFontBoldCenter(receiptNo);
            //   string date = string.Format("{0,-10}{1,-26}", UI.OrderHistory_Date, printModel.OrderMaster.InvoiceDate);
            bw.NormalFontCenter(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // string dotLine = "-----------------------------------------";
            bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Global_Articles, " "));
            //  bw.NormalFont(dotLine);
            foreach (var item in _printModel.Items)
            {
                // var idx = InvoiceItems.IndexOf(item) + 1;
                if (item.ItemName.Length > 17)
                {
                    if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                    {
                        if (item.Quantity == 1)
                        {
                            string s = string.Format("{0,-20} {1,10} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }
                        else
                        {
                            string s = string.Format("{0,-20} {1,10} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        bw.NormalFont(item.ItemName);
                        string s = string.Format("{0,-17} {1,13} {2,10}", " ", item.CartQty + "x" + item.UnitPrice, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                }
                else
                {
                    if (item.Quantity != 1)
                    {
                        string s = string.Format("{0,-17} {1,13} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        string s = string.Format("{0,-17} {1,13} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            if (!string.IsNullOrEmpty(item.DiscountDescription))
                                discountDesc = item.DiscountDescription;
                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                }
                if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                {
                    if (item.ItemDetails != null)
                    {
                        foreach (var _item in item.ItemDetails)
                        {
                            if (_item.ItemName.Length >= 17)
                            {
                                if (_item.Quantity != 1)
                                {
                                    string qty = "(" + _item.CartQty + "x" + _item.UnitPrice + ")";
                                    string s = string.Format("{0,-20} {1,10} {2,-8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), qty);
                                    bw.NormalFont(s);
                                }
                                else
                                {
                                    string s = string.Format("{0,-20} {1,10} {2,8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");
                                    bw.NormalFont(s);
                                }
                            }
                            else
                            {
                                if (_item.Quantity != 1)
                                {
                                    string qty = "(" + _item.CartQty + "x" + _item.UnitPrice + ")";
                                    string s = string.Format("{0,-17} {1,13} {2,-8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), qty);
                                    bw.NormalFont(s);
                                }
                                else
                                {
                                    string s = string.Format("{0,-17} {1,13} {2,8}", " " + _item.ItemName, Math.Round(_item.GrossTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), " ");
                                    bw.NormalFont(s);
                                }
                            }
                        }
                    }
                }
            }

            //  bw.NormalFont(dotLine);
            bw.FeedLines(1);
            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                string endTotal = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalReturnBill + " (" + Defaults.CurrencyName + ")", _printModel.GrandTotal.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);
            }
            else
            {
                string endTotal = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalBill + " (" + Defaults.CurrencyName + ")", _printModel.GrandTotal.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);
            }
            if (_printModel.OrderMaster.RoundedAmount != 0)
            {
                string roundedAmount = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_RoundOff + ":", _printModel.OrderMaster.RoundedAmount.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFontBold(roundedAmount);
            }


            bw.FeedLines(1);

            var discount = _printModel.OrderMaster.OrderLines.Where(a => a.ItemDiscount > 0).Sum(a => a.ItemDiscount);
            if (discount != 0)
            {
                var discountLine = String.Format("{0,-31} {1,10}", "Total rabatt:", Math.Round(discount).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(discountLine);
                bw.FeedLines(1);
            }

            // bw.NormalFont(dotLine);
            var lstPayments = _printModel.Payments;
            if (_printModel.OrderMaster.Type == OrderType.Return)
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Report_ReturnPayment, " ")); // "Åter"
            }
            else
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Report_Payment, " "));
            }



            foreach (var payment in lstPayments)
            {
                // var idx = InvoiceItems.IndexOf(item) + 1;
                if (payment.CashCollected != 0)
                {
                    string s = string.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.CashCollected, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }
                else
                {
                    string s = string.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.PaidAmount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }
            }

            //  bw.NormalFont(dotLine);
            //decimal returncash = lstPayments.Where(pmt => pmt.TypeId == 1 || pmt.TypeId == 7 || pmt.TypeId == 4).Sum(tot => tot.CashChange);
            decimal returncash = lstPayments.Where(pmt => UtilityConstants.ListCashBack.Contains(pmt.TypeId)).Sum(tot => tot.CashChange);
            decimal collectedcash = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.CashCollected);
            decimal collectedcard = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.CashCollected);
            decimal tip = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.TipAmount);
            decimal cardtip = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.TipAmount);
            if (returncash > 0)
            {
                bw.FeedLines(1);

                //if (collectedcash > 0 && printModel.OrderMaster.Type != OrderType.Return)
                //{
                //    string lblCashCollected = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_ReceivedCash, collectedcash.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                //    bw.NormalFont(lblCashCollected);
                //}
                //if (collectedcard > 0 && printModel.OrderMaster.Type != OrderType.Return)
                //{
                //    string lblKortCollected = String.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_ReceivedCard, collectedcard.ToString("N", (CultureInfo)Defaults.UICultureInfo));
                //    bw.NormalFont(lblKortCollected);
                //}
                string lblreturnCash = string.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_CashBack + " " + UI.CheckOutOrder_Method_Cash, ((-1) * returncash).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(lblreturnCash);
            }
            bw.FeedLines(1);
            // bw.NormalFont("Moms %     NETTO       MOMS          SUMMA");
            string vatheader = string.Format("{0,-13} {1,-11} {2,-10} {3,2}", UI.Global_VAT + "%", UI.Global_VAT, UI.Report_TotalNet, UI.Global_Total);
            bw.NormalFont(vatheader);
            foreach (var vat in _printModel.VatDetails)
            {
                string s = string.Format("{0,5} {1,12} {2,12} {3,10}", vat.VATPercent.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.VATTotal, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.NetAmount, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(vat.Total, 2).ToString("N", (CultureInfo)Defaults.UICultureInfo));
                bw.NormalFont(s);
            }
            //bw.NormalFont("Datum     2016-04-10 20:01");

            //Comments
            //bw.FeedLines(1);
            //bw.NormalFont(string.Format("{0,-40}", "Kvitto Text:"));
            if (Defaults.ShowComments)
            {
                string comments = "";
                if (_printModel.Comment.Contains(":"))
                {
                    string[] str1 = _printModel.Comment.Split(':');
                    var strLable = str1[0];
                    if (!string.IsNullOrEmpty(strLable))
                        bw.NormalFont(string.Format("{0,-40}", strLable + ":"));
                    comments = str1[1];
                }
                else
                    comments = _printModel.Comment;
                if (comments.Length > 37)
                {
                    var str = SplitComment(comments, 37).ToList();
                    int counter = 0;
                    bw.NormalFont(string.Format("{0,-40}", "kvitto Text:"));
                    foreach (var s in str)
                    {

                        bw.NormalFont(string.Format("{0,-40}", s));
                        //bw.NormalFont(string.Format("{0,-5}{1,-30}", "kvitto Test:", s));
                        counter++;
                    }

                    while (counter < 5)
                    {
                        bw.FeedLines(1);
                        counter++;
                    }

                }
                else
                {
                    bw.NormalFont(string.Format("{0,-40}", "kvitto Text:"));
                    bw.NormalFont(string.Format("{0,-40}", comments));
                    //bw.NormalFont(string.Format("{0,-15}{1,-40}", "Kvitto Text:", comments));
                    bw.FeedLines(2);

                }
            }

            bw.FeedLines(1);
            if (!string.IsNullOrEmpty(_printModel.ControlUnitName))
                bw.NormalFont(string.Format("{0,-14} {1,-20}", UI.Global_ControlUnit, _printModel.ControlUnitName));

            string cashier = string.Format("{0,-15}{1,-20}", UI.Report_Cashier, Defaults.User.UserName);
            bw.NormalFont(cashier);
            bw.NormalFont(string.Format("{0,-14} {1,-20}", UI.Global_Terminal, Defaults.Terminal.UniqueIdentification));

            if (_printModel.OrderMaster.TableId > 0)
            {
                int floor = 1;
                if (_printModel.OrderMaster.SelectedTable != null)
                {
                    floor = _printModel.OrderMaster.SelectedTable.Floor.Id;
                }
                if (floor > 1)
                    _printModel.OrderMaster.TableName = _printModel.OrderMaster.TableName + " (" + floor + ")";
                string TableNo = "";

                if (_orderMaster.SelectedTable.Name.Contains("Table"))
                {
                    TableNo = string.Format("{0,-40}", _orderMaster.SelectedTable.Name);
                }
                else
                    TableNo = string.Format("{0,-14} {1,-20}", UI.OpenOrder_TableButton, _printModel.OrderMaster.TableName);


                bw.NormalFontBoldCenter(TableNo);
            }
            bw.FeedLines(1);

            if (_isMarchantReceipt)
            {
                if (!string.IsNullOrEmpty(_printModel.MarchantPaymentReceipt))
                    bw.NormalFont(_printModel.MarchantPaymentReceipt);
                _isMarchantReceipt = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(_printModel.CustomerPaymentReceipt))
                    bw.NormalFont(_printModel.CustomerPaymentReceipt);
            }

            bw.FeedLines(1);
            bw.FeedLines(1);
            bw.FeedLines(1);
            bw.NormalFontCenter("Sign ...................................");
            // if (!string.IsNullOrEmpty(Defaults.Outlet.TaxDescription))
            //    bw.NormalFontCenter(Defaults.Outlet.TaxDescription);
            bw.FeedLines(1);
            bw.FeedLines(1);
            bw.FeedLines(1);
            bw.NormalFontCenter("Personnr ...................................");
            bw.FeedLines(1);
            bw.FeedLines(1);
            //  bw.NormalFontCenter(Defaults.Outlet.FooterText); //Footer
            //bw.NormalFontCenter("Toilet code: 4321"); //Footer
            //bw.FeedLines(1);
            //bw.NormalFontCenter("Wifi Koppsguest code: elvis2015"); //Footer
            //bw.FeedLines(1);
            //bw.NormalFontCenter("SPARA KVITTOT - KUNDENS EX");
            bw.Finish();
        }

        private List<OrderLine> FormatOrderList(List<OrderLine> orderLines, OrderType type)
        {
            var _orderLines = new List<OrderLine>();
            var direction = (type == OrderType.Return || type == OrderType.TakeAwayReturn) ? -1 : 1;
            foreach (var line in orderLines)
            {
                var orderLine = new OrderLine();
                orderLine = line;
                orderLine.Product = new Product
                {
                    Id = line.ItemId,
                    Description = line.ItemName,
                    Price = line.UnitPrice,
                    Bong = line.Product.Bong
                };
                if (!string.IsNullOrEmpty(line.ItemComments))
                {
                    //  orderLine.ItemName = line.ItemName;
                    orderLine.ItemComments = line.ItemComments;
                }
                orderLine.Quantity = direction * orderLine.Quantity;
                _orderLines.Add(orderLine);
            }
            return _orderLines;
        }



        private IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        private IEnumerable<string> SplitComment(string str, int chunkSize)
        {
            char[] charsToTrim3 = { '\n', '\r' };
            int start = 0;
            List<string> lst = new List<string>();
            while (true)
            {
                if (start > str.Length)
                    break;
                if (start + chunkSize < str.Length)
                {
                    var tmp = str.Substring(start, chunkSize);
                    lst.Add(tmp.Trim(charsToTrim3));
                }
                else
                {
                    var tmp = str.Substring(start, str.Length - start);
                    lst.Add(tmp.Trim(charsToTrim3));
                }

                start = start + chunkSize;
            }

            return lst;
        }

        #endregion End Print Receipt

        #region Print Bill/Performa

        private byte[] GetBillDocument()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintBillDocument(bw);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// This is the method we print the receipt the way we want. Note the spaces.
        /// Wasted a lot of paper on this to get it right.
        /// </summary>
        /// <param name="bw"></param>
        private void PrintBillDocument(BinaryWriter bw)
        {
            PrintHeader(bw);

            if (Defaults.Outlet.HasAddress)
            {
                PrintAddress(bw);
            }

            bool isNewLine = false;
            if (_printModel.OrderMaster.Type != OrderType.TableOrder || _printModel.OrderMaster.Type == OrderType.Standard)
            {
                PrintOrderType(_printModel.OrderMaster.Type, bw);
                isNewLine = true;
            }


            if (Defaults.User.TrainingMode)
            {
                PrintTrainingMode(bw);

                isNewLine = true;
            }

            if (isNewLine)
            {
                bw.FeedLines(1);
                isNewLine = false;
            }

            bw.LargeTextCenter(UI.CheckOutOrder_Not_Receipt); //Not Receipt
            bw.FeedLines(1);
            bw.NormalFontCenter(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            PrintItems(_printModel.Items, bw);
            PrintFooter(bw);
            bw.FeedLines(1);
            PrintVat(_printModel.VatDetails, bw);
            bw.FeedLines(1);
            PrintCashier(bw);
            PrintTable(bw);
            bw.FeedLines(1);
            bw.Finish();
        }




        #endregion END Print Bill/Performa

        #region Print Journal
        private byte[] GetJournalDocument()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                //RenderLogo(bw);
                PrintJournal(bw);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        private void PrintJournal(BinaryWriter bw)
        {
            if (Defaults.LogoEnable)
            {
                bw.PrintLogo();
                bw.FeedLines(1);
                bw.NormalFontCenter(_printModel.Header); //Name of Outlet
            }
            else
            {
                bw.LargeTextCenter(_printModel.Header);
            }

            string address = Defaults.Outlet.Address1;
            if (Defaults.Outlet.Address1.Length > 26)
            {
                string adrs = address.Substring(0, 26);
                bw.NormalFontCenter(adrs);
                address = "";
                int counter = 1;
                foreach (var s in Defaults.Outlet.Address1.Skip(26))
                {
                    if (counter % 20 == 0)
                    {
                        //   adrs = string.Format("{0,-10}{1,-26}", "", address);
                        bw.NormalFontCenter(address);
                        address = "";
                    }
                    address += s;
                    counter++;
                }
                if (!string.IsNullOrEmpty(address))
                {
                    //  adrs = string.Format("{0,-10}{1,-26}", " ", address);
                    bw.NormalFontCenter(address);
                }
            }
            else
            {
                // string addres = string.Format("{0,-10}{1,-26}", UI.Report_Address, address);
                bw.NormalFontCenter(address);
            }
            bw.NormalFontCenter(Defaults.Outlet.PostalCode + " " + Defaults.Outlet.City);
            string orgNo = UI.Report_OrgNo + " " + Defaults.CompanyInfo.OrgNo;
            bw.NormalFontCenter(orgNo);
            string phon = UI.Report_Phone + " " + Defaults.CompanyInfo.Phone;
            bw.NormalFontCenter(phon);
            bw.FeedLines(1);

            foreach (var item in _journalLogs)
            {

                bw.NormalFont(item.Created.ToString("yyyy-MM-dd hh:mm:ss"));
                if (item.LogMessage.Length > 39)
                {
                    var str = Split(item.LogMessage, 39).ToList();

                    foreach (var s in str)
                    {
                        bw.NormalFont(s);
                    }
                }
                else
                {
                    bw.NormalFont(item.LogMessage);
                }
            }
            bw.FeedLines(1);

            bw.Finish();
        }
        #endregion END Print Journal



        #region receipt data


        private bool _isMarchantReceipt;

        decimal remainingAmount = 0;
        private void GetReceiptData(Guid orderId, bool isCopy)
        {
            try
            {
                _isCopy = isCopy;
                var orderRepo = new OrderRepository(db);
                var depositHistoryForOrder = orderRepo.GetDepositHistoryForOrder(orderId);

                _printModel = new PrintModel
                {
                    Footer = string.IsNullOrEmpty(Defaults.Outlet.FooterText) ? " . " : Defaults.Outlet.FooterText,
                    Header = string.IsNullOrEmpty(Defaults.Outlet.HeaderText) ? Defaults.Outlet.Name : Defaults.Outlet.HeaderText,
                    CompanyInfo = Defaults.CompanyInfo,
                    TaxDesc = string.IsNullOrEmpty(Defaults.Outlet.TaxDescription) ? "  " : Defaults.Outlet.TaxDescription,
                    Address = string.IsNullOrEmpty(Defaults.Outlet.Address1) ? Defaults.Outlet.Name : Defaults.Outlet.Address1,
                };

                _printModel.HasBalanceValue = false;

                if (depositHistoryForOrder != null)
                {
                    _printModel.HasBalanceValue = true;
                    _printModel.NewBalance = depositHistoryForOrder.NewBalance;
                    _printModel.OldBalance = depositHistoryForOrder.OldBalance;
                }

                _printModel.OrderMaster = orderRepo.GetOrderMasterDetailById(orderId);

                _orderMaster = _printModel.OrderMaster;
                _printModel.Comment = "";
                if (!string.IsNullOrEmpty(_orderMaster.Comments))
                {
                    _printModel.Comment = _orderMaster.Comments;
                }
                else if (!string.IsNullOrEmpty(_orderMaster.OrderComments))
                {
                    _printModel.Comment = _orderMaster.OrderComments;
                }
                //_printModel.Bong ="Bong# "+ _orderMaster.Bong;


                var receipt = _printModel.OrderMaster.Receipt;
                if (receipt != null)
                {
                    _printModel.ReceiptNo = receipt.ReceiptNumber.ToString();
                    _printModel.ControlUnitCode = receipt.ControlUnitCode;
                    _printModel.ControlUnitName = receipt.ControlUnitName;
                    receipt.ReceiptNumber.ToString();
                    if (receipt.IsSignature)
                    {
                        _isMarchantReceipt = true;
                        _printModel.MarchantPaymentReceipt = receipt.MerchantPaymentReceipt;
                        _printModel.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;

                    }
                    else if (receipt.CustomerPaymentReceipt != null)
                    {

                        _printModel.CustomerPaymentReceipt = receipt.CustomerPaymentReceipt;

                    }
                }

                _printModel.ReceiptDate = DateTime.Now;
                _printModel.Cashier = Defaults.User.UserCode;

                var orderDetails = _printModel.OrderMaster.OrderLines.ToList();
                // if (orderDetails != null || orderDetails.Count>0)
                _printModel.Items = orderDetails;
                //SetVatDetail(orderDetails);
                SetVatDetail(_orderMaster);


                var lstPayments = _printModel.OrderMaster.Payments.Where(p => p.PaidAmount != 0).ToList();
                _printModel.Payments = lstPayments;


                decimal grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.GrossAmountDiscounted()), 2);
                if (_orderMaster.Type == OrderType.Return)
                    grossTotal = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ReturnGrossAmountDiscounted()), 2);
                var totalVat = orderDetails.Sum(ol => ol.VatAmount());
                if (_orderMaster.Type == OrderType.Return)
                    totalVat = Math.Round(orderDetails.Where(itm => itm.IsValid).Sum(tot => tot.ReturnVatAmount()), 2);
                var paidAmount = lstPayments.Sum(p => p.PaidAmount);
                if (_orderMaster.Type != OrderType.Return || _orderMaster.Type == OrderType.TakeAwayReturn)
                    remainingAmount = Convert.ToInt32(grossTotal - paidAmount);
                decimal ingredientTotal = 0;
                decimal ingredientVat = 0;
                foreach (var item in orderDetails)
                {

                    if (item.IngredientItems != null)
                    {
                        ingredientTotal = item.IngredientItems.Sum(s => s.GrossAmountDiscounted());
                        if (_orderMaster.Type == OrderType.Return)
                            ingredientTotal = item.IngredientItems.Sum(s => s.ReturnGrossAmountDiscounted());
                        ingredientVat = item.IngredientItems.Sum(s => s.VatAmount());
                        if (_orderMaster.Type == OrderType.Return)
                            ingredientVat = item.IngredientItems.Sum(s => s.ReturnVatAmount());
                        grossTotal = grossTotal + ingredientTotal;
                        totalVat = totalVat + ingredientVat;
                    }
                }


                decimal netTotal = grossTotal - totalVat;
                _printModel.GrandTotal = grossTotal;// - itemsDiscount;
                _printModel.NetTotal = netTotal;

                _printModel.Items = orderDetails.Where(itm => itm.IsValid).ToList();


                //while (_printModel.VATAmounts.Count < 4)
                //{
                //    _printModel.VATAmounts.Add(new VAT(0, 0));
                //}

                while (_printModel.VATAmounts.Count < 4)
                {
                    if (_printModel.VATAmounts.FirstOrDefault(a => a.VATPercent == 0) == null)
                        _printModel.VATAmounts.Add(new VAT(0, 0));
                    if (_printModel.VATAmounts.FirstOrDefault(a => a.VATPercent == 6) == null)
                        _printModel.VATAmounts.Add(new VAT(6, 0));
                    if (_printModel.VATAmounts.FirstOrDefault(a => a.VATPercent == 12) == null)
                        _printModel.VATAmounts.Add(new VAT(12, 0));
                    if (_printModel.VATAmounts.FirstOrDefault(a => a.VATPercent == 25) == null)
                        _printModel.VATAmounts.Add(new VAT(25, 0));
                }
                if (_printModel.OrderMaster.Receipt != null)
                    _printModel.OrderMaster.Receipt.VatDetails = _printModel.VATAmounts;

                // 
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private void SetVatDetail(Order order)//List<OrderLine> orderDetails
        {
            List<OrderLine> orderDetails = new List<OrderLine>();
            orderDetails = order.OrderLines.ToList();
            _orderMaster = order;

            var vatGroups = orderDetails.Where(i => i.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Individuals).GroupBy(od => od.TaxPercent);
            var vatDetails = new List<VATModel>();
            var vatAmounts = new List<VAT>();
            //if (_orderMaster.Type == OrderType.Return)
            //{

            //}
            foreach (var grp in vatGroups)
            {
                var itm = grp.First();
                decimal vat = itm.TaxPercent;
                decimal total = grp.Sum(tot => tot.GrossAmountDiscounted());
                if (_orderMaster.Type == OrderType.Return)
                    total = grp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                decimal net = grp.Sum(tot => tot.NetAmount());
                if (_orderMaster.Type == OrderType.Return)
                    net = grp.Sum(tot => tot.ReturnNetAmount());
                decimal vatAmount = grp.Sum(tot => tot.VatAmount());
                if (_orderMaster.Type == OrderType.Return)
                    vatAmount = grp.Sum(tot => tot.ReturnVatAmount());
                var vatModel = new VATModel(vat, vatAmount)
                {
                    NetAmount = net,
                    Total = total
                };
                vatDetails.Add(vatModel);
                vatAmounts.Add(vatModel.GetVatAmounts());
            }
            var ingrideintItems = new List<OrderLine>();
            foreach (var ingrItem in orderDetails)
            {
                if (ingrItem.IngredientItems != null && ingrItem.IngredientItems.Count() > 0)
                {
                    ingrideintItems.AddRange(ingrItem.IngredientItems.ToList());
                }
            }
            if (ingrideintItems.Count > 0)
            {

                var ingredientVatGroups = ingrideintItems.GroupBy(od => od.TaxPercent);
                foreach (var ingrp in ingredientVatGroups)
                {
                    var _itm = ingrp.First();
                    decimal invat = _itm.TaxPercent;
                    decimal intotal = ingrp.Sum(tot => tot.GrossAmountDiscounted());
                    if (_orderMaster.Type == OrderType.Return)
                        intotal = ingrp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                    decimal innet = ingrp.Sum(tot => tot.NetAmount());
                    if (_orderMaster.Type == OrderType.Return)
                        innet = ingrp.Sum(tot => tot.ReturnNetAmount());
                    decimal invatAmount = ingrp.Sum(tot => tot.VatAmount());
                    if (_orderMaster.Type == OrderType.Return)
                        invatAmount = ingrp.Sum(tot => tot.ReturnVatAmount());
                    var _vatModel = new VATModel(invat, invatAmount)
                    {
                        NetAmount = innet,
                        Total = intotal
                    };
                    vatDetails.Add(_vatModel);
                    vatAmounts.Add(_vatModel.GetVatAmounts());
                }

            }
            foreach (var detail in orderDetails.Where(p => p.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group))
            {
                var vatInnerGroups = detail.ItemDetails.GroupBy(od => od.TaxPercent);
                foreach (var grp in vatInnerGroups)
                {
                    decimal vat = grp.First().TaxPercent;
                    decimal total = grp.Sum(tot => tot.GrossAmountDiscounted());
                    if (_orderMaster.Type == OrderType.Return)
                        total = grp.Sum(tot => tot.ReturnGrossAmountDiscounted());
                    decimal vatAmount = grp.Sum(tot => tot.VatAmount());
                    if (_orderMaster.Type == OrderType.Return)
                        vatAmount = grp.Sum(tot => tot.ReturnVatAmount());
                    decimal net = grp.Sum(tot => tot.NetAmount());
                    if (_orderMaster.Type == OrderType.Return)
                        net = grp.Sum(tot => tot.ReturnNetAmount());
                    var vatModel = new VATModel(vat, vatAmount)
                    {
                        NetAmount = net,
                        Total = total
                    };
                    vatDetails.Add(vatModel);
                    vatAmounts.Add(vatModel.GetVatAmounts());
                }
            }
            var vatDetailsGroup = vatDetails.GroupBy(o => o.VATPercent).ToList();
            var vat_Details = new List<VATModel>();
            foreach (var vatgrp in vatDetailsGroup)
            {
                decimal vat = vatgrp.First().VATPercent;
                decimal total = vatgrp.Sum(tot => tot.Total);
                decimal net = vatgrp.Sum(tot => tot.NetAmount);
                decimal vatAmount = vatgrp.Sum(tot => tot.VATTotal);
                var vatModel = new VATModel(vat, vatAmount)
                {
                    NetAmount = net,
                    Total = total
                };

                vat_Details.Add(vatModel);

            }

            _printModel.VatDetails = vat_Details.OrderBy(o => o.VATPercent).ToList();
            _printModel.VATAmounts = vatAmounts;
        }


        #endregion




        private void VerifyPrinterName()
        {
            PrintQueueCollection printers = new PrintServer().GetPrintQueues();
            PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
            var data = printers.Where(pn => pn.Name == _printerName);
            if (data.Any())
            {
                prntque = printers.Single(pn => pn.Name == _printerName);
            }
            else // Fall back to XPS Document Writer
            {
                _printerName = "Microsoft XPS Document Writer";
            }
        }

        private void pdoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;

            int fontHeight = Convert.ToInt32(this._paragraphFont.GetHeight());
            int h1fontHeight = Convert.ToInt32(this._h1Font.GetHeight());
            const int startX = 0;
            const int startY = 0;
            int Offset = 0;
            //  Defaults.CompanyInfo.Name;

            graphics.DrawString("NIM POS", this._h1Font, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += h1fontHeight;

            string orgNo = string.Format("{0,-10}{1,-26}", "Org No", "123");
            graphics.DrawString(orgNo, this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;
            graphics.DrawString(" ", this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;
            string address = Defaults.CompanyInfo.Address1;

            string addres = string.Format("{0,-10}{1,-26}", "Address", address);
            graphics.DrawString(addres, this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;

            graphics.DrawString(" ", this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;
            string phon = string.Format("{0,-10}{1,-26}", "Phone", Defaults.CompanyInfo.Phone);
            graphics.DrawString(phon, this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;
            graphics.DrawString(" ", this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
            Offset += fontHeight;
            string[] lines = this._printData.Split((char)13);
            foreach (string line in lines)
            {
                graphics.DrawString(line, this._paragraphFont, new SolidBrush(Color.Black), startX, startY + Offset);
                Offset += fontHeight;
            }
        }

        public void PrintData(string content)
        {
            //_printerName = Utilities.GetPrinterById(Defaults.Outlet.BillPrinterId);

            _printerName = Utilities.GetPrinterByTerminalId(Guid.Parse(Defaults.TerminalId));
            VerifyPrinterName();

            byte[] bytes = GetbytesFromString(content);

            Print(PrinterName, bytes);
        }

        private byte[] GetbytesFromString(string data)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');
                // Render the logo
                if (Defaults.LogoEnable)
                {
                    bw.PrintLogo();
                    bw.FeedLines(1);
                    bw.NormalFontCenter(Defaults.CompanyInfo.Name); //Name of Outlet
                }
                else
                {
                    bw.LargeTextCenter(Defaults.CompanyInfo.Name);
                    bw.FeedLines(2);
                }


                //bw.NormalFontCenter(Defaults.CompanyInfo.Name);
                bw.NormalFontCenter(UI.Report_OrgNo + "" + Defaults.CompanyInfo.OrgNo);
                bw.NormalFontCenter(UI.Report_Address + "" + Defaults.CompanyInfo.Address);
                bw.NormalFontCenter(UI.Report_Phone + "" + Defaults.CompanyInfo.Phone);

                string[] lines = data.Split((char)13);
                foreach (var item in lines)
                {
                    if (item.Contains("Opened") || item.Contains("Closed") || item.Contains("Öppnades") || item.Contains("Stängdes"))
                    {
                        bw.NormalFontCenter(item.Trim());
                    }
                    else
                    {
                        bw.NormalFontCenter(item.TrimStart());
                    }
                }


                //bw.NormalFontCenter(data);

                bw.NormalFontCenter(Defaults.Outlet.FooterText);
                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        internal void SetOrderMaster(Order masterOrder)
        {
            
        }
    }

}