using Domain;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace FilesClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<MyFile> MyFiles { get; set; }
        private string FilePath { get; set; } 
        private byte[] FileData { get; set; } 


        public MainWindow()
        {
            InitializeComponent();
            MyFiles = new List<MyFile>();
            dataGrid.ItemsSource = MyFiles;
        }


        private void AddFileButton(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = null;
            AddFile();       
            dataGrid.ItemsSource = MyFiles;
        }

        private void DeleteFileButton(object sender, RoutedEventArgs e)
        {

        }

        private void DownloadFileButton(object sender, RoutedEventArgs e)
        {

        }

        public bool AddFile()
        {
            var newFile = new MyFile();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                newFile.Name = System.IO.Path.GetFileName(openFileDialog.FileName);
                newFile.Size = new FileInfo(openFileDialog.FileName).Length.ToString();
                MyFiles.Add(newFile);
                FilePath = openFileDialog.FileName;
                FileData = File.ReadAllBytes(FilePath);

                var myFile = JsonConvert.SerializeObject(newFile);
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                    using (var stream = client.GetStream())
                    {
                        var data = Encoding.UTF8.GetBytes(myFile);
                        stream.Write(data, 0, data.Length);
                    }
                }
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                    using (var stream = client.GetStream())
                    {
                        stream.Write(FileData, 0, FileData.Length);
                    }
                }
                return true;
            }
            return false;
        } 
    }
}
