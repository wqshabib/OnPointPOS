using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POSSUM.Utils.nu.kontantkort.extdev;
namespace POSSUM.Utils
{

    public class SeamlessException : Exception
    {
        public int ErrorCode { get; set; }

        public SeamlessException(string message) : base(message)
        {

        }

    }


    public enum SeamlessResult
    {
        OK = 0,
        TransactionReversed = 78,
        LoginFailed,
        SystemError = 90,
        TransactionNotFound = 91
    }

   

    

}
