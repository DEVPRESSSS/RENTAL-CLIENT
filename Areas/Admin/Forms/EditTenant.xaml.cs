using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
using Rental.Models;
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

namespace Rental.Areas.Admin.Forms
{
    /// <summary>
    /// Interaction logic for EditTenant.xaml
    /// </summary>
    public partial class EditTenant : Window
    {

        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;

        public event EventHandler tenantUpdated;
        public EditTenant()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
          
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        string? tenantId = "";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var tenant = DataContext as TenantModel;

            if (tenant != null)
            {
                tenantId = tenant.TenantID;
                TenantName.Text = tenant.Name;
                Contact.Text = tenant.Contact;
                Address.Text = tenant.Address;
                Email.Text = tenant.Email;
            }
        }


        private void Email_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Email_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.EmailTextComposition(sender, e);

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            UpdateTenantInfo();
        }

        private void UpdateTenantInfo()
        {
            string query = @"UPDATE Tenants
                     SET Name = @Name,
                         Contact = @Contact,
                         Address = @Address,
                         Email = @Email
                     WHERE TenantID = @TenantID";

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(TenantName?.Text) ||
                    string.IsNullOrWhiteSpace(Contact?.Text) ||
                    string.IsNullOrWhiteSpace(Address?.Text) ||
                    string.IsNullOrWhiteSpace(Email?.Text))
                {
                    MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Clear();
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
                    cmd.Parameters.AddWithValue("@TenantID", tenantId);
                    cmd.Parameters.AddWithValue("@Name", TenantName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Contact", Contact.Text.Trim());
                    cmd.Parameters.AddWithValue("@Address", Address.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", Email.Text.Trim());

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Tenant information updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        tenantUpdated?.Invoke(this, EventArgs.Empty); 
                        this.Close(); // Close update window
                    }
                    else
                    {
                        MessageBox.Show("No tenant record found to update.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            TenantName.Clear();
            Contact.Clear();
            Address.Clear();
            Email.Clear();
        }

        private void TenantName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.PersonNameTextKeyDown(sender, e);

        }

        private void TenantName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.PersonNameTextComposition(sender, e);

        }

        private void Contact_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);

        }

   
        private void Contact_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }
    }
}
