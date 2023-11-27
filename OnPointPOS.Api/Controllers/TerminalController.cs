using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace POSSUM.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/Terminal")]
    public class TerminalController : BaseAPIController
    {

        private string connectionString = "";
        private bool nonAhenticated = true;

        public TerminalController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }
        [Route("GetAllTerminals")]
        public async Task<List<TerminalViewModel>> GetAllTerminals()
        {
            try
            {

                LogWriter.LogWrite("GetAllTerminals calling and connectionstrings is:  " + connectionString);
                List<TerminalViewModel> lstTerminals = new List<TerminalViewModel>();
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var terminals = db.Terminal.Where(t => !t.IsDeleted).ToList();
                    foreach (var terminal in terminals)
                    {
                        var termnal = new TerminalViewModel
                        {
                            Id = terminal.Id,
                            OutletId = terminal.OutletId,
                            Description = terminal.UniqueIdentification
                        };
                        lstTerminals.Add(termnal);
                    }
                    return lstTerminals;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return new List<TerminalViewModel>();
            }

        }


        [Route("GetAllTerminalsByOutletId")]
        public async Task<List<TerminalViewModel>> GetAllTerminalsByOutletId(string outletId)
        {
            try
            {
                List<TerminalViewModel> lstTerminals = new List<TerminalViewModel>();
                Guid outletID = Guid.Parse(outletId);
                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                {
                    var terminals = db.Terminal.Where(o => o.OutletId == outletID).ToList();
                    foreach (var terminal in terminals)
                    {
                        var termnal = new TerminalViewModel
                        {
                            Id = terminal.Id,
                            OutletId = terminal.OutletId,
                            Description = terminal.UniqueIdentification
                        };
                        lstTerminals.Add(termnal);
                    }
                    return lstTerminals;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                ProductData productData = new ProductData();
                return new List<TerminalViewModel>();
            }

        }
    }
    public class TerminalViewModel
    {
        public Guid Id { get; set; }
        public Guid OutletId { get; set; }
        public string Description { get; set; }
    }
}
