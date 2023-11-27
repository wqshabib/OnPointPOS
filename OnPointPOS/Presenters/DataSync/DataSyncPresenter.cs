using System;
using System.Threading;
using POSSUM.Utils.Controller;

namespace POSSUM.Presenters.DataSync
{
    public class DataSyncPresenter
    {
        private readonly IDataSyncView _view;

        public DataSyncPresenter(IDataSyncView view)
        {
            _view = view;
        }

        public void HandelUserSyncClick()
        {
            try
            {
                bool res = false;
                var progressDialog = new ProgressWindow();

                var backgroundThread = new Thread(() =>
                {
                    UserController userController = new UserController(Defaults.LocalConnectionString);
                    res = userController.UpdateUser();
                    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                if (res)
                    _view.Message("Users updated successfully");
                else
                    _view.ShowError("Error", "Error in updating user see log for detail");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error", ex.Message);
            }
        }

        public void HandelOrderSyncClick()
        {
            try
            {
                bool res = false;
                var progressDialog = new ProgressWindow();

                var backgroundThread = new Thread(() =>
                {
                    OrderController orderController = new OrderController(Defaults.LocalConnectionString);
                    res = orderController.UpdateOrder();
                    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                if (res)
                    _view.Message("Orders updated successfully");
                else
                    _view.ShowError("Error", "Error in updating order see log for detail");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error", ex.Message);
            }
        }

        public void HandelSettingSyncClick()
        {
            try
            {
                bool res = false;
                var progressDialog = new ProgressWindow();

                var backgroundThread = new Thread(() =>
                {
                    SettingController settingController = new SettingController(Defaults.LocalConnectionString, PosState.GetInstance().Context);
                    res = settingController.UpdateSettings();
                    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                if (res)
                    _view.Message("Outlet and Terminal updated successfully");
                else
                    _view.ShowError("Error", "Error in updating outlet see log for detail");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error", ex.Message);
            }
        }

        public void HandelProductSyncClick()
        {
            try
            {
                bool res = false;
                var progressDialog = new ProgressWindow();

                var backgroundThread = new Thread(() =>
                {
                    var productController = new ProductController(PosState.GetInstance().Context);
                    res = productController.UpdateProduct();
                    progressDialog.Closed += (arg, ev) => { progressDialog = null; };
                    progressDialog.Dispatcher.BeginInvoke(new Action(() => { progressDialog.Close(); }));
                });
                backgroundThread.Start();
                progressDialog.ShowDialog();
                backgroundThread.Join();
                if (res)
                    _view.Message("Product and Categories updated successfully");
                else
                    _view.ShowError("Error", "Error in updating product see log for detail");
            }
            catch (Exception ex)
            {
                _view.ShowError("Error", ex.Message);
            }
        }
    }
}