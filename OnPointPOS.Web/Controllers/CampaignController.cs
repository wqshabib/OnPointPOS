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
    public class CampaignController : MyBaseController
    {
        public CampaignController()
        {

        }
        // GET: Campaign
        public ActionResult Index()
        {
            using (var db = GetConnection)
            {
                try
                {
                    var list = db.Campaign.Where(c => !c.IsDeleted).ToList();
                    return View(list);

                }
                catch (Exception ex)
                {
                    return View(new List<Campaign>());
                }
            }
        }


        public ActionResult Create(int id)
        {
            Campaign model = new Campaign();
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now.AddDays(1);
            if (id > 0)
            {
                using (var db = GetConnection)
                {

                    model = db.Campaign.FirstOrDefault(c => c.Id == id);
                    if (model == null)
                    {
                        model = new Campaign();
                        ViewBag.DiscountTypeList = new SelectList(new List<SelectListItem>()
                        {
                            new SelectListItem()
                            {
                                Text = "Percentage",
                                Value = "0"
                            },
                            new SelectListItem()
                            {
                                Text = "Price",
                                Value = "1"
                            }
                        }, "Value", "Text");
                    }
                    else
                    {
                        ViewBag.DiscountTypeList = new SelectList(new List<SelectListItem>()
                        {
                            new SelectListItem()
                            {
                                Text = "Percentage",
                                Value = "0"
                            },
                            new SelectListItem()
                            {
                                Text = "Price",
                                Value = "1"
                            }
                        }, "Value", "Text", model.DiscountType);
                    }
                }
            }
            else
            {
                ViewBag.DiscountTypeList = new SelectList(new List<SelectListItem>()
                        {
                            new SelectListItem()
                            {
                                Text = "Percentage",
                                Value = "0"
                            },
                            new SelectListItem()
                            {
                                Text = "Price",
                                Value = "1"
                            }
                        }, "Value", "Text");
            }

            return PartialView(model);
        }
        [HttpPost]
        public ActionResult Create(Campaign viewModel)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var campaignRepo = uof.CampaignRepository;

                    Campaign campaign = new Campaign();

                    if (viewModel.Id != 0)
                    {
                        campaign = campaignRepo.Single(t => t.Id == viewModel.Id);
                    }
                    else
                    {
                        try
                        {
                            var lastId = campaignRepo.GetAll().Max(i => i.Id);
                            campaign.Id = lastId + 1;
                        }
                        catch
                        {

                            campaign.Id = 1;
                        }

                    }
                    campaign.DiscountType = viewModel.DiscountType;
                    campaign.FreeOffer = viewModel.FreeOffer;
                    campaign.BuyLimit = viewModel.BuyLimit;
                    campaign.Description = viewModel.Description;
                    campaign.StartDate = viewModel.StartDate;
                    campaign.EndDate = Convert.ToDateTime(viewModel.EndDate.Year + "-" + viewModel.EndDate.Month + "-" + viewModel.EndDate.Day + "  11:59:00 PM");
                    campaign.IsDiscount = viewModel.IsDiscount;
                    campaign.DiscountPercentage = viewModel.DiscountPercentage;
                    campaign.LimitDiscountPercentage = viewModel.LimitDiscountPercentage;
                    campaign.OnceOnly = viewModel.OnceOnly;
                    campaign.Updated = DateTime.Now;

                    campaignRepo.AddOrUpdate(campaign);
                    uof.Commit();
                }
                msg = "Success" + ":" + Resource.Campaign + " " + Resource.saved + " " + Resource.successfully;
            }
            catch (Exception ex)
            {
                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteCampaign(int id)
        {
            string msg = "";
            try
            {
                using (var uof = new UnitOfWork(GetConnection))
                {
                    var campaingRepo = uof.CampaignRepository;
                    var productRepo = uof.ItemCampaignRepository;
                    var categoryRepo = uof.CategoryCampaignRepository;
                    var existingProCompaing = productRepo.FirstOrDefault(c => c.CampaignId == id && !c.IsDeleted);
                    var existingCatCompaing = categoryRepo.FirstOrDefault(c => c.CampaignId == id && !c.IsDeleted);
                    if (existingCatCompaing != null || existingProCompaing != null)
                    {
                        msg = Resource.Error + ":" + Resource.Campaign + " already in Used";
                        return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
                    }

                    var campaign = campaingRepo.Single(o => o.Id == id);
                    campaign.IsDeleted = true;
                    campaign.Updated = DateTime.Now;
                    campaingRepo.AddOrUpdate(campaign);
                    uof.Commit();
                }
                msg = Resource.Success + ":" + Resource.Campaign + " " + Resource.Deleted + " " + Resource.successfully;
            }
            catch (Exception ex)
            {

                msg = Resource.Error + ":" + ex.Message;
            }
            return Json(new { Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}