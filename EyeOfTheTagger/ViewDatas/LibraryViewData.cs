using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using EyeOfTheTagger.ItemDatas;
using EyeOfTheTagger.ItemDatas.Abstractions;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Datas;
using EyeOfTheTaggerLib.Events;

namespace EyeOfTheTagger.ViewDatas
{
    /// <summary>
    /// Library view data.
    /// </summary>
    internal class LibraryViewData
    {
        /// <summary>
        /// <see cref="LibraryEngine.TotalFilesCount"/>
        /// </summary>
        public int TotalFilesCount { get { return _library.TotalFilesCount; } }

        private LibraryEngine _library;

        private readonly Dictionary<Type, List<KeyValuePair<string, bool>>> _itemDatasSort =
            new Dictionary<Type, List<KeyValuePair<string, bool>>>
            {
                {
                    typeof(AlbumArtistItemData), new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool> (nameof(AlbumArtistItemData.Name), false) }
                },
                {
                    typeof(AlbumItemData), new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool> (nameof(AlbumItemData.Name), false) }
                },
                {
                    typeof(PerformerItemData), new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool> (nameof(PerformerItemData.Name), false) }
                },
                {
                    typeof(GenreItemData), new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool> (nameof(GenreItemData.Name), false) }
                },
                {
                    typeof(YearItemData), new List<KeyValuePair<string, bool>> { new KeyValuePair<string, bool> (nameof(YearItemData.Year), false) }
                },
                {
                    typeof(TrackItemData), new List<KeyValuePair<string, bool>>
                    {
                        new KeyValuePair<string, bool>(nameof(TrackItemData.AlbumArtist), false),
                        new KeyValuePair<string, bool>(nameof(TrackItemData.Album), false),
                        new KeyValuePair<string, bool>(nameof(TrackItemData.Number), false),
                        new KeyValuePair<string, bool>(nameof(TrackItemData.Name), false)
                    }
                }
            };

        private AlbumArtistData _albumArtistGlobalFilter = null;
        private AlbumData _albumGlobalFilter = null;
        private PerformerData _performerGlobalFilter = null;
        private GenreData _genreGlobalFilter = null;
        private uint? _yearGlobalFilter = null;

        /// <summary>
        /// Constructor.
        /// Instanciates the inner <see cref="LibraryEngine"/> itself.
        /// </summary>
        /// <param name="bgwCallback">Delegate to get at runtime the <see cref="BackgroundWorker"/> which report progress.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bgwCallback"/> is <c>Null</c>.</exception>
        public LibraryViewData(Func<BackgroundWorker> bgwCallback)
        {
            if (bgwCallback == null)
            {
                throw new ArgumentNullException(nameof(bgwCallback));
            }

            _library = new LibraryEngine(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
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
        /// Proceeds to call <see cref="LibraryEngine.Reload(IEnumerable{string}, IEnumerable{string})"/>.
        /// </summary>
        public void Reload()
        {
            _library.Reload(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions));
        }

        #region Get item datas

        /// <summary>
        /// Gets a list of <see cref="AlbumArtistItemData"/> with specified filters.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="AlbumArtistItemData"/>.</returns>
        public IEnumerable<AlbumArtistItemData> GetAlbumArtists(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<AlbumArtistItemData> albumArtistItems =
                _library.AlbumArtists
                    .Select(a => new AlbumArtistItemData(a, _library));

            return ApplySort(ApplyFilters(albumArtistItems, checkDuplicates, checkEmpty));
        }

        /// <summary>
        /// Gets a list of <see cref="AlbumItemData"/> with specified filters.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <param name="checkFrontCovers">Filters invalid front covers.</param>
        /// <param name="checkYears">Filters invalid years.</param>
        /// <param name="checkTrackNumberSequences">Filters invalid track number sequences.</param>
        /// <returns>List of <see cref="AlbumItemData"/>.</returns>
        public IEnumerable<AlbumItemData> GetAlbums(bool checkDuplicates, bool checkEmpty,
            bool checkFrontCovers, bool checkYears, bool checkTrackNumberSequences)
        {
            IEnumerable<AlbumItemData> albumItems =
                _library.Albums
                    .Select(a => new AlbumItemData(a, _library));

            return ApplySort(ApplyFilters(albumItems, checkDuplicates, checkEmpty,
                checkFrontCovers, checkYears, checkTrackNumberSequences));
        }

        /// <summary>
        /// Gets a list of <see cref="PerformerItemData"/> with specified filters.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="PerformerItemData"/>.</returns>
        public IEnumerable<PerformerItemData> GetPerformers(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<PerformerItemData> performerItems =
                _library.Performers
                    .Select(p => new PerformerItemData(p, _library));

            return ApplySort(ApplyFilters(performerItems, checkDuplicates, checkEmpty));
        }

        /// <summary>
        /// Gets a list of <see cref="GenreItemData"/> with specified filters.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="GenreItemData"/>.</returns>
        public IEnumerable<GenreItemData> GetGenres(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<GenreItemData> genreItems =
                _library.Genres
                    .Select(g => new GenreItemData(g, _library));

            return ApplySort(ApplyFilters(genreItems, checkDuplicates, checkEmpty));
        }

        /// <summary>
        /// Gets a list of <see cref="YearItemData"/> with specified filters.
        /// </summary>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="YearItemData"/>.</returns>
        public IEnumerable<YearItemData> GetYears(bool checkEmpty)
        {
            IEnumerable<YearItemData> yearItems =
                _library.Years
                    .Select(y => new YearItemData(y, _library));

            return ApplySort(ApplyFilters(yearItems, checkEmpty));
        }

        /// <summary>
        /// Gets a list of <see cref="TrackItemData"/> with specified filters and global filters.
        /// </summary>
        /// <param name="checkInvalidNumber">Checks invalid track number.</param>
        /// <param name="checkEmpty">Checks empty track title.</param>
        /// <param name="checkEmptyAlbumArtist">Checks empty album artist.</param>
        /// <param name="checkSeveralAlbumArtist">Checks multiple alum artists.</param>
        /// <param name="checkEmptyAlbum">Checks empty album.</param>
        /// <param name="checkEmptyPerformer">Checks empty performer.</param>
        /// <param name="checkDuplicatePerformers">Checks duplicate performers.</param>
        /// <param name="checkEmptyGenre">Checks empty genre.</param>
        /// <param name="checkDuplicateGenres">Checks duplicate genres.</param>
        /// <param name="checkInvalidYear">Checks invalid year.</param>
        /// <param name="checkInvalidFrontCover">Checks tracks without front cover.</param>
        /// <returns>List of <see cref="TrackItemData"/>.</returns>
        public IEnumerable<TrackItemData> GetTracks(bool checkInvalidNumber, bool checkEmpty, bool checkEmptyAlbumArtist,
            bool checkSeveralAlbumArtist, bool checkEmptyAlbum, bool checkEmptyPerformer, bool checkDuplicatePerformers,
            bool checkEmptyGenre, bool checkDuplicateGenres, bool checkInvalidYear, bool checkInvalidFrontCover)
        {
            IEnumerable<TrackItemData> trackItems =
                _library.Tracks
                    .Where(t =>
                        (_albumArtistGlobalFilter == null || t.Album.AlbumArtist == _albumArtistGlobalFilter)
                        && (_albumGlobalFilter == null || t.Album == _albumGlobalFilter)
                        && (_performerGlobalFilter == null || t.Performers.Contains(_performerGlobalFilter))
                        && (_genreGlobalFilter == null || t.Genres.Contains(_genreGlobalFilter))
                        && (!_yearGlobalFilter.HasValue || t.Year == _yearGlobalFilter.Value))
                    .Select(t => new TrackItemData(t));

            return ApplySort(ApplyFilters(trackItems, checkInvalidNumber, checkEmpty, checkEmptyAlbumArtist,
                checkSeveralAlbumArtist, checkEmptyAlbum, checkEmptyPerformer, checkDuplicatePerformers, checkEmptyGenre,
                checkDuplicateGenres, checkInvalidYear, checkInvalidFrontCover));
        }

        #endregion Get item datas

        #region Apply filters on data

        /// <summary>
        /// Checks current filters on a <see cref="AlbumArtistItemData"/> instance.
        /// </summary>
        /// <param name="albumArtistItem"><see cref="AlbumArtistItemData"/></param>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="albumArtistItem"/> is <c>Null</c>.</exception>
        public bool CheckArtistAlbumFilters(AlbumArtistItemData albumArtistItem,
            bool checkDuplicates, bool checkEmpty)
        {
            if (albumArtistItem == null)
            {
                throw new ArgumentNullException(nameof(albumArtistItem));
            }

            return ApplyFilters(new List<AlbumArtistItemData> { albumArtistItem },
                checkDuplicates, checkEmpty
            ).Any();
        }

        /// <summary>
        /// Checks current filters on a <see cref="AlbumItemData"/> instance.
        /// </summary>
        /// <param name="albumItem"><see cref="AlbumItemData"/></param>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <param name="checkFrontCovers">Filters invalid front covers.</param>
        /// <param name="checkYears">Filters invalid years.</param>
        /// <param name="checkTrackNumberSequences">Filters invalid track number sequences.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="albumItem"/> is <c>Null</c>.</exception>
        public bool CheckAlbumsFilters(AlbumItemData albumItem,
            bool checkDuplicates, bool checkEmpty, bool checkFrontCovers, bool checkYears, bool checkTrackNumberSequences)
        {
            if (albumItem == null)
            {
                throw new ArgumentNullException(nameof(albumItem));
            }

            return ApplyFilters(new List<AlbumItemData> { albumItem },
                checkDuplicates, checkEmpty, checkFrontCovers, checkYears, checkTrackNumberSequences
            ).Any();
        }

        /// <summary>
        /// Checks current filters on a <see cref="AlbumArtistItemData"/> instance.
        /// </summary>
        /// <param name="performerItem"><see cref="PerformerItemData"/></param>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="performerItem"/> is <c>Null</c>.</exception>
        public bool CheckPerformersFilters(PerformerItemData performerItem,
            bool checkDuplicates, bool checkEmpty)
        {
            if (performerItem == null)
            {
                throw new ArgumentNullException(nameof(performerItem));
            }

            return ApplyFilters(new List<PerformerItemData> { performerItem },
                checkDuplicates, checkEmpty
            ).Any();
        }

        /// <summary>
        /// Checks current filters on a <see cref="GenreItemData"/> instance.
        /// </summary>
        /// <param name="genreItem"><see cref="GenreItemData"/></param>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="genreItem"/> is <c>Null</c>.</exception>
        public bool CheckGenresFilters(GenreItemData genreItem, bool checkDuplicates, bool checkEmpty)
        {
            if (genreItem == null)
            {
                throw new ArgumentNullException(nameof(genreItem));
            }

            return ApplyFilters(new List<GenreItemData> { genreItem },
                checkDuplicates, checkEmpty
            ).Any();
        }

        /// <summary>
        /// Checks current filters on a <see cref="YearItemData"/> instance.
        /// </summary>
        /// <param name="yearItem"><see cref="YearItemData"/></param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="yearItem"/> is <c>Null</c>.</exception>
        public bool CheckYearsFilters(YearItemData yearItem, bool checkEmpty)
        {
            if (yearItem == null)
            {
                throw new ArgumentNullException(nameof(yearItem));
            }

            return ApplyFilters(new List<YearItemData> { yearItem },
                checkEmpty
            ).Any();
        }

        /// <summary>
        /// Checks current filters on a <see cref="TrackItemData"/> instance.
        /// </summary>
        /// <param name="trackItem"><see cref="TrackItemData"/></param>
        /// <param name="checkInvalidNumber">Checks invalid track number.</param>
        /// <param name="checkEmpty">Checks empty track title.</param>
        /// <param name="checkEmptyAlbumArtist">Checks empty album artist.</param>
        /// <param name="checkSeveralAlbumArtist">Checks multiple alum artists.</param>
        /// <param name="checkEmptyAlbum">Checks empty album.</param>
        /// <param name="checkEmptyPerformer">Checks empty performer.</param>
        /// <param name="checkDuplicatePerformers">Checks duplicate performers.</param>
        /// <param name="checkEmptyGenre">Checks empty genre.</param>
        /// <param name="checkDuplicateGenres">Checks duplicate genres.</param>
        /// <param name="checkInvalidYear">Checks invalid year.</param>
        /// <param name="checkInvalidFrontCover">Checks tracks without front cover.</param>
        /// <returns><c>True</c> if all filters apply; <c>False</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackItem"/> is <c>Null</c>.</exception>
        public bool CheckTracksFilters(TrackItemData trackItem, bool checkInvalidNumber, bool checkEmpty, bool checkEmptyAlbumArtist,
            bool checkSeveralAlbumArtist, bool checkEmptyAlbum, bool checkEmptyPerformer, bool checkDuplicatePerformers,
            bool checkEmptyGenre, bool checkDuplicateGenres, bool checkInvalidYear, bool checkInvalidFrontCover)
        {
            if (trackItem == null)
            {
                throw new ArgumentNullException(nameof(trackItem));
            }

            return ApplyFilters(new List<TrackItemData> { trackItem },
                checkInvalidNumber, checkEmpty, checkEmptyAlbumArtist, checkSeveralAlbumArtist,
                checkEmptyAlbum, checkEmptyPerformer, checkDuplicatePerformers, checkEmptyGenre,
                checkDuplicateGenres, checkInvalidYear, checkInvalidFrontCover
            ).Any();
        }

        #endregion Apply filters on data

        #region Set filters and sorts

        /// <summary>
        /// Sets global tracks filters.
        /// </summary>
        /// <param name="albumArtistFilter">Optionnal; <see cref="AlbumData.AlbumArtist"/> filter.</param>
        /// <param name="albumFilter">Optionnal; <see cref="TrackData.Album"/> filter.</param>
        /// <param name="genreFilter">Optionnal; <see cref="TrackData.Genres"/> filter.</param>
        /// <param name="performerFilter">Optionnal; <see cref="TrackData.Performers"/> filter.</param>
        /// <param name="yearFilter">Optionnal; <see cref="TrackData.Year"/> filter.</param>
        public void SetGlobalTracksFilters(AlbumArtistData albumArtistFilter = null, AlbumData albumFilter = null,
            GenreData genreFilter = null, PerformerData performerFilter = null, uint? yearFilter = null)
        {
            _albumArtistGlobalFilter = albumArtistFilter;
            _albumGlobalFilter = albumFilter;
            _genreGlobalFilter = genreFilter;
            _performerGlobalFilter = performerFilter;
            _yearGlobalFilter = yearFilter;
        }

        /// <summary>
        /// Adds a sort column to a specified item data type.
        /// </summary>
        /// <typeparam name="TItemData">The type of item data.</typeparam>
        /// <param name="propertyName">The property name.</param>
        public void AddSort<TItemData>(string propertyName) where TItemData : BaseItemData
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return;
            }

            System.Reflection.PropertyInfo propertyInfo = Tools.GetProperty<TItemData>(propertyName);
            if (propertyInfo == null)
            {
                return;
            }

            bool descendingSort = false;
            if (_itemDatasSort[typeof(TItemData)].Any(kvp => kvp.Key == propertyName))
            {
                KeyValuePair<string, bool> propWithVal = _itemDatasSort[typeof(TItemData)].First(kvp => kvp.Key == propertyName);
                descendingSort = !propWithVal.Value;
                _itemDatasSort[typeof(TItemData)].Remove(propWithVal);
            }

            _itemDatasSort[typeof(TItemData)].Insert(0, new KeyValuePair<string, bool>(propertyName, descendingSort));
        }

        #endregion Set filters and sorts

        #region Apply filters and sorts

        private IEnumerable<AlbumArtistItemData> ApplyFilters(IEnumerable<AlbumArtistItemData> albumArtistItems,
            bool checkDuplicates, bool checkEmpty)
        {
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

        private IEnumerable<AlbumItemData> ApplyFilters(IEnumerable<AlbumItemData> albumItems,
            bool checkDuplicates, bool checkEmpty, bool checkFrontCovers, bool checkYears, bool checkTrackNumberSequences)
        {
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

        private IEnumerable<PerformerItemData> ApplyFilters(IEnumerable<PerformerItemData> performerItems,
            bool checkDuplicates, bool checkEmpty)
        {
            if (checkDuplicates)
            {
                performerItems = performerItems
                    .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                    .Where(p => p.Count() > 1)
                    .SelectMany(p => p);
            }

            if (checkEmpty)
            {
                performerItems = performerItems.Where(p => p.HasEmptyName());
            }

            return performerItems;
        }

        private IEnumerable<GenreItemData> ApplyFilters(IEnumerable<GenreItemData> genreItems,
            bool checkDuplicates, bool checkEmpty)
        {
            if (checkDuplicates)
            {
                genreItems = genreItems
                    .GroupBy(g => g.Name.Trim().ToLowerInvariant())
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);
            }

            if (checkEmpty)
            {
                genreItems = genreItems.Where(g => g.HasEmptyName());
            }

            return genreItems;
        }

        private IEnumerable<YearItemData> ApplyFilters(IEnumerable<YearItemData> yearItems,
            bool checkEmpty)
        {
            if (checkEmpty)
            {
                yearItems = yearItems.Where(aa => aa.Year == 0);
            }

            return yearItems;
        }

        private IEnumerable<TrackItemData> ApplyFilters(IEnumerable<TrackItemData> trackItems,
            bool checkInvalidNumber, bool checkEmpty, bool checkEmptyAlbumArtist, bool checkSeveralAlbumArtist,
            bool checkEmptyAlbum, bool checkEmptyPerformer, bool checkDuplicatePerformers, bool checkEmptyGenre,
            bool checkDuplicateGenres, bool checkInvalidYear, bool checkInvalidFrontCover)
        {
            if (checkInvalidNumber)
            {
                trackItems = trackItems.Where(t => t.Number == 0);
            }

            if (checkEmpty)
            {
                trackItems = trackItems.Where(t => t.HasEmptyName());
            }

            if (checkEmptyAlbumArtist)
            {
                trackItems = trackItems.Where(t => string.IsNullOrWhiteSpace(t.AlbumArtist));
            }

            if (checkSeveralAlbumArtist)
            {
                trackItems = trackItems.Where(t => t.HasSeveralAlbumArtists());
            }

            if (checkEmptyAlbum)
            {
                trackItems = trackItems.Where(t => string.IsNullOrWhiteSpace(t.Album));
            }

            if (checkEmptyPerformer)
            {
                trackItems = trackItems.Where(t => t.HasEmptyPerformer());
            }

            if (checkDuplicatePerformers)
            {
                trackItems = trackItems.Where(t => t.HasDuplicatePerformers());
            }

            if (checkEmptyGenre)
            {
                trackItems = trackItems.Where(t => t.HasEmptyGenre());
            }

            if (checkDuplicateGenres)
            {
                trackItems = trackItems.Where(t => t.HasDuplicateGenres());
            }

            if (checkInvalidYear)
            {
                trackItems = trackItems.Where(t => t.Year == 0);
            }

            if (checkInvalidFrontCover)
            {
                trackItems = trackItems.Where(t => t.HasNoFrontCover());
            }

            return trackItems;
        }

        private IEnumerable<TItemData> ApplySort<TItemData>(IEnumerable<TItemData> trackItems) where TItemData : BaseItemData
        {
            IOrderedEnumerable<TItemData> ordereredItems = trackItems.OrderBy(_ => 1);

            bool isFirstProperty = true;
            foreach (KeyValuePair<string, bool> kvp in _itemDatasSort[typeof(TItemData)])
            {
                Func<TItemData, object> keySelector = typeof(TItemData).GetProperty(kvp.Key).GetValue;
                if (isFirstProperty)
                {
                    ordereredItems = kvp.Value ? ordereredItems.OrderByDescending(keySelector)
                        : ordereredItems.OrderBy(keySelector);
                }
                else
                {
                    ordereredItems = kvp.Value ? ordereredItems.ThenByDescending(keySelector)
                        : ordereredItems.ThenBy(keySelector);
                }
                isFirstProperty = false;
            }

            return ordereredItems;
        }

        #endregion Apply filters and sorts
    }
}
