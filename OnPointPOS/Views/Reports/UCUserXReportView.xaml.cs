using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using POSSUM.Model;
using System.Globalization;
using POSSUM.Res;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for UCUserXReportView.xaml
    /// </summary>
    public partial class UCUserXReportView : UserControl
    {
        public UCUserXReportView()
        {
            InitializeComponent();
            FillEmployeeCombo();
            FillEmployeeCombo();
            dtpFrom.Text = DateTime.Now.ToShortDateString();
            dtpTo.Text = DateTime.Now.ToShortDateString();
        }
        private void FillEmployeeCombo()
        {
            //UserRepository repository = new UserRepository();
            //List<ClsUser> lstUser = repository.GetEmployeesByOutletId();
            //cmbEmployee.ItemsSource = lstUser;
            //if (cmbEmployee.Items.Count > 0)
            //    cmbEmployee.SelectedIndex = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                {
                    var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new Size(400, gdMain.ActualHeight + 400);

                    printDialog.PrintDocument(paginator, "User's X Report View");

                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(UI.Message_PrintingProblemTryLater, Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int userId = Convert.ToInt32(cmbEmployee.SelectedValue);
                //string sysFormatName = CultureInfo.CurrentCulture.Name;//.CompareInfo.Name;
                DateTime dtFrom = Convert.ToDateTime(dtpFrom.Text).Date;
                DateTime dtTo = Convert.ToDateTime(dtpTo.Text).AddDays(1);
              //  string DB_DateF = "";
               // string DB_DateT = "";

              //  DB_DateF = Utilities.GetMySQLDateTimeFormat(dtFrom);//.ToString();
             //   DB_DateT = Utilities.GetMySQLDateTimeFormat(dtTo);//.ToString();
                //if (sysFormatName == "en-US")
                //{
                //    DB_DateF = dtFrom.ToString();
                //    DB_DateT = dtTo.ToString();
                //}
                //else
                //{
                //    DB_DateF = (dtFrom).ToString(new CultureInfo("en-US"));
                //    DB_DateT = (dtTo).ToString(new CultureInfo("en-US"));

                //}

                //UserRepository repository = new UserRepository();

                //List<UserReportview> lstViews = repository.UserXReportView(DB_DateF, DB_DateT, userId);
                //LBVewUserLog.ItemsSource = lstViews;
                //lblTotalView.Text = lstViews.Count.ToString();
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
