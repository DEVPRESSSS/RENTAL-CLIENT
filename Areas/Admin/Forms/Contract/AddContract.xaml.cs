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

namespace Rental.Areas.Admin.Forms.Contract
{
    /// <summary>
    /// Interaction logic for AddContract.xaml
    /// </summary>
    public partial class AddContract : Window
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public event EventHandler tenantCreated;
        public AddContract()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

            LoadTenants();
            LoadProperties();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            CreateContract();
        }

        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Deposit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);
        }

        private void Deposit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void MonthlyRent_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void MonthlyRent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadTenants()
        {
            var tenants = new List<TenantModel>();
            string query = "SELECT TenantID, Name FROM Tenants ORDER BY Name ASC";

            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenants.Add(new TenantModel
                        {
                            TenantID = reader["TenantID"].ToString(),
                            Name = reader["Name"].ToString()
                        });
                    }
                }

                TenantName.ItemsSource = tenants;
                TenantName.DisplayMemberPath = "Name";
                TenantName.SelectedValuePath = "TenantID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tenants: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void LoadProperties()
        {
            var properties = new List<PropertyModel>();
            string query = "SELECT PropertyID, PropertyName, MonthlyRent FROM Properties WHERE Status = 'Available' ORDER BY PropertyName ASC";

            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        properties.Add(new PropertyModel
                        {
                            PropertyID = reader["PropertyID"].ToString(),
                            PropertyName = reader["PropertyName"].ToString(),
                            MonthlyRent = reader["MonthlyRent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["MonthlyRent"])
                                : 0
                        });
                    }
                }

                PropertyName.ItemsSource = properties;
                PropertyName.DisplayMemberPath = "PropertyName";
                PropertyName.SelectedValuePath = "PropertyID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading properties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void PropertyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PropertyName.SelectedItem is PropertyModel selectedProperty)
            {
                MonthlyRent.Text = selectedProperty.MonthlyRent?.ToString("F2");
            }
        }

        private void CreateContract()
        {
            string query = @"
            INSERT INTO RentalContracts 
            (ContractID, PropertyID, TenantID, StartDate, EndDate, MonthlyRent, DepositAmount, Status, CreatedAt)
            VALUES 
            (@ContractID, @PropertyID, @TenantID, @StartDate, @EndDate, @MonthlyRent, @DepositAmount, @Status, @CreatedAt)";

            try
            {
                // Validate required fields
                if (PropertyName.SelectedValue == null ||
                    TenantName.SelectedValue == null ||
                    StartDate.SelectedDate == null ||
                    EndDate.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(MonthlyRent.Text) ||
                    string.IsNullOrWhiteSpace(Deposit.Text))
                {
                    MessageBox.Show("All required fields must be filled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();
                    return;
                }
                DateTime startDate = StartDate.SelectedDate.Value;
                DateTime endDate = EndDate.SelectedDate.Value;

                if (endDate < startDate)
                {
                    MessageBox.Show("End Date cannot be earlier than Start Date.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal minimumDeposit = 0;
                //Calculate deposit
                if(decimal.TryParse(Deposit.Text, out var deposit))
                {
                    minimumDeposit = Convert.ToDecimal(MonthlyRent.Text) / 2;
                    if(deposit < minimumDeposit)
                    {
                        MessageBox.Show("Minimum deposit should be 50% or Higher of the Rent", "Deposit minimum", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                string? selectedPropertyId = ((PropertyModel)PropertyName.SelectedItem).PropertyID;
                // Open connection
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@ContractID", $"CONTRACT-{Guid.NewGuid().ToString().ToUpper().Substring(0, 10)}");
                    cmd.Parameters.AddWithValue("@PropertyID", ((PropertyModel)PropertyName.SelectedItem).PropertyID);
                    cmd.Parameters.AddWithValue("@TenantID", ((TenantModel)TenantName.SelectedItem).TenantID);
                    cmd.Parameters.AddWithValue("@StartDate", StartDate.SelectedDate.Value);
                    cmd.Parameters.AddWithValue("@EndDate", EndDate.SelectedDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthlyRent", Convert.ToDecimal(MonthlyRent.Text));
                    cmd.Parameters.AddWithValue("@DepositAmount", deposit);
                    cmd.Parameters.AddWithValue("@Status", ((ComboBoxItem)Status.SelectedItem).Content.ToString());
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Contract successfully added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        tenantCreated?.Invoke(this, EventArgs.Empty);

                        sqlConnection.Close();

                        //Update property availability
                        UpdateStatusOfProperty(selectedPropertyId);
                        Clear();

                    }
                    else
                    {
                        MessageBox.Show("Failed to add contract.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Clear();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Clear(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Clear(); ;
            }
            finally
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void Clear()
        {

            PropertyName.SelectedItem = null;
            TenantName.SelectedItem = null;
            StartDate.SelectedDate = null;
            EndDate.SelectedDate = null;
            Status.SelectedItem = null;
            MonthlyRent.Clear();
            Deposit.Clear();
        }

        private void UpdateStatusOfProperty(string? propertyId)
        {

            string query = @"UPDATE Properties SET Status = 'Occupied' WHERE PropertyID = @PropertyID";

            try
            {
             
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PropertyID",propertyId);
                 

                    int rowsAffected = cmd.ExecuteNonQuery();

                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Clear(); ;
            }
            finally
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }
        }
    }
}
