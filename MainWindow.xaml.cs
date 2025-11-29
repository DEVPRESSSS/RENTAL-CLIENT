using Rental.Areas.Admin.UCPages;
using Rental.Auth;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rental
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _user;
        public MainWindow(string user)
        {
            InitializeComponent();
            MainContentArea.Content = new Dashboard();
            _user = user;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TenantsBtn_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Tenants("ROLE-101");
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

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Properties("ROLE-101");

        }

        private void Contracts_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Contracts("ROLE-101",_user);

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

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Dashboard();

        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Payments();
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new Users();
        }
    }
}