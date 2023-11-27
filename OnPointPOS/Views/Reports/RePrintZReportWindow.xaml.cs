using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Printing;
using POSSUM.Model;
using POSSUM.Handlers;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using POSSUM.Res;
using System.Collections.Generic;
using POSSUM.Views.PrintOrder;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class RePrintZReportWindow : Window
    {
        public RePrintZReportWindow()
        {
            InitializeComponent();
            dtpFrom.Text = Convert.ToDateTime(DateTime.Now.Year + "-" + DateTime.Now.Month + "-01").ToShortDateString();// DateTime.Now.AddDays(-8).ToShortDateString();
            dtpTo.Text = DateTime.Now.ToShortDateString();
            LoadReport();
        }



        private void View_Click(object sender, RoutedEventArgs e)
        {
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                DateTime dtFrom = Convert.ToDateTime(dtpFrom.Text).Date;
                DateTime dtTo = Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");
                ReportGenerator reportGenerator = new ReportGenerator();
                ReportDataGrid.ItemsSource = ReportGenerator.LoadZReports(dtFrom, dtTo);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }

        private void Pint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var report = (Report)(sender as Button).DataContext;
                if (report != null)
                {
                    var reportData = ReportGenerator.GetReport(report.Id, 1, Defaults.User.UserName);
                    PrintReportWindow printReport = new PrintReportWindow(reportData, ReportType.ZReport, report.Id);
                    printReport.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnViewCompleteReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reportGenerator = new ReportGenerator();
                DateTime dtFrom = Convert.ToDateTime(dtpFrom.Text).Date;
                DateTime dtTo = Convert.ToDateTime(dtpTo.Text + "  11:59:00 PM");

                var reportData = reportGenerator.GetReportByDateRange(Defaults.TerminalId,dtFrom, dtTo, Defaults.User.UserName);
                PrintReportWindow printReport = new PrintReportWindow(reportData, ReportType.ZReport, default(Guid));
                printReport.ShowDialog();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
