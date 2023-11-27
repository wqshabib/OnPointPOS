using POSSUM.Base;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Presenter.Products
{
    public interface IProductView:IBaseView
    {
        //List<Product> GetProductsByCategory(int categoryId);
        //List<Product> GetAllSpecialProducts();
        //Product GetProductByBarCode(string barcode);
        //Product GetProductByPLU(string pluNO); 
        Product GetProduct();
       

    }
}
