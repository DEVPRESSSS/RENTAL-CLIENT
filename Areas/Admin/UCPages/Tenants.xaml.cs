using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.Forms;
using Rental.DatabaseConnection;
using Rental.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
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
    /// Interaction logic for Tenants.xaml
    /// </summary>
    public partial class Tenants : UserControl
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        private CollectionViewSource collectionViewSource;
        private string _role;
        public Tenants(string role)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            collectionViewSource = new CollectionViewSource();
            FetchAllTenants();
            _role = role;
        }

        private void NewTenantButton_Click(object sender, RoutedEventArgs e)
        {
            AddTenant tenant = new AddTenant();

            tenant.tenantCreated += (s, e) => {
                FetchAllTenants();
            };
            tenant.ShowDialog();
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedTenant = btn.DataContext as TenantModel;

            if (selectedTenant != null)
            {
                EditTenant updateTenant = new EditTenant
                {
                    DataContext = selectedTenant
                };

                updateTenant.tenantUpdated += (s, e) => {

                    FetchAllTenants();
                };

                updateTenant.ShowDialog();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedClient = btn.DataContext as TenantModel;

            if (selectedClient != null)
            {

                MessageBoxResult result = MessageBox.Show(
                  "Are you sure you want to delete this tenant?",
                  "Confirmation",
                  MessageBoxButton.YesNo,
                  MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Tenants WHERE TenantID = @TenantID", sqlConnection);
                        cmd.Parameters.AddWithValue("@TenantID", selectedClient.TenantID);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Tenant deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        sqlConnection.Close();

                        FetchAllTenants();
                    }
                    catch 
                    {
                        MessageBox.Show("This tenant has related records or active contract" ,"Error" ,MessageBoxButton.OK, MessageBoxImage.Error);
                        sqlConnection.Close();

                    }

                }

            }
        }

        private void FetchAllTenants()
        {
            var tenantModels = new List<TenantModel>();

            string query = @"
                SELECT 
                    t.TenantID,
                    t.Name,
                    t.Contact,
                    t.Address,
                    t.Email,
                    t.DateJoined,
                    rc.MonthlyRent,
                    rc.DepositAmount,
                    rc.Status
                FROM Tenants AS t
                LEFT JOIN RentalContracts AS rc 
                    ON t.TenantID = rc.TenantID
            ";
             


            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenantModels.Add(new TenantModel
                        {
                            TenantID = reader["TenantID"].ToString(),
                            Name = reader["Name"].ToString(),
                            Contact = reader["Contact"].ToString(),
                            Address = reader["Address"].ToString(),
                            Email = reader["Email"].ToString(),
                            DateJoined = reader["DateJoined"] != DBNull.Value
                                ? Convert.ToDateTime(reader["DateJoined"])
                                : DateTime.MinValue,
                            DepositAmount = reader["DepositAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DepositAmount"]) : (decimal?)null,
                            MonthlyRent = reader["MonthlyRent"] != DBNull.Value ? Convert.ToDecimal(reader["MonthlyRent"]) : (decimal?) null,
                            Status = reader["Status"].ToString()
                        }); 
                    }
                }

                collectionViewSource.Source = tenantModels;
                TenantTable.ItemsSource = collectionViewSource.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while retrieving tenants: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collectionViewSource?.View != null)
            {
                collectionViewSource.View.Filter = item =>
                {
                    var tenant = item as TenantModel;
                    if (tenant == null) return false;

                    string searchText = Search.Text.Trim().ToLower();
                    if (string.IsNullOrEmpty(searchText))
                        return true; 

                    return
                        (!string.IsNullOrEmpty(tenant.TenantID) && tenant.TenantID.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Name) && tenant.Name.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Contact) && tenant.Contact.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Address) && tenant.Address.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Email) && tenant.Email.ToLower().Contains(searchText)) ||
                        (tenant.DateJoined != DateTime.MinValue && tenant.DateJoined.ToString("yyyy-MM-dd").Contains(searchText));
                };
            }

        }

        private void TenantTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
