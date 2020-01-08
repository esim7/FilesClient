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
        private string FirstMessage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MyFiles = new List<MyFile>();
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                using (var stream = client.GetStream())
                {
                    var resultText = string.Empty;
                    do
                    {
                        var buffer = new byte[128];
                        stream.Read(buffer, 0, buffer.Length);
                        resultText += System.Text.Encoding.UTF8.GetString(buffer);

                        //if (!stream.DataAvailable)
                        //{
                        //    System.Threading.Thread.Sleep(1);
                        //}
                    }
                    while (stream.DataAvailable);

                    var myFiles = JsonConvert.DeserializeObject<List<MyFile>>(resultText);
                    MyFiles = myFiles;
                    dataGrid.ItemsSource = MyFiles;
                }
            }
        }


        private async void AddFileButton(object sender, RoutedEventArgs e)
        {
            FirstMessage = "recive";
            FirstMessageToServer();
            dataGrid.ItemsSource = null;
            await AddFileAsync();
            dataGrid.ItemsSource = MyFiles;
        }

        private async void DeleteFileButton(object sender, RoutedEventArgs e)
        {
            var index = dataGrid.SelectedIndex;
            var id = MyFiles.ElementAt(index).Id;
            FirstMessage = "delete";
            FirstMessageToServer();
            dataGrid.ItemsSource = null;
            await DeleteFileAsync(id);
            dataGrid.ItemsSource = MyFiles;
        }

        private async void DownloadFileButton(object sender, RoutedEventArgs e)
        {
            var index = dataGrid.SelectedIndex;
            var id = MyFiles.ElementAt(index).Id;
            var size = MyFiles.ElementAt(index).Size;
            var formatFile = MyFiles.ElementAt(index).Name.Substring(MyFiles.ElementAt(index).Name.IndexOf('.')); // получаю расширение файла и передаю его в метод, 
            FirstMessage = "send";                                                                                // вставляю его в конце имени файла при сохранении 
            FirstMessageToServer();                                                                               // его в выбранной директории
            await DownloadFileAsync(id, size, formatFile);
        }

        public void FirstMessageToServer()
        {
            using (var client = new TcpClient())
            {
                client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                using (var stream = client.GetStream())
                {
                    var data = Encoding.UTF8.GetBytes(FirstMessage);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        public async Task AddFileAsync()
        {
            await Task.Run(() =>
            {
                var newFile = new MyFile();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    newFile.Name = System.IO.Path.GetFileName(openFileDialog.FileName);
                    newFile.Size = new FileInfo(openFileDialog.FileName).Length.ToString();
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
                            var resultText = string.Empty;
                            do
                            {
                                var buffer = new byte[128];
                                stream.Read(buffer, 0, buffer.Length);
                                resultText += System.Text.Encoding.UTF8.GetString(buffer);
                            }
                            while (stream.DataAvailable);
                            var myFiles = JsonConvert.DeserializeObject<List<MyFile>>(resultText);
                            MyFiles = myFiles;
                        }
                    }
                }
            });
        }
        public async Task DownloadFileAsync(Guid id, string size, string format)
        {
            await Task.Run(() =>
            {
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                    using (var stream = client.GetStream())
                    {
                        var data = Encoding.UTF8.GetBytes(id.ToString());
                        stream.Write(data, 0, data.Length);

                        byte[] buffer = new byte[int.Parse(size)];
                        stream.Read(buffer, 0, buffer.Length);
                        SaveToComputer(buffer, format);
                    }
                };
            });
                
        }

        public async Task DeleteFileAsync(Guid id)
        {
            await Task.Run(() =>
            {
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3231));
                    using (var stream = client.GetStream())
                    {
                        var data = Encoding.UTF8.GetBytes(id.ToString());
                        stream.Write(data, 0, data.Length);

                        var resultText = string.Empty;
                        do
                        {
                            var buffer = new byte[128];
                            stream.Read(buffer, 0, buffer.Length);
                            resultText += System.Text.Encoding.UTF8.GetString(buffer);
                        }
                        while (stream.DataAvailable);
                        var myFiles = JsonConvert.DeserializeObject<List<MyFile>>(resultText);
                        MyFiles = myFiles;
                    }
                }
            });
        }

        public void SaveToComputer(byte[] data, string format)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(saveFileDialog.FileName + format, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
