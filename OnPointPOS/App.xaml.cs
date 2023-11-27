using POSSUM.Res;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex mutex = new Mutex(true, "{AC162FCE-20DE-420C-8780-8C8CD31F5F9E}");
        //receipt
        public PosState State { get; set; }
        public new static MainWindow MainWindow { get; set; }
        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string architectureSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    Environment.Is64BitProcess ? "x64" : "x86",
                    assemblyName);

                return File.Exists(architectureSpecificPath)
                    ? Assembly.LoadFile(architectureSpecificPath)
                    : null;
            }

            return null;
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogWriter.LogWrite("PaymentPresenter => onTerminalNeedClose: " + e.Exception.ToString());

            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "POSSUM", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    State = new PosState();
                    Defaults.UICultureInfo = new CultureInfo(Defaults.CultureString);
                    UI.Culture = (CultureInfo)Defaults.UICultureInfo;
                    /**
                     * SET the UI Currency
                     **/
                    FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                        new FrameworkPropertyMetadata(
                            XmlLanguage.GetLanguage(((CultureInfo)Defaults.CurrencyCultureInfo).IetfLanguageTag)));
                }
                catch (Exception ex)
                {

                }
                
                base.OnStartup(e);
            }
            else
            {
                Environment.Exit(0);
            }

            /*
                        PaymentWindow w = new PaymentWindow(200, 50, 10);
                        if (w.ShowDialog() ?? false)
                        {
                            return;
                        }
                        else
                        {
                            return;
                        }*/

            /**
                         * SET the UI Language 
                         **/
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogWriter.LogWrite("App => CurrentDomain_UnhandledException: " + e.ExceptionObject.ToString());
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogWriter.LogWrite("App => Current_DispatcherUnhandledException: " + e.Exception.ToString());
        }
    }
}
