using POSSUM.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for DeviceConnectionWindow.xaml
    /// </summary>
    public partial class CUConnectionWindow : Window
    {
        //  System.Timers.Timer timer;
        public CUConnectionWindow()
        {
            InitializeComponent();
            //timer = new System.Timers.Timer();
            //timer.Elapsed += Timer_Elapsed;
            //timer.Interval = 1000 * 2;
            //timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool connected = false;

                var cuAction = PosState.GetInstance().ControlUnitAction;
                cuAction.ControlUnit.Close();
                cuAction.ControlUnit.Open();
                ControlUnitStatus status = cuAction.ControlUnit.CheckStatus();
                connected = (status == ControlUnitStatus.OK);


                if (connected)
                {
                    this.DialogResult = true;

                }
                else
                {
                    txtMessage.Text = "Fel!" + Environment.NewLine + "Anslutningsfel till kontrollenheten. Kontrollera utrustningen.";
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
    }
}
