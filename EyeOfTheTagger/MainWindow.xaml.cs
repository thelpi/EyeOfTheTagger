using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EyeOfTheTagger.ViewData;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Event;

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
        private Dictionary<string, bool> _albumArtistsViewSort = new Dictionary<string, bool>();
        private Dictionary<string, bool> _albumsViewSort = new Dictionary<string, bool>();
        private Dictionary<string, bool> _genresViewSort = new Dictionary<string, bool>();
        private Dictionary<string, bool> _performersViewSort = new Dictionary<string, bool>();
        private Dictionary<string, bool> _yearsViewSort = new Dictionary<string, bool>();
        private Dictionary<string, bool> _tracksViewSort = new Dictionary<string, bool>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Title = Tools.GetAppName();

            // First thing to do!
            _library = new LibraryData(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                    Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions), false);
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
                _library.Reload(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                    Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions));
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
            TracksView.ItemsSource = BaseViewData.GetTracksViewData(_library);
            AlbumArtistsView.ItemsSource = BaseViewData.GetAlbumArtistsViewData(_library);
            AlbumsView.ItemsSource = BaseViewData.GetAlbumsViewData(_library);
            GenresView.ItemsSource = BaseViewData.GetGenresViewData(_library);
            PerformersView.ItemsSource = BaseViewData.GetPerformersViewData(_library);
            YearsView.ItemsSource = BaseViewData.GetYearsViewData(_library);
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
                            sw.WriteLine($"{log.Date.ToString("dd/MM/yyyy HH:mm:ss")}\t{adKey}\t{log.AdditionalDatas[adKey]}");
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

        private void AlbumArtistsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumArtistsView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _albumArtistsViewSort,
                BaseViewData.GetAlbumArtistsViewData(_library));
        }

        private void AlbumsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumsView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _albumsViewSort,
                BaseViewData.GetAlbumsViewData(_library));
        }

        private void GenresView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GenresView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _genresViewSort,
                BaseViewData.GetGenresViewData(_library));
        }

        private void PerformersView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            PerformersView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _performersViewSort,
                BaseViewData.GetPerformersViewData(_library));
        }

        private void YearsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            YearsView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _yearsViewSort,
                BaseViewData.GetYearsViewData(_library));
        }

        private void TracksView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            TracksView.ItemsSource = ManageSort(sender as GridViewColumnHeader,
                _tracksViewSort,
                BaseViewData.GetTracksViewData(_library));
        }

        private IEnumerable<T> ManageSort<T>(GridViewColumnHeader header,
            Dictionary<string, bool> sortState,
            IEnumerable<T> dataRetrieved) where T : BaseViewData
        {
            string propertyName = header.Tag.ToString();
            
            if (!sortState.ContainsKey(propertyName) || !sortState[propertyName])
            {
                dataRetrieved = dataRetrieved.OrderByDescending(d => d.GetValue(propertyName));
            }
            else
            {
                dataRetrieved = dataRetrieved.OrderBy(d => d.GetValue(propertyName));
            }

            if (!sortState.ContainsKey(propertyName))
            {
                sortState.Add(propertyName, true);
            }
            else
            {
                sortState[propertyName] = !sortState[propertyName];
            }

            return dataRetrieved;
        }
    }
}
