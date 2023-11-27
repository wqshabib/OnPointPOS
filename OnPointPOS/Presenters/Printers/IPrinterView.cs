using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.Printers
{
    public interface IPrinterView : IBaseView
    {
        //Printer GetPrinter(int id);
        //List<Printer> GetPrinters();
        void SetPrinterResult(List<Printer> printers);
        string GetKeyword();
    }
}