using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.Forms;
using Rental.Areas.Admin.Forms.Contract;
using Rental.Areas.Admin.Forms.Pay;
using Rental.DatabaseConnection;
using Rental.Models;
using Rental.Template;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rental.Areas.Admin.UCPages
{
    /// <summary>
    /// Interaction logic for Contracts.xaml
    /// </summary>
    public partial class Contracts : UserControl
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        private CollectionViewSource collectionViewSource;

        private string _role;
        public Contracts(string role)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            collectionViewSource = new CollectionViewSource();

            FetchAllContracts();
            _role = role;
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedTenant = btn.DataContext as RentalContractsModel;

            if (selectedTenant != null)
            {
                EditContract updateTenant = new EditContract
                {
                    DataContext = selectedTenant
                };

                updateTenant.tenantUpdated += (s, e) => {

                    FetchAllContracts();
                };

                updateTenant.ShowDialog();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedClient = btn.DataContext as RentalContractsModel;

            if (selectedClient != null)
            {

                MessageBoxResult result = MessageBox.Show(
                  "Are you sure you want to delete this contracts?",
                  "Confirmation",
                  MessageBoxButton.YesNo,
                  MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM RentalContracts WHERE ContractID = @ContractID", sqlConnection);
                        cmd.Parameters.AddWithValue("@ContractID", selectedClient.ContractID);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Contract deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        sqlConnection.Close();

                        FetchAllContracts();
                    }
                    catch 
                    {
                        MessageBox.Show("This contract has related records in payments", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        sqlConnection.Close();

                    }

                }

            }
        }

        private void NewPopertiesButton_Click(object sender, RoutedEventArgs e)
        {
            AddContract tenant = new AddContract();

            tenant.tenantCreated += (s, e) => {
                FetchAllContracts();
            };
            tenant.ShowDialog();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collectionViewSource?.View != null)
            {
                collectionViewSource.View.Filter = item =>
                {
                    var contract = item as RentalContractsModel;
                    if (contract == null) return false;

                    string searchText = Search.Text.Trim().ToLower();
                    if (string.IsNullOrEmpty(searchText))
                        return true; // Show all if no search text

                    return
                        (!string.IsNullOrEmpty(contract.ContractID) && contract.ContractID.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(contract.PropertyID) && contract.PropertyID.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(contract.PropName) && contract.PropName.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(contract.TenantID) && contract.TenantID.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(contract.FullName) && contract.FullName.ToLower().Contains(searchText)) ||
                        (contract.StartDate.HasValue && contract.StartDate.Value.ToString("yyyy-MM-dd").Contains(searchText)) ||
                        (contract.EndDate.HasValue && contract.EndDate.Value.ToString("yyyy-MM-dd").Contains(searchText)) ||
                        (contract.MonthlyRent.HasValue && contract.MonthlyRent.Value.ToString().Contains(searchText)) ||
                        (contract.DepositAmount.HasValue && contract.DepositAmount.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(contract.Status) && contract.Status.ToLower().Contains(searchText)) ||
                        (contract.CreatedAt != DateTime.MinValue && contract.CreatedAt.ToString("yyyy-MM-dd").Contains(searchText));
                };
            }

        }

        private void FetchAllContracts()
        {
            var contractModels = new List<RentalContractsModel>();

            //string query = @"
            //SELECT 
            //    RC.ContractID,
            //    RC.PropertyID,
            //    P.PropertyName,
            //    RC.TenantID,
            //    T.Name AS FullName,
            //    RC.StartDate,
            //    RC.EndDate,
            //    RC.MonthlyRent,
            //    RC.MonthlyRent - RC.DepositAmount AS Balance,
            //    RC.DepositAmount,
            //    RC.Status,
            //    RC.CreatedAt
            //FROM RentalContracts RC
            //INNER JOIN Properties P ON RC.PropertyID = P.PropertyID
            //INNER JOIN Tenants T ON RC.TenantID = T.TenantID
            //ORDER BY RC.CreatedAt DESC;";



            string query = @"
            SELECT 
                RC.ContractID,
                RC.PropertyID,
                P.PropertyName,
                RC.TenantID,
                T.Name AS FullName,
                RC.StartDate,
                RC.EndDate,
                RC.MonthlyRent,
                RC.DepositAmount,

                -- Compute Actual Balance
                (RC.MonthlyRent - RC.DepositAmount) 
                  - ISNULL(SUM(PY.Amount), 0) AS Balance,

                RC.Status,
                RC.CreatedAt
            FROM RentalContracts RC
            INNER JOIN Properties P ON RC.PropertyID = P.PropertyID
            INNER JOIN Tenants T ON RC.TenantID = T.TenantID
            LEFT JOIN Payments PY ON RC.ContractID = PY.ContractID
            GROUP BY 
                RC.ContractID,
                RC.PropertyID,
                P.PropertyName,
                RC.TenantID,
                T.Name,
                RC.StartDate,
                RC.EndDate,
                RC.MonthlyRent,
                RC.DepositAmount,
                RC.Status,
                RC.CreatedAt
            ORDER BY RC.CreatedAt DESC;
            ";



            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var contract = new RentalContractsModel
                        {
                            ContractID = reader["ContractID"].ToString(),
                            PropertyID = reader["PropertyID"].ToString(),
                            PropName = reader["PropertyName"].ToString(),
                            TenantID = reader["TenantID"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            StartDate = reader["StartDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["StartDate"])
                                : null,
                            EndDate = reader["EndDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["EndDate"])
                                : null,
                            MonthlyRent = reader["MonthlyRent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["MonthlyRent"])
                                : null,
                            DepositAmount = reader["DepositAmount"] != DBNull.Value
                                ? Convert.ToDecimal(reader["DepositAmount"])
                                : null,
                            Balance = reader["Balance"] != DBNull.Value
                                ? Convert.ToDecimal(reader["Balance"])
                                : null,
                            Status = reader["Status"].ToString(),
                            CreatedAt = reader["CreatedAt"] != DBNull.Value
                                ? Convert.ToDateTime(reader["CreatedAt"])
                                : DateTime.MinValue
                        };

                        contractModels.Add(contract);
                    }
                }

                collectionViewSource.Source = contractModels;
                ContractsTable.ItemsSource = collectionViewSource.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while retrieving rental contracts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            var listOfPayments = ContractsTable.ItemsSource.Cast<RentalContractsModel>().ToList();

            ContractsReport paymentReport = new ContractsReport(listOfPayments);
            paymentReport.ShowDialog();
        }

        private void RenewContract_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedContract = btn.DataContext as RentalContractsModel;

            if (selectedContract != null)
            {
                Add updateTenant = new Add
                {
                    DataContext = selectedContract
                };

                updateTenant.paymentUpdated += (s, e) => {

                    FetchAllContracts();
                };

                updateTenant.ShowDialog();
            }
        }

        private void RenewContract_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteBtn_Loaded(object sender, RoutedEventArgs e)
        {
            if (_role == "ROLE-102")
            {
                ((Button)sender).Visibility = Visibility.Collapsed;
            }
        }
    }
}
