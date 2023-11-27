using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;
using POSSUM.Data;

namespace POSSUM.Views.PrintOrder
{
    /// <summary>
    /// Interaction logic for PrintOrderWindow.xaml
    /// </summary>
    public partial class PrintOrderWindow : Window
    {
        private Order currentorderMaster;

        public PrintOrderWindow()
        {
            InitializeComponent();
        }

        public PrintOrderWindow(Guid orderId)
        {
            InitializeComponent();
            var reRetailitory = new OrderRepository(PosState.GetInstance().Context);
            currentorderMaster = reRetailitory.GetOrderMasterDetailById(orderId);
            txtOrderNo.Text = currentorderMaster.OrderNoOfDay;
            txtOrderDate.Text = currentorderMaster.CreationDate.ToShortDateString() + "  " +
                                currentorderMaster.CreationDate.ToShortTimeString();
            txtOrderBy.Text = Defaults.User.UserName;

            txtOrderComment.Text = currentorderMaster.OrderComments;
            if (currentorderMaster.Type == OrderType.Return)
                txtCancel.Text = "**** " + UI.Global_Return + " ****";
            txtComment.Text = string.IsNullOrEmpty(currentorderMaster.Comments) ? "" : currentorderMaster.Comments;
            var lstDetail = FormatOrderList(currentorderMaster.OrderLines.ToList(), currentorderMaster.Type);
            lvItems.ItemsSource = lstDetail;
        }

        public void PrintOrder(Order orderMaster, List<OrderLine> lstDetail, bool isKitchen, int printerId)
        {
            //int Direction = 1;
            if (orderMaster.TableId > 0)
            {
                txtTableNo.Text = UI.OpenOrder_TableButton + " " + orderMaster.TableId;
            }
            else
            {
                txtTableNo.Visibility = Visibility.Collapsed;
            }
            if (orderMaster.Type == OrderType.Return)
            {
                txtCancel.Text = "**** " + UI.Global_Return + " ****";
                txtCancel.Visibility = Visibility.Visible;
            }
            else if (orderMaster.Type == OrderType.TakeAway)
            {
                txtCancel.Text = "**** TAKE AWAY ****";
                txtCancel.Visibility = Visibility.Visible;
            }
            else if (orderMaster.Type == OrderType.TakeAwayReturn)
            {
                txtCancel.Text = "**** RETUR TAKE AWAY ****";
                txtCancel.Visibility = Visibility.Visible;
            }
            lstDetail = FormatOrderList(lstDetail, orderMaster.Type);
            currentorderMaster = orderMaster;
            InvoiceHandler ordRepo = new InvoiceHandler();
            txtOrderNo.Text = orderMaster.OrderNoOfDay;
            int dailyBongCounter = 0;
            int bongNo = ordRepo.GetLastBongNo(out dailyBongCounter);
            txtShiftOrderNo.Text = "BONG#" + bongNo;
            //ordRepo.UpdateBongNo(currentorderMaster.Id, dailyBongCounter);
            txtOrderDate.Text = orderMaster.CreationDate.ToShortDateString() + "  " +
                                orderMaster.CreationDate.ToShortTimeString();
            txtOrderBy.Text = Defaults.User.UserName;

            if (isKitchen)
            {
                txtOrderComment.Text = string.IsNullOrEmpty(orderMaster.OrderComments) ? "" : orderMaster.OrderComments;
                txtComment.Text = string.IsNullOrEmpty(orderMaster.Comments) ? "" : orderMaster.Comments;
                txtComment.Visibility = Visibility.Visible;
            }
            else
            {
                txtOrderComment.Text = "";
                txtComment.Text = "";
            }

            lvItems.ItemsSource = lstDetail;
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptKitchen), currentorderMaster.Id);
            var dlg = new PrintDialog();

            // GM:TODO
            string val = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Kitchen");

            if (string.IsNullOrEmpty(val))
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    RenderGridSize();
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(400, gdMain.ActualHeight + 200);

                    printDialog.PrintDocument(paginator, "Order Print");

