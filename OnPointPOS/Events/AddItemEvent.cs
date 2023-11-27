using POSSUM.Model;
using System.Collections.Generic;

namespace POSSUM.Events
{
    public delegate void AddItemEventHandler(object sender, List<OrderLine> orderLines);  
}
