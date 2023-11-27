using System.Text;
using System.Runtime.InteropServices;

namespace POSSUM.Integration.CashGuard
{
	public class CashGuardAPI
	{
        #region DLL Import Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CgStatusCallDelegate(int amount, short status, short mode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CgErrorCallDelegate(int errorCode, string errorText, string extInfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CgLevelWarningCallDelegate(short warningType, int denomination, short numberOf, string typeString, string denominationString, string warningMessage, string extInfo);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CgCashSessionCallDelegate(short infoType, int amount, string extInfo);

        #endregion DLL Import Delegates


        #region DLL Import Functions "cglogics.dll"

        [DllImport("cglogics.dll")]
        public static extern short registerEvents3CG(CgStatusCallDelegate status,
                                                     CgErrorCallDelegate error,
                                                     CgLevelWarningCallDelegate levelWarning,
                                                     CgCashSessionCallDelegate cashSession,
                                                     StringBuilder message);

        [DllImport("cglogics.dll")]
		public static extern short initCG(string portSettings, string initSettings, StringBuilder message);

		[DllImport("cglogics.dll")]
		public static extern short exitCG(StringBuilder message);

		[DllImport("cglogics.dll")]
		public static extern short loginStrCG(string cashRegId, string cashierId, StringBuilder message);

		[DllImport("cglogics.dll")]
		public static extern short logoutCG(StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short isOpenCG(out int open, StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short infoCG(out int level, StringBuilder commands, StringBuilder master, StringBuilder info, StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short changeSettingsCG(string initSettings, short durationType, StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short changeCG(StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short regretCG(short regretType, StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short resetCG(StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short askPayout2CG(int amountAskPay, out int amountRest, StringBuilder message);

        // Alternative 1: The two traditional alternative functions for depositing/dispensing cash

        [DllImport("cglogics.dll")]
        public static extern short payout2CG(int amountDispense, int amountDeposit, string transactId, out int amountRest, StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short amountDueCG(int amountDue, string transactId, out int amountRest, StringBuilder message);

        // Alternative 2: The new functions for depositing/dispensing cash, with added features to lock/unlock the possibility to insert cash.

        [DllImport("cglogics.dll")]
        public static extern short enablePayinCG(StringBuilder message);

        [DllImport("cglogics.dll")]
        public static extern short disablePayinCG(StringBuilder message);

        // Deposit is only allowed from disabled state
        [DllImport("cglogics.dll")]
        public static extern short depositCG(string transactId, StringBuilder message);

        // Deposit must be followed by Dispense
        [DllImport("cglogics.dll")]
        public static extern short dispenseCG(int amountDispense, string transactId, out int amountRest, StringBuilder message);

        #endregion DLL Import Functions "cglogics.dll"
    }
}
