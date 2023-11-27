using POSSUM.Model;
using POSSUM.Presenters.Employees;
using POSSUM.Res;
using POSSUM.Views.PrintOrder;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POSSUM
{

    public partial class EmployeeWindow : Window, IEmployeeView
    {

        EmployeePresenter presenter;
        public Employee SelectedEmployee { get; set; }


        public EmployeeWindow()
        {
            InitializeComponent();
            presenter = new EmployeePresenter(this);


        }



        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }


        private void Search_Click(object sender, RoutedEventArgs e)
        {
            presenter.GetEmplooyeeBySSN("");
        }




        private void New_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow newEmployee = new AddEmployeeWindow(txtSSN.Text);
            if (newEmployee.ShowDialog() == true)
            {
                SelectedEmployee = newEmployee.currentEmployee;
                btnNewEmployee.Visibility = Visibility.Collapsed;
                CheckInButton.IsEnabled = true;
                // CheckOutButton.IsEnabled = true;
            }
            else
            {
                CheckInButton.IsEnabled = false;
                CheckOutButton.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSSN.Text))
            {
                if (txtSSN.Text.Length == 12)
                    return;
            }
            txtSSN.Text = txtSSN.Text + (sender as Button).Content;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            txtSSN.Text = "";
            txtSSN.Focus();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {

            PrintEmployeeLogWindow printEmployeeLog = new PrintEmployeeLogWindow();
            printEmployeeLog.ShowDialog();

        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            SaveLog(false);
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            SaveLog(true);
        }
        private void SaveLog(bool isCheckIn)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSSN.Text))
                {
                    MessageBox.Show("Enter SSN to proceed", Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    txtSSN.Focus();
                    return;
                }
                else
                {
                    bool res = presenter.SaveLog(SelectedEmployee, isCheckIn);
                    if (res)
                        this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
        private void txtSSN_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            decimal val = 0;
            e.Handled = !decimal.TryParse(e.Text, out val);
        }

        private void txtSSN_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }
        private void txtSSN_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtSSN.Text.Length == 12)
                {
                    SelectedEmployee = presenter.GetEmplooyeeBySSN(txtSSN.Text);
                    if (SelectedEmployee == null)
                    {
                        MessageBox.Show("User not found", Defaults.AppProvider.AppTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        btnNewEmployee.Visibility = Visibility.Visible;
                        CheckInButton.IsEnabled = false;
                        CheckOutButton.IsEnabled = false;
                    }
                    else
                    {
                        btnNewEmployee.Visibility = Visibility.Collapsed;
                        if (SelectedEmployee.Logs != null && SelectedEmployee.Logs.Count > 0)
                            CheckOutButton.IsEnabled = true;
                        else
                            CheckInButton.IsEnabled = true;

                    }
                }
                else
                {
                    btnNewEmployee.Visibility = Visibility.Collapsed;
                    CheckInButton.IsEnabled = false;
                    CheckOutButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }

        }
    }


    //Place Holder 
    public class PlaceHodlerData
    {
        public string PlaceHolder { get; set; }
    }
    public class TextBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(TextBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));



        public static int GetTextLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetTextLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("TextLength", typeof(int), typeof(TextBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as TextBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.TextChanged += TextChanged;
            }
            else
            {
                pb.TextChanged -= TextChanged;
            }
        }

        static void TextChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as TextBox;
            if (pb == null)
            {
                return;
            }
            SetTextLength(pb, pb.Text.Length);
        }
    }
}
