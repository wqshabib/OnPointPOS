using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace POSSUM.Web.Controllers
{
    [Authorize]
    public class PricePolicyController : MyBaseController
    {

        public PricePolicyController()
        {

        }
        // GET: Campaign
        public ActionResult Index()
        {
            using (var db = GetConnection)
            {
                var list = db.PricePolicy.ToList();
                return View(list);
            }
        }


        public ActionResult Create(int id)
        {
            PricePolicy model = new PricePolicy();
            if (id > 0)
            {
                using (var db = GetConnection)
                {

                    model = db.PricePolicy.FirstOrDefault(c => c.Id == id);
                    if (model == null)
                        model = new PricePolicy();
                }
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(PricePolicy viewModel)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var policyRepo = uof.PricePolicyRepository;

                    PricePolicy policy = new PricePolicy();

                    if (viewModel.Id != 0)
                    {
                        policy = policyRepo.Single(t => t.Id == viewModel.Id);
                    }
                    policy.DiscountAmount = viewModel.DiscountAmount;
                    policy.BuyLimit = viewModel.BuyLimit;
                    policy.Description = viewModel.Description;
                    policy.Updated = DateTime.Now;

                    policyRepo.AddOrUpdate(policy);


                    uof.Commit();
                }
                msg = "Success" + ":" + Resource.Price + " "+Resource.Policy+" " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePloicy(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var policyRepo = uof.PricePolicyRepository;
                    var policy = policyRepo.Single(o => o.Id == id);
                    policyRepo.Delete(policy);
                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Policy + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}