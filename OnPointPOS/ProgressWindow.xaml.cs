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
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
            //ResourceDictionary resourceDictionary = new ResourceDictionary();
            //resourceDictionary.Source = new Uri("/POSSUM;component/Theme/Default.xaml", UriKind.Relative);
            //var _loadingImage = resourceDictionary["ImageLoading"] as DrawingImage;
            //imgLoading.Source = _loadingImage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
