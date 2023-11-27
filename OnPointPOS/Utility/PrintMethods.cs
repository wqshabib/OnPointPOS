using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using POSSUM.Model;
using POSSUM.Res;


namespace POSSUM
{
    public partial class DirectPrint
    {
        public void PrintHeader(BinaryWriter bw)
        {
            if (Defaults.LogoEnable)
            {
                bw.PrintLogo();
                bw.FeedLines(1);
                bw.NormalFontCenter(_printModel.Header); //Name of Outlet
            }
            else
            {
                bw.NormalFontCenter(_printModel.Header);
            }
        }

        private void PrintPayments(List<Payment> lstPayments, BinaryWriter bw)
        {
            if (_printModel.OrderMaster.Status == OrderStatus.ReturnOrder)
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", "Åter", " "));
            }
            else
            {
                bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Report_Payment, " "));
            }

            foreach (var payment in lstPayments)
            {
                if (payment.CashCollected != 0)
                {
                    string s = string.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.CashCollected, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }
                else
                {
                    string s = string.Format("{0,-31} {1,10}", payment.PaymentRef, Math.Round(payment.PaidAmount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                    bw.NormalFont(s);
                }
            }

            decimal returncash = lstPayments.Where(pmt => pmt.TypeId == 1 || pmt.TypeId == 7 || pmt.TypeId == 4 || pmt.TypeId == 9 || pmt.TypeId == 10 || pmt.TypeId == 11).Sum(tot => tot.CashChange);
            decimal collectedcash = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.CashCollected);
            decimal collectedcard = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.CashCollected);
            decimal tip = lstPayments.Where(pmt => pmt.TypeId == 1).Sum(tot => tot.TipAmount);
            decimal cardtip = lstPayments.Where(pmt => pmt.TypeId == 4).Sum(tot => tot.TipAmount);

            if (returncash > 0)
            {
                bw.FeedLines(1);

                string lblreturnCash = string.Format("{0,-31} {1,10}", UI.CheckOutOrder_Label_CashBack + " " + UI.CheckOutOrder_Method_Cash, ((-1) * returncash).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                bw.NormalFont(lblreturnCash);
            }

            bw.FeedLines(1);
        }

        private void PrintCopyInfo(BinaryWriter bw)
        {
            bw.FeedLines(1);
            bw.LargeTextCenter(UI.Report_ReceiptCopy); //Kopia order
        }

        private void PrintTable(BinaryWriter bw)
        {
            if (_printModel.OrderMaster.TableId > 0)
            {
                int floor = 1;

                if (_printModel.OrderMaster.SelectedTable != null)
                {
                    floor = _printModel.OrderMaster.SelectedTable.FloorId;
                }

                if (floor > 1)
                {
                    _printModel.OrderMaster.TableName = _printModel.OrderMaster.TableName + " (" + floor + ")";
                }

                string TableNo = string.Format("{0,-14} {1,-20}", UI.OpenOrder_TableButton, _printModel.OrderMaster.TableName);

                bw.NormalFontBoldCenter(TableNo);
            }
        }

        private void PrintFooter(BinaryWriter bw)
        {
            bw.FeedLines(1);
            if (_printModel.OrderMaster.Status == OrderStatus.ReturnOrder)
            {
                string endTotal = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalReturnBill + " (" + Defaults.CurrencyName + ")", _printModel.GrandTotal.ToString("N", (CultureInfo) Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);
            }
            else
            {
                string endTotal = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_Label_TotalBill + " (" + Defaults.CurrencyName + ")", _printModel.GrandTotal.ToString("N", (CultureInfo) Defaults.UICultureInfo));
                bw.NormalFontBold(endTotal);
            }
            if (_printModel.OrderMaster.RoundedAmount != 0)
            {
                string roundedAmount = string.Format("{0,-32}{1,10}", UI.CheckOutOrder_RoundOff + ":", _printModel.OrderMaster.RoundedAmount.ToString("N", (CultureInfo) Defaults.UICultureInfo));
                bw.NormalFontBold(roundedAmount);
            }
        }

        private void PrintCashier(BinaryWriter bw)
        {
            string cashier = string.Format("{0,-15}{1,-20}", UI.Report_Cashier, Defaults.User.UserName);
            bw.NormalFont(cashier);
            bw.NormalFont(string.Format("{0,-14} {1,-20}", UI.Global_Terminal, Defaults.Terminal.UniqueIdentification));
        }

        private void PrintControlUnit(BinaryWriter bw)
        {
            bw.NormalFont(string.Format("{0,-14} {1,-20}", UI.Global_ControlUnit, _printModel.ControlUnitName));
        }

        private void PrintVat(List<VATModel> vatDetails, BinaryWriter bw)
        {
            string vatheader = string.Format("{0,-13} {1,-11} {2,-10} {3,2}", UI.Global_VAT + "%", UI.Global_VAT, UI.Report_TotalNet, UI.Global_Total);
            bw.NormalFont(vatheader);

            foreach (var vat in vatDetails)
            {
                string s = string.Format("{0,5} {1,12} {2,12} {3,10}", vat.VATPercent.ToString("N", (CultureInfo) Defaults.UICultureInfo), Math.Round(vat.VATTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), Math.Round(vat.NetAmount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), Math.Round(vat.Total, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                bw.NormalFont(s);
            }
        }

        private void PrintReceiptInfo(BinaryWriter bw)
        {
            string receiptNo = UI.Global_Receipt + " " + _printModel.ReceiptNo;
            bw.NormalFontBoldCenter(receiptNo);
            var printDate = _printModel.OrderMaster.InvoiceDate.HasValue ? Convert.ToDateTime(_printModel.OrderMaster.InvoiceDate) : _orderMaster.CreationDate;
            bw.NormalFontCenter(printDate.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private void PrintTrainingMode(BinaryWriter bw)
        {
            bw.LargeTextCenter(UI.Report_Trainingmode); //Training Mode
        }

        private void PrintAddress(BinaryWriter bw)
        {
            string address = Defaults.Outlet.Address1;

            if (!string.IsNullOrEmpty(address))
            {
                if (address.Length > 26)
                {
                    string adrs = address.Substring(0, 26);
                    bw.NormalFontCenter(adrs);
                    address = "";
                    int counter = 1;
                    var builder = new System.Text.StringBuilder();
                    builder.Append(address);

                    foreach (var s in address.Skip(26))
                    {
                        if (counter % 20 == 0)
                        {
                            bw.NormalFontCenter(address);
                            address = "";
                        }

                        builder.Append(s);
                        counter++;
                    }

                    address = builder.ToString();
                    if (!string.IsNullOrEmpty(address))
                    {
                        bw.NormalFontCenter(address);
                    }
                }
                else
                {
                    bw.NormalFontCenter(address);
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
                    bw.FeedLine();
                }
            }
        }

        private void PrintOrderType(OrderType type, BinaryWriter bw)
        {
            if (type == OrderType.Return)
            {
                bw.LargeTextCenter(UI.Global_Return); //return order
            }
            if (_printModel.OrderMaster.Type == OrderType.TakeAway || _printModel.OrderMaster.Type == OrderType.TableTakeAwayOrder)
            {
                bw.LargeTextCenter(UI.Sales_TakeAwayButton); //takeaway order
            }
        }

        private void PrintItems(List<OrderLine> items, BinaryWriter bw)
        {
            bw.NormalFontBold(string.Format("{0,-10}{1,-32}", UI.Global_Articles, " "));

            foreach (var item in items)
            {
                if (item.ItemName.Length > 17)
                {
                    if (item.Product.ReceiptMethod == ReceiptMethod.Show_Product_As_Group)
                    {
                        if (item.Quantity == 1)
                        {
                            string s = string.Format("{0,-20} {1,10} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }
                        else
                        {
                            string s = string.Format("{0,-20} {1,10} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                            bw.NormalFont(s);
                        }

                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                            {
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            }

                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        bw.NormalFont(item.ItemName);

                        string s = string.Format("{0,-17} {1,13} {2,10}", " ", item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));

                        bw.NormalFont(s);

                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;

                            if (item.DiscountType == DiscountType.Offer)
                            {
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            }

                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                }
                else
                {
                    if (item.Quantity != 1)
                    {
                        string s = string.Format("{0,-17} {1,13} {2,10}", item.ItemName, item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                            {
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            }

                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));

                            bw.NormalFont(dis);
                        }
                    }
                    else
                    {
                        string s = string.Format("{0,-17} {1,13} {2,10}", item.ItemName, "  ", Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                        if (item.ItemDiscount > 0)
                        {
                            string discountDesc = UI.Sales_Discount;
                            if (item.DiscountType == DiscountType.Offer)
                                discountDesc = UI.Global_Campaign + " " + discountDesc;
                            string dis = string.Format("{0,-21}{1,10} {2,10} ", "  " + discountDesc, " ", Math.Round((-1) * item.ItemDiscount, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                            bw.NormalFont(dis);
                        }
                    }
                }

                if (item.HasIngredients)
                {
                    PrintIngredients(item.IngredientItems, bw);
                }

                if (item.ItemDetails!=null && item.ItemDetails.Count>0)
                {
                    PrintGroup(item.ItemDetails, bw);
                }
            }
        }

        private void PrintGroup(List<OrderLine> orderItemDetails, BinaryWriter bw)
        {
            if (orderItemDetails != null)
            {
                foreach (var item in orderItemDetails)
                {
                    if (item.ItemName.Length >= 17)
                    {
                        if (item.Quantity != 1)
                        {
                            string qty = "(" + item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                            string s = String.Format("{0,-20} {1,10} {2,-8}", " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), qty);
                            bw.NormalFont(s);
                        }
                        else
                        {
                            string s = String.Format("{0,-20} {1,10} {2,8}", " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), " ");
                            bw.NormalFont(s);
                        }
                    }
                    else
                    {
                        if (item.Quantity != 1)
                        {
                            string qty = "(" + item.CartQty + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                            string s = string.Format("{0,-17} {1,13} {2,-8}", " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), qty);
                            bw.NormalFont(s);
                        }
                        else
                        {
                            string s = string.Format("{0,-17} {1,13} {2,8}", " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo), " ");
                            bw.NormalFont(s);
                        }
                    }
                }
            }
        }

        private void PrintIngredients(List<OrderLine> ingredients, BinaryWriter bw)
        {
            foreach (var item in ingredients.OrderBy(o => o.IngredientMode).ToList())
            {
            
                if (item.ItemName.Length >= 17)
                {
                    if (item.Quantity != 1)
                    {
                        string s = string.Format("{0,-30}  {2,10}", " " + item.IngredientMode + " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                    }
                    else
                    {
                        string s = string.Format("{0,-30} {1,10} ", " " + item.IngredientMode + " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                    }
                }
                else
                {
                    if (item.Quantity != 1)
                    {
                        string qty = "(" + item.Quantity + "x" + item.UnitPrice.ToString("N", (CultureInfo)Defaults.UICultureInfo) + ")";
                        string s = string.Format("{0,-23}{1,8} {2,10}", "  " + item.IngredientMode + " " + item.ItemName, Math.Round(item.Quantity, 0), Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                    }
                    else
                    {
                        string s = string.Format("{0,-31} {1,10}", "  " + item.IngredientMode + " " + item.ItemName, Math.Round(item.GrossTotal, 2).ToString("N", (CultureInfo) Defaults.UICultureInfo));
                        bw.NormalFont(s);
                    }
                }
            }
        }
    }
}