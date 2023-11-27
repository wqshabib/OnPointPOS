using POSSUM.Model;
using POSSUM.Presenters.PromptInfo;
using POSSUM.PromptInfo;
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
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class PromptInfoWindow : Window, IPromptInfoView
    {
        PromptInfoPresenter presenter;

        public PromptInfoWindow(PromptInfoPresenter.PromptInfoConfig config)
        {

            InitializeComponent();
            presenter = new PromptInfoPresenter(this, config);
            txtComments.Focus();

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            presenter.HandleOk();
        }

        private void btnCommentsCancel_Click(object sender, RoutedEventArgs e)
        {
            presenter.HandleCancel();
        }

        public void SetMultiline(bool multiline)
        {
            txtComments.AcceptsReturn = multiline;
        }

        public void SetTitle(string title)
        {
            txtTitle.Text = title;
        }

        public void SetDescription(string description)
        {
            txtDescription.Content = description;
        }

        public void SetValue(string value)
        {
            txtComments.Text = value;
            if (!string.IsNullOrEmpty(txtComments.Text))
            {
                txtComments.SelectionStart = txtComments.Text.Length; // add some logic if length is 0
                txtComments.SelectionLength = 0;
            }
        }

        public void ShowError(string title, string message)
        {
            throw new NotImplementedException();
        }

        public string GetValue()
        {
            return txtComments.Text;
        }

        public void CloseWithStatus(bool success)
        {
            this.DialogResult = success;
        }

    }
}
