namespace POSSUM.Model
{
    public enum OrderStatus
    {
        New = 0,
        [LocalizedDescModel(@"OrderStatus_Pending", "POSSUM.Res.UI")] Pending = 1,
        [LocalizedDescModel(@"OrderStatus_OrderSPOAssign", "POSSUM.Res.UI")] OrderSPOAssign = 2,
        [LocalizedDescModel(@"OrderStatus_AssignedKitchenBar", "POSSUM.Res.UI")] AssignedKitchenBar = 3,
        [LocalizedDescModel(@"OrderStatus_AvailabilityPartialComplete", "POSSUM.Res.UI")] AvailabilityPartialComplete = 4,
        [LocalizedDescModel(@"OrderStatus_AvailabilityPartialCompleteSomeItemsUnavailable", "POSSUM.Res.UI")] AvailabilityPartialCompleteSomeItemsUnavailable = 5,

        [LocalizedDescModel(@"OrderStatus_OrderAvailabilityComplete", "POSSUM.Res.UI")] OrderAvailabilityComplete = 6,
        [LocalizedDescModel(@"OrderStatus_OrderPackerAssigned", "POSSUM.Res.UI")] OrderPackerAssigned = 8,
        [LocalizedDescModel(@"OrderStatus_OrderPackerComplete", "POSSUM.Res.UI")] OrderPackerComplete = 9,
        [LocalizedDescModel(@"OrderStatus_Served", "POSSUM.Res.UI")] Served = 10,
        [LocalizedDescModel(@"OrderStatus_ReadyToServe", "POSSUM.Res.UI")] ReadyToServe = 11,
        [LocalizedDescModel(@"OrderStatus_Completed", "POSSUM.Res.UI")] Completed = 13,
        [LocalizedDescModel(@"OrderStatus_OrderCancelled", "POSSUM.Res.UI")] OrderCancelled = 14,
        ReturnOrder = 15,
        Deleted = 16,
        [LocalizedDescModel(@"OrderStatus_CleanCashFailed", "POSSUM.Res.UI")] CleanCashFailed = 17,
        [LocalizedDescModel(@"OrderStatus_CleanCashReturnOrderFailed", "POSSUM.Res.UI")] CleanCashReturnOrderFailed = 18,
        OnHold = 19,
        RejectedViewOrder = 21,
        Rejected = 20
    }
}