using System;
using System.Collections.Generic;
using POSSUM.Model;
using POSSUM.Data;

namespace POSSUM.Presenters.Printers
{
    public class PrinterPresenter
    {
        private readonly IPrinterView _view;

        public PrinterPresenter(IPrinterView view)
        {
            this._view = view;
        }

        public PrinterPresenter()
        {
        }

        public List<Printer> GetPrinters(string keyword)
        {
            var printerList = new List<Printer>();
            try
            {

                Defaults.Printers = printerList = new SettingRepository().GetPrinters(keyword);

            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
            }
            return printerList;
        }

        internal bool UpdatePrinter(Printer printer)
        {
            try
            {
                return new SettingRepository().UpdatePrinter(printer);
            }
            catch (Exception ex)
            {
                _view.ShowError(Defaults.AppProvider.AppTitle, ex.Message);

                return false;
            }
        }

        internal void LoadPrinterClick()
        {
            string keyword = _view.GetKeyword();

            List<Printer> printers = GetPrinters(keyword);

            _view.SetPrinterResult(printers);
        }
    }
}