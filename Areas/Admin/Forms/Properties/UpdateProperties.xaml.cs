using Microsoft.Data.SqlClient;
using Rental.Areas.Admin.UCPages;
using Rental.DatabaseConnection;
using Rental.Models;
using Rental.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace Rental.Areas.Admin.Forms.Properties
{
    /// <summary>
    /// Interaction logic for UpdateProperties.xaml
    /// </summary>
    public partial class UpdateProperties : Window
    {

        private readonly Connection connection = new Connection();
        private SqlConnection sqlConnection;
        public event EventHandler propertiesUpdated;
        private readonly FileDialogService _fileService;
        string relativePath = "";

        public UpdateProperties(FileDialogService? fileService = null)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            _fileService = fileService ?? new FileDialogService();

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PropertyName_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void PropertyName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void MonthlyRent_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            HelperValidation.ValidationHelper.AllowOnlyNumbers(sender, e);

        }

        private void MonthlyRent_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HelperValidation.ValidationHelper.NoSpaceOnly(sender, e);

        }

        private void Description_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void Description_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            UpdatePropertyInfo();
        }
        string? propertyId = "";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var property = DataContext as PropertyModel;

            if (property != null)
            {
                propertyId = property.PropertyID;
                PropertyName.Text = property.PropertyName;
                Type.Text = property.Type;
                MonthlyRent.Text = property.MonthlyRent?.ToString("F2");
                Status.Text = property.Status;
                Description.Text = property.Description;

                try
                {
                    string query = "SELECT ImagePath FROM Properties WHERE PropertyID = @PropertyID";
                    sqlConnection.Open();

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@PropertyID", propertyId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string? relativePath = reader["ImagePath"]?.ToString();

                                if (!string.IsNullOrEmpty(relativePath))
                                {
                                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

                                    string adjustedRelativePath = relativePath.Replace("Resources", "PropertiesImages");

                                    string absolutePath = Path.Combine(projectRoot, adjustedRelativePath);

                                    if (File.Exists(absolutePath))
                                    {
                                        PropertyImage.Source = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
                                    }
                                   
                                }
                            }
                        }
                    }
                }
                catch
                {
                   
                }
                finally
                {
                    if (sqlConnection.State == System.Data.ConnectionState.Open)
                        sqlConnection.Close();
                }
            }
        }


        private void UpdatePropertyInfo()
        {
            string query = @"UPDATE Properties
                     SET PropertyName = @PropertyName,
                         Type = @Type,
                         MonthlyRent = @MonthlyRent,
                         Status = @Status,
                         Description = @Description,
                         ImagePath = @ImagePath
                     WHERE PropertyID = @PropertyID";

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(PropertyName?.Text) ||
                    string.IsNullOrWhiteSpace(Type?.Text) ||
                    string.IsNullOrWhiteSpace(MonthlyRent?.Text) ||
                    string.IsNullOrWhiteSpace(Status?.Text))
                {
                    MessageBox.Show("Property Name, Type, Monthly Rent, and Status are required.",
                                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(MonthlyRent.Text, out decimal rentValue) || rentValue <= 0)
                {
                    MessageBox.Show("Monthly Rent must be a valid positive number.",
                                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);


                    return;
                }

                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PropertyID", propertyId);
                    cmd.Parameters.AddWithValue("@PropertyName", PropertyName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Type", Type.Text.Trim());
                    cmd.Parameters.AddWithValue("@MonthlyRent", rentValue);
                    cmd.Parameters.AddWithValue("@Status", Status.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description",
                        string.IsNullOrWhiteSpace(Description.Text) ? (object)DBNull.Value : Description.Text.Trim());
                    cmd.Parameters.AddWithValue("@ImagePath", relativePath ?? "");

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Property information updated successfully.",
                                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        propertiesUpdated?.Invoke(this, EventArgs.Empty);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No property record found to update.",
                                        "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("Duplicate Property Name detected. Please use a unique name.",
                                    "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"SQL Error: {sqlEx.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void ChooseFilBtn_Click(object sender, RoutedEventArgs e)
        {
            var fullPath = _fileService.GetAbsolutePath();

            relativePath = _fileService.GetFilePathToDisplay(fullPath);
            PropertyImage.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
        }
    }
}
