using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace Rental.Areas.Admin
{
    /// <summary>
    /// Interaction logic for AddTenant.xaml
    /// </summary>
    public partial class AddTenant : Window
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public event EventHandler tenantCreated;

        public AddTenant()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
        }

        private void Email_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.EmailTextComposition(sender, e);

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void CreateTenant()
        {
            string query = @"INSERT INTO Tenants (TenantID, Name, Contact, Address, Email, DateJoined)
                     VALUES (@TenantID, @Name, @Contact, @Address, @Email, @DateJoined)";

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(TenantName.Text) ||
                    string.IsNullOrWhiteSpace(Contact.Text) ||
                    string.IsNullOrWhiteSpace(Address.Text) ||
                    string.IsNullOrWhiteSpace(Email.Text))
                {
                    MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();

                    return;
                }

                // Validate contact number
                if (Contact.Text.Length < 11)
                {
                    MessageBox.Show("Contact must be 11 digits.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();

                    return;
                }

                // Open connection
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@TenantID", $"TENANT-{Guid.NewGuid().ToString().ToUpper().Substring(0, 10)}");
                    cmd.Parameters.AddWithValue("@Name", TenantName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Contact", Contact.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", Address.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());
                    cmd.Parameters.AddWithValue("@DateJoined", DateTime.Now);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Tenant added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Clear();
                        tenantCreated?.Invoke(this, EventArgs.Empty);

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
                Clear();

            }
        }

        private void Clear()
        {

            TenantName.Clear();
            Contact.Clear();
            Address.Clear();
            Email.Clear();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            CreateTenant();
        }

        private void TenantName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.PersonNameTextKeyDown(sender, e);
        }

        private void TenantName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.PersonNameTextComposition(sender, e);

        }

        private void Contact_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Contact_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);

        }

        private void Address_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Address_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
