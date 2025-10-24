using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.UCPages;
using Rental.DatabaseConnection;
using Rental.Models;
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

namespace Rental.Areas.Admin.Forms.Pay
{
    /// <summary>
    /// Interaction logic for Add.xaml
    /// </summary>
    public partial class Add : Window
    {

        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        string? tenantId = "";
        string? contractId = "";


        public event EventHandler paymentUpdated;
        public Add()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            CreateContract();
        }

        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CreateContract()
        {
            string query = @"
            INSERT INTO Payments 
            (PaymentId, TenantId, Amount, DateOfPayment, ContractID)
            VALUES 
            (@PaymentId, @TenantId, @Amount, @DateOfPayment, @ContractID)";

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(Amount.Text))
                {
                    MessageBox.Show("All required fields must be filled.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Open connection
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PaymentId", $"CONTRACT-{Guid.NewGuid().ToString().ToUpper().Substring(0, 10)}");
                    cmd.Parameters.AddWithValue("@TenantId", tenantId);
                    cmd.Parameters.AddWithValue("@Amount", Amount.Text.Trim());
                    cmd.Parameters.AddWithValue("@DateOfPayment", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ContractID", contractId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Payment added successfully!,Contract renewed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        sqlConnection.Close();

                        UpdateDates();
                        paymentUpdated?.Invoke(this, EventArgs.Empty);

                        this.Close();


                    }
                    else
                    {
                        MessageBox.Show("Failed to add payment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = DataContext as RentalContractsModel;
            if (data != null)
            {
                tenantId = data.TenantID;
                contractId = data.ContractID;
                TenantName.Text = data.FullName;
                PropertyName.Text = data.PropName;
                Amount.Text = data.MonthlyRent.ToString();
            }
        }

        private void Amount_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Amount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);

        }

        private void Amount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void UpdateDates()
        {
            string query = @"UPDATE RentalContracts SET StartDate = @StartDate, EndDate = @EndDate WHERE ContractID = @ContractID";

            try
            {
                DateTime startDate = DateTime.Now;
                DateTime endDate = startDate.AddDays(30);

                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@StartDate",DateTime.Now );
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@ContractID", contractId);


                    int rowsAffected = cmd.ExecuteNonQuery();

                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == System.Data.ConnectionState.Open)
                    sqlConnection.Close();
            }

        }
    }
}
