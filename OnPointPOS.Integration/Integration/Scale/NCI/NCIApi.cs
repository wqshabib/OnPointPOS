using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Scale.NCI
{
    class NCIApi
    {
        SerialPort scalePort;
        private Queue<byte> recievedData = new Queue<byte>();
        protected bool isConnected = false;

        public NCIApi()
        {

        }

        public void Connect(string port, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            //scalePort = new SerialPort(port, 9600, 0, 8, StopBits.One);
            scalePort = new SerialPort(port, baudRate, parity, dataBits, stopBits);
            //scalePort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            scalePort.Open();

            this.isConnected = true;
        }
        public void Disconnect()
        {
            scalePort.Close();
            this.isConnected = false;
        }

        public string SendCommand(string command)
        {
            return "";
        }

        private static void DataReceivedHandler(
                                object sender,
                                SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.Write(indata);
        }

        public decimal GetWeight()
        {
            scalePort.Write("W" + (char)13);

            int counter = 0;
            string indata = "";
            string scaleweight = "";
            string status = "";
            decimal weight = 0;
            bool taken = false;

            if (taken == false)
            {
                while (scaleweight.Length == 0 || status.Length == 0)
                {
                    if (scalePort.BytesToRead > 0)
                    {
                        indata = indata + scalePort.ReadExisting();

                        string[] d = indata.Split((char)13);

                        foreach (string line in d)
                        {
                            if (line.Trim().Contains("KG") || line.Trim().Contains("kg"))
                            {

                                scaleweight = line.Trim().Replace("w", "").Trim();
                                status = "SUCESS";
                                taken = true;
                                break;
                            }
                            else if (line.Trim().Contains("S00"))
                            {
                                status = line.Trim();
                            }
                        }
                        if (taken)
                            break;
                    }
                    counter++;

                    //TODO:BAD WAY?
                    if (counter > 50000)
                    {
                        scaleweight = "ERROR";
                        status = "ERROR";
                        weight = -1;
                        break;
                    }
                }

                if (!scaleweight.Equals("ERROR"))
                {

                    string temp = scaleweight.Substring(0, 6);
                    if (CultureInfo.CurrentUICulture.Name == "sv-SE")
                    {
                        temp = scaleweight.Substring(0, 6).Replace(".", ",");
                    }
                    weight = Decimal.Parse(temp, CultureInfo.CurrentUICulture);
                    // CONVERT TO GRAMS
                    weight = weight * 1000;
                }
            }
            return weight;
        }
        public decimal GetHighresWeight()
        {
            String res = SendCommand("H");

            //Response sample:
            //00.1178KG
            //S00

            //TODO: convert to decimal

            return 0;

        }
        public int GetUnitOfMesure()
        {
            int unitOfMesure = 0;

            // = ‘1’: Grams
            // = ‘2’: Kilograms
            // = ‘3’: Ounces
            // = ‘4’: Pounds

            String res = SendCommand("u");

            return unitOfMesure;
        }
        public void Reset()
        {
            String res = SendCommand("Z");
        }
        public string GetStatus()
        {
            return SendCommand("S");
        }


    }
}