                    // printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocument).DocumentPaginator, "Order Print");
                    Close();
                }
            }
            else
            {
                string printerName = val;
                PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                var data = printers.Where(pn => pn.Name == printerName);
                if (data.Count() > 0)
                    prntque = printers.Single(pn => pn.Name == printerName);

                dlg.PrintQueue = prntque;
                RenderGridSize();

                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(400, gdMain.ActualHeight + 200); //lvItems.Items.Count * 20 + 400);

                dlg.PrintDocument(paginator, "Print Order");
            }
        }

        private List<OrderLine> FormatOrderList(List<OrderLine> orderLines, OrderType type)
        {
            var _orderLines = new List<OrderLine>();
            var direction = type == OrderType.Return || type == OrderType.TakeAwayReturn ? -1 : 1;
            foreach (var line in orderLines)
            {
                var orderLine = new OrderLine();
                orderLine = line;
                if (!string.IsNullOrEmpty(line.ItemComments))
                {
                    orderLine.Product.Description = line.ItemName + " (" + line.ItemComments + ")";
                }
                orderLine.Quantity = direction * orderLine.Quantity;
                _orderLines.Add(orderLine);
            }
            return _orderLines;
        }

        private void RenderGridSize()
        {
            gdMain.Measure(new Size(double.MaxValue, double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }

        public void PrintCancelOrder(Order orderMaster, List<OrderLine> lstDetail, int printerId)
        {
            currentorderMaster = orderMaster;
            InvoiceHandler ordRepo = new InvoiceHandler();
            if (currentorderMaster.Type == OrderType.Return)
                txtCancel.Text = "**** " + UI.Global_Return + " ****";
            else if (currentorderMaster.Type == OrderType.TakeAwayReturn)
                txtCancel.Text = "****  RETUR TAKE AWAY ****";
            lstDetail = FormatOrderList(lstDetail, orderMaster.Type);

            txtOrderNo.Text = orderMaster.OrderNoOfDay;
            int dailyBongCounter = 0;
            txtShiftOrderNo.Text = "BONG#" + ordRepo.GetLastBongNo(out dailyBongCounter); //orderMaster.ShiftOrderNo.ToString();//
            //ordRepo.UpdateBongNo(currentorderMaster.Id, dailyBongCounter);
            txtCancel.Text = "---- MAKULERAD ----";
            txtOrderDate.Text = orderMaster.CreationDate.ToShortDateString() + "  " +
                                orderMaster.CreationDate.ToShortTimeString();
            txtOrderBy.Text = Defaults.User.UserName; //+ " - " + orderMaster.TableName;

            txtOrderComment.Text = string.IsNullOrEmpty(orderMaster.OrderComments) ? "" : orderMaster.OrderComments;
            txtComment.Text = string.IsNullOrEmpty(orderMaster.Comments) ? "" : orderMaster.Comments;

            lvItems.ItemsSource = lstDetail;
            LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReceiptKitchen), currentorderMaster.Id);
            var dlg = new PrintDialog();

            // GM:TODO
            string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Kitchen");

            if (string.IsNullOrEmpty(printerName))
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    RenderGridSize();
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(400, gdMain.ActualHeight + 200);

                    printDialog.PrintDocument(paginator, "Order Print");

                    // printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocument).DocumentPaginator, "Order Print");
                    Close();
                }
            }
            else
            {
                PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                var data = printers.Where(pn => pn.Name == printerName);
                if (data.Count() > 0)
                    prntque = printers.Single(pn => pn.Name == printerName);

                dlg.PrintQueue = prntque;
                RenderGridSize();

                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(400, gdMain.ActualHeight + 200); //lvItems.Items.Count * 20 + 400);

                dlg.PrintDocument(paginator, "Print Order");
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    InvoiceHandler ordRepo = new InvoiceHandler();
                    //int dailyBongCounter = 0;
                    //ordRepo.UpdateBongNo(currentorderMaster.Id, dailyBongCounter);

                    //printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocument).DocumentPaginator, "Order Print");

                    RenderGridSize();
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(400, gdMain.ActualHeight + 200);

                    printDialog.PrintDocument(paginator, "Order Print");
                    Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}