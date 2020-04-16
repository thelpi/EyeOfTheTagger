using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using EyeOfTheTagger.ItemDatas;
using EyeOfTheTagger.ViewDatas;
using Microsoft.Win32;

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
                ConsoleViewData.Default.AddLog(e.UserState as EyeOfTheTaggerLib.Datas.LogData);
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

        private void AlbumArtistsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<AlbumArtistItemData>(GetPropertyNameFromColumnHeader(sender));
            SetAlbumArtistsViewSource();
        }

        private void AlbumsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<AlbumItemData>(GetPropertyNameFromColumnHeader(sender));
            SetAlbumsViewSource();
        }

        private void GenresView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<GenreItemData>(GetPropertyNameFromColumnHeader(sender));
            SetGenresViewSource();
        }

        private void PerformersView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<PerformerItemData>(GetPropertyNameFromColumnHeader(sender));
            SetPerformersViewSource();
        }

        private void YearsView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<YearItemData>(GetPropertyNameFromColumnHeader(sender));
            SetYearsViewSource();
        }

        private void TracksView_GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.AddSort<TrackItemData>(GetPropertyNameFromColumnHeader(sender));
            SetTracksViewSource();
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
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkAlbumArtist_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(albumArtistFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Album?.AlbumArtist);
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkAlbum_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(albumFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Album);
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkPerformer_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(performerFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Performers?.FirstOrDefault());
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkGenre_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(genreFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.SourceData?.Genres?.FirstOrDefault());
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void LinkYear_Click(object sender, RoutedEventArgs e)
        {
            _libraryViewData.SetGlobalTracksFilters(yearFilter:
                ((sender as Hyperlink)?.DataContext as TrackItemData)?.Year);
            SetTracksViewSource();
            MainView.SelectedItem = TracksTab;
        }

        private void AlbumArtistsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetAlbumArtistsViewSource();
        }

        private void ClearAlbumArtistFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumArtistsCheckBox.IsChecked = false;
            EmptyAlbumArtistsCheckBox.IsChecked = false;
            SetAlbumArtistsViewSource();
        }

        private void AlbumsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetAlbumsViewSource();
        }

        private void ClearAlbumFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateAlbumsCheckBox.IsChecked = false;
            EmptyAlbumsCheckBox.IsChecked = false;
            InvalidTracksOrderCheckBox.IsChecked = false;
            MultipleYearsCheckBox.IsChecked = false;
            InvalidFrontCoverCheckBox.IsChecked = false;
            SetAlbumsViewSource();
        }

        private void PerformersCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetPerformersViewSource();
        }

        private void ClearPerformerFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicatePerformersCheckBox.IsChecked = false;
            EmptyPerformersCheckBox.IsChecked = false;
            SetPerformersViewSource();
        }

        private void GenresCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetGenresViewSource();
        }

        private void ClearGenreFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            DuplicateGenresCheckBox.IsChecked = false;
            EmptyGenresCheckBox.IsChecked = false;
            SetGenresViewSource();
        }

        private void YearsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetYearsViewSource();
        }

        private void ClearYearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            EmptyYearsCheckBox.IsChecked = false;
            SetYearsViewSource();
        }

        private void TracksCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SetTracksViewSource();
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
            SetTracksViewSource();
        }

        private void ChangeFrontCoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.DataContext is AlbumItemData album))
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = $"{Tools.GetAppName()} - Select an image as new front cover",
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Image Files(*.bmp;*.png;*.jpg;*.gif)|*.bmp;*.png;*.jpg;*.gif|All files (*.*)|*.*"
            };
            openFileDialog.ShowDialog();
            string selectedFileName = openFileDialog.FileName;
            if (!string.IsNullOrWhiteSpace(selectedFileName))
            {
                Dictionary<string, Exception> errors = album.SetFrontCover(selectedFileName);
                foreach (string errorFile in errors.Keys)
                {
                    MessageBox.Show($"The following error has occured while saving the picture as a front cover:\r\n{errors[errorFile]}\r\nFile: {errorFile}", $"{Tools.GetAppName()} - error");
                }
                MatchAlbumToViewSource(album);
                SetTracksViewSource();
            }
        }

        private void ScanMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConfigurationMenu_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ConsoleMenu_Click(object sender, RoutedEventArgs e)
        {
            new ConsoleWindow().ShowDialog();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion Window events

        #region Private helper methods

        private void DisplayWhileNotLoading()
        {
            LoadingBar.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
            SetTracksViewSource();
            SetAlbumArtistsViewSource();
            SetAlbumsViewSource();
            SetGenresViewSource();
            SetPerformersViewSource();
            SetYearsViewSource();
            ScanMenu.IsEnabled = true;
            ClearTracksFiltersButton.IsEnabled = true;
        }

        private void DisplayWhileLoading()
        {
            LoadingBar.Visibility = Visibility.Visible;
            LoadingBar.Value = 0;
            MainView.Visibility = Visibility.Collapsed;
            ScanMenu.IsEnabled = false;
            ClearTracksFiltersButton.IsEnabled = false;
        }

        private static string GetPropertyNameFromColumnHeader(object sender)
        {
            return (sender as GridViewColumnHeader).Tag.ToString();
        }

        private void SetAlbumArtistsViewSource()
        {
            AlbumArtistsView.ItemsSource = _libraryViewData.GetAlbumArtists(
                DuplicateAlbumArtistsCheckBox.IsChecked == true,
                EmptyAlbumArtistsCheckBox.IsChecked == true);
        }

        private void SetAlbumsViewSource()
        {
            AlbumsView.ItemsSource = _libraryViewData.GetAlbums(
                DuplicateAlbumsCheckBox.IsChecked == true,
                EmptyAlbumsCheckBox.IsChecked == true,
                InvalidFrontCoverCheckBox.IsChecked == true,
                MultipleYearsCheckBox.IsChecked == true,
                InvalidTracksOrderCheckBox.IsChecked == true);
        }

        private void SetGenresViewSource()
        {
            GenresView.ItemsSource = _libraryViewData.GetGenres(
                DuplicateGenresCheckBox.IsChecked == true,
                EmptyGenresCheckBox.IsChecked == true);
        }

        private void SetPerformersViewSource()
        {
            PerformersView.ItemsSource = _libraryViewData.GetPerformers(
                DuplicatePerformersCheckBox.IsChecked == true,
                EmptyPerformersCheckBox.IsChecked == true);
        }

        private void SetYearsViewSource()
        {
            YearsView.ItemsSource = _libraryViewData.GetYears(
                EmptyYearsCheckBox.IsChecked == true);
        }

        private void SetTracksViewSource()
        {
            TracksView.ItemsSource = _libraryViewData.GetTracks(
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

        private void MatchAlbumArtistToViewSource(AlbumArtistItemData albumArtistItem)
        {
            bool itemMatchFilter = _libraryViewData.CheckArtistAlbumFilters(albumArtistItem,
                DuplicateAlbumArtistsCheckBox.IsChecked == true,
                EmptyAlbumArtistsCheckBox.IsChecked == true);

            if (!itemMatchFilter)
            {
                AlbumArtistsView.Items.Remove(albumArtistItem);
            }
        }

        private void MatchAlbumToViewSource(AlbumItemData albumItem)
        {
            bool itemMatchFilter  = _libraryViewData.CheckAlbumsFilters(albumItem,
                DuplicateAlbumsCheckBox.IsChecked == true,
                EmptyAlbumsCheckBox.IsChecked == true,
                InvalidFrontCoverCheckBox.IsChecked == true,
                MultipleYearsCheckBox.IsChecked == true,
                InvalidTracksOrderCheckBox.IsChecked == true);

            if (!itemMatchFilter)
            {
                AlbumsView.Items.Remove(albumItem);
            }
        }

        private void MatchGenreToViewSource(GenreItemData genreItem)
        {
            bool itemMatchFilter = _libraryViewData.CheckGenresFilters(genreItem,
                DuplicateGenresCheckBox.IsChecked == true,
                EmptyGenresCheckBox.IsChecked == true);

            if (!itemMatchFilter)
            {
                GenresView.Items.Remove(genreItem);
            }
        }

        private void MatchPerformerToViewSource(PerformerItemData performerItem)
        {
            bool itemMatchFilter = _libraryViewData.CheckPerformersFilters(performerItem,
                DuplicatePerformersCheckBox.IsChecked == true,
                EmptyPerformersCheckBox.IsChecked == true);

            if (!itemMatchFilter)
            {
                PerformersView.Items.Remove(performerItem);
            }
        }

        private void MatchYearToViewSource(YearItemData yearItem)
        {
            bool itemMatchFilter = _libraryViewData.CheckYearsFilters(yearItem,
                EmptyYearsCheckBox.IsChecked == true);

            if (!itemMatchFilter)
            {
                YearsView.Items.Remove(yearItem);
            }
        }

        private void MatchTrackToViewSource(TrackItemData trackItem)
        {
            bool itemMatchFilter = _libraryViewData.CheckTracksFilters(trackItem,
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

            if (!itemMatchFilter)
            {
                TracksView.Items.Remove(trackItem);
            }
        }

        #endregion Private helper methods
    }
}
