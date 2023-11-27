using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Category : BaseEntity
    {
        public Category()
        {
            Active = true;
            Deleted = false;
            Created = DateTime.Now;
            IconId = 0;
        }

        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        public int Parant { get; set; }
        public int CategoryLevel { get; set; }
        public CategoryType Type { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string ColorCode { get; set; }
        public int SortOrder { get; set; }
        public int? IconId { get; set; }
        public int ReportOrder { get; set; }

        //new field ImageURL Description for food order system
        public string ImageURL { get; set; }
        public string Description { get; set; }

        [NotMapped]
        public List<Category> SubCategories { get; set; }
        [NotMapped]
        public ObservableCollection<SubCategoryModel> Children { get; set; }
        [NotMapped]
        public List<Product> Products { get; set; }
    }
    public class Categorytype
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
    }
    public enum CategoryType
    {
        General = 0,
        Root = 1,
        Ingredient = 2
    }
    public class SubCategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}