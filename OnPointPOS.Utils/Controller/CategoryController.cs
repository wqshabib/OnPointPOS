using POSSUM.Data;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class CategoryController
    {
        private string connectionString = "";
        public CategoryController(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public List<Category> GetSearchByName(string searchToken)
        {

            List<Category> result = new List<Category>();
          

            using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
            {
                var catRepo = uof.CategoryRepository;


                if (!string.IsNullOrEmpty(searchToken))
                {
                    result = catRepo.GetAll().Where(c => c.Name.ToLower().Contains(searchToken.ToLower())).Select(c => new Category
                    {
                        Id = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel,
                        Active = c.Active

                    }).ToList();


                }
                else
                {
                    result = catRepo.GetAll().Select(c => new Category
                    {
                        Id = c.Id,
                        Parant = (int)c.Parant,
                        Name = c.Name,
                        CategoryLevel = c.CategoryLevel,
                        Active = c.Active

                    }).ToList();


                }
            }
            return result;
        }

        public void Create(Category viewModel)
        {
           
            using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
            {
                var catRepo = uof.CategoryRepository;

                var parent = catRepo.FirstOrDefault(c => c.Id == viewModel.Parant);
                int level = 1;
                if (parent != null)
                    level = parent.CategoryLevel + 1;

                int lastId = catRepo.GetAll().Max(c => c.Id);
                Category category = new Category
                {
                    Id = lastId + 1,
                    Name = viewModel.Name,
                    Parant = viewModel.Parant,
                    CategoryLevel = level,
                    Active = viewModel.Active,
                    ColorCode = viewModel.ColorCode,
                    IconId = viewModel.IconId,
                    SortOrder = viewModel.SortOrder,
                    Type=viewModel.Type,
                    Deleted = false
                };
                category.Updated = DateTime.Now;
                category.Created = DateTime.Now;

                catRepo.Add(category);
                uof.Commit();

            }

        }


    }
}
