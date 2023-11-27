using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BarcodeLib;
using POSSUM.Model;
using POSSUM.Handlers;
using POSSUM.Res;
using Color = System.Drawing.Color;

namespace POSSUM.Views.PrintOrder
{
    public partial class PrintVoucherWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        ReportType Type;
        Barcode b = new Barcode();
        VoucherViewModel model;
        VoucherViewModel printModel = new VoucherViewModel();
        public PrintVoucherWindow()
        {
            InitializeComponent();
            this.DataContext = printModel;
        }
        public void PrintVoucher(VoucherViewModel model, ReportType type)
        {
            this.model = model;
            this.Type = type;
            GenerateReport();
            //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            //worker.RunWorkerAsync();

        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            GenerateReport();
            Dispatcher.BeginInvoke((Action)(() =>
            {
                SendPrint();
            }));

            // run all background tasks here
        }
        private void GenerateReport()
        {

            var text = Regex.Replace(model.Description, "<.*?>", string.Empty);
            printModel.Description = text;
            //if (File.Exists(Defaults.CompanyInfo.Logo))
            //{
            //    var uriSource = new Uri(Defaults.CompanyInfo.Logo);
            //    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Source = new BitmapImage(uriSource)));
            //}
            //else
            //{
            //    logoImage.Dispatcher.BeginInvoke(new Action(() => logoImage.Visibility = Visibility.Collapsed));
            //    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Text = Defaults.Outlet.Name));
            //    lblHeader.Dispatcher.BeginInvoke(new Action(() => lblHeader.Visibility = Visibility.Visible));
            //}
            try
            {
                //seamless product image

                if (!string.IsNullOrEmpty(model.ImagePath))
                {
                    var uriSource = new Uri(model.ImagePath);
                  //  printModel.ProductLogo = new BitmapImage(uriSource);
                    // prodLogoImage.Dispatcher.BeginInvoke(new Action(() => prodLogoImage.Source = new BitmapImage(uriSource)));
                }
                else
                {
                    prodLogoImage.Dispatcher.BeginInvoke(new Action(() => prodLogoImage.Visibility = Visibility.Collapsed));
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
            //barcode image print
            try
            {
                if (string.IsNullOrEmpty(model.ProductEAN))
                {
                    //  throw new Exception("Invalid barcode");
                }
                int W = 280;
                int H = 75;
                b.Alignment = BarcodeLib.AlignmentPositions.CENTER;
                b.IncludeLabel = true;
                BarcodeLib.TYPE eanType = BarcodeLib.TYPE.UNSPECIFIED;
                b.EncodedType = eanType = model.ProductEAN.Length == 13 ? TYPE.EAN13 : TYPE.EAN8;
                b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;
                var image = b.Encode(eanType, model.ProductEAN, System.Drawing.Color.Black, System.Drawing.Color.White, W, H);

                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                System.Windows.Media.Imaging.BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(ms.ToArray());
                bImg.EndInit();
                //  barcodeImage.Source = bImg;
              //  printModel.BarCodeImage = bImg;
                // barcodeImage.Dispatcher.BeginInvoke(new Action(() => barcodeImage.Source = bImg));
                this.Dispatcher.BeginInvoke(new Action(() => this.DataContext = printModel));
                //  txtDescription.Dispatcher.BeginInvoke(new Action(() => txtDescription.Text = text));

            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);

            }


            SendPrint();

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
        public void Print_Click(object sender, RoutedEventArgs e)
        {
            SendPrint();
        }
        private void SendPrint()
        {
            try
            {
                var dlg = new PrintDialog();
             
                string printerName = Utilities.GetPrinterByOutLetId(Defaults.Outlet.Id, "Voucher");
                RenderGridSize();
                var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                paginator.PageSize = new Size(400, gdMain.ActualHeight + 300);
                if (this.Type == ReportType.XReport)
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportXPrinted));
                else if (this.Type == ReportType.ZReport)
                    LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.ReportZPrinted));
                if (string.IsNullOrEmpty(printerName))
                {
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(paginator, "X-Report Print");
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
                    dlg.PrintDocument(paginator, "Print Order");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RenderGridSize()
        {
            gdMain.Measure(new Size(Double.MaxValue, Double.MaxValue));
            Size visualSize = gdMain.DesiredSize;
            gdMain.Arrange(new Rect(new Point(0, 0), visualSize));
            gdMain.UpdateLayout();
        }
    }
    public class VoucherPrintModel : CompanyInfo
    {
        public string ProductEAN { get; set; }
        public string ProductName { get; set; }
        public string BarCode { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Description { get; set; }
        public string Serial { get; set; }
        public string ImagePath { get; set; }
        public string ImageTage { get; set; }
        public ImageSource ProductLogo { get; set; }
        public ImageSource BarCodeImage { get; set; }
    }
}