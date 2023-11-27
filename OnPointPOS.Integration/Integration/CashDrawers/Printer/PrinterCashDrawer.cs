using System;
using System.IO.Ports;
using System.Linq;

namespace POSSUM.Integration.CashDrawers.Printer
{
    public class PrinterCashDrawer : ICashDrawer
    {
        // Need to implement i have just copied PartnerCashdrawer clss code: Arshad
        //inpout.dll

        //Public Methods
        public void Open()
        {
            SerialPort COM20 = new SerialPort("COM20", 115200, 0, 8, StopBits.One);
            COM20.Open();

            string stropencode =
                "1B 21 00 1B 4D 00 1B 32 1B 45 00 1B 24 00 00 1B 20 00 1B 21 00 1D 48 02 1B 24 0A 00 43 4F 44 45 31 32 38 0D 0A 1B 24 0A 00 1D 77 02 1D 68 32 1D 6B 49 0C 7B 41 30 31 32 33 34 35 36 37 38 39 0A 0A 42 61 72 20 63 6F 64 65 20 70 72 69 6E 74 69 6E 67 20 66 69 6E 69 73 68 65 64 21 0A 1D 56 42 00 1B 40 1B 70 0 10 20";
            byte[] bytes = stropencode.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();
            COM20.Write(bytes, 0, bytes.Length);

            COM20.Close();
        }

        public bool IsOpen()
        {
            return false;
        }

        public void Close()
        {
        }
    }
}