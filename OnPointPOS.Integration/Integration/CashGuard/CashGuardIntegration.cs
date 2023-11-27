using System.Text;

namespace POSSUM.Integration.CashGuard
{
	public class CashGuardIntegration
	{
        public delegate void CgIntegrationStatusCallDelegate(int amount, short status, short mode);
        public delegate void CgIntegrationErrorCallDelegate(int errorCode, string errorText, string extInfo);
        public delegate void CgIntegrationLevelWarningCallDelegate(short warningType, int denomination, short numberOf, string typeString, string denominationString, string warningMessage, string extInfo);
        public delegate void CgIntegrationCashSessionCallDelegate(short infoType, int amount, string extInfo);
        
		public CashGuardAPI.CgStatusCallDelegate StatusDelegate;
		public CashGuardAPI.CgErrorCallDelegate ErrorDelegate;
		public CashGuardAPI.CgLevelWarningCallDelegate LevelWarningDelegate;
		public CashGuardAPI.CgCashSessionCallDelegate CashSessionDelegate;

	    private CgIntegrationStatusCallDelegate _statusEventDelegate;
	    private CgIntegrationErrorCallDelegate _errorEventDelegate;
	    private CgIntegrationLevelWarningCallDelegate _levelWarningEventDelegate;
	    private CgIntegrationCashSessionCallDelegate _cashSessionEventDelegate;

        readonly IntegrationLogWriter _logWriter;
        
        public CashGuardIntegration()
		{
			StatusDelegate = new CashGuardAPI.CgStatusCallDelegate(CGCApiCGStatusEvent);
			ErrorDelegate = new CashGuardAPI.CgErrorCallDelegate(CGCApiCGErrorEvent);
			LevelWarningDelegate = new CashGuardAPI.CgLevelWarningCallDelegate(CGCApiCGLevelWarningEvent);
			CashSessionDelegate = new CashGuardAPI.CgCashSessionCallDelegate(CGCApiCGCashSessionEvent);

			_logWriter = new IntegrationLogWriter();
			_logWriter.CashGaurdLog("CashGuardIntegration has been created.");
		}

        // ==============
        // Configurations
        // ==============
        #region Configurations

        public void SetVerboseLogging(bool on)
        {
            //_logWriter.SetVerboseLogging(on);
        }

        public void StopLogging()
        {
           // _logWriter.Shutdown();
        }

        #endregion Configurations

        // ===================
        // CashGuard Functions
        // ===================
        #region CashGuard Functions

