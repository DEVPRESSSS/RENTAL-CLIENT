using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.Forms;
using Rental.Areas.Admin.Forms.User;
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
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : UserControl
    {
        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        private CollectionViewSource collectionViewSource;
        public Users()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            collectionViewSource = new CollectionViewSource();
            FetchAllUsers();
        }

        private void UserBtn_Click(object sender, RoutedEventArgs e)
        {
            var users = new AddUser();
            users.userCreated += (s, e) => {
                FetchAllUsers();
            };
            users.ShowDialog();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collectionViewSource?.View != null)
            {
                collectionViewSource.View.Filter = item =>
                {
                    var tenant = item as UserModel;
                    if (tenant == null) return false;

                    string searchText = Search.Text.Trim().ToLower();
                    if (string.IsNullOrEmpty(searchText))
                        return true;

                    return
                        (!string.IsNullOrEmpty(tenant.UserId) && tenant.UserId.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.FullName) && tenant.FullName.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Contact) && tenant.Contact.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Username) && tenant.Username.ToLower().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(tenant.Email) && tenant.Email.ToLower().Contains(searchText)) ||
                        (tenant.CreatedAt != DateTime.MinValue && tenant.CreatedAt.ToString("yyyy-MM-dd").Contains(searchText));
                };
            }
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedTenant = btn.DataContext as UserModel;

            if (selectedTenant != null)
            {
                UpdateUser updateTenant = new UpdateUser
                {
                    DataContext = selectedTenant
                };

                updateTenant.tenantUpdated += (s, e) => {

                    FetchAllUsers();
                };

                updateTenant.ShowDialog();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var selectedClient = btn.DataContext as UserModel;

            if (selectedClient != null)
            {

                MessageBoxResult result = MessageBox.Show(
                  "Are you sure you want to delete this user?",
                  "Confirmation",
                  MessageBoxButton.YesNo,
                  MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", sqlConnection);
                        cmd.Parameters.AddWithValue("@UserID", selectedClient.UserId);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("User deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        sqlConnection.Close();

                        FetchAllUsers();
                    }
                    catch
                    {
                        MessageBox.Show("This user has related records", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        sqlConnection.Close();

                    }

                }

            }
        }

        private void FetchAllUsers()
        {
            var userModels = new List<UserModel>();

            string query = @"SELECT * FROM Users WHERE RoleID <> 'ROLE-101'";



            try
            {
                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userModels.Add(new UserModel
                        {
                            UserId = reader["UserId"].ToString(),
                            Username = reader["Username"].ToString(),
                            FullName = reader["FullName"].ToString(),
                            Contact = reader["Phone"].ToString(),
                            Email = reader["Email"].ToString(),
                            CreatedAt = reader["CreatedAt"] != DBNull.Value
                                ? Convert.ToDateTime(reader["CreatedAt"])
                                : DateTime.MinValue,
                        });
                    }
                }

                collectionViewSource.Source = userModels;
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

        private void TenantTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
