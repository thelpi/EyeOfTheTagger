using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Event;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Library view data.
    /// </summary>
    internal class LibraryViewData
    {
        /// <summary>
        /// <see cref="LibraryData.TotalFilesCount"/>
        /// </summary>
        public int TotalFilesCount { get { return _library.TotalFilesCount; } }

        private LibraryData _library;

        private readonly Dictionary<Type, Dictionary<string, bool>> _sortState =
            new Dictionary<Type, Dictionary<string, bool>>
            {
                { typeof(AlbumArtistViewData), new Dictionary<string, bool>() },
                { typeof(AlbumViewData), new Dictionary<string, bool>() },
                { typeof(GenreViewData), new Dictionary<string, bool>() },
                { typeof(PerformerViewData), new Dictionary<string, bool>() },
                { typeof(YearViewData), new Dictionary<string, bool>() },
                { typeof(TrackViewData), new Dictionary<string, bool>() },
            };
        private AlbumArtistData _albumArtistFilter = null;
        private AlbumData _albumFilter = null;
        private PerformerData _performerFilter = null;
        private GenreData _genreFilter = null;
        private uint? _yearFilter = null;

        /// <summary>
        /// Constructor.
        /// Instanciates the inner <see cref="LibraryData"/> itself.
        /// </summary>
        /// <param name="bgwCallback">Delegate to get at runtime the <see cref="BackgroundWorker"/> which report progress.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bgwCallback"/> is <c>Null</c>.</exception>
        public LibraryViewData(Func<BackgroundWorker> bgwCallback)
        {
            if (bgwCallback == null)
            {
                throw new ArgumentNullException(nameof(bgwCallback));
            }

            _library = new LibraryData(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions), false);

            _library.LoadingLogHandler += delegate (object sender, LoadingLogEventArgs e)
            {
                if (e?.Log != null && TotalFilesCount > -1)
                {
                    int progressPercentage = e.TrackIndex == -1 ? 100 : Convert.ToInt32(e.TrackIndex / (decimal)TotalFilesCount * 100);
                    bgwCallback.Invoke().ReportProgress(progressPercentage, e.Log);
                }
            };
        }

        /// <summary>
        /// Proceeds to call <see cref="LibraryData.Reload(IEnumerable{string}, IEnumerable{string})"/>.
        /// </summary>
        public void Reload()
        {
            _library.Reload(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions));
        }

        /// <summary>
        /// Tranforms <see cref="LibraryData.AlbumArtists"/> into a list of <see cref="AlbumArtistViewData"/>.
        /// Results are sorted by <see cref="AlbumArtistData.Name"/>.
        /// </summary>
        /// <returns>List of <see cref="AlbumArtistViewData"/>.</returns>
        public IEnumerable<AlbumArtistViewData> GetAlbumArtistsViewData()
        {
            return _library
                        .AlbumArtists
                        .Select(aa => new AlbumArtistViewData(aa, _library))
                        .OrderBy(aa => aa.Name);
        }

        /// <summary>
        /// Tranforms <see cref="LibraryData.Albums"/> into a list of <see cref="AlbumViewData"/>.
        /// Results are sorted by <see cref="AlbumData.Name"/>.
        /// </summary>
        /// <returns>List of <see cref="AlbumViewData"/>.</returns>
        public IEnumerable<AlbumViewData> GetAlbumsViewData()
        {
            return _library
                        .Albums
                        .Select(a => new AlbumViewData(a, _library))
                        .OrderBy(a => a.Name);
        }

        /// <summary>
        /// Tranforms <see cref="LibraryData.Genres"/> into a list of <see cref="GenreViewData"/>.
        /// Results are sorted by <see cref="GenreData.Name"/>.
        /// </summary>
        /// <returns>List of <see cref="GenreViewData"/>.</returns>
        public IEnumerable<GenreViewData> GetGenresViewData()
        {
            return _library
                        .Genres
                        .Select(g => new GenreViewData(g, _library))
                        .OrderBy(g => g.Name);
        }

        /// <summary>
        /// Tranforms <see cref="LibraryData.Performers"/> into a list of <see cref="PerformerViewData"/>.
        /// Results are sorted by <see cref="PerformerData.Name"/>.
        /// </summary>
        /// <returns>List of <see cref="PerformerViewData"/>.</returns>
        public IEnumerable<PerformerViewData> GetPerformersViewData()
        {
            return _library
                        .Performers
                        .Select(p => new PerformerViewData(p, _library))
                        .OrderBy(p => p.Name);
        }

        /// <summary>
        /// Tranforms <see cref="LibraryData.Years"/> into a list of <see cref="YearViewData"/>.
        /// Results are sorted by <see cref="YearViewData.Year"/>.
        /// </summary>
        /// <returns>List of <see cref="YearViewData"/>.</returns>
        public IEnumerable<YearViewData> GetYearsViewData()
        {
            return _library
                        .Years
                        .Select(y => new YearViewData(y, _library))
                        .OrderBy(p => p.Year);
        }

        /// <summary>
        /// Filters and transforms <see cref="LibraryData.Tracks"/> into a list of <see cref="TrackViewData"/>.
        /// Results are sorted by <see cref="AlbumArtistData.Name"/>,
        /// then by <see cref="AlbumData.Name"/>,
        /// and finally by <see cref="TrackData.Number"/>.
        /// </summary>
        /// <returns>List of <see cref="TrackViewData"/>.</returns>
        public IEnumerable<TrackViewData> GetTracksViewData()
        {
            return _library
                        .Tracks
                        .Where(t =>
                            (_albumArtistFilter == null || t.Album.AlbumArtist == _albumArtistFilter)
                            && (_albumFilter == null || t.Album == _albumFilter)
                            && (_performerFilter == null || t.Performers.Contains(_performerFilter))
                            && (_genreFilter == null || t.Genres.Contains(_genreFilter))
                            && (!_yearFilter.HasValue || t.Year == _yearFilter.Value))
                        .Select(t => new TrackViewData(t))
                        .OrderBy(t => t.AlbumArtist)
                        .ThenBy(t => t.Album)
                        .ThenBy(t => t.Number);
        }

        /// <summary>
        /// Applies filters on the base list of <see cref="AlbumArtistViewData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="AlbumArtistViewData"/>.</returns>
        public IEnumerable<AlbumArtistViewData> ApplyArtistAlbumsFilters(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<AlbumArtistViewData> albumArtistItems = GetAlbumArtistsViewData();

            if (checkDuplicates)
            {
                albumArtistItems = albumArtistItems
                    .GroupBy(aa => aa.Name.Trim().ToLowerInvariant())
                    .Where(aa => aa.Count() > 1)
                    .SelectMany(aa => aa);
            }

            if (checkEmpty)
            {
                albumArtistItems = albumArtistItems.Where(aa => aa.HasEmptyName());
            }

            return albumArtistItems;
        }

        /// <summary>
        /// Applies filters on the base list of <see cref="AlbumViewData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <param name="checkFrontCovers">Filters invalid front covers.</param>
        /// <param name="checkYears">Filters invalid years.</param>
        /// <param name="checkTrackNumberSequences">Filters invalid track number sequences.</param>
        /// <returns>List of <see cref="AlbumViewData"/>.</returns>
        public IEnumerable<AlbumViewData> ApplyAlbumsFilters(bool checkDuplicates, bool checkEmpty,
            bool checkFrontCovers, bool checkYears, bool checkTrackNumberSequences)
        {
            IEnumerable<AlbumViewData> albumItems = GetAlbumsViewData();

            if (checkDuplicates)
            {
                albumItems = albumItems
                    .GroupBy(a => new KeyValuePair<string, string>(a.Name.Trim().ToLowerInvariant(), a.AlbumArtist))
                    .Where(a => a.Count() > 1)
                    .SelectMany(a => a);
            }

            if (checkEmpty)
            {
                albumItems = albumItems.Where(a => a.HasEmptyName());
            }

            if (checkFrontCovers)
            {
                albumItems = albumItems.Where(a => a.HasInvalidFrontCover());
            }

            if (checkYears)
            {
                albumItems = albumItems.Where(a => a.HasMultipleYears());
            }

            if (checkTrackNumberSequences)
            {
                albumItems = albumItems.Where(a => a.HasInvalidTrackSequence());
            }

            return albumItems;
        }

        /// <summary>
        /// Applies sort on a list of datas.
        /// </summary>
        /// <typeparam name="T">Type of datas.</typeparam>
        /// <param name="propertyName">Name of property on which the sort applies.</param>
        /// <param name="dataRetrieved">Initial list of datas.</param>
        /// <returns>Sorted list of datas.</returns>
        public IEnumerable<T> SortDatas<T>(string propertyName, IEnumerable<T> dataRetrieved)
        {
            if (!_sortState.ContainsKey(typeof(T)))
            {
                return dataRetrieved;
            }

            if (!_sortState[typeof(T)].ContainsKey(propertyName) || !_sortState[typeof(T)][propertyName])
            {
                dataRetrieved = dataRetrieved.OrderByDescending(d => d.GetType().GetProperty(propertyName).GetValue(d));
            }
            else
            {
                dataRetrieved = dataRetrieved.OrderBy(d => d.GetType().GetProperty(propertyName).GetValue(d));
            }

            if (!_sortState[typeof(T)].ContainsKey(propertyName))
            {
                _sortState[typeof(T)].Add(propertyName, true);
            }
            else
            {
                _sortState[typeof(T)][propertyName] = !_sortState[typeof(T)][propertyName];
            }

            return dataRetrieved;
        }

        /// <summary>
        /// Sets tracks filters.
        /// </summary>
        /// <param name="albumArtistFilter">Optionnal; <see cref="AlbumData.AlbumArtist"/> filter.</param>
        /// <param name="albumFilter">Optionnal; <see cref="TrackData.Album"/> filter.</param>
        /// <param name="genreFilter">Optionnal; <see cref="TrackData.Genres"/> filter.</param>
        /// <param name="performerFilter">Optionnal; <see cref="TrackData.Performers"/> filter.</param>
        /// <param name="yearFilter">Optionnal; <see cref="TrackData.Year"/> filter.</param>
        public void SetTracksFilters(AlbumArtistData albumArtistFilter = null, AlbumData albumFilter = null,
            GenreData genreFilter = null, PerformerData performerFilter = null, uint? yearFilter = null)
        {
            _albumArtistFilter = albumArtistFilter;
            _albumFilter = albumFilter;
            _genreFilter = genreFilter;
            _performerFilter = performerFilter;
            _yearFilter = yearFilter;
        }
    }
}
