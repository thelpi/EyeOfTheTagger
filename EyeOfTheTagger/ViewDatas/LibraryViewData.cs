﻿using System;
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

        private readonly Dictionary<Type, Dictionary<string, bool>> _sortState =
            Tools.GetSubTypes(typeof(BaseItemData)).ToDictionary(t => t, t => new Dictionary<string, bool>());
        private AlbumArtistData _albumArtistFilter = null;
        private AlbumData _albumFilter = null;
        private PerformerData _performerFilter = null;
        private GenreData _genreFilter = null;
        private uint? _yearFilter = null;

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
        
        private IEnumerable<AlbumArtistItemData> GetAlbumArtistItemDatas()
        {
            return _library
                        .AlbumArtists
                        .Select(aa => new AlbumArtistItemData(aa, _library))
                        .OrderBy(aa => aa.Name);
        }

        private IEnumerable<AlbumItemData> GetAlbumItemDatas()
        {
            return _library
                        .Albums
                        .Select(a => new AlbumItemData(a, _library))
                        .OrderBy(a => a.Name);
        }

        private IEnumerable<GenreItemData> GetGenreItemDatas()
        {
            return _library
                        .Genres
                        .Select(g => new GenreItemData(g, _library))
                        .OrderBy(g => g.Name);
        }

        private IEnumerable<PerformerItemData> GetPerformerItemDatas()
        {
            return _library
                        .Performers
                        .Select(p => new PerformerItemData(p, _library))
                        .OrderBy(p => p.Name);
        }

        private IEnumerable<YearItemData> GetYearItemDatas()
        {
            return _library
                        .Years
                        .Select(y => new YearItemData(y, _library))
                        .OrderBy(p => p.Year);
        }

        private IEnumerable<TrackItemData> GetTrackItemDatas()
        {
            return _library
                        .Tracks
                        .Where(t =>
                            (_albumArtistFilter == null || t.Album.AlbumArtist == _albumArtistFilter)
                            && (_albumFilter == null || t.Album == _albumFilter)
                            && (_performerFilter == null || t.Performers.Contains(_performerFilter))
                            && (_genreFilter == null || t.Genres.Contains(_genreFilter))
                            && (!_yearFilter.HasValue || t.Year == _yearFilter.Value))
                        .Select(t => new TrackItemData(t))
                        .OrderBy(t => t.AlbumArtist)
                        .ThenBy(t => t.Album)
                        .ThenBy(t => t.Number);
        }

        /// <summary>
        /// Applies filters on the base list of <see cref="AlbumArtistItemData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="AlbumArtistItemData"/>.</returns>
        public IEnumerable<AlbumArtistItemData> ApplyArtistAlbumsFilters(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<AlbumArtistItemData> albumArtistItems = GetAlbumArtistItemDatas();

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
        /// Applies filters on the base list of <see cref="AlbumItemData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <param name="checkFrontCovers">Filters invalid front covers.</param>
        /// <param name="checkYears">Filters invalid years.</param>
        /// <param name="checkTrackNumberSequences">Filters invalid track number sequences.</param>
        /// <returns>List of <see cref="AlbumItemData"/>.</returns>
        public IEnumerable<AlbumItemData> ApplyAlbumsFilters(bool checkDuplicates, bool checkEmpty,
            bool checkFrontCovers, bool checkYears, bool checkTrackNumberSequences)
        {
            IEnumerable<AlbumItemData> albumItems = GetAlbumItemDatas();

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
        /// Applies filters on the base list of <see cref="PerformerItemData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="PerformerItemData"/>.</returns>
        public IEnumerable<PerformerItemData> ApplyPerformerssFilters(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<PerformerItemData> performerItems = GetPerformerItemDatas();

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

        /// <summary>
        /// Applies filters on the base list of <see cref="GenreItemData"/>.
        /// </summary>
        /// <param name="checkDuplicates">Filters duplicates names.</param>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="GenreItemData"/>.</returns>
        public IEnumerable<GenreItemData> ApplyGenresFilters(bool checkDuplicates, bool checkEmpty)
        {
            IEnumerable<GenreItemData> genreItems = GetGenreItemDatas();

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

        /// <summary>
        /// Applies filters on the base list of <see cref="YearItemData"/>.
        /// </summary>
        /// <param name="checkEmpty">Filters invalid names.</param>
        /// <returns>List of <see cref="YearItemData"/>.</returns>
        public IEnumerable<YearItemData> ApplyYearsFilters(bool checkEmpty)
        {
            IEnumerable<YearItemData> yearItems = GetYearItemDatas();

            if (checkEmpty)
            {
                yearItems = yearItems.Where(aa => aa.Year == 0);
            }

            return yearItems;
        }

        /// <summary>
        /// Gets every instances of the specified <typeparamref name="TItemData"/>, sorted by the specified property name if any.
        /// For the <see cref="TrackItemData"/> type, filters will apply.
        /// </summary>
        /// <typeparam name="TItemData">The item data type.</typeparam>
        /// <param name="propertyName">Optionnal; name of property on which the sort applies.</param>
        /// <returns>Sorted list of datas.</returns>
        public IEnumerable<TItemData> GetSortedData<TItemData>(string propertyName = null) where TItemData : BaseItemData
        {
            // TODO: multiple columns sort

            IEnumerable<TItemData> dataRetrieved = GetDatas<TItemData>();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return dataRetrieved;
            }

            var propertyInfo = Tools.GetProperty<TItemData>(propertyName);
            if (propertyInfo == null)
            {
                return dataRetrieved;
            }

            if (!_sortState[typeof(TItemData)].ContainsKey(propertyName) || !_sortState[typeof(TItemData)][propertyName])
            {
                dataRetrieved = dataRetrieved.OrderByDescending(d => propertyInfo.GetValue(d));
            }
            else
            {
                dataRetrieved = dataRetrieved.OrderBy(d => propertyInfo.GetValue(d));
            }

            if (!_sortState[typeof(TItemData)].ContainsKey(propertyName))
            {
                _sortState[typeof(TItemData)].Add(propertyName, true);
            }
            else
            {
                _sortState[typeof(TItemData)][propertyName] = !_sortState[typeof(TItemData)][propertyName];
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
        
        private IEnumerable<TViewData> GetDatas<TViewData>() where TViewData : BaseItemData
        {
            // TODO: awfull design.
            if (typeof(TViewData) == typeof(AlbumArtistItemData))
            {
                return GetAlbumArtistItemDatas().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(AlbumItemData))
            {
                return GetAlbumItemDatas().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(GenreItemData))
            {
                return GetGenreItemDatas().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(PerformerItemData))
            {
                return GetPerformerItemDatas().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(YearItemData))
            {
                return GetYearItemDatas().Cast<TViewData>();
            }
            else if (typeof(TViewData) == typeof(TrackItemData))
            {
                return GetTrackItemDatas().Cast<TViewData>();
            }

            return new List<TViewData>();
        }
    }
}
