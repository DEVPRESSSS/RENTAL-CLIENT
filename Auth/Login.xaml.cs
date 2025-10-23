using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
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

namespace Rental.Auth
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public Login()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

        }

        private void Usernametxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.UsernameTextComposition(sender, e);

        }

        private void Usernametxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Usernametxt.Text.Length > 0)
            {

                //UsernameErrorMessage.Text = "";
                Usernametxt.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272"));
                Usernametxt.BorderThickness = new Thickness(0, 0, 0, 2);

            }
        }

        private void Usernametxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Passwordtxt_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Passwordtxt.Password.Length > 0)
            {
                Eye.Visibility = Visibility.Visible;
                Passwordtxt.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#727272"));
                Passwordtxt.BorderThickness = new Thickness(0, 0, 0, 2);
            }
            else
            {
                Eye.Visibility = Visibility.Collapsed;
            }
        }

        private void Passwordtxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void Passwordtxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void PasswordUnmask_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordUnmask.Text.Length == 0)
            {

                Eye2.Visibility = Visibility.Hidden;
            }
            else
            {
                Eye2.Visibility = Visibility.Visible;
            }
        }

        private void Eye_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Auth();
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
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

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Eye2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PasswordUnmask.Visibility == Visibility.Visible)
            {
                Eye.Visibility = Visibility.Visible;
                Eye2.Visibility = Visibility.Collapsed;

                Passwordtxt.Password = PasswordUnmask.Text;
                Passwordtxt.Visibility = Visibility.Visible;
                PasswordUnmask.Visibility = Visibility.Hidden;

            }
        }

        private void Eye_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (Passwordtxt.Visibility == Visibility.Visible)
            {
                Eye.Visibility = Visibility.Collapsed;
                Eye2.Visibility = Visibility.Visible;

                PasswordUnmask.Text = Passwordtxt.Password;
                PasswordUnmask.Visibility = Visibility.Visible;
                Passwordtxt.Visibility = Visibility.Hidden;

            }
            else
            {

                PasswordUnmask.Visibility = Visibility.Collapsed;

                Passwordtxt.Visibility = Visibility.Visible;

            }

        }

        private void forgotpassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ForgotPassword pass = new ForgotPassword();
            pass.Show();
            this.Close();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
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

        private void ClearTextBoxes()
        {
            Usernametxt.Text = "";
            Passwordtxt.Password = "";
            PasswordUnmask.Text = "";
        }

        private void Auth()
        {
            // Use whichever textbox is currently visible for password
            string username = Usernametxt.Text.Trim();
            string password = Passwordtxt.Visibility == Visibility.Visible
                                ? Passwordtxt.Password
                                : PasswordUnmask.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ClearTextBoxes();
                return;
            }

            try
            {
                using (sqlConnection = new SqlConnection(connection.ConnectionString))
                {
                    sqlConnection.Open();

                    string query = @"
                        SELECT RoleID
                        FROM Users
                        WHERE Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS
                          AND PasswordHash = @PasswordHash COLLATE SQL_Latin1_General_CP1_CS_AS"
                    ;
                    ;

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@PasswordHash", password);

                        var role = cmd.ExecuteScalar() as string;

                        if (!string.IsNullOrEmpty(role))
                        {
                            string user = username;

                            if (role.Equals("ROLE-101", StringComparison.OrdinalIgnoreCase))
                            {
                                MainWindow main = new MainWindow();
                                main.Show();
                                this.Close();
                            }
                         
                        }
                        else
                        {
                            MessageBox.Show("Invalid Username or Password", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            ClearTextBoxes();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ClearTextBoxes();
            }
        }

    }
}
