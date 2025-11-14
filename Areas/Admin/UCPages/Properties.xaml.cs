using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.Forms;
using Rental.Areas.Admin.Forms.Properties;
using Rental.DatabaseConnection;
using Rental.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : UserControl
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        private CollectionViewSource collectionViewSource;
        private string _role;

        public Properties(string role)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            collectionViewSource = new CollectionViewSource();
            FetchAllProperties();
            _role = role;

        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collectionViewSource?.View != null)
            {
                collectionViewSource.View.Filter = item =>
                {
                    var property = item as PropertyModel;
                    if (property == null) return false;

                    string searchText = Search.Text.Trim().ToLower();
                    if (string.IsNullOrEmpty(searchText))
                        return true;

                    return
                        (!string.IsNullOrEmpty(property.PropertyID) && property.PropertyID.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(property.PropertyName) && property.PropertyName.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(property.Type) && property.Type.ToLower().Contains(searchText)) ||
                        (property.MonthlyRent.HasValue && property.MonthlyRent.Value.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(property.Status) && property.Status.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(property.Description) && property.Description.ToLower().Contains(searchText));
                };
            }

        }

        private void NewPopertiesButton_Click(object sender, RoutedEventArgs e)
        {
            AddProperties properties = new AddProperties();

            properties.propertiesCreated += (s, e) => {
                FetchAllProperties();
            };
            properties.ShowDialog();
        }

        private void FetchAllProperties()
        {
            var propertyModels = new List<PropertyModel>();

            string query = @"
            SELECT 
                PropertyID,
                PropertyName,
                Type,
                MonthlyRent,
                Status,
                Description
            FROM Properties";

            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        propertyModels.Add(new PropertyModel
                        {
                            PropertyID = reader["PropertyID"].ToString(),
                            PropertyName = reader["PropertyName"].ToString(),
                            Type = reader["Type"].ToString(),
                            MonthlyRent = reader["MonthlyRent"] != DBNull.Value
                                ? Convert.ToDecimal(reader["MonthlyRent"])
                                : (decimal?)null,
                            Status = reader["Status"].ToString(),
                            Description = reader["Description"].ToString()
                        });
                    }
                }

                collectionViewSource.Source = propertyModels;
                PropertyTable.ItemsSource = collectionViewSource.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while retrieving properties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedTenant = btn.DataContext as PropertyModel;

            if (selectedTenant != null)
            {
                UpdateProperties updateTenant = new UpdateProperties
                {
                    DataContext = selectedTenant
                };

                updateTenant.propertiesUpdated += (s, e) => {

                    FetchAllProperties();
                };

                updateTenant.ShowDialog();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedClient = btn.DataContext as PropertyModel;

            if (selectedClient != null)
            {

                MessageBoxResult result = MessageBox.Show(
                  "Are you sure you want to delete this Property?",
                  "Confirmation",
                  MessageBoxButton.YesNo,
                  MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Properties WHERE PropertyID = @PropertyID", sqlConnection);
                        cmd.Parameters.AddWithValue("@PropertyID", selectedClient.PropertyID);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Property deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        sqlConnection.Close();
                        FetchAllProperties();

                    }
                    catch (Exception Ex)
                    {
                        MessageBox.Show(Ex.Message);
                        sqlConnection.Close();

                    }

                }

            }
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
