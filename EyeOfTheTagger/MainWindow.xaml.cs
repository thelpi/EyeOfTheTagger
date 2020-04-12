using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        private AlbumArtistData _albumArtistFilter = null;
        private AlbumData _albumFilter = null;
        private GenreData _genreFilter = null;
        private PerformerData _performerFilter = null;
        private uint? _yearFilter = null;

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
                    LogsView.Items.Add(e.UserState as LogData);
                }
            };
            _bgw.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                DisplayWhileNotLoading();
            };

            DisplayWhileNotLoading();

            LoadingButton_Click(null, null);
        }

        #region Window events

        private void LoadingButton_Click(object sender, RoutedEventArgs e)
        {
            if (_bgw != null && !_bgw.IsBusy)
            {
                DisplayWhileLoading();
                _bgw.RunWorkerAsync();
            }
        }

        private void ShowLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogsView.Visibility == Visibility.Visible)
            {
                LogsView.Visibility = Visibility.Collapsed;
                MainView.Visibility = Visibility.Visible;
                ShowLogsButton.Content = "Show logs";
            }
            else
            {
                LogsView.Visibility = Visibility.Visible;
                MainView.Visibility = Visibility.Collapsed;
                ShowLogsButton.Content = "Show library";
            }
        }

        private void DumpLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (LogsView.Items.Count == 0)
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
            dumpWorker.RunWorkerCompleted += delegate (object subSender, RunWorkerCompletedEventArgs subE)
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
            dumpWorker.RunWorkerAsync(LogsView.Items.Cast<LogData>());
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
                GetTracksView());
        }

        private void FilterTracks_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            _albumArtistFilter = (btn?.DataContext as AlbumArtistViewData)?.SourceData;
            _albumFilter = (btn?.DataContext as AlbumViewData)?.SourceData;
            _genreFilter = (btn?.DataContext as GenreViewData)?.SourceData;
            _performerFilter = (btn?.DataContext as PerformerViewData)?.SourceData;
            _yearFilter = (btn?.DataContext as YearViewData)?.Year;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void ClearTracksFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = null;
            _albumFilter = null;
            _genreFilter = null;
            _performerFilter = null;
            _yearFilter = null;
            TracksView.ItemsSource = GetTracksView();
        }

        private void LinkAlbumArtist_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = ((sender as Hyperlink)?.DataContext as TrackViewData)?.SourceData?.Album?.AlbumArtist;
            _albumFilter = null;
            _genreFilter = null;
            _performerFilter = null;
            _yearFilter = null;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkAlbum_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = null;
            _albumFilter = ((sender as Hyperlink)?.DataContext as TrackViewData)?.SourceData?.Album;
            _genreFilter = null;
            _performerFilter = null;
            _yearFilter = null;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkPerformer_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = null;
            _albumFilter = null;
            _genreFilter = null;
            _performerFilter = ((sender as Hyperlink)?.DataContext as TrackViewData)?.SourceData?.Performers?.FirstOrDefault();
            _yearFilter = null;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkGenre_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = null;
            _albumFilter = null;
            _genreFilter = ((sender as Hyperlink)?.DataContext as TrackViewData)?.SourceData?.Genres?.FirstOrDefault();
            _performerFilter = null;
            _yearFilter = null;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkYear_Click(object sender, RoutedEventArgs e)
        {
            _albumArtistFilter = null;
            _albumFilter = null;
            _genreFilter = null;
            _performerFilter = null;
            _yearFilter = ((sender as Hyperlink)?.DataContext as TrackViewData)?.Year;
            TracksView.ItemsSource = GetTracksView();
            MainView.SelectedItem = TracksTab;
        }

        private void ClearAlbumArtistFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumArtistsCheckBox.IsChecked = false;
            EmptyAlbumArtistsCheckBox.IsChecked = false;
            ApplyArtistAlbumsFilters();
        }

        private void EmptyAlbumArtistsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyArtistAlbumsFilters();
        }

        private void DuplicateAlbumArtistsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyArtistAlbumsFilters();
        }

        #endregion Window events

        #region Private helper methods

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            LogsView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            TracksView.ItemsSource = GetTracksView();
            AlbumArtistsView.ItemsSource = BaseViewData.GetAlbumArtistsViewData(_library);
            AlbumsView.ItemsSource = BaseViewData.GetAlbumsViewData(_library);
            GenresView.ItemsSource = BaseViewData.GetGenresViewData(_library);
            PerformersView.ItemsSource = BaseViewData.GetPerformersViewData(_library);
            YearsView.ItemsSource = BaseViewData.GetYearsViewData(_library);
            LoadingButton.IsEnabled = true;
            LoadingButton.Content = "Reload";
            ShowLogsButton.Content = "Show logs";
            DumpLogsButton.IsEnabled = true;
            ShowLogsButton.IsEnabled = true;
            ClearTracksFiltersButton.IsEnabled = true;
        }

        private void DisplayWhileLoading()
        {
            LoadingBar.Visibility = Visibility.Visible;
            LoadingBar.Value = 0;
            LogsView.Visibility = Visibility.Visible;
            MainView.Visibility = Visibility.Collapsed;
            LoadingButton.IsEnabled = false;
            LoadingButton.Content = "Loading...";
            ShowLogsButton.Content = "Show library";
            DumpLogsButton.IsEnabled = false;
            ShowLogsButton.IsEnabled = false;
            ClearTracksFiltersButton.IsEnabled = false;
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

        private IEnumerable<TrackViewData> GetTracksView()
        {
            return BaseViewData.GetTracksViewData(_library, _albumArtistFilter, _albumFilter,
                _performerFilter, _genreFilter, _yearFilter);
        }

        private void ApplyArtistAlbumsFilters()
        {
            IEnumerable<AlbumArtistViewData> albumArtistItems = BaseViewData.GetAlbumArtistsViewData(_library);

            if (DuplicateAlbumArtistsCheckBox.IsChecked == true)
            {
                albumArtistItems = albumArtistItems
                    .GroupBy(aa => aa.Name.Trim().ToLowerInvariant())
                    .Where(aa => aa.Count() > 1)
                    .SelectMany(aa => aa);
            }

            if (EmptyAlbumArtistsCheckBox.IsChecked == true)
            {
                albumArtistItems = albumArtistItems.Where(aa => aa.Name.Trim() == string.Empty || aa.SourceData.IsDefault);
            }

            AlbumArtistsView.ItemsSource = albumArtistItems;
        }

        #endregion Private helper methods
    }
}
