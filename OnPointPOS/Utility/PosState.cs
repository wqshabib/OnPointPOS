using System;
using System.Linq;
using System.Windows;

using POSSUM.Integration;
using POSSUM.Model;
using POSSUM.Data;
using System.ComponentModel;

namespace POSSUM
{
    public class PosState
    {
        public PosState()
        {

        }

        public static PosState GetInstance()
        {
            return (App.Current as App).State;//??new PosState();
        }

        public static void OpenDrawer()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                var cashdrawer = db.CashDrawer.First(x => x.TerminalId == Defaults.Terminal.Id);

                // Log that drawer was opened
                cashdrawer.OpenCashDrawer(Defaults.User.Id);


                // Kick drawer
                if (Defaults.CashDrawerType == CashDrawerType.PRINTER)
                {
                    DirectPrint directPrint = new DirectPrint();
                    directPrint.OpenCashDrawer();
                }
                else
                    PosState.GetInstance().CashDrawer.Open();

                // Save log
                db.CashDrawerLog.Add(cashdrawer.Logs.First());
                db.SaveChanges();
                //  LogWriter.JournalLog(Defaults.ActionList[JournalActionCode.OpenCashDrawer].Id);
            }
        }

        //GET FROM USER SESSION OBJECT
        //public bool TRAINING { get; set; }


        /// <summary>
        /// Get the configured cash drawer
        /// </summary>
        public ICashDrawer CashDrawer
        {
            get
            {
                if (cashDrawer == null)
                {
                    cashDrawer = CashDrawerFactory.GetCashDrawer(Defaults.CashDrawerType, Defaults.CashDrawerHardwarePort);
                }
                return cashDrawer;
            }
        }
        private ICashDrawer cashDrawer;



        /// <summary>
        /// Get the configured scale
        /// </summary>
        public IScale Scale
        {
            get
            {
                if (scale == null)
                    scale = ScaleFactory.GetScale(Defaults.ScaleType, Defaults.SCALEPORT);
                return scale;
            }
        }
        private IScale scale;

        /// <summary>
        /// Get the configured paymentterminal
        /// </summary>
        public IPaymentDevice PaymentDevice
        {
            get
            {
                if (paymentDevice == null)
                    paymentDevice = PaymentDeviceFactory.GetPaymentDevice(Defaults.PaymentDeviceType, Defaults.PaymentDevicConnectionString);// "babstcp://127.0.0.1:2000"
                return paymentDevice;
            }
        }
        private IPaymentDevice paymentDevice;

        /// <summary>
        /// Get the configured control unit
        /// </summary>
        public ControlUnitAction ControlUnitAction
        {
            get
            {
                if (controlUnit == null)
                {

                    string posId = Defaults.PosIdType == PosIdType.UniqueId ? Defaults.Terminal.UniqueIdentification : Defaults.Terminal.TerminalNo.ToString();
                    controlUnit = ControlUnitFactory.GetControlUnit(Defaults.ControlUnitType, Defaults.ControlUnitConnectionString, Defaults.Outlet.OrgNo, posId, Defaults.Terminal);
                }
                return new ControlUnitAction(controlUnit);
            }
        }
        public void ReTryControlUnit()
        {
            try
            {


                ControlUnitAction.ControlUnit.Close();
               // controlUnit = null;
                ControlUnitAction.ControlUnit.Open();
            }
            catch (Exception ex)
            {
                LogWriter.LogException(ex);
            }
        }
        private IControlUnit controlUnit;



        public ApplicationDbContext Context
        {
            get
            {
                //if (_context == null)
                //    _context = new ApplicationDbContext();
                return new ApplicationDbContext();
            }
        }
       


    }
}