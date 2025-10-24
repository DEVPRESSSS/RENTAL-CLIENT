using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.UCPages;
using Rental.DatabaseConnection;
using Rental.Models;
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

namespace Rental.Areas.Admin.Forms.Contract
{
    /// <summary>
    /// Interaction logic for EditContract.xaml
    /// </summary>
    public partial class EditContract : Window
    {

        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;

        public event EventHandler tenantUpdated;
        public EditContract()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            LoadTenants();
            LoadProperties();
        }

        private void PropertyName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PropertyName.SelectedItem is PropertyModel selectedProperty)
            {
                MonthlyRent.Text = selectedProperty.MonthlyRent?.ToString("F2");
            }
        }

        private void MonthlyRent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void MonthlyRent_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Deposit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender,e);
        }

        private void Deposit_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        string? ContractID = "";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var contract = DataContext as RentalContractsModel;

            if (contract != null)
            {
                // Fill combobox selections if possible
                ContractID = contract.ContractID;
                PropertyName.Text = contract.PropName;
                TenantName.Text = contract.FullName;

                // Dates
                StartDate.SelectedDate = contract.StartDate;
                EndDate.SelectedDate = contract.EndDate;

                // Monetary values
                MonthlyRent.Text = contract.MonthlyRent?.ToString("0.00");
                Deposit.Text = contract.DepositAmount?.ToString("0.00");

                // Status
                Status.Text = contract.Status;

                
            }
        }

        private void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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

        private void Update()
        {

            // Validate required fields
            if (PropertyName.SelectedValue == null)
            {
                MessageBox.Show("Please select a property.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TenantName.SelectedValue == null)
            {
                MessageBox.Show("Please select a tenant.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!StartDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MonthlyRent.Text))
            {
                MessageBox.Show("Monthly rent is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmation
            if (MessageBox.Show("Are you sure you want to update this contract?", "Confirm Update",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            string query = @"
        UPDATE RentalContracts
        SET 
            PropertyID = @PropertyID,
            TenantID = @TenantID,
            StartDate = @StartDate,
            EndDate = @EndDate,
            MonthlyRent = @MonthlyRent,
            DepositAmount = @DepositAmount,
            Status = @Status
        WHERE ContractID = @ContractID";

            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PropertyID", PropertyName.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@TenantID", TenantName.SelectedValue.ToString());
                    cmd.Parameters.AddWithValue("@StartDate", StartDate.SelectedDate.Value);

                    if (EndDate.SelectedDate.HasValue)
                        cmd.Parameters.AddWithValue("@EndDate", EndDate.SelectedDate.Value);
                    else
                        cmd.Parameters.AddWithValue("@EndDate", DBNull.Value);

                    if (decimal.TryParse(MonthlyRent.Text, out decimal rent))
                        cmd.Parameters.AddWithValue("@MonthlyRent", rent);
                    else
                        cmd.Parameters.AddWithValue("@MonthlyRent", 0);

                    if (decimal.TryParse(Deposit.Text, out decimal deposit))
                        cmd.Parameters.AddWithValue("@DepositAmount", deposit);
                    else
                        cmd.Parameters.AddWithValue("@DepositAmount", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Status", Status.Text ?? "Active");
                    cmd.Parameters.AddWithValue("@ContractID", ContractID);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Contract updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        tenantUpdated?.Invoke(this, EventArgs.Empty);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No contract was updated. Please check the record.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating contract: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }
    }
}
