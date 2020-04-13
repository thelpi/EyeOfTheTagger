using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Events;

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
                { typeof(TrackViewData), new Dictionary<string, bool>() }
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
        
        private IEnumerable<AlbumArtistViewData> GetAlbumArtistsViewData()
        {
            return _library
                        .AlbumArtists
                        .Select(aa => new AlbumArtistViewData(aa, _library))
                        .OrderBy(aa => aa.Name);
        }

        private IEnumerable<AlbumViewData> GetAlbumsViewData()
        {
            return _library
                        .Albums
                        .Select(a => new AlbumViewData(a, _library))
                        .OrderBy(a => a.Name);
        }

        private IEnumerable<GenreViewData> GetGenresViewData()
        {
            return _library
                        .Genres
                        .Select(g => new GenreViewData(g, _library))
                        .OrderBy(g => g.Name);
        }

        private IEnumerable<PerformerViewData> GetPerformersViewData()
        {
            return _library
                        .Performers
                        .Select(p => new PerformerViewData(p, _library))
                        .OrderBy(p => p.Name);
        }

        private IEnumerable<YearViewData> GetYearsViewData()
        {
            return _library
                        .Years
                        .Select(y => new YearViewData(y, _library))
                        .OrderBy(p => p.Year);
        }

        private IEnumerable<TrackViewData> GetTracksViewData()
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
        /// Gets every instances of the specified <typeparamref name="TViewData"/>, sorted by the specified property name if any.
        /// For the <see cref="TrackViewData"/> type, filters will apply.
        /// </summary>
        /// <typeparam name="TViewData">The view data type.</typeparam>
        /// <param name="propertyName">Optionnal; name of property on which the sort applies.</param>
        /// <returns>Sorted list of datas.</returns>
        public IEnumerable<TViewData> GetSortedData<TViewData>(string propertyName = null)
        {
            IEnumerable<TViewData> dataRetrieved = GetDatas<TViewData>();

            if (string.IsNullOrWhiteSpace(propertyName) || !_sortState.ContainsKey(typeof(TViewData)))
            {
                return dataRetrieved;
            }

            var propertyInfo = Tools.GetProperty<TViewData>(propertyName);
            if (propertyInfo == null)
            {
                return dataRetrieved;
            }

            if (!_sortState[typeof(TViewData)].ContainsKey(propertyName) || !_sortState[typeof(TViewData)][propertyName])
            {
                dataRetrieved = dataRetrieved.OrderByDescending(d => propertyInfo.GetValue(d));
            }
            else
            {
                dataRetrieved = dataRetrieved.OrderBy(d => propertyInfo.GetValue(d));
            }

            if (!_sortState[typeof(TViewData)].ContainsKey(propertyName))
            {
                _sortState[typeof(TViewData)].Add(propertyName, true);
            }
            else
            {
                _sortState[typeof(TViewData)][propertyName] = !_sortState[typeof(TViewData)][propertyName];
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
        
        private IEnumerable<TViewData> GetDatas<TViewData>()
        {
            // TODO : awfull design.
            if (typeof(TViewData) == typeof(AlbumArtistViewData))
            {
                return GetAlbumArtistsViewData().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(AlbumViewData))
            {
                return GetAlbumsViewData().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(GenreViewData))
            {
                return GetGenresViewData().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(PerformerViewData))
            {
                return GetPerformersViewData().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(YearViewData))
            {
                return GetYearsViewData().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(TrackViewData))
            {
                return GetTracksViewData().Cast<TViewData>();
            }

            return new List<TViewData>();
        }
    }
}
