using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using EyeOfTheTagger.Data;
using EyeOfTheTagger.Data.Event;
using EyeOfTheTagger.ViewData;

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

            LoadingButton_Click(null, null);
        }

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            Console.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            TracksView.ItemsSource = _library.Tracks;
            AlbumArtistsView.ItemsSource = GetAlbumArtistsViewData();
            LoadingButton.IsEnabled = true;
            LoadingButton.Content = "Reload";
            DumpButton.IsEnabled = true;
        }

        private void DisplayWhileLoading()
        {
            LoadingBar.Visibility = Visibility.Visible;
            LoadingBar.Value = 0;
            Console.Visibility = Visibility.Visible;
            MainView.Visibility = Visibility.Collapsed;
            LoadingButton.IsEnabled = false;
            LoadingButton.Content = "Loading...";
            DumpButton.IsEnabled = false;
        }

        private void LoadingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_bgw != null && !_bgw.IsBusy)
            {
                DisplayWhileLoading();
                _bgw.RunWorkerAsync();
            }
        }

        private IEnumerable<AlbumArtistViewData> GetAlbumArtistsViewData()
        {
            return _library.AlbumArtists
                            .Select(aa => new AlbumArtistViewData(aa, _library))
                            .OrderBy(aa => aa.Name);
        }

        private void DumpButton_Click(object sender, RoutedEventArgs e)
        {
            if (Console.Items.Count == 0)
            {
                MessageBox.Show("No logs to dump.", $"{Tools.GetAppName()} - information");
                return;
            }
            else if (!Directory.Exists(Properties.Settings.Default.DumpLogPath))
            {
                MessageBox.Show("Invalid log folder path. Please check your configuration.", $"{Tools.GetAppName()} - error");
                return;
            }
            else if (!Tools.HasWriteAccessToFolder(Properties.Settings.Default.DumpLogPath))
            {
                MessageBox.Show("The current user can't write dump file into the specified folder. Please check your configuration.", $"{Tools.GetAppName()} - error");
                return;
            }
            
            string filePath = Path.Combine(Properties.Settings.Default.DumpLogPath,
                $"{Tools.GetAppName()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_logs.csv");

            var dumpWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            // TODO : GUID should be disabled while processing.
            dumpWorker.DoWork += delegate (object subSender, DoWorkEventArgs subE)
            {
                using (var sw = new StreamWriter(filePath, false))
                {
                    sw.WriteLine($"Date\tType\tMessage");
                    foreach (LogData log in subE.Argument as IEnumerable<LogData>)
                    {
                        sw.WriteLine($"{log.Date.ToString("dd/MM/yyyy HH:mm:ss")}\t{log.Level}\t{log.Message}");
                        foreach (string adKey in log.AdditionalDatas.Keys)
                        {
                            sw.WriteLine($"{log.Date.ToString("dd/MM/yyyy HH:mm:ss")}\t{adKey}\t{log.AdditionalDatas[adKey] ?? Constants.UnknownInfo}");
                        }
                    }
                }
            };
            dumpWorker.RunWorkerCompleted += delegate(object subSender, RunWorkerCompletedEventArgs subE)
            {
                if (subE.Error != null)
                {
                    MessageBox.Show($"The following error occured while dumping:\r\n\r\n{subE.Error.Message}",
                        $"{Tools.GetAppName()} - error");
                }
                else
                {
                    MessageBox.Show($"Log file created:\r\n\r\n{filePath}",
                        $"{Tools.GetAppName()} - information");
                }
            };
            dumpWorker.RunWorkerAsync(Console.Items.Cast<LogData>());
        }
    }
}
