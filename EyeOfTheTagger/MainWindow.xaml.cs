using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EyeOfTheTagger.Data;
using EyeOfTheTagger.Data.Event;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Interaction logic for <c>MainWindow.xaml</c>
    /// </summary>
    /// <seealso cref="Window"/>
    public partial class MainWindow : Window
    {
        private LibraryData _library;
        private BackgroundWorker _bgw;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Title = Tools.GetAppName();
            CheckTracks0.Content = "Tracks without track number";
            CheckTracks1.Content = "Tracks without year";
            CheckTracks2.Content = "Tracks without genre";
            CheckTracks3.Content = "Tracks without artist";
            CheckTracks4.Content = "Tracks with duplicate genre";
            CheckTracks5.Content = "Tracks with duplicate artist";
            CheckTracks6.Content = "Tracks without album";
            CheckTracks7.Content = "Tracks without album artist";
            CheckTracks8.Content = "Tracks with several album artists";
            CheckTracks9.Content = "Tracks without title";

            // First thing to do!
            _library = new LibraryData(false);
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
                DisplayWhileNotLoading();
            };

            DisplayWhileNotLoading();

            //LoadingButton_Click(null, null);
        }

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            Console.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            TracksView.ItemsSource = _library.Tracks;
            LoadingButton.IsEnabled = true;
            LoadingButton.Content = "Reload";
        }

        private void DisplayWhileLoading()
        {
            LoadingBar.Visibility = Visibility.Visible;
            LoadingBar.Value = 0;
            Console.Visibility = Visibility.Visible;
            MainView.Visibility = Visibility.Collapsed;
            LoadingButton.IsEnabled = false;
            LoadingButton.Content = "Loading...";
        }

        private void LoadingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_bgw != null && !_bgw.IsBusy)
            {
                DisplayWhileLoading();
                _bgw.RunWorkerAsync();
            }
        }

        private void CheckTracks_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
