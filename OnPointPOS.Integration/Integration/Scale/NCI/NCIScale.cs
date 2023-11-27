using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Scale.NCI
{
    class NCIScale : IScale
    {
        private string port = "COM1";
        private volatile NCIApi api;
        private int baudRate = 9600;
        private Parity parity = Parity.None;
        private int dataBits = 8;
        private StopBits stopBits = StopBits.One;

        public NCIScale(string port)
        {
            this.port = port;
            api = new NCIApi();
        }

        public NCIScale(string port, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            this.port = port;
            this.baudRate = baudRate;
            this.parity = parity;
            this.dataBits = dataBits;
            this.stopBits = stopBits;
            api = new NCIApi();
        }

        public NCIScale()
        {
            this.port =DefaultsIntegration.SCALEPORT;
            api = new NCIApi();
        }

        public void Connect() {
            api.Connect(port, baudRate, parity,dataBits, stopBits);
        }

        public void Disconnect()
        {
            api.Disconnect();
        }

        public decimal GetWeight()
        {
            return api.GetWeight();
        }
        public decimal GetHighresWeight()
        {
            return api.GetHighresWeight();
        }
        public int GetUnitOfMesure()
        {
            return api.GetUnitOfMesure();
        }
        public void Reset()
        {
            api.Reset();
        }
        public string GetStatus()
        {
            return api.GetStatus();
        }
    }
}
