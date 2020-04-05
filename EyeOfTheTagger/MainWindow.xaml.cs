using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using EyeOfTheTagger.Data;
using EyeOfTheTagger.Data.Event;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibraryData _library;
        private BackgroundWorker _bgw;

        public MainWindow()
        {
            InitializeComponent();
            Title = Constants.AppName;
            _library = new LibraryData(new List<string> { Constants.LibraryPath }, false);
            _library.LoadingLogHandler += delegate (object sender, LoadingLogEventArgs e)
            {
                if (e?.Log != null && _library.TotalFilesCount > -1)
                {
                    int progressPercentage = e.TrackIndex == -1 ? 100 : Convert.ToInt32(e.TrackIndex / (decimal)_library.TotalFilesCount * 100);
                    _bgw.ReportProgress(progressPercentage, e.Log);
                }
            };

            _bgw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            _bgw.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                _library.Reload();
            };
            _bgw.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                LoadingBar.Value = e.ProgressPercentage;
                if (e.UserState is LogData)
                {
                    Console.Items.Add(e.UserState as LogData);
                }
            };
            _bgw.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                LoadingBar.Value = 0;
                Console.Visibility = Visibility.Collapsed;
                MainView.Visibility = Visibility.Visible;
                MainView.ItemsSource = _library.Tracks;
            };
            _bgw.RunWorkerAsync();
        }
    }
}
