using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration
{
    public interface IScale
    {
        void Connect();
        void Disconnect();
        decimal GetWeight();
        decimal GetHighresWeight();
        int GetUnitOfMesure();
        void Reset();
        string GetStatus();
    }

    public enum ScaleType
    {
        NONE,
        DUMMY,
        NCIPROTOCOL,
        NCIPROTOCOL48,
        NCRPROTOCOL
    }

}
