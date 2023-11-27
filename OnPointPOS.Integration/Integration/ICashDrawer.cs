using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration
{
    public interface ICashDrawer
    {
        void Open();
        bool IsOpen();

        void Close();

    }

    public enum CashDrawerType
    {
        DUMMY,
        DIRECT_HARDWARE_48C, 
        PARTNER,
        PRINTER,
        DIRECT_HARDWARE
    }
   

    public enum CashDrawerStatus
    {
        CLOSED,
        OPEN,
        //MANUALY_OPEN, perhaps future function... check if drawer is open without first sending open command could set this
        UNKNOWN


    }

}
