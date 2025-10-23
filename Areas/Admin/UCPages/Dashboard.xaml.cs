using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rental.Areas.Admin.UCPages
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public Dashboard()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);

        }
        private int GetRowCount(string tableName)
        {
            string query = $"SELECT COUNT(*) FROM {tableName}";
            using (SqlConnection conn = new SqlConnection(connection.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Tenants.Text = GetRowCount("Tenants").ToString();
            //Payments.Text = GetRowCount("Payments").ToString();
            Contracts.Text = GetRowCount("RentalContracts").ToString();
            Properties.Text = GetRowCount("Properties").ToString();
        }
    }
}
