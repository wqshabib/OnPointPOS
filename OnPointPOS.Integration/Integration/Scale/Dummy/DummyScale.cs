using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Scale.Dummy
{
    class DummyScale : IScale
    {
        private string paymentDeviceTypeConnectionString;

        public DummyScale(string paymentDeviceTypeConnectionString)
        {
            this.paymentDeviceTypeConnectionString = paymentDeviceTypeConnectionString;
        }

        public DummyScale()
        {

        }

        public void Connect()
        {

        }

        public void Disconnect() {

        }

        public decimal GetWeight()
        {
            decimal weight = 0.150m;

            //CONVERT TO GRAMS
            weight = weight * 1000;

            return weight;
        }
        public decimal GetHighresWeight()
        {
            decimal weight = 0.1501m;

            return weight;
        }
        public int GetUnitOfMesure()
        {
            int unitOfMesure = 1;

            return unitOfMesure;
        }
        public void Reset()
        {

        }
        public string GetStatus()
        {
            string status = "STATUS";

            return status;
        }

    }
}
