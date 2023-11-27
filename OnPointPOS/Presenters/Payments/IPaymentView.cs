using POSSUM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Presenters.Payments
{
    public interface IPaymentView : IBaseView
    {

        void SetStatusText(string infoText);
        
        void SetErroText(string infoText);

        void SetTerminalDisplay(string displayText, int? status, int? transactionType, int? transactionResult);

        void SetPaymentText(string paymentText);

        void CloseWithStatus(bool result);

        void ShowKeypad(bool p);

        void ShowAbort(bool p);
        void ShowClose(bool p);
        void ShowWindowClose(bool p);
        void ShowReconnect(bool p);

        void ShowOk(bool p);

        void ShowOk(string text, bool visible);

        void ShowAbort(string text, bool visisble);

        void ShowOption(string text, bool visisble);

        void ShowOption(bool p);
    }
}
