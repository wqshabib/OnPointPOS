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
	public partial class ProductReportForm : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				
				string catId = Request.QueryString["catId"];
				string rptName = Request.QueryString["rptName"];
				string isFiltered = Request.QueryString["isFiltered"];
				if (isFiltered == "1")
				{
					SaleDataProvider provider = new SaleDataProvider();
					int cateId = 0;
					int.TryParse(catId, out cateId);
					var data = provider.GetItemStock(GetConnection, cateId);
					var lst = Session["FilterProducts"] as List<Guid>;
					if (lst!=null && lst.Count > 0)
					{
						data = data.Where(a => lst.Contains(a.Id)).ToList();
					}
					MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("itemStock", data));
					MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ItemStock_MatrixStyle.rdlc");
				}
				else
				{
					LoadReport(catId, rptName);
				}
			}
		}

		private void LoadReport(string catId, string reportName)
		{
			try
			{
				SaleDataProvider provider = new SaleDataProvider();

				 if (reportName == "rptproductprice")
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
                else if (reportName == "rptProductList")
                {

                    var data = provider.GetProductList(GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("productList", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ProductList.rdlc");

                }
                 else if(reportName == "rptOneProductBarcode")
                {
                    Guid itemId;
                    Guid.TryParse(catId, out itemId);
                    var data = provider.GetItemStockById(GetConnection, itemId);
                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("itemStock", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ItemStock_MatrixStyleSingle.rdlc");
                }
                else if (reportName == "rptProductBarcode")
                {
                    int cateId = 0;
                    int.TryParse(catId, out cateId);

                    var data = provider.GetItemStock(GetConnection, cateId);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("itemStock", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ItemStock_MatrixStyle.rdlc");

                }
                else if (reportName == "rptExpiryItems")
                {

                    var data = provider.GetExpiryItems(GetConnection);

                    MyReportViewer.LocalReport.DataSources.Add(new ReportDataSource("dataset1", data));

                    MyReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/ExpiryItems.rdlc");

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