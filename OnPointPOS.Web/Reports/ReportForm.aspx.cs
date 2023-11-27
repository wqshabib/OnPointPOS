using POSSUM.Data;
using POSSUM.Web.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
namespace POSSUM.Web.Reports
{
	public partial class ReportForm : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				string dtFrom = Request.QueryString["dtFrom"];
				string dtTo = Request.QueryString["dtTo"];
				string terminalId = Request.QueryString["terminalId"];
				string rptName = Request.QueryString["rptName"];
				LoadReport(dtFrom, dtTo, terminalId, rptName);
			}
		}

		private void LoadReport(string dtFrom, string dtTo, string terminalId, string reportName)
		{
			try
			{
				SaleDataProvider provider = new SaleDataProvider();

				if (reportName == "rptcat")
				{
					var SaleData = provider.GetCategorySale(dtFrom, dtTo, terminalId, GetConnection);
					var catSale = SaleData.CategorySaleDetail;
					var payments = SaleData.PaymentDetails;
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Categories", catSale));
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Payments", payments));
					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/CategorySaleReport.rdlc");
					ReportParameter rpdtFrom = new ReportParameter("pmDTFrom", dtFrom.ToString());
					ReportParameter rpdtTo = new ReportParameter("pmDtTo", dtTo.ToString());
					ReportParameter outlet = new ReportParameter("pmOutletName", SaleData.Outlet);
					ReportParameter terminal = new ReportParameter("pmTerminalCode", SaleData.Terminal);
					MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, outlet, terminal });
				}
				else if (reportName == "rptuser")
				{
					string userId = Request.QueryString["userId"];
					var SaleData = provider.GetCategorySaleByUser(userId, dtFrom, dtTo, GetConnection);
					var catSale = SaleData.CategorySaleDetail;
					var payments = SaleData.PaymentDetails;
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Categories", catSale));
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Payments", payments));
					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/UserSaleReport.rdlc");
					ReportParameter rpdtFrom = new ReportParameter("pmDTFrom", dtFrom.ToString());
					ReportParameter rpdtTo = new ReportParameter("pmDtTo", dtTo.ToString());
					ReportParameter outlet = new ReportParameter("pmOutletName", SaleData.Outlet);
					ReportParameter user = new ReportParameter("pmUserName", SaleData.UserName);
					MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, outlet, user });
				}
				else if (reportName == "rptAccounting")
				{
					var SaleData = provider.GetAccountingSale(dtFrom, dtTo, terminalId, GetConnection);
					var catSale = SaleData.CategorySaleDetail;
					var payments = SaleData.PaymentDetails;
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Categories", catSale));
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("Payments", payments));
					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/AccountingSaleReport.rdlc");
					ReportParameter rpdtFrom = new ReportParameter("pmDTFrom", dtFrom.ToString());
					ReportParameter rpdtTo = new ReportParameter("pmDtTo", dtTo.ToString());
					ReportParameter outlet = new ReportParameter("pmOutletName", SaleData.Outlet);
					ReportParameter terminal = new ReportParameter("pmTerminalCode", SaleData.Terminal);
					MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, outlet, terminal });
				}
				else if (reportName == "rptvatsale")
				{
					string outlet = "";
					string terminal = "";
					var SaleData = provider.GetVATSale(dtFrom, dtTo, terminalId, out outlet, out terminal, GetConnection);

					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("VatSale", SaleData));

					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/VatSaleReport.rdlc");
					ReportParameter rpdtFrom = new ReportParameter("pmDTFrom", dtFrom.ToString());
					ReportParameter rpdtTo = new ReportParameter("pmDtTo", dtTo.ToString());
					ReportParameter outletName = new ReportParameter("pmOutletName", outlet);
					ReportParameter terminalName = new ReportParameter("pmTerminalCode", terminal);
					MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpdtFrom, rpdtTo, outletName, terminalName });
				}
                else if (reportName == "rptproductprice")
                {
                   
                    var SaleData = provider.GetProductPrices( GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("productPrices", SaleData));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProductPrices.rdlc");
                    
                }
                else if (reportName == "rptitemstock")
                {

                    var data = provider.GetItemStock(GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("itemStock", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ItemStock.rdlc");

                }
                else if (reportName == "rptProductBarcode")
                {

                    var data = provider.GetItemStock(GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("itemStock", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ItemStock_MatrixStyle.rdlc");

                }
                else if (reportName == "rptExpiryItems")
                {

                    var data = provider.GetExpiryItems(GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("dataset1", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ExpiryItems.rdlc");

                }
                else

                {
					var userName = User.Identity.Name;
					var reportInfo = provider.GetZReport(dtFrom, dtTo, terminalId, userName, GetConnection);
					var rows = reportInfo.ReportRows;
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("ReportData", rows));
					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ZReport.rdlc");
					StringBuilder b = new StringBuilder();
					string orgNo = string.Format("{0,-10}{1,-32}", Resource.OrgNo + ":", reportInfo.OrgNo);
					b.AppendLine(orgNo);
					string address = reportInfo.Address1;
					if (reportInfo.Address1.Length > 32)
					{
						string adrs = address.Substring(0, 32);

						b.AppendLine(string.Format("{0,-10}{1,-32}", Resource.Address, adrs));
						address = "";
						int counter = 1;
						foreach (var s in reportInfo.Address1.Skip(32))
						{

							if (counter % 20 == 0)
							{

								adrs = string.Format("{0,-10}{1,-32}", "", address);
								b.AppendLine(adrs);
								address = "";
							}
							address = address + s;
							counter++;
						}
						if (!string.IsNullOrEmpty(address))
						{
							adrs = string.Format("{0,-10}{1,-32}", " ", address);
							b.AppendLine(adrs);
						}
					}
					else
					{
						string addres = string.Format("{0,-9}{1,-32}", Resource.Report_Address, address);
						b.AppendLine(addres);
					}
					string city = string.Format("{0,-15}{1,-27}", " ", reportInfo.PostalCode + " " + reportInfo.City);
					b.AppendLine(city);

					string phon = string.Format("{0,-10}{1,-32}", Resource.Phone + ":", reportInfo.Phone);
					b.AppendLine(phon);

					ReportParameter rpmheader = new ReportParameter("header", reportInfo.Header);
					ReportParameter rpmfooter = new ReportParameter("footer", reportInfo.Footer);
					ReportParameter rpminfo = new ReportParameter("companyInfo", b.ToString());

					MyReportViewer.LocalReport.SetParameters(new ReportParameter[] { rpmheader, rpmfooter, rpminfo });

				}



				MyReportViewer.LocalReport.EnableExternalImages = true;
				MyReportViewer.LocalReport.Refresh();
			} 
			catch (Exception ex)
			{

				var path = Server.MapPath("~/Reports/log.txt");
				using (StreamWriter sw = File.AppendText(path))
				{
					sw.WriteLine(ex.Message);
				}


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