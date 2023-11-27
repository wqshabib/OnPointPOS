using POSSUM.Data;
using POSSUM.Web.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
namespace POSSUM.Web.Reports
{
    public partial class OrderReportForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                using (ApplicationDbContext dbContext = GetConnection) 
                {
                    SaleDataProvider provider = new SaleDataProvider();
                    string dtFrom = Request.QueryString["dtFrom"];
                    string dtTo = Request.QueryString["dtTo"];
                    string terminal = Request.QueryString["terminal"];
                    Guid guidTerminal = Guid.Parse(terminal);
                    string objDefaultTerminal = "Web Shop";

                    List<OrderViewModel> SaleData = new List<OrderViewModel>();
                    List<PaymentTypeTotalViewModel> PaymentTypeSaleDataTotal = new List<PaymentTypeTotalViewModel>();
                    List<ReportTotalViewModel> SaleDataTotal = new List<ReportTotalViewModel>();
                    List<OrderLineViewModel> ProductsSoldDataTotal = new List<OrderLineViewModel>();

                    DateTime startDate = Convert.ToDateTime(dtFrom).Date;
                    DateTime endDate = Convert.ToDateTime(dtTo + " 23:59:59");
                    ReportParameter rpdtFrom = new ReportParameter("pmDTFrom", startDate.ToString());
                    ReportParameter rpdtTo = new ReportParameter("pmDtTo", endDate.ToString());
                    ReportParameter rpdtTerminal = new ReportParameter("pmTerminalId", terminal.ToString());

                    var dbTerminal = dbContext.Terminal.FirstOrDefault(obj => obj.Id == guidTerminal);

                    if (!string.IsNullOrEmpty(terminal))
                    {
                        if (dbTerminal != null && (!string.IsNullOrEmpty(dbTerminal.Description) && dbTerminal.Description.ToLower().Contains(objDefaultTerminal.ToLower())))
                        {
                            SaleData = provider.GetOrderHistoryWebShopTerminal(dtFrom, dtTo, terminal, GetConnection);
                            SaleDataTotal = provider.GetReportTotalWebShopTerminal(dtFrom, dtTo, terminal, GetConnection);
                            ProductsSoldDataTotal = provider.GetOrderDetailHistoryTerminal(dtFrom, dtTo, terminal, GetConnection);

                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", SaleData));
                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", SaleDataTotal));
                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet3", ProductsSoldDataTotal));
                            MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/OrderHistoryWebShopTerminalReport.rdlc");
                            MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, rpdtTerminal });
                        }
                        else
                        {
                            SaleData = provider.GetOrderHistoryTerminal(dtFrom, dtTo, terminal, GetConnection);
                            SaleDataTotal = provider.GetReportTotalTerminal(dtFrom, dtTo, terminal, GetConnection);
                            ProductsSoldDataTotal = provider.GetOrderDetailHistoryTerminal(dtFrom, dtTo, terminal, GetConnection);
                            PaymentTypeSaleDataTotal = provider.GetPaymentTypeTotalTerminal(dtFrom, dtTo, terminal, GetConnection);

                            if (PaymentTypeSaleDataTotal.Count > 0)
                            {
                                PaymentTypeTotalViewModel typeTotalViewModel = new PaymentTypeTotalViewModel();
                                typeTotalViewModel.PaymentTypeId = 0;
                                typeTotalViewModel.PaymentTypeName = "Total";
                                typeTotalViewModel.PaymentTypeTotal = PaymentTypeSaleDataTotal.Sum(obj => obj.PaymentTypeTotal);

                                PaymentTypeSaleDataTotal.Add(typeTotalViewModel);
                            }

                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", SaleData));
                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet2", PaymentTypeSaleDataTotal));
                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet3", ProductsSoldDataTotal));
                            MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet4", SaleDataTotal));
                            MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/OrderHistoryTerminalReport.rdlc");
                            MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, rpdtTerminal });
                        }
                    }
                    else
                    {
                        SaleData = provider.GetOrderHistory(dtFrom, dtTo, GetConnection);

                        MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", SaleData));
                        MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/OrderHistoryReport.rdlc");
                        MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo });
                    }

                    MyReportViewer.LocalReport.EnableExternalImages = true;
                    MyReportViewer.LocalReport.Refresh();
                }
            }
            catch (Exception ex)
            {
                DataLogger.Exception(ex);
            }
        }

        public ApplicationDbContext GetConnection
        {
            get
            {
                //int multitenant = 0;
                //int.TryParse(ConfigurationManager.AppSettings["Multitenant"], out multitenant);

                string connectionString = "";
                using (MasterData.MasterDbContext db = new MasterData.MasterDbContext())
                {
                    string userID = User.Identity.GetUserId();
                    var user = db.Users.FirstOrDefault(u => u.Id == userID);
                    connectionString = user.Company.ConnectionString;
                }
                return new ApplicationDbContext(connectionString);
            }
        }
    }
}