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

namespace EyeOfTheTagger
{
    /// <summary>
    /// Interaction logic for <c>MainWindow.xaml</c>
    /// </summary>
    /// <seealso cref="Window"/>
    public partial class MainWindow : Window
    {
        private LibraryViewData _libraryViewData;
        private BackgroundWorker _bgw;
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

            // keep this line at the first to do.
            _libraryViewData = new LibraryViewData(() => _bgw);

            _bgw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };
            _bgw.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                _libraryViewData.Reload();
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

            Tools.DumpLogsIntoFile(filePath, LogsView.Items.Cast<LogData>(),
                () => MessageBox.Show($"Log file created:\r\n\r\n{filePath}", $"{Tools.GetAppName()} - information"),
                (string msg) => MessageBox.Show($"The following error occured while dumping:\r\n\r\n{msg}", $"{Tools.GetAppName()} - error"));
        }

        private void AlbumArtistsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumArtistsView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
                _libraryViewData.GetAlbumArtistsViewData());
        }

        private void AlbumsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumsView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
                _libraryViewData.GetAlbumsViewData());
        }

        private void GenresView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GenresView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
                _libraryViewData.GetGenresViewData());
        }

        private void PerformersView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            PerformersView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
                _libraryViewData.GetPerformersViewData());
        }

        private void YearsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            YearsView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
                _libraryViewData.GetYearsViewData());
        }

        private void TracksView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            TracksView.ItemsSource = _libraryViewData.SortDatas(GetPropertyNameFromColumnHeader(sender),
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

        private void AlbumArtistsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyArtistAlbumsFilters();
        }

        private void AlbumsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyAlbumsFilters();
        }

        private void ClearAlbumFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumsCheckBox.IsChecked = false;
            EmptyAlbumsCheckBox.IsChecked = false;
            InvalidTracksOrderCheckBox.IsChecked = false;
            MultipleYearsCheckBox.IsChecked = false;
            InvalidFrontCoverCheckBox.IsChecked = false;
            ApplyAlbumsFilters();
        }

        #endregion Window events

        #region Private helper methods

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            LogsView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            TracksView.ItemsSource = GetTracksView();
            AlbumArtistsView.ItemsSource = _libraryViewData.GetAlbumArtistsViewData();
            AlbumsView.ItemsSource = _libraryViewData.GetAlbumsViewData();
            GenresView.ItemsSource = _libraryViewData.GetGenresViewData();
            PerformersView.ItemsSource = _libraryViewData.GetPerformersViewData();
            YearsView.ItemsSource = _libraryViewData.GetYearsViewData();
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

        private IEnumerable<TrackViewData> GetTracksView()
        {
            return _libraryViewData.GetTracksViewData(_albumArtistFilter, _albumFilter,
                _performerFilter, _genreFilter, _yearFilter);
        }

        private void ApplyArtistAlbumsFilters()
        {
            AlbumArtistsView.ItemsSource = _libraryViewData.ApplyArtistAlbumsFilters(
                DuplicateAlbumArtistsCheckBox.IsChecked == true,
                EmptyAlbumArtistsCheckBox.IsChecked == true);
        }

        private void ApplyAlbumsFilters()
        {
            AlbumsView.ItemsSource = _libraryViewData.ApplyAlbumsFilters(
                DuplicateAlbumsCheckBox.IsChecked == true,
                EmptyAlbumsCheckBox.IsChecked == true,
                InvalidFrontCoverCheckBox.IsChecked == true,
                MultipleYearsCheckBox.IsChecked == true,
                InvalidTracksOrderCheckBox.IsChecked == true);
        }

        private static string GetPropertyNameFromColumnHeader(object sender)
        {
            return (sender as GridViewColumnHeader).Tag.ToString();
        }

        #endregion Private helper methods
    }
}
