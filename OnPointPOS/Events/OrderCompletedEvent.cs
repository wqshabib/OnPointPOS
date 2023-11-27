using System;

namespace POSSUM.Events
{
    public delegate void OrderCompletedEventHandler(object sender, Guid orderId);
    public delegate void NewOrderEventHandler(object sender, string msg);
    public delegate void DallasEventHandler(object sender, string msg);
    public delegate void InsertedAmountEventHandler(object sender, int amout);
    public delegate void CGWarningEventHandler(object sender, string warning);
}
