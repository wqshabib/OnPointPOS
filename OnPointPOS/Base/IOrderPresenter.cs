using POSSUM.Model;


namespace POSSUM.Base
{
    public interface IOrderPrsenter
    {
        Order MasterOrder { get; set; }
        OrderType Type { get; set; }
        FoodTable SelectedTable { get; set; }
    }
}