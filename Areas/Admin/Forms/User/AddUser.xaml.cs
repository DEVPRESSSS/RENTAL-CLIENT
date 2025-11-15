using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public event EventHandler userCreated;
        public AddUser()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            CreateTenant();
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
            HelperValidation.ValidationHelper.NoSpaceOnly(sender,e);    
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

        private void PasswordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);
        }

        private void PasswordInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void CreateTenant()
        {
            string query = @"INSERT INTO Users (UserID, FullName, Phone, Username, PasswordHash, Email, RoleID, CreatedAt)
                     VALUES (@UserID, @FullName, @Phone, @Username, @PasswordHash, @Email, @RoleID, @CreatedAt)";

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(UserFullname.Text) ||
                    string.IsNullOrWhiteSpace(Contact.Text) ||
                    string.IsNullOrWhiteSpace(Username.Text) ||
                    string.IsNullOrWhiteSpace(PasswordInput.Text) ||
                    string.IsNullOrWhiteSpace(Email.Text))
                {
                    MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate contact number
                if (Contact.Text.Length < 11)
                {
                    MessageBox.Show("Contact must be 11 digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Open connection
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@UserID", $"USER-{Guid.NewGuid().ToString().ToUpper().Substring(0, 10)}");
                    cmd.Parameters.AddWithValue("@FullName", UserFullname.Text.Trim());
                    cmd.Parameters.AddWithValue("@Phone", Contact.Text.Trim());
                    cmd.Parameters.AddWithValue("@Username", Username.Text.Trim());
                    cmd.Parameters.AddWithValue("@PasswordHash", PasswordInput.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@RoleID", "ROLE-102");
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Clear();
                        userCreated?.Invoke(this, EventArgs.Empty);

                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("Duplicate Name, Contact, or Email detected. Please use unique values.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Clear();

                }
                else
                {
                    MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();

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
            PasswordInput.Clear();
            Email.Clear();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
