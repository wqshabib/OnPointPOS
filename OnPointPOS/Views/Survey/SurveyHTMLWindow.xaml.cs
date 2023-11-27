using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace POSSUM.Views.Survey
{
    /// <summary>
    /// Interaction logic for SurveyHTMLWindow.xaml
    /// </summary>
    public partial class SurveyHTMLWindow : Window
    {
        public SurveyHTMLWindow()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            wbMain.Address = ConfigurationManager.AppSettings["SurveyURL"];
        }
    }
}
