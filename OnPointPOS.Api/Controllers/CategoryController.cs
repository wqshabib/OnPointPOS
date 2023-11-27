using Newtonsoft.Json;
using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace POSSUM.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/Category")]

    public class CategoryController : BaseAPIController
    {
        private string connectionString = "";
        bool nonAhenticated = true;
        public CategoryController()
        {
            connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
                nonAhenticated = true;
        }

        [HttpPost]
        [Route("PostCategory")]
        public IHttpActionResult PostCategory(Category category)
        {
            try
            {

                using (ApplicationDbContext db = new ApplicationDbContext(connectionString))//var uof = PosState.GetInstance().CreateUnitOfWork()
                {
                    var categoryStr = JsonConvert.SerializeObject(category);
                    
                    //LogWriter.LogWrite(new Exception("category " + categoryStr));
                    var _category = db.Category.FirstOrDefault(u => u.Id == category.Id);
                    if (_category == null)
                    {
                        db.Category.Add(category);
                    }
                    else
                    {

                        _category.Name = category.Description;
                        _category.Parant = category.Parant;
                        _category.CategoryLevel = category.CategoryLevel;
                        _category.Type = category.Type;
                        _category.Active = category.Active;
                        _category.Deleted = category.Deleted;
                        _category.Created = category.Created;
                        _category.Updated = category.Updated;
                        _category.ColorCode = category.ColorCode;
                        _category.SortOrder = category.SortOrder;
                        _category.IconId = category.IconId;
                        _category.ReportOrder = category.ReportOrder;
                        _category.Description = category.Description;
                        _category.ImageURL = category.ImageURL;
                        _category.Deleted = category.Deleted;
                        db.Entry(_category).State = System.Data.Entity.EntityState.Modified;
                    }
                    //  uof.Commit();
                    db.SaveChanges();
                }
                return StatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }



    }
}
