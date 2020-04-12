using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Constructor.
        /// Instanciates the inner <see cref="LibraryData"/> itself.
        /// </summary>
        /// <param name="loadingLogHandler">Callback method at the <see cref="LibraryData.LoadingLogHandler"/> event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="loadingLogHandler"/> is <c>Null</c>.</exception>
        public LibraryViewData(EventHandler<LoadingLogEventArgs> loadingLogHandler)
        {
            _library = new LibraryData(Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories),
                Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions), false);

            _library.LoadingLogHandler += loadingLogHandler ?? throw new ArgumentNullException(nameof(loadingLogHandler));
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
        /// <param name="albumArtistFilter">Optionnal; <see cref="AlbumArtistData"/> filter.</param>
        /// <param name="albumFilter">Optionnal; <see cref="AlbumData"/> filter.</param>
        /// <param name="performerFilter">Optionnal; <see cref="PerformerData"/> filter.</param>
        /// <param name="genreFilter">Optionnal; <see cref="GenreData"/> filter.</param>
        /// <param name="yearFilter">Optionnal; year filter.</param>
        /// <returns>List of <see cref="TrackViewData"/>.</returns>
        public IEnumerable<TrackViewData> GetTracksViewData(AlbumArtistData albumArtistFilter = null,
            AlbumData albumFilter = null, PerformerData performerFilter = null,
            GenreData genreFilter = null, uint? yearFilter = null)
        {
            return _library
                        .Tracks
                        .Where(t =>
                            (albumArtistFilter == null || t.Album.AlbumArtist == albumArtistFilter)
                            && (albumFilter == null || t.Album == albumFilter)
                            && (performerFilter == null || t.Performers.Contains(performerFilter))
                            && (genreFilter == null || t.Genres.Contains(genreFilter))
                            && (!yearFilter.HasValue || t.Year == yearFilter.Value))
                        .Select(t => new TrackViewData(t))
                        .OrderBy(t => t.AlbumArtist)
                        .ThenBy(t => t.Album)
                        .ThenBy(t => t.Number);
        }
    }
}
