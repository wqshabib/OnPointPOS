using POSSUM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Presenters.Payments
{
    public interface IPaymentTerminalUtilView : IBaseView
    {

        void ShowOption(int optionNr, bool visisble);
        void ShowOption(int optionNr, string textValue, bool visible);
        void ShowKeypad(bool p);
        void ShowAbort(bool p);
        void ShowOk(bool p);
        void ShowOk(string text, bool visible);
        void ShowResetMenu(string text, bool visisble);
        void SetInfoWindow(string infoText);

        void Close(bool success);
    }
}
