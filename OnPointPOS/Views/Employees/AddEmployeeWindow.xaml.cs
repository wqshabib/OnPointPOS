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
using POSSUM.Model;
using POSSUM.Presenters.Employees;
using POSSUM.Res;

namespace POSSUM
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class AddEmployeeWindow : Window, IEmployeeView
    {
        EmployeePresenter presenter;
        public Employee currentEmployee = null;
        public AddEmployeeWindow(string ssNo)
        {
            InitializeComponent();
            presenter = new EmployeePresenter(this);
            currentEmployee = new Employee();
            currentEmployee.SSNO = ssNo;
            layoutGrid.DataContext = currentEmployee;
            txtFirstName.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SaveEmployee();
        }
        private void SaveEmployee()
        {
            try
            {
                if (string.IsNullOrEmpty(currentEmployee.FirstName))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(currentEmployee.LastName))
                {
                    MessageBox.Show(UI.Message_NameMissing, UI.Global_Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(currentEmployee.Email))
                {
                    currentEmployee.Email = " ";
                }
                currentEmployee.Id = Guid.NewGuid();
                currentEmployee.Updated = DateTime.Now;
                bool res = presenter.SaveEmployee(currentEmployee);
                if (res)
                    this.DialogResult = true;
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
            }
        }
        private void BtnCloseAccountInfo_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public void ShowError(string title, string message)
        {
            LogWriter.LogWrite(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtLastName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SaveEmployee();
        }
    }
}
