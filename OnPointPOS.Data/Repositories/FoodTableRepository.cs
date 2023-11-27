using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class FoodTableRepository : GenericRepository<FoodTable>, IDisposable
    {
        private readonly ApplicationDbContext context;

        public FoodTableRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public FoodTableRepository() : base(new ApplicationDbContext())
        {
        }
        public List<Floor> LoadFloor()
        {
            List<Floor> floors = context.Floor.ToList();
            
            return floors;
        }

        public List<Order> GetOpenOrderByTable(int tableId)
        {
           
                return context.OrderMaster.Where(c => c.TableId == tableId && c.Status != OrderStatus.CleanCashFailed && c.Status != OrderStatus.CleanCashReturnOrderFailed && c.Status != OrderStatus.Completed && c.Status != OrderStatus.OrderCancelled).ToList();
            
        }

        public List<FoodTable> GetTables(int floorId,string tblName)
        {
            

                var orders = context.OrderMaster.Where(o => o.Status == OrderStatus.AssignedKitchenBar).ToList();
                var tables = context.FoodTable.Where(t => t.Floor.Id == floorId && t.Id > 0).ToList();

                var models = new List<FoodTable>();

                foreach (var table in tables)
                {
                  
                    var model = new FoodTable();
                model.Height = table.Height;
                model.Width = table.Width;
                if (orders.Any(o => o.TableId == table.Id && o.OrderTotal>0))
                    {
                    if (table.Name.Length < 4)
                        model.Name = tblName + " " + table.Name;
                    else
                        model.Name = table.Name;
                        model.ColorCode = "#FFFFC0CB";
                        model.OrderCount = orders.Count;
                  
                    string customerName = orders.First(or => or.TableId == table.Id).Comments;

                        //if (!string.IsNullOrEmpty(customerName))
                        //{
                        //    model.Name = customerName;
                        //}
                        //else
                        //    model.Name ="Board "+ table.Name;
                    }
                    else
                    {
                        model.ColorCode = "#FFFFFFFF";
                        model.OrderCount = 0;
                        model.Name = tblName + " " + table.Name;
                    }
                    model.Id = table.Id;

                    model.PositionX = table.PositionX;
                    model.PositionY = table.PositionY;
                    model.ImageUrl = table.ImageUrl;
                    model.Chairs = table.Chairs;
                    models.Add(model);
                }
                return models;
            
        }

        public bool UpdateLocation(FoodTable table)
        {
           

                var item = context.FoodTable.FirstOrDefault(t => t.Id == table.Id);
                if (item != null)
                {
                    item.PositionX = table.PositionX;
                    item.PositionY = table.PositionY;
                }
            context.SaveChanges();
            return true;
            
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
