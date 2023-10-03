using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WebClientDownloader.ViewModels
{
    internal class WebDownloaderModel : INotifyPropertyChanged
    {
        private WebClient webClient;
        private readonly string resource = @"https://source.unsplash.com/random/";
        private string saveImagePath = string.Empty;
        private int imageIndex = 1;
        private int progressValue;
        private string saveDirectory = string.Empty;
        private ImageData? downloadedImage = default;
        private bool isDownload;

        private void downloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            if (!OpenAfterDovnload) return;
            openImage(saveImagePath);
        }

        private void openDirectory()
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveDirectory = fbd.SelectedPath;
        }


        private void openImage(string path)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        private bool IsDownload
        {
            get => isDownload;
            set
            {
                isDownload = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private  void downloadProgressChanged(object sender,DownloadProgressChangedEventArgs e)
        {
           Application.Current.Dispatcher.Invoke(new Action(() => ProgressValue = e.ProgressPercentage));
        }

        private async Task downloadAsync()
        {
            IsDownload = true;
            ProgressValue = 0;
            string address = $"{resource}/{Width}x{Heigth}/?{SelectedCategory}&1";
            string filename = $"{SelectedCategory}_{imageIndex++}.jpg";
            saveImagePath = Path.Combine(SaveDirectory, filename);
            ImageData data = new()
            {
                ImageName = filename,
                ImagePath = saveImagePath,
                Category = SelectedCategory
            };
            await webClient.DownloadFileTaskAsync(address, saveImagePath);
            Images.Add(data);
            DownloadedImage = data;
            IsDownload = false;
        }

        public WebDownloaderModel()
        {
            webClient = new();
            webClient.DownloadFileCompleted += downloadFileCompleted;
            webClient.DownloadProgressChanged += downloadProgressChanged;
        }

        public ImageData? DownloadedImage 
        {
            get => downloadedImage;
            set 
            {
                downloadedImage = value;
                OnPropertyChanged();
            }
        }

        public int ProgressValue
        {
            get => progressValue;
            set 
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        public string SaveDirectory
        {
            get => saveDirectory;
            set 
            {
                saveDirectory = value;
                OnPropertyChanged();
            }
        }

        public bool OpenAfterDovnload { get; set; }

        public string SelectedCategory { get; set; }  = string.Empty;

        public int Heigth { get; set; }

        public int Width { get; set; }

        public string[] Categories => new string[6] {"Animal", "Building", "Tree", "Plant", "Bug", "Nature" };

        public RelayCommand Download => new(async (o) => await downloadAsync(), (o) => Directory.Exists(SaveDirectory) && !IsDownload);

        public RelayCommand OpenDirectory => new((o) => openDirectory());

        public RelayCommand OpenImage => new((o) => openImage((o is ImageData image) ? image.ImagePath : ""));

        public RelayCommand Exit => new((o) => Environment.Exit(0));

        public ObservableCollection<ImageData> Images { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    }

}
