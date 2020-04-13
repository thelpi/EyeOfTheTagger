﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using EyeOfTheTagger.ItemDatas;
using EyeOfTheTagger.ViewDatas;

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
                if (e.UserState != null)
                {
                    LogsView.Items.Add(e.UserState);
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

            Tools.DumpLogsIntoFile(filePath, LogsView.Items.Cast<EyeOfTheTaggerLib.Datas.LogData>(),
                () => MessageBox.Show($"Log file created:\r\n\r\n{filePath}", $"{Tools.GetAppName()} - information"),
                (string msg) => MessageBox.Show($"The following error occured while dumping:\r\n\r\n{msg}", $"{Tools.GetAppName()} - error"));
        }

        private void AlbumArtistsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumArtistsView.ItemsSource = _libraryViewData.GetSortedData<AlbumArtistItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void AlbumsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            AlbumsView.ItemsSource = _libraryViewData.GetSortedData<AlbumItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void GenresView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GenresView.ItemsSource = _libraryViewData.GetSortedData<GenreItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void PerformersView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            PerformersView.ItemsSource = _libraryViewData.GetSortedData<PerformerItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void YearsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            YearsView.ItemsSource = _libraryViewData.GetSortedData<YearItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void TracksView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            TracksView.ItemsSource = _libraryViewData.GetSortedData<TrackItemData>(GetPropertyNameFromColumnHeader(sender));
        }

        private void FilterTracks_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            _libraryViewData.SetGlobalTracksFilters(
                (btn?.DataContext as AlbumArtistItemData)?.SourceData,
                (btn?.DataContext as AlbumItemData)?.SourceData,
                (btn?.DataContext as GenreItemData)?.SourceData,
                (btn?.DataContext as PerformerItemData)?.SourceData,
                (btn?.DataContext as YearItemData)?.Year);
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkAlbumArtist_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(albumArtistFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Album?.AlbumArtist);
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkAlbum_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(albumFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Album);
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkPerformer_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(performerFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Performers?.FirstOrDefault());
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkGenre_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(genreFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Genres?.FirstOrDefault());
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkYear_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(yearFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.Year);
            SetTracksViewSourceWithFilters();
            MainView.SelectedItem = TracksTab;
        }

        private void AlbumArtistsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            AlbumArtistsView.ItemsSource = _libraryViewData.ApplyArtistAlbumsFilters(
                DuplicateAlbumArtistsCheckBox.IsChecked == true,
                EmptyAlbumArtistsCheckBox.IsChecked == true);
        }

        private void ClearAlbumArtistFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumArtistsCheckBox.IsChecked = false;
            EmptyAlbumArtistsCheckBox.IsChecked = false;
            AlbumArtistsView.ItemsSource = _libraryViewData.ApplyArtistAlbumsFilters(
                DuplicateAlbumArtistsCheckBox.IsChecked == true,
                EmptyAlbumArtistsCheckBox.IsChecked == true);
        }

        private void AlbumsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            AlbumsView.ItemsSource = _libraryViewData.ApplyAlbumsFilters(
                DuplicateAlbumsCheckBox.IsChecked == true,
                EmptyAlbumsCheckBox.IsChecked == true,
                InvalidFrontCoverCheckBox.IsChecked == true,
                MultipleYearsCheckBox.IsChecked == true,
                InvalidTracksOrderCheckBox.IsChecked == true);
        }

        private void ClearAlbumFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumsCheckBox.IsChecked = false;
            EmptyAlbumsCheckBox.IsChecked = false;
            InvalidTracksOrderCheckBox.IsChecked = false;
            MultipleYearsCheckBox.IsChecked = false;
            InvalidFrontCoverCheckBox.IsChecked = false;
            AlbumsView.ItemsSource = _libraryViewData.ApplyAlbumsFilters(
                DuplicateAlbumsCheckBox.IsChecked == true,
                EmptyAlbumsCheckBox.IsChecked == true,
                InvalidFrontCoverCheckBox.IsChecked == true,
                MultipleYearsCheckBox.IsChecked == true,
                InvalidTracksOrderCheckBox.IsChecked == true);
        }

        private void PerformersCheckBox_Click(object sender, RoutedEventArgs e)
        {
            PerformersView.ItemsSource = _libraryViewData.ApplyPerformerssFilters(
                DuplicatePerformersCheckBox.IsChecked == true,
                EmptyPerformersCheckBox.IsChecked == true);
        }

        private void ClearPerformerFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicatePerformersCheckBox.IsChecked = false;
            EmptyPerformersCheckBox.IsChecked = false;
            PerformersView.ItemsSource = _libraryViewData.ApplyPerformerssFilters(
                DuplicatePerformersCheckBox.IsChecked == true,
                EmptyPerformersCheckBox.IsChecked == true);
        }

        private void GenresCheckBox_Click(object sender, RoutedEventArgs e)
        {
            GenresView.ItemsSource = _libraryViewData.ApplyGenresFilters(
                DuplicateGenresCheckBox.IsChecked == true,
                EmptyGenresCheckBox.IsChecked == true);
        }

        private void ClearGenreFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateGenresCheckBox.IsChecked = false;
            EmptyGenresCheckBox.IsChecked = false;
            GenresView.ItemsSource = _libraryViewData.ApplyGenresFilters(
                DuplicateGenresCheckBox.IsChecked == true,
                EmptyGenresCheckBox.IsChecked == true);
        }

        private void YearsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            YearsView.ItemsSource = _libraryViewData.ApplyYearsFilters(
                EmptyYearsCheckBox.IsChecked == true);
        }

        private void ClearYearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyYearsCheckBox.IsChecked = false;
            YearsView.ItemsSource = _libraryViewData.ApplyYearsFilters(
                EmptyYearsCheckBox.IsChecked == true);
        }

        private void TracksCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetTracksViewSourceWithFilters();
        }

        private void ClearTracksFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            InvalidNumberTracksCheckBox.IsChecked = false;
            EmptyTracksCheckBox.IsChecked = false;
            EmptyAlbumArtistTracksCheckBox.IsChecked = false;
            SeveralAlbumArtistTracksCheckBox.IsChecked = false;
            EmptyAlbumTracksCheckBox.IsChecked = false;
            EmptyPerformerTracksCheckBox.IsChecked = false;
            DuplicatePerformersTracksCheckBox.IsChecked = false;
            EmptyGenreTracksCheckBox.IsChecked = false;
            DuplicateGenresTracksCheckBox.IsChecked = false;
            InvalidYearTracksCheckBox.IsChecked = false;
            InvalidFrontCoverTracksCheckBox.IsChecked = false;
            _libraryViewData.SetGlobalTracksFilters();
            SetTracksViewSourceWithFilters();
        }

        #endregion Window events

        #region Private helper methods

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            LogsView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            SetTracksViewSourceWithFilters();
            AlbumArtistsView.ItemsSource = _libraryViewData.ApplyArtistAlbumsFilters(false, false);
            AlbumsView.ItemsSource = _libraryViewData.ApplyAlbumsFilters(false, false, false, false, false);
            GenresView.ItemsSource = _libraryViewData.ApplyGenresFilters(false, false);
            PerformersView.ItemsSource = _libraryViewData.ApplyPerformerssFilters(false, false);
            YearsView.ItemsSource = _libraryViewData.ApplyYearsFilters(false);
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

        private static string GetPropertyNameFromColumnHeader(object sender)
        {
            return (sender as GridViewColumnHeader).Tag.ToString();
        }

        private void SetTracksViewSourceWithFilters()
        {
            TracksView.ItemsSource = _libraryViewData.ApplyTracksFilters(
                InvalidNumberTracksCheckBox.IsChecked == true,
                EmptyTracksCheckBox.IsChecked == true,
                EmptyAlbumArtistTracksCheckBox.IsChecked == true,
                SeveralAlbumArtistTracksCheckBox.IsChecked == true,
                EmptyAlbumTracksCheckBox.IsChecked == true,
                EmptyPerformerTracksCheckBox.IsChecked == true,
                DuplicatePerformersTracksCheckBox.IsChecked == true,
                EmptyGenreTracksCheckBox.IsChecked == true,
                DuplicateGenresTracksCheckBox.IsChecked == true,
                InvalidYearTracksCheckBox.IsChecked == true,
                InvalidFrontCoverTracksCheckBox.IsChecked == true);
        }

        #endregion Private helper methods
    }
}
