using POSSUM.Model;
using POSSUM.Integration;
using POSSUM.Integration.CashDrawers.DirectHardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace POSSUM.Integration.CashDrawers.DirectHardware
{
    public class DirectHWCashDrawer : ICashDrawer
    {

        short port;

        short p1_open = 0x0C; // for 0x482 open=0x10;// // 0 .. 0000 1100
        short p1_close = 0x00; // 0 .. 0000 0000
        short p1_sbit = 0x40; // from 0x482 close=0x8;// // 0 .. 0100 0000  <- bit marks drawer closed

        /* not yet used

        short p2_open = 0x30; //  0 .. 0011 0000
        short p2_close = 0x00; // 0 .. 0000 0000
        short p2_sbit = 0x80;  // 0 .. 1000 0000  <- bit marks drawer closed

         */

        RawIO rIO;

        /*
         *  0x42 vid stängd och satt stängd
         *  0x0E vid satt öppen
         *  0x02 vid satt stängd men öppen
         */


        public DirectHWCashDrawer(short port = 0x48C)
        {
            if (DefaultsIntegration.CashDrawerHardwarePort == 1154)
            {
                p1_open = 0x10;
                p1_close = 0x00;
                p1_sbit = 0x8;
            }
            this.port = port;
            rIO = new RawIO(port);
        }

        public void Open()
        {
            rIO.Write(p1_open);
            Thread.Sleep(100);
            Close();
        }

        public void Close()
        {
            rIO.Write(p1_close);
        }

        public bool IsOpen()
        {
            byte b = rIO.Read();
            bool openDrawer = (b & p1_sbit) == 0;
            return openDrawer;

        }

    }
}