        public void RegisterEventListenersCG(CgIntegrationStatusCallDelegate statusEventDelegate, CgIntegrationErrorCallDelegate errorEventDelegate, CgIntegrationLevelWarningCallDelegate levelWarningEventDelegate, CgIntegrationCashSessionCallDelegate cashSessionEventDelegate, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog("RegisterEventListenersCG called.");
            _statusEventDelegate = statusEventDelegate;
            _errorEventDelegate = errorEventDelegate;
            _levelWarningEventDelegate = levelWarningEventDelegate;
            _cashSessionEventDelegate = cashSessionEventDelegate;

            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.registerEvents3CG(StatusDelegate, ErrorDelegate, LevelWarningDelegate, CashSessionDelegate, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("RegisterEventListenersCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

		public void InitCG(string portSettings, string initSettings, out short returnValue, out string returnMessage)
		{
            _logWriter.CashGaurdLog(string.Format("InitCG called with portSettings={0}, initSettings={1}.", portSettings, initSettings));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.initCG(portSettings, initSettings, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("InitCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
		}

		public void ExitCG(out short returnValue, out string returnMessage)
		{
          //  _logWriter.CashGaurdLog("ExitCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.exitCG(strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdEventLog("ExitLog",string.Format("ExitCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
		}

		public void LoginCG(string cashRegId, string cashierId, out short returnValue, out string returnMessage)
		{
		  //  _logWriter.CashGaurdLog(string.Format("LoginCG called with cashRegID={0}, cashierId={1}.", cashRegId, cashierId));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.loginStrCG(cashRegId, cashierId, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdEventLog("LoginLog",string.Format("LoginCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
		}

		public void LogoutCG(out short returnValue, out string returnMessage)
		{
          //  _logWriter.CashGaurdLog("LogoutCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.logoutCG(strMessage);
            returnMessage = strMessage.ToString();
           _logWriter.CashGaurdEventLog("LogoutLog",string.Format("LogoutCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
		}

        public void IsOpenCG(out int open, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog("IsOpenCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.isOpenCG(out open, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("IsOpenCG returns with open={0}, returnValue={1}, returnMessage={2}.", open, returnValue, returnMessage));
        }

        public void InfoCG(out int level, out string commands, out string master, out string info, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog("InfoCG called.");
            var strCommands = new StringBuilder(256);
            var strMaster = new StringBuilder(256);
            var strInfo = new StringBuilder(256);
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.infoCG(out level, strCommands, strMaster, strInfo, strMessage);
            commands = strCommands.ToString();
            master = strMaster.ToString();
            info = strInfo.ToString();
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("InfoCG returns with level={0}, commands={1}, master={2}, info={3}, returnValue={4}, returnMessage={5}.", level, commands, master, info, returnValue, returnMessage));
        }

        public void ChangeSettingsCG(string initSettings, short durationType, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("ChangeSettingsCG called with initSettings={0}, durationType={1}.", initSettings, durationType));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.changeSettingsCG(initSettings, durationType, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("ChangeSettingsCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void ChangeCG(out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog("ChangeCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.changeCG(strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("ChangeCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void RegretCG(short regretType, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("RegretCG called with regretType={0}.", regretType));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.regretCG(regretType, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("RegretCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void ResetCG(out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog("ResetCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.resetCG(strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("ResetCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void AskPayoutCG(int amountAskPay, out int amountRest, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("AskPayoutCG called with amountAskPay={0}.", amountAskPay));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.askPayout2CG(amountAskPay, out amountRest, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("AskPayoutCG returns with amountRest={0}, returnValue={1}, returnMessage={2}.", amountRest, returnValue, returnMessage));
        }

        // Alternative 1: The two traditional alternative functions for depositing/dispensing cash

        public void PayoutCG(int amountDispense, int amountDeposit, string transactId, out int amountRest, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("PayoutCG called with amountDispense={0},  amountDeposit={1},  transactId={2}.", amountDispense, amountDeposit, transactId));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.payout2CG(amountDispense, amountDeposit, transactId, out amountRest, strMessage);
            returnMessage = strMessage.ToString();
           // _logWriter.CashGaurdLog(string.Format("PayoutCG returns with amountRest={0}, returnValue={1}, returnMessage={2}.", amountRest, returnValue, returnMessage));
        }

        public void AmountDueCG(int amountDue, string transactId, out int amountRest, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("AmountDueCG called with amountDue={0},  transactId={1}.", amountDue, transactId));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.amountDueCG(amountDue, transactId, out amountRest, strMessage);
            returnMessage = strMessage.ToString();
            _logWriter.CashGaurdLog(string.Format("AmountDueCG returns with amountRest={0}, returnValue={1}, returnMessage={2}.", amountRest, returnValue, returnMessage));
        }

        // Alternative 2: The new functions for depositing/dispensing cash, with added features to lock/unlock the possibility to insert cash.

        public void EnablePayinCG(out short returnValue, out string returnMessage)
        {
          //  _logWriter.CashGaurdLog("EnablePayinCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.enablePayinCG(strMessage);
            returnMessage = strMessage.ToString();
          //  returnMessage=string.Format("EnablePayinCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage);
        }

        public void DisablePayinCG(out short returnValue, out string returnMessage)
        {
          //  _logWriter.CashGaurdLog("DisablePayinCG called.");
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.disablePayinCG(strMessage);
            returnMessage = strMessage.ToString();
          //  _logWriter.CashGaurdLog(string.Format("DisablePayinCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void DepositCG(string transactId, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("DepositCG called with transactId={0}.", transactId));
            var strMessage = new StringBuilder(256);
            returnValue = CashGuardAPI.depositCG(transactId, strMessage);
            returnMessage = strMessage.ToString();
          //  _logWriter.CashGaurdLog(string.Format("DepositCG returns with returnValue={0}, returnMessage={1}.", returnValue, returnMessage));
        }

        public void DispenseCG(int amountDispense, string transactId, out int amountRest, out short returnValue, out string returnMessage)
        {
            _logWriter.CashGaurdLog(string.Format("DispenseCG called with amountDispense={0},  transactId={1}.", amountDispense, transactId));
            var strMessage = new StringBuilder(256);
           
            returnValue = CashGuardAPI.dispenseCG(amountDispense, transactId, out amountRest, strMessage);
          
            returnMessage = strMessage.ToString();
            //_logWriter.CashGaurdLog(string.Format("DispenseCG returns with amountRest={0}, returnValue={1}, returnMessage={2}.", amountRest, returnValue, returnMessage));
        }

        #endregion CashGuard Functions

        // =====================
        // Events from CashGuard
        // =====================
        #region Events from CashGuard
        
        void CGCApiCGStatusEvent(int amount, short status, short mode)
		{
            _logWriter.CashGaurdEventLog("StatusLog", string.Format("CGCApiCGStatusEvent triggered with incoming parameters: amount={0}, status={1}, mode={2}.", amount, status, mode));
            _statusEventDelegate(amount, status, mode);
           // _logWriter.CashGaurdLog("CGCApiCGStatusEvent returns.");
		}

        void CGCApiCGErrorEvent(int errorCode, string errorText, string extInfo)
        {
            _logWriter.CashGaurdEventLog("ErrorLog", string.Format("CGCApiCGErrorEvent triggered with incoming parameters: errorCode={0}, errorText={1}, extInfo={2}.", errorCode, errorText, extInfo));
            _errorEventDelegate(errorCode, errorText, extInfo);
           // _logWriter.CashGaurdEventLog("Error", "CGCApiCGErrorEvent returns.");
        }

		void CGCApiCGLevelWarningEvent(short warningType, int denomination, short numberOf, string typeString, string denominationString, string warningMessage, string extInfo)
		{
          //  _logWriter.CashGaurdLog(string.Format("CGCApiCGLevelWarningEvent triggered with incoming parameters: warningType={0}, denomination={1}, numberOf={2}, typeString={3}, denominationString={4}, warningMessage={5}, extInfo={6}.", warningType, denomination, numberOf, typeString, denominationString, warningMessage, extInfo));
		    _levelWarningEventDelegate(warningType, denomination, numberOf, typeString, denominationString, warningMessage, extInfo);
           // _logWriter.CashGaurdLog("CGCApiCGLevelWarningEvent returns.");
		}

		void CGCApiCGCashSessionEvent(short infoType, int amount, string extInfo)
		{
            _logWriter.CashGaurdEventLog("sessionLog",string.Format("CGCApiCGCashSessionEvent triggered with incoming parameters: infoType={0}, amount={1}, extInfo={2}.", infoType, amount, extInfo));
		    _cashSessionEventDelegate(infoType, amount, extInfo);
            //_logWriter.CashGaurdEventLog("sessionLog", "CGCApiCGCashSessionEvent returns.");
		}

        #endregion Events from CashGuard
    }
}
