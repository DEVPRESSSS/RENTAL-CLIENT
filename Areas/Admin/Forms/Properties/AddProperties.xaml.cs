using Microsoft.Data.SqlClient;
using Rental.DatabaseConnection;
using Rental.Models;
using Rental.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace Rental.Areas.Admin.Forms.Properties
{
    /// <summary>
    /// Interaction logic for AddProperties.xaml
    /// </summary>
    public partial class AddProperties : Window
    {
        private readonly Connection connection = new Connection();
        private readonly FileDialogService _fileService;
        private SqlConnection sqlConnection;
        public event EventHandler propertiesCreated;
        string relativePath = "";
        public AddProperties(FileDialogService? fileService = null)
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connection.ConnectionString);
            _fileService = fileService ?? new FileDialogService();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {

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

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }

        private void Submit_Click_1(object sender, RoutedEventArgs e)
        {
            CreateProperty();
        }

        private void CreateProperty()
        {
            string query = @"INSERT INTO Properties 
                    (PropertyID, PropertyName, Type, MonthlyRent, Status, Description, ImagePath)
                    VALUES (@PropertyID, @PropertyName, @Type, @MonthlyRent, @Status, @Description, @ImagePath)";

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(PropertyName.Text) ||
                    string.IsNullOrWhiteSpace(Type.Text) ||
                    string.IsNullOrWhiteSpace(MonthlyRent.Text))
                {
                    MessageBox.Show("Property Name, Type, and Monthly Rent are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear();
                    return;
                }

                if (!decimal.TryParse(MonthlyRent.Text.Trim(), out decimal rentValue) || rentValue < 0)
                {
                    MessageBox.Show("Monthly Rent must be a valid positive number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear(); 
                    return;
                }

                sqlConnection.Open();

                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@PropertyID", $"PROP-{Guid.NewGuid().ToString().ToUpper().Substring(0, 10)}");
                    cmd.Parameters.AddWithValue("@PropertyName", PropertyName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Type", Type.Text.Trim());
                    cmd.Parameters.AddWithValue("@MonthlyRent", rentValue);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(Status.Text) ? "Available" : Status.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", Description.Text.Trim());
                    cmd.Parameters.AddWithValue("@ImagePath", relativePath ?? "");

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Property added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Clear();
                        propertiesCreated?.Invoke(this, EventArgs.Empty); // Event handler for refreshing table
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("Duplicate Property Name detected. Please use a unique name.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Clear(); 
                }
                else
                {
                    MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Clear(); ;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                    sqlConnection.Close();
            }
        }

        private void Clear()
        {
            PropertyName.Clear();
            Status.Text= string.Empty;
            Description.Text= string.Empty;
            Type.Text= string.Empty;
            MonthlyRent.Clear();
            PropertyImage.Source = null;
        }

        private void ChooseFilBtn_Click(object sender, RoutedEventArgs e)
        {

            var fullPath = _fileService.GetAbsolutePath();

            relativePath = _fileService.GetFilePathToDisplay(fullPath);
            PropertyImage.Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute));
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //var dataContext = DataContext as PropertyModel;
            //try
            //{

            //    string query = "SELECT QrCode FROM AccessCards WHERE ClientId=@ClientId";
            //    sqlConnection.Open();

            //    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
            //    {
            //        cmd.Parameters.AddWithValue("@ClientId", clientId);

            //        using (SqlDataReader reader = cmd.ExecuteReader())
            //        {
            //            if (reader.Read())
            //            {
            //                string relativePath = reader["QrCode"].ToString();

            //                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            //                string absolutePath = Path.Combine(projectRoot, relativePath);

            //                QRCodePictureBox.Source = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
            //            }
            //        }
            //    }






            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error generating QR code: {ex.Message}");
            //}
            //finally
            //{
            //    if (sqlConnection.State == System.Data.ConnectionState.Open)
            //        sqlConnection.Close();
            //}
        }

        private void Description_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void Description_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
