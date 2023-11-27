using POSSUM.Integration.CashGuard;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace POSSUM.Integration
{
    public class CashGuardFactory
    {
        private int _transactionNumber = 1;
        private int _insertedAmount = 0;
        private CashGuardStatus _cgStatus;
        private CashGuardMode _cgMode;
        private bool _initialized;
        private bool _registered;
        private bool _completed;

        public CashGuardStatus Status => _cgStatus;

        public CashGuardMode Mode => _cgMode;
        public bool Initialized => _initialized;
        public bool Registered => _registered;
        public bool Completed => _completed;
        public int InsertedAmount => _insertedAmount;


        public CashGuardIntegration.CgIntegrationStatusCallDelegate StatusEventDelegate;
        public CashGuardIntegration.CgIntegrationErrorCallDelegate ErrorEventDelegate;
        public CashGuardIntegration.CgIntegrationLevelWarningCallDelegate LevelWarningEventDelegate;
        public CashGuardIntegration.CgIntegrationCashSessionCallDelegate CashSessionEventDelegate;


        CashGuardIntegration _cashGuard;

        public CashGuardFactory()
        {
            _cgMode = CashGuardMode.LoggedOut;
            _cgStatus = CashGuardStatus.Unknown;
            _transactionNumber = 1;

            StatusEventDelegate = new CashGuardIntegration.CgIntegrationStatusCallDelegate(CGIntegrationStatusEvent);
            ErrorEventDelegate = new CashGuardIntegration.CgIntegrationErrorCallDelegate(CGIntegrationErrorEvent);
            LevelWarningEventDelegate = new CashGuardIntegration.CgIntegrationLevelWarningCallDelegate(CGIntegrationLevelWarningEvent);
            CashSessionEventDelegate = new CashGuardIntegration.CgIntegrationCashSessionCallDelegate(CGIntegrationCashSessionEvent);
            RegisterEventListeners();
            Init();

        }


        public void RegisterEventListeners()
        {
            // Register Event Listeners
            try
            {

                short returnValue;
                string returnMessage;
                if (_cashGuard == null)
                {
                    _cashGuard = new CashGuardIntegration();
                    _cashGuard.SetVerboseLogging(true);
                }
                ClearCommandReturnValues();
                _cashGuard.RegisterEventListenersCG(StatusEventDelegate, ErrorEventDelegate,
                                                    LevelWarningEventDelegate, CashSessionEventDelegate,
                                                    out returnValue, out returnMessage);
                SetCommandReturnValues(returnValue, returnMessage);
                if (returnValue == 0)
                {
                    _registered = true;
                    //"Registered";
                }
            }
            catch (Exception ex)
            {
                App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
            }
        }

        public void Init()
        {
            // Initiate API, including COM port allocation
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;
                    ClearCommandReturnValues();
                    _cashGuard.InitCG("Type:RS232;Name:COM" + DefaultsIntegration.CASH_GuardPort, "", out returnValue, out returnMessage);
                    SetCommandReturnValues(returnValue, returnMessage);
                    if (returnValue == 0)
                    {
                        _initialized = true;
                        Login();
                    }
                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API event listeners have not been registered.");
            }
        }

        public void Login()
        {
            // Login
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;

                    _cashGuard.LoginCG(DefaultsIntegration.Terminal.UniqueIdentification, DefaultsIntegration.User.UserCode, out returnValue,
                        out returnMessage);
                    SetCommandReturnValues(returnValue, returnMessage);
                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }

            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }

        //public void ExitApi()
        //{
        //    App.MainWindow.SetCGErrVisibilty();
        //    // Exit API
        //    if (_cashGuard != null)
        //    {
        //        try
        //        {

        //            short returnValue;
        //            string returnMessage;

        //            ClearCommandReturnValues();
        //            _cashGuard.ExitCG(out returnValue, out returnMessage);
        //            SetCommandReturnValues(returnValue, returnMessage);

        //            _cgStatus = CashGuardStatus.Unknown;
        //            _cgMode = CashGuardMode.LoggedOut;
                   
        //            _initialized = false;
        //            _registered = false;

        //        }
        //        catch (Exception ex)
        //        {
        //            App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
        //        }
        //    }
        //    else
        //    {
        //        App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
        //    }
        //}
        public void RegretCG(short regretType, out short returnValue, out string returnMessage)
        {

            returnValue = 0;
            returnMessage = "";
            if (_cashGuard != null)
            {
                try
                {


                    var strMessage = new StringBuilder(256);
                    _cashGuard.RegretCG(regretType, out returnValue, out returnMessage);
                    returnMessage = strMessage.ToString();
                    returnMessage = string.Format("RegretCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                }
            }
        }

        public void ResetCG(out short returnValue, out string returnMessage)
        {

            returnValue = 0;
            returnMessage = "";
            if (_cashGuard != null)
            {
                try
                {

                    var strMessage = new StringBuilder(256);
                    _cashGuard.ResetCG(out returnValue, out returnMessage);
                    returnMessage = strMessage.ToString();
                    returnMessage = string.Format("Reset CG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage);
                }
                catch (Exception ex)
                {
                    returnMessage = ex.Message;
                }
            }
        }


        public void Logout()
        {
            // Logout
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;

                    ClearCommandReturnValues();
                    _cashGuard.LogoutCG(out returnValue, out returnMessage);
                    SetCommandReturnValues(returnValue, returnMessage);
                    _initialized = false;
                    _cgMode = CashGuardMode.LoggedOut;

                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }

        public void AmountDue(decimal DueAmount)
        {
            if (_cashGuard != null)
            {
                try
                {
                    int val = (int)DueAmount;
                    short returnValue;
                    string returnMessage;
                    int amountRest;
                    var transactionId = string.Format("{0}", _transactionNumber);

                    ClearCommandReturnValues();
                    _cashGuard.AmountDueCG(val, transactionId, out amountRest, out returnValue,
                        out returnMessage);
                    SetCommandReturnValues(returnValue, returnMessage);
                    // SetRestAmount(DueAmount, amountRest);
                    _transactionNumber++;

                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }
        decimal _backAmount = 0;
        //public void PayOutAmount(decimal insertedAmount, decimal backAmount)
        //{
        //    if (_cashGuard != null)
        //    {
        //        _insertedAmount = (int)Math.Round(insertedAmount, 2)*100;
        //        _backAmount =Math.Round( backAmount,0)*100;
        //        PerformPayOutAmount();
        //        if(_insertedAmount==0)
        //            LogWriter.SaveCashGuardLog(CashGuardActivityType.CashOut, _backAmount);
        //        else
        //        {
        //            LogWriter.SaveCashGuardLog(CashGuardActivityType.CashBySale, _insertedAmount);
        //            if (_backAmount > 0)
        //            {
        //                LogWriter.SaveCashGuardLog(CashGuardActivityType.CashOut, _backAmount);
        //            }
        //        }
        //        //BackgroundWorker bgWorker = new BackgroundWorker();
        //        //bgWorker.DoWork += BgWorker_DoWork;
        //        //bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
        //        //bgWorker.RunWorkerAsync();
        //    }
        //    else
        //    {
        //        App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
        //    }
        //}

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _backAmount = 0;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PerformPayOutAmount();
        }

        private void PerformPayOutAmount()
        {

            try
            {
                int val = (int)_backAmount;
                int val2 = _insertedAmount;
                short returnValue;
                string returnMessage;
                int amountRest;
                var transactionId = string.Format("{0}", _transactionNumber);

                ClearCommandReturnValues();
                _cashGuard.PayoutCG(val, val2, transactionId, out amountRest, out returnValue,
                    out returnMessage);
                SetCommandReturnValues(returnValue, returnMessage);
                // SetRestAmount(DueAmount, amountRest);
                _transactionNumber++;

            }
            catch (Exception ex)
            {
                App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
            }
        }
        public void EnablePayinCG(out string strMessage)
        {
            strMessage = "";
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;

                    ClearCommandReturnValues();
                    _cashGuard.EnablePayinCG(out returnValue,
                        out returnMessage);
                    strMessage = returnMessage;
                    SetCommandReturnValues(returnValue, returnMessage);


                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }

        public void DisablePayinCG(out string strMessage)
        {
            strMessage = "";
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;

                    ClearCommandReturnValues();
                    _cashGuard.DisablePayinCG(out returnValue,
                        out returnMessage);
                    strMessage = returnMessage;
                    SetCommandReturnValues(returnValue, returnMessage);


                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }
        public void ChangeCG(out string strMessage)
        {
            strMessage = "";
            if (_cashGuard != null)
            {
                try
                {

                    short returnValue;
                    string returnMessage;

                    ClearCommandReturnValues();
                    _cashGuard.ChangeCG(out returnValue,
                        out returnMessage);
                    strMessage = returnMessage;
                    SetCommandReturnValues(returnValue, returnMessage);


                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }
        public string DispenseAmount(int amountDispense)
        {
            try
            {
                if (_cashGuard != null)
                {
                    _insertedAmount = amountDispense*100;
                    BackgroundWorker bgDispense = new BackgroundWorker();
                    bgDispense.DoWork += BgDispense_DoWork;
                    bgDispense.RunWorkerCompleted += BgDispense_RunWorkerCompleted;
                    bgDispense.RunWorkerAsync();
                    return "In Progress";
                }
                else
                return "Not initialized";
            }
            catch (Exception ex)
            {

                App.MainWindow.ShowError("CASH GUARD ERRO", ex.Message);
                return ex.Message;
            }

        }

        private void BgDispense_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _insertedAmount = 0;
        }

        private void BgDispense_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispense();
        }

        private void Dispense()
        {

            int amountRest = 0;
            short resetVal = 0;
            string strMessage = "";
            short resetVal1 = 0;
            string strMessage1 = "";

            _cashGuard.DisablePayinCG(out resetVal1, out strMessage1);
            _cashGuard.DispenseCG(_insertedAmount, _transactionNumber.ToString(), out amountRest, out resetVal, out strMessage);
            _cashGuard.EnablePayinCG(out resetVal1, out strMessage1);
           if (!strMessage.Contains("Done"))
            {
                App.MainWindow.ShowError("CASH GUARD ERROR", strMessage);
            }
           //else
           //     LogWriter.SaveCashGuardLog(CashGuardActivityType.CashOut, _insertedAmount);
        }
        public string DipositAmount()
        {
            try
            {
                if (_cashGuard != null)
                {



                    short resetVal = 0;
                    string strMessage = "";
                    short resetVal1 = 0;
                    string strMessage1 = "";

                    _cashGuard.DisablePayinCG(out resetVal1, out strMessage1);
                    _cashGuard.DepositCG(_transactionNumber.ToString(), out resetVal, out strMessage);
                    _cashGuard.EnablePayinCG(out resetVal1, out strMessage1);
                    //LogWriter.SaveCashGuardLog(CashGuardActivityType.CashManualInsert, resetVal);
                    return strMessage;
                }
                return "Not initialized";
            }
            catch (Exception ex)
            {

                App.MainWindow.ShowError("CASH GUARD ERRO", ex.Message);
                return ex.Message;
            }

        }

        public void InfoCG(out string strMessage)
        {
            strMessage = "";
            if (_cashGuard != null)
            {
                try
                {
                    int level;
                    string commands;
                    string master;
                    string info;
                    short returnValue;
                    string returnMessage;
                    ClearCommandReturnValues();
                    _cashGuard.InfoCG(out level, out commands, out master, out info, out returnValue, out returnMessage);
                    SetCommandReturnValues(returnValue, returnMessage);
                    var strInfoCG1 = string.Format("level={0}, commands={1}", level, commands);
                    var strInfoCG2 = string.Format("master={0}, info={1}", master, info);

                    strMessage += returnMessage + "\n";
                    strMessage += strInfoCG1 + "\n";
                    strMessage += strInfoCG2;


                }
                catch (Exception ex)
                {
                    App.MainWindow.ShowError("Cash Gaurd Error", ex.Message);
                }
            }
            else
            {
                App.MainWindow.ShowError("Cash Gaurd Error", "CashGuard API has not been initialized.");
            }
        }

        private void SetCommandReturnValues(short code, string message)
        {

            string result = string.Format("Code: {0}, Message: {1}", code, message);

            if (code == 0)
            {
                //Done;
                _completed = true;
            }
            else
            {
                // Fail;
                _completed = false;
                new IntegrationLogWriter().CashGaurdLog(result);
                App.MainWindow.ShowError("CASH GUARD ERROR", result);
            }

        }

        private void ClearCommandReturnValues()
        {

        }
        #region Event from CashGuardIntegration
        void CGIntegrationErrorEvent(int errorCode, string errorText, string extInfo)
        {
            const string caption = "Error from CashGuard";
            var text = string.Format("Error Code: {0}{1}Error Message: {2}", errorCode, Environment.NewLine, errorText);
            App.MainWindow.ShowError(caption, text);
            new IntegrationLogWriter().CashGaurdLog(text);

        }
        void CGIntegrationStatusEvent(int amount, short status, short mode)
        {
            amount = amount > 0 ? amount / 100 : amount;
            SetInsertedAmount(amount);

            SetCashGuardStatus(status);
            SetCashGuardMode(mode);
            if (false && amount > 0 && (status == 0 || status == 3))
            {
                // Regret All
                if (_cashGuard != null)
                {
                    try
                    {

                        short returnValue;
                        string returnMessage;

                        // First parameter to RegretCG is regret type, where:
                        // 0 = Regret last inserted note
                        // 1 = Regret all inserted cash
                        // "Regret" is also known as "Cancel".
                        _cashGuard.RegretCG(1, out returnValue, out returnMessage);

                    }
                    finally
                    {
                        //   Cursor.Current = Cursors.Default;
                    }
                }
                else
                {
                    App.MainWindow.ShowError("Cash Gaurd", "CashGuard API has not been initialized.");
                }
            }
        }
        void CGIntegrationLevelWarningEvent(short warningType, int denomination, short numberOf, string typeString, string denominationString, string warningMessage, string extInfo)
        {
            if (string.IsNullOrEmpty(warningMessage))
                DefaultsIntegration.CGWarnings = new List<CGWarning>();
            else
                DefaultsIntegration.CGWarnings.Add(new CGWarning(DateTime.Now,  warningMessage));
            AppendToLevelWarnings(warningMessage);

        }

        void CGIntegrationCashSessionEvent(short infoType, int amount, string extInfo)
        {
            // Ignoring this type of event
        }

        #endregion
        void AppendToLevelWarnings(string value)
        {
            //App.MainWindow.SetCGWarning(value);
        }
        void SetInsertedAmount(int value)
        {

            _insertedAmount = value;
            //App.MainWindow.SetInsertedAmount(value);
        }

        void SetCashGuardStatus(int value)
        {

            switch (value)
            {
                case 0:
                    _cgStatus = CashGuardStatus.Idle;

                    break;
                case 1:
                    _cgStatus = CashGuardStatus.Busy;

                    break;
                case 2:
                    _cgStatus = CashGuardStatus.Error;

                    break;
                case 3:
                    _cgStatus = CashGuardStatus.PayinDisabled;

                    break;
                default:
                    _cgStatus = CashGuardStatus.Unknown;

                    break;
            }


        }

        void SetCashGuardMode(int value)
        {
            switch (value)
            {
                case 0:
                    _cgMode = CashGuardMode.LoggedOut;
                    break;
                case 1:
                    _cgMode = CashGuardMode.LoggedIn;

                    break;
                case 2:
                    _cgMode = CashGuardMode.BackOffice;
                    break;
                default:
                    _cgMode = CashGuardMode.BackOffice;
                    break;
            }


        }
    }

    public enum CashGuardStatus
    {
        Idle = 0,
        Busy = 1,
        Error = 2,
        PayinDisabled = 3,
        Unknown = 4
    }
    public enum CashGuardMode
    {
        LoggedOut = 0,
        LoggedIn = 1,
        BackOffice = 2,
        Unknown = 3
    }

}
