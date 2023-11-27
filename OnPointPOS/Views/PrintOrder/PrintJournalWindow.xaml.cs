using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;

namespace POSSUM.Views.PrintOrder
{
    public partial class PrintJournalWindow : Window
    {
        public PrintJournalWindow()
        {
            InitializeComponent();
            dtpFrom.Text = DateTime.Now.ToLongDateString();
            dtpTo.Text = DateTime.Now.ToLongDateString();
            if (File.Exists(Defaults.CompanyInfo.Logo))
            {
                var uriSource = new Uri(Defaults.CompanyInfo.Logo);
                logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));
            }
            else
            {
                logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Visibility = Visibility.Collapsed));
                lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Text = Defaults.Outlet.Name));
                lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Visibility = Visibility.Visible));
            }
            InfoCanvas.DataContext = Defaults.CompanyInfo;
            txtOrgNo.Dispatcher.BeginInvoke(new Action(() => txtOrgNo.Text = Defaults.CompanyInfo.OrgNo));
            txtAddress.Dispatcher.BeginInvoke(new Action(() => txtAddress.Text = Defaults.CompanyInfo.Address));
            txtPhoneno.Dispatcher.BeginInvoke(new Action(() => txtPhoneno.Text = Defaults.CompanyInfo.Phone));
            txtDescription.Text = "Journal Log on " + DateTime.Now.ToShortDateString();
            PrintReport();

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        List<Journal> journalLogs = new List<Journal>();
        public void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //DirectPrint directPrint = new DirectPrint();
                //directPrint.PrintJournal(journalLogs);

                var dlg = new PrintDialog();

                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(350, gdMain.ActualHeight + 200);

                if (string.IsNullOrEmpty(printerName))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "Journal Print");
                        this.Close();
                    }
                }
                else
                {

                    PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                    PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                    var data = printers.Where(pn => pn.Name == printerName);
                    if (data.Count() > 0)
                    {
                        prntque = printers.Single(pn => pn.Name == printerName);
                    }
                    else // Fall back to XPS Document Writer
                    {
                        printerName = "Microsoft XPS Document Writer";
                        printers = new PrintServer().GetPrintQueues();
                        prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                        data = printers.Where(pn => pn.Name == printerName);
                        if (data.Count() > 0)
                        {
                            prntque = printers.Single(pn => pn.Name == printerName);
                        }
                    }

                    dlg.PrintQueue = prntque;
                    dlg.PrintDocument(paginator, "Journal");

                    this.Close();
                }

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PrintReport()
        {
            DateTime dtFrom = Convert.ToDateTime(dtpFrom.Text).Date;
            DateTime dtTo = Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
            var journals = new List<Journal>();
            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    journals = ReportGenerator.PrintJournalReport(dtFrom, dtTo);

                    progressDialog.Closed += (arg, ew) =>
                    {
                        progressDialog = null;
                    };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressDialog.Close();

                    }));

                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();
            //  FlowDocumentReader.Visibility = Visibility.Visible;
            txtDescription.Text = "Journal Log from " + dtFrom.ToShortDateString() + " To:" + dtTo.ToShortDateString();
            journalLogs = journals;
            LBLogs.ItemsSource = journals;
            dgJournals.ItemsSource = journals;
        }
        private void RenderGridSize()
        {
            gdMain.Measure(new Size(Double.MaxValue, Double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            PrintReport();
        }

        private void btnDiscountClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //if (dgJournals.Items.Count > 0)
                //    DataGridExcelTools.ExportToExcel(dgJournals);
            }
            catch
            {

            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(350, gdMain.ActualHeight + 100);
                PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();

                printerName = "Microsoft XPS Document Writer";
                printers = new PrintServer().GetPrintQueues();
                prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                var data = printers.Where(pn => pn.Name == printerName);
                if (data.Count() > 0)
                {
                    prntque = printers.Single(pn => pn.Name == printerName);
                }


                dlg.PrintQueue = prntque;
                dlg.PrintDocument(paginator, "Journal");

                this.Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Bill");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(350, gdMain.ActualHeight + 100);
                PrintQueueCollection printers = new PrintServer().GetPrintQueues();
                PrintQueue prntque = new PrintServer().GetPrintQueues().FirstOrDefault();

                printerName = "Microsoft XPS Document Writer"; 
                //printerName = "Microsoft Print to PDF"; 
                printers = new PrintServer().GetPrintQueues();
                prntque = new PrintServer().GetPrintQueues().FirstOrDefault();
                var data = printers.Where(pn => pn.Name == printerName);
                if (data.Count() > 0)
                {
                    prntque = printers.Single(pn => pn.Name == printerName);
                }
                dlg.PrintQueue = prntque;
                dlg.PrintDocument(paginator, "Journal");

                this.Close();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}