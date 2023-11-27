using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using POSSUM.Integration.Scale.NCI;
using POSSUM.Integration.Scale.Dummy;

namespace POSSUM.Integration
{
    public class ScaleFactory
    {
        public static IScale GetScale(ScaleType type, string port)
        {
            switch (type)
            {
                case ScaleType.NCIPROTOCOL:
                    return new NCIScale(port, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                case ScaleType.NCIPROTOCOL48:
                    return new NCIScale(port, 4800, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                case ScaleType.NCRPROTOCOL:
                    return new NCIScale(port, 9600, System.IO.Ports.Parity.Even, 8, System.IO.Ports.StopBits.One);
                case ScaleType.DUMMY:
                    return new DummyScale();
            }

            MessageBox.Show("SCALE NOT FOUND", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
}
