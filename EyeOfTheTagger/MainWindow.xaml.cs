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
        private int _expectedTracksCount;

        public MainWindow()
        {
            InitializeComponent();
            _library = new LibraryData(new List<string> { Constants.LibraryPath }, false);
            _library.NewTrackAdded += delegate (object sender, NewTrackAddedEventArgs e)
            {
                if (e != null)
                {
                    _bgw.ReportProgress(Convert.ToInt32(e.CurrentCount / (decimal)_expectedTracksCount * 100),
                        $"Added track : {e.FileName}");
                }
            };
            _library.TrackLoadingError += delegate (object sender, TrackLoadingErrorEventArgs e)
            {
                if (e != null)
                {
                    _bgw.ReportProgress(Convert.ToInt32(e.CurrentCount / (decimal)_expectedTracksCount),
                        $"Error : {e?.FileName ?? Constants.UnknownInfo} - {e?.ErrorMessage ?? Constants.UnknownInfo}");
                }
            };
            _library.CountTrackComputed += delegate (object sender, CountTrackComputedEventArgs e)
            {
                if (e != null)
                {
                    _expectedTracksCount = e.TrackCount;
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
                if (e.UserState != null)
                {
                    Console.Items.Add(e.UserState);
                }
            };
            _bgw.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                Console.Visibility = Visibility.Collapsed;
                MainView.Visibility = Visibility.Visible;
                MainView.ItemsSource = _library.Tracks;
            };
            _bgw.RunWorkerAsync();
        }
    }
}
