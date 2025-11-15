using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.UCPages;
using Rental.DatabaseConnection;
using Rental.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Net;
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

namespace Rental.Areas.Admin.Forms.User
{
    /// <summary>
    /// Interaction logic for UpdateUser.xaml
    /// </summary>
    public partial class UpdateUser : Window
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;

        public event EventHandler tenantUpdated;
        public UpdateUser()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            UpdateTenantInfo();
        }

        private void Email_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);
        }

        private void Email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.EmailTextComposition(sender, e);
        }

        private void Contact_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);
        }

        private void Contact_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);

        }

        private void UserFullname_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);
        }

        private void UserFullname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void Username_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Username_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.UsernameTextComposition(sender, e);

        }

        //private void PasswordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);
        //}

        //private void PasswordInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{

        //}

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        string? userId = "";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var tenant = DataContext as UserModel;

            if (tenant != null)
            {
                userId = tenant.UserId;
                UserFullname.Text = tenant.FullName;
                Contact.Text = tenant.Contact;
                Username.Text = tenant.Username;
                //PasswordInput.Text = tenant.Password;
                Email.Text = tenant.Email;
            }
        }

        private void UpdateTenantInfo()
        {
            string query = @"UPDATE Users
                     SET FullName = @FullName,
                         Phone = @Phone,
                         Username = @Username,
                         Email = @Email
                     WHERE UserID = @UserID";

            try
            {
                // Validation
               // Validate required fields
                if (string.IsNullOrWhiteSpace(UserFullname.Text) ||
                    string.IsNullOrWhiteSpace(Contact.Text) ||
                    string.IsNullOrWhiteSpace(Username.Text) ||
                    string.IsNullOrWhiteSpace(Email.Text))
                {
                    MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Contact.Text.Length < 11)
                {
                    MessageBox.Show("Contact must be 11 digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();
                    return;
                }

                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@FullName", UserFullname.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", Contact.Text.Trim());
                    cmd.Parameters.AddWithValue("@Username", Username.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@RoleID", "ROLE-102");

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User information updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        tenantUpdated?.Invoke(this, EventArgs.Empty);
                        this.Close(); // Close update window
                    }
                    else
                    {
                        MessageBox.Show("No user record found to update.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("Duplicate Name, Contact, or Email detected. Please use unique values.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"SQL Error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Clear();
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void Clear()
        {

            UserFullname.Clear();
            Contact.Clear();
            Username.Clear();
            Email.Clear();
        }
    }
}
