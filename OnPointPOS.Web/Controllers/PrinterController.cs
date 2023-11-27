using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    public class PrinterController : MyBaseController
    {


        public PrinterController()
        {

        }
        public ActionResult Index()
        {
            try
            {
                using (var db = GetConnection)
                {
                    var printers = db.Printer.ToList();
                    return View(printers);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult Create(int? id)
        {
            Printer model = new Printer();
            if (id != null)
            {

                int printerId = Convert.ToInt16(id);
                using (var db = GetConnection)
                {
                    model = db.Printer.Where(t => t.Id == printerId).FirstOrDefault();
                    if (model == null)
                        model = new Printer();
                }
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(Printer viewModel)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var printerRepo = uof.PrinterRepository;

                    Printer printer = new Printer();
                    bool isEdit = false;
                    if (viewModel.Id != 0)
                    {
                        printer = printerRepo.Single(t => t.Id == viewModel.Id);
                        isEdit = true;

                    }
                    else
                    {
                        try
                        {
                            int maxId = printerRepo.GetAll().Max(p => p.Id);
                            printer.Id = maxId + 1;
                        }
                        catch
                        {
                            printer.Id = 1;
                        }
                    }
                    printer.LocationName = viewModel.LocationName;
                    printer.PrinterName = viewModel.PrinterName;
                    printer.Updated = DateTime.Now;

                    if (isEdit == false)
                    {
                        printerRepo.Add(printer);
                    }
                    uof.Commit();
                }
                msg = "Success" + ":" + Resource.Printer + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePrinter(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var printerRepo = uof.PrinterRepository;
                    var printer = printerRepo.Single(o => o.Id == id);
                    printerRepo.Delete(printer);
                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Terminal + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}