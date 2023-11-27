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
    // [Authorize(Roles = "Admin")]
    [Authorize]
    public class FoodTableController : MyBaseController
    {
        private readonly IUnitOfWork uof;
        public FoodTableController()
        {
            // uof = PosState.GetInstance().CreateUnitOfWork();
        }
        public ActionResult Index()
        {
            List<FoodTableViewModel> terminals = new List<FoodTableViewModel>();
            try
            {

                using (var db = GetConnection)

                {
                    terminals = (from t in db.FoodTable
                                 select new FoodTableViewModel
                                 {
                                     Id = t.Id,
                                     Floor = t.Floor,
                                     FloorId = t.FloorId,
                                     Name = t.Name,
                                      PositionX = t.PositionX,
                                     PositionY = t.PositionY, 
                                     Status=t.Status

                                 }).ToList();
                }
                return View(terminals);





            }




            catch (Exception ex)
            {

                throw;
            }
        }
        public ActionResult Create(string id)
        {
            FoodTableViewModel model = new FoodTableViewModel();

            using (var db = GetConnection)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    int tableId =Convert.ToInt16(id);
                    model = db.FoodTable.Where(t => t.Id == tableId).Select(t => new FoodTableViewModel
                    {
                        Id = t.Id,
                        Floor = t.Floor,
                        FloorId = t.FloorId,
                        Name = t.Name,
                        PositionX = t.PositionX,
                        PositionY = t.PositionY,
                        Height=t.Height,
                        Width=t.Width,
                        Status = t.Status
                    }).FirstOrDefault();
                }
                if (model == null)
                    model = new FoodTableViewModel();


                var floors = db.Floor.ToList();
                if (floors == null)
                    floors = new List<Floor>();
                floors.Add(new Floor { Id = 0, Name = "Select Floor" });
                model.Floors = floors.OrderBy(o => o.Id).Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name }).ToList();

            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(FoodTableViewModel viewModel)
        {
            string msg = "";
            try
            {
               
                using (var uof = new UnitOfWork(GetConnection))
                {

                    var foodTableRepo = uof.FoodTableRepository;
                  
                    FoodTable table = new FoodTable();

                    if (viewModel.Id != 0)
                    {
                        table = foodTableRepo.Single(t => t.Id == viewModel.Id);
                       
                    }
                    else
                    {
                        int lastId = 0;
                        try
                        {
                            lastId =(int) foodTableRepo.Max(t => t.Id)+1;
                        }
                        catch (Exception)
                        {

                            lastId = 1;
                        }
                        table.Id = lastId;
                        viewModel.Id = table.Id;
                    }
                    table.Name = viewModel.Name;
                    table.FloorId = viewModel.FloorId; 
                    table.Status = viewModel.Status;
                    table.PositionX = viewModel.PositionX;
                    table.PositionY = viewModel.PositionY;
                    table.Width = viewModel.Width;
                    table.Height = viewModel.Height;
                    table.Updated = DateTime.Now;

                    foodTableRepo.AddOrUpdate(table);

                    uof.Commit();

                }
               
                msg = "Success" + ":" + Resource.Table + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
     
        [HttpPost]
        public ActionResult DeleteTable(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var terminalRepo = uof.FoodTableRepository;
                    var terminal = terminalRepo.Single(o => o.Id == id);
                    terminal.Status = TableStatus.Available;
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