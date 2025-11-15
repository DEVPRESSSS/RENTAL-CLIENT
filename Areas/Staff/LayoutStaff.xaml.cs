using Rental.Areas.Admin.UCPages;
using Rental.Auth;
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

namespace Rental.Areas.Staff
{
    /// <summary>
    /// Interaction logic for LayoutStaff.xaml
    /// </summary>
    public partial class LayoutStaff : Window
    {

        private string _role;
        public LayoutStaff(string role)
        {
            InitializeComponent();
            _role = role;
            MainContentArea.Content = new Contracts(_role);

        }

        private void Contracts_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Contracts(_role);

        }

        private void TenantsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Tenants(_role);

        }

        //private void Properties_Click(object sender, RoutedEventArgs e)
        //{
        //    MainContentArea.Content = new Properties(_role);

        //}

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {

                WindowState = WindowState.Minimized;
            }
            else
            {

                WindowState = WindowState.Normal;

            }
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;

            }
            else
            {

                WindowState = WindowState.Normal;

            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                var logout = new Login();
                logout.Show();
                this.Close();

            }
        }
    }
}
