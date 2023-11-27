using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Views.Sales;
using POSSUM.Integration;
using POSSUM.Utils;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Protocol;
using POSSUM.ViewModels;
using System.Collections.Generic;
using System.Windows;
using Notifications.Wpf;
using Newtonsoft.Json;
using System.Configuration;
using MQTTnet.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using POSSUM.Handlers;

namespace POSSUM.Presenters.Login
{
    public class LoginPresenter
    {
        private readonly ILoginView _view;


        public LoginPresenter(ILoginView view)
        {
            _view = view;
            view.ShowLogin();
            view.SetFocusUsername();
            if (Defaults.Terminal != null && Defaults.TerminalMode == TerminalMode.SingleOutlet)
                view.ShowIsClosed(!Defaults.Terminal.IsOpen);
        }

        public LoginPresenter()
        {
        }
        string _userName;
        string _password;

        internal void HandleLoginClick()
        {
            _userName = _view.GetUsername();
            _password = _view.GetPassword();
            int id;
            if (string.IsNullOrEmpty(_userName) || !int.TryParse(_userName, out id))
            {
                _view.ShowError(UI.Login_Error, UI.Login_PinMissing);
                _view.SetFocusUsername();
                return;
            }

            bool isLogedIn = false;

            var progressDialog = new ProgressWindow();
            var backgroundThread = new Thread(() =>
            {
                bool res = IsValid(id.ToString(), _password, isLogedIn);
                LogWriter.JournalLog(res
                    ? Convert.ToInt32(JournalActionCode.LoggedIn)
                    : Convert.ToInt32(JournalActionCode.LoggedInFailure));
                progressDialog.Closed += (arg, e) => { progressDialog = null; };
                progressDialog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressDialog.Close();
                    isLogedIn = res;
                }));
            });
            backgroundThread.Start();
            progressDialog.ShowDialog();
            backgroundThread.Join();

            if (!isLogedIn)
            {
                _view.ShowError(UI.Login_Error, UI.Login_WrongPin);
                _view.SetPassword("");
                _view.SetFocusPassword();
                return;
            }

            if (Defaults.ShowExternalOrder)
            {
                App.MainWindow.ShowExternalOrder();
            }
            try
            {
                var cu = PosState.GetInstance().ControlUnitAction;

                var status = cu.ControlUnit.CheckStatus();
                if (status != ControlUnitStatus.OK)
                {
                    PosState.GetInstance().ReTryControlUnit();
                    cu = PosState.GetInstance().ControlUnitAction;
                    status = cu.ControlUnit.CheckStatus();
                }
                if (status != ControlUnitStatus.OK)
                {
                    PosState.GetInstance().ReTryControlUnit();
                    cu = PosState.GetInstance().ControlUnitAction;
                    status = cu.ControlUnit.CheckStatus();
                }
                if (status != ControlUnitStatus.OK)
                {
                    App.MainWindow.ShowError(UI.CannotConnectToController, "");
                    CUConnectionWindow ucConnectionWindow = new CUConnectionWindow();
                    if (ucConnectionWindow.ShowDialog() == true)
                    {

                    }
                    else
                        Environment.Exit(0);
                }
            }
            catch (ControlUnitException e)
            {
                //here need to call retry
                PosState.GetInstance().ReTryControlUnit();
                var cu = PosState.GetInstance().ControlUnitAction;
                var status = cu.ControlUnit.CheckStatus();
                if (status != ControlUnitStatus.OK)
                {
                    //App.MainWindow.ShowError(UI.Global_Error, e.Message);
                    CUConnectionWindow ucConnectionWindow = new CUConnectionWindow();
                    if (ucConnectionWindow.ShowDialog() == true)
                    {

                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }
            VerifyTerminalOpen();
        }

        private void VerifyTerminalOpen()
        {
            try
            {
                
                if (Defaults.TerminalMode == TerminalMode.MultiOutlet)
                {
                    Defaults.Init();
                    if (!Defaults.Terminal.IsOpen)
                        OpenTerminal();
                }
                if (Defaults.Terminal.IsOpen)
                {
                    try
                    {
                        if (Defaults.ISMQTTInitialize=="1")
                        {
                            App.MainWindow.InitializeMQTTPOSSUMClient();
                            App.MainWindow.InitializeMQTTPOSSUMForMobileClientAlive();                 
                        }

                        if (Defaults.ISMQTTInitializeForTerminal == "1")
                        {
                            App.MainWindow.InitializeMQTTPOSSUMForMobilePosMiniClientAlive();
                            App.MainWindow.InitializeMQTTPOSSUMForMobilePosMini();
                        }
                        if (Defaults.ISMQTTFORPOSMINI == "1")
                        {
                            MQTTHandler.InitializeMQTTPOSSUMForMobilePosMiniClientAlive();
                            App.MainWindow.InitializeMQTTPOSSUMClientMiniOrderInCart();
                        }
                        if (Defaults.ISMQTTFORONLINEORDER == "1")
                        {
                            MQTTHandler.InitializeMQTTPOSSUMForMobilePosOnlineOrderClientAlive();
                            App.MainWindow.InitializeMQTTPOSSUMClientOnlineOrder();
                            App.MainWindow.InitializeMQTTPOSSUMForMobilePosMiniDirCheckoutOrder();
                        }

                        //InitializeMQTTPOSSUMClient();
                    }
                    catch (Exception e)
                    {
                        Log.LogException(e);
                    }
                    //App.MainWindow.AdminArea.Dispatcher.BeginInvoke(
                    //    new Action(() => App.MainWindow.AdminArea.Visibility = Visibility.Collapsed));
                    //App.MainWindow.UserListGrid.Dispatcher.BeginInvoke(
                    //    new Action(() => App.MainWindow.UserListGrid.Visibility = Visibility.Visible));
                    Defaults.PerformanceLog.AddEntry("starting to prepare sale screen");

                    if (Defaults.SaleType == SaleType.Restaurant)
                    {
                        App.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var uc = new UCSale();
                            App.MainWindow.AddControlToMainCanvas(uc);

                            //if (Defaults.ScreenResulution == ScreenResulution.SR_1000X768)
                            //{
                            //    var uc = new UCSales();
                            //    App.MainWindow.AddControlToMainCanvas(uc);
                            //}
                            //else if(Defaults.ScreenResulution == ScreenResulution.SR_1366X768)
                            //{
                            //    var uc = new UCSales1366_768();
                            //    App.MainWindow.AddControlToMainCanvas(uc);
                            //}
                            //else
                            //{
                            //    var uc = new UCSalesTablet();
                            //    App.MainWindow.AddControlToMainCanvas(uc);
                            //}
                        }));
                    }
                    else
                    {
                        var uc = new UCSale();
                        App.MainWindow.AddControlToMainCanvas(uc);
                        //if (Defaults.ScreenResulution == ScreenResulution.SR_1000X768)
                        //{
                        //    var uc = new UCPlaceRetailOrder();
                        //    App.MainWindow.AddControlToMainCanvas(uc);
                        //}
                        //else
                        //{
                        //    var uc = new UCPlaceRetailOrder1366_768();
                        //    App.MainWindow.AddControlToMainCanvas(uc);
                        //}
                    }
                    App.MainWindow.AddUserActivityMenu();
                    Defaults.PerformanceLog.Add("sale screen prepared");
                    App.MainWindow.btnQuitApp.Visibility = Visibility.Collapsed;

                    if (ConfigurationManager.AppSettings["ChangeUserURL"] == "1")
                    {
                        App.MainWindow.btnUserInfo.Content = "USER LOGGED IN: " + _userName;
                        App.MainWindow.btnUserInfo.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    _view.ShowOpening();
                    try
                    {

                        if (Defaults.CASH_GUARD == false)
                            PosState.OpenDrawer();
                    }
                    catch (Exception ex)
                    {
                        LogWriter.LogWrite(ex);
                        App.MainWindow.ShowError(UI.Message_Error, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                _view.ShowError(UI.Message_Error, ex.Message);
            }
        }

        internal void LoginByDallaKey(OutletUser user)
        {
            try
            {

                Defaults.PerformanceLog.Add("going to log user login entry    -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));
                var isLogedIn = IsLoggedInByDallasKey(user);

                if (!isLogedIn)
                {
                    LogWriter.JournalLog(Convert.ToInt32(JournalActionCode.LoggedInFailure));

                    _view.ShowError(UI.Login_Error, UI.Login_WrongPin);
                    _view.SetPassword("");
                    _view.SetFocusPassword();
                    return;
                }
                else
                {
                    LogWriter.JournalLog(Convert.ToInt32(JournalActionCode.LoggedIn));


                    if (Defaults.ShowExternalOrder)
                    {
                        App.MainWindow.ShowExternalOrder();
                    }
                }
                Defaults.PerformanceLog.Add("going to open terminal   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                VerifyTerminalOpen();


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }

        internal void LoginAndUpdateDallaKey(OutletUser userModel)
        {

            bool isLogedIn;

            try
            {
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {
                    isLogedIn = loginRepo.LoginAndUpdateDallaKey(userModel, Defaults.DallasKey);
                }
                Defaults.PerformanceLog.Add("going to log user login entry    -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));


                if (!isLogedIn)
                {
                    LogWriter.JournalLog(Convert.ToInt32(JournalActionCode.LoggedInFailure));

                    _view.ShowError(UI.Login_Error, UI.Login_WrongPin);
                    _view.SetPassword("");
                    _view.SetFocusPassword();
                    return;
                }
                else
                {
                    LogWriter.JournalLog(Convert.ToInt32(JournalActionCode.LoggedIn));


                    if (Defaults.ShowExternalOrder)
                    {
                        App.MainWindow.ShowExternalOrder();
                    }
                }
                Defaults.PerformanceLog.Add("going to open terminal   -> " + string.Format("{0:yyyy-MM-dd_hh-mm-ss-fff}", DateTime.Now));

                VerifyTerminalOpen();


            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }


        internal void HandleOpeningSave()
        {
            decimal openingAmount = _view.GetOpeningAmount();
            bool confirmed = _view.GetConfirm();
            if (!confirmed)
            {
                _view.ShowError(UI.Message_CashChange, UI.Message_ConfirmChange);
                return;
            }

            try
            {
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {
                    var terminal = loginRepo.OpeningSave(Defaults.Terminal.Id, Defaults.Terminal.IsOpen, Defaults.User.Id, openingAmount);
                    //UPDATE TERMINAL SESSION OBJECT
                    Defaults.Terminal = new Terminal
                    {
                        Id = terminal.Id,
                        CashDrawer = terminal.CashDrawer,
                        Category = new Category { Id = terminal.Category.Id, Name = terminal.Category.Name, Parant = terminal.Category.Parant, CategoryLevel = terminal.Category.CategoryLevel },
                        Created = terminal.Created,
                        Description = terminal.Description,
                        HardwareAddress = terminal.HardwareAddress,
                        IsDeleted = terminal.IsDeleted,
                        Outlet = new Outlet { Id = terminal.Outlet.Id, Name = terminal.Outlet.Name, Address1 = terminal.Outlet.Address1, Address2 = terminal.Outlet.Address2, Address3 = terminal.Outlet.Address3, City = terminal.Outlet.City, PostalCode = terminal.Outlet.PostalCode, BillPrinterId = terminal.Outlet.BillPrinterId, KitchenPrinterId = terminal.Outlet.KitchenPrinterId, Email = terminal.Outlet.Email, Phone = terminal.Outlet.Phone, OrgNo = terminal.Outlet.OrgNo, WebUrl = terminal.Outlet.WebUrl, TaxDescription = terminal.Outlet.TaxDescription, HeaderText = terminal.Outlet.HeaderText, FooterText = terminal.Outlet.FooterText, Created = terminal.Outlet.Created, Updated = terminal.Outlet.Updated },
                        RootCategoryId = terminal.Category.Id,
                        Status = terminal.Status,
                        TerminalNo = terminal.TerminalNo,
                        UniqueIdentification = terminal.UniqueIdentification,
                        Updated = terminal.Updated,
                        TerminalType = terminal.TerminalType,
                        OutletId = terminal.Outlet.Id
                    };

                }
                // Log cashdrawer
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.TerminalOpened));
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                // throw ex;
            }
            // Set terminalstatus

            VerifyTerminalOpen();
        }

        private void OpenTerminal()
        {
            try
            {
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {

                    var terminal = loginRepo.OpenTerminal(Defaults.Terminal.Id, Defaults.Terminal.IsOpen, Defaults.User.Id);
                    Defaults.Terminal = new Terminal
                    {
                        Id = terminal.Id,
                        CashDrawer = terminal.CashDrawer,
                        Category = new Category { Id = terminal.Category.Id, Name = terminal.Category.Name, Parant = terminal.Category.Parant, CategoryLevel = terminal.Category.CategoryLevel },
                        Created = terminal.Created,
                        Description = terminal.Description,
                        HardwareAddress = terminal.HardwareAddress,
                        IsDeleted = terminal.IsDeleted,
                        Outlet = new Outlet { Id = terminal.Outlet.Id, Name = terminal.Outlet.Name, Address1 = terminal.Outlet.Address1, Address2 = terminal.Outlet.Address2, Address3 = terminal.Outlet.Address3, City = terminal.Outlet.City, PostalCode = terminal.Outlet.PostalCode, BillPrinterId = terminal.Outlet.BillPrinterId, KitchenPrinterId = terminal.Outlet.KitchenPrinterId, Email = terminal.Outlet.Email, Phone = terminal.Outlet.Phone, OrgNo = terminal.Outlet.OrgNo, WebUrl = terminal.Outlet.WebUrl, TaxDescription = terminal.Outlet.TaxDescription, HeaderText = terminal.Outlet.HeaderText, FooterText = terminal.Outlet.FooterText, Created = terminal.Outlet.Created, Updated = terminal.Outlet.Updated },
                        RootCategoryId = terminal.Category.Id,
                        Status = terminal.Status,
                        TerminalNo = terminal.TerminalNo,
                        UniqueIdentification = terminal.UniqueIdentification,
                        Updated = terminal.Updated,
                        TerminalType = terminal.TerminalType,
                        OutletId = terminal.Outlet.Id
                    };
                }
                // Log cashdrawer
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.TerminalOpened));
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                // throw ex;
            }
        }

        public OutletUser CheckUserKey()
        {
            try
            {
                string username = _view.GetUsername();
                string password = _view.GetPassword();
                string strPd = CalculateSha1(password, Encoding.UTF8);
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {

                    return loginRepo.CheckUserKey(username, strPd);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                return null;
            }
        }
        private bool IsValid(string userId, string password, bool isLogedIn)
        {
            try
            {
                string strPd = CalculateSha1(password, Encoding.UTF8);

                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {

                    var user = loginRepo.ValidateLogin(userId, strPd, isLogedIn);
                    if (user != null)
                    {
                        Defaults.User = new OutletUser
                        {
                            Id = user.Id,
                            UserCode = user.UserCode,
                            UserName = user.UserName,
                            DallasKey = user.DallasKey,
                            Email = user.Email,

                        };

                        DefaultsIntegration.User = new TillUser
                        {
                            Id = user.Id,
                            UserCode = user.UserCode,
                            UserName = user.UserName,
                            DallasKey = user.DallasKey,
                            Email = user.Email,
                        };

                        if (!DefaultsIntegration.IsInitialized)
                        {
                            DefaultsIntegration.Init();
                            DefaultsIntegration.CannotConnectToController = UI.CannotConnectToController;
                            DefaultsIntegration.CannotRegisterToController = UI.CannotRegisterToController;
                            DefaultsIntegration.Message_CashDrawerUnitControlNotDefined = UI.Message_CashDrawerUnitControlNotDefined;

                        }

                        return true;
                    }
                    else
                        return false;

                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return false;
            }
        }

        public OutletUser IsRegisterByDallasKey()
        {
            try
            {
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {
                    var user = loginRepo.IsRegisterByDallasKey(Defaults.DallasKey);
                    if (user != null)
                    {

                        return new OutletUser
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            DallasKey = user.DallasKey,
                            Email = user.Email,

                        };
                    }
                    else
                        return null;
                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return null;
            }
        }

        private bool IsLoggedInByDallasKey(OutletUser user)
        {
            try
            {
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {
                    return loginRepo.IsLoggedInByDallasKey(user, Defaults.User.Id);
                }
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return false;
            }
        }
        internal string RegisterUser(OutletUser userModel, bool isNewUser)
        {
            string msg;
            try
            {
                string strPd = CalculateSha1(userModel.Password, Encoding.UTF8);
                OutletUser tillUser;
                using (var loginRepo = new LoginRepository(PosState.GetInstance().Context))
                {
                    msg = loginRepo.RegisterUser(userModel, strPd, Defaults.DallasKey, out tillUser);
                }

                if (tillUser != null)
                {

                    Defaults.User = tillUser;

                    msg = "Success:User Registerd";
                    //Utils.ServiceClient client = new Utils.ServiceClient(Defaults.SyncAPIUri, "", "");
                    //var userViewModel = new UserModel
                    //{
                    //    Id = tillUser.Id,
                    //    UserName = userModel.UserName,
                    //    UserCode = userModel.UserCode,
                    //    Password = userModel.Password,
                    //    Email = userModel.Email,
                    //    DallasKey = userModel.DallasKey,
                    //    OutletId = Defaults.Outlet.Id,
                    //    TerminalId = Defaults.Terminal.Id
                    //};
                    //client.PostAndGetUser(userViewModel);

                }
                return msg;
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                msg = "Error:" + exp.Message;
                return msg;
            }
        }


        private static string CalculateSha1(string text, Encoding enc)
        {
            try
            {
                byte[] buffer = enc.GetBytes(text);
                var cryptoTransformSha1 = new SHA1CryptoServiceProvider();
                return BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");
            }
            catch (Exception exp)
            {
                LogWriter.LogWrite(exp);
                return string.Empty;
            }
        }





    }
}