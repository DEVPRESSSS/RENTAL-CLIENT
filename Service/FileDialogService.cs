using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rental.Service
{
    public class FileDialogService
    {

       public string GetFilePathToDisplay(string? fullPath)
        {

                string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                string resourceFolder = Path.Combine(projectRoot, "PropertiesImages");
               
                //This will create directory if it is not exist
                if(!Directory.Exists(resourceFolder))
                {
                    Directory.CreateDirectory(resourceFolder);
                }

                //Get the filename of the selected file
                string fileName = Path.GetFileName(fullPath);
    
                if(!string.IsNullOrEmpty(fileName))
                {
                    string destinantionPath = Path.Combine(resourceFolder, fileName);
                    File.Copy(fullPath, destinantionPath, true);
                 }
                    

                return Path.Combine("Resources", fileName);
        }

        public string GetAbsolutePath()
        {
            var openfileDialog = new OpenFileDialog();
            openfileDialog.Title = "Choose image";
            openfileDialog.DefaultExt = "png";
            openfileDialog.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            bool? result = openfileDialog.ShowDialog();
            if (result == true)
            {

                string selectedFile = openfileDialog.FileName;
                string fullPath = Path.GetFullPath(selectedFile);
                return fullPath;

            }



            return null;
        }
    }
}
