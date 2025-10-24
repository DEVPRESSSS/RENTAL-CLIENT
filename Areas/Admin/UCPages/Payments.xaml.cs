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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rental.Areas.Admin.UCPages
{
    /// <summary>
    /// Interaction logic for Payments.xaml
    /// </summary>
    public partial class Payments : UserControl
    {

        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        private CollectionViewSource collectionViewSource;

        public Payments()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            collectionViewSource = new CollectionViewSource();

            FetchAllTenants();

        }



        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FetchAllTenants()
        {
            var tenantModels = new List<PaymentModel>();

            string query = @"
              SELECT p.PaymentId,
			    p.TenantId,
			    t.Name as TenantName,
			    p.Amount,
			    p.DateOfPayment
			    from Payments p 
			    INNER JOIN Tenants t ON p.TenantId = t.TenantID
                
            ";



            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenantModels.Add(new PaymentModel
                        {
                            PaymentId = reader["PaymentId"].ToString(),
                            TenantId = reader["TenantID"].ToString(),
                            Name = reader["TenantName"].ToString(),
                            DateOfPayment = reader["DateOfPayment"] != DBNull.Value
                                ? Convert.ToDateTime(reader["DateOfPayment"])
                                : DateTime.MinValue,
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : (decimal?)null,
                       
                        });
                    }
                }

                collectionViewSource.Source = tenantModels;
                PaymentTable.ItemsSource = collectionViewSource.View;
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
    }
}
