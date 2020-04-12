using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Abstraction of every kind of view datas.
    /// </summary>
    internal class BaseViewData
    {
        /// <summary>
        /// Tries to get the value of a property of this instance by its name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property value for this instance; <c>Null</c> if failure.</returns>
        public object GetValue(string propertyName)
        {
            try
            {
                return GetType().GetProperty(propertyName).GetValue(this);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tranforms a list of <see cref="AlbumArtistData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="AlbumArtistViewData"/>.
        /// Results are sorted by <see cref="AlbumArtistData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="AlbumArtistViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<AlbumArtistViewData> GetAlbumArtistsViewData(LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.AlbumArtists
                            .Select(aa => new AlbumArtistViewData(aa, library))
                            .OrderBy(aa => aa.Name);
        }

        /// <summary>
        /// Tranforms a list of <see cref="AlbumData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="AlbumViewData"/>.
        /// Results are sorted by <see cref="AlbumData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="AlbumViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<AlbumViewData> GetAlbumsViewData(LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.Albums
                            .Select(a => new AlbumViewData(a, library))
                            .OrderBy(a => a.Name);
        }

        /// <summary>
        /// Tranforms a list of <see cref="GenreData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="GenreViewData"/>.
        /// Results are sorted by <see cref="GenreData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="GenreViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<GenreViewData> GetGenresViewData(LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.Genres
                            .Select(g => new GenreViewData(g, library))
                            .OrderBy(g => g.Name);
        }

        /// <summary>
        /// Tranforms a list of <see cref="PerformerData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="PerformerViewData"/>.
        /// Results are sorted by <see cref="PerformerData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="PerformerViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<PerformerViewData> GetPerformersViewData(LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.Performers
                            .Select(p => new PerformerViewData(p, library))
                            .OrderBy(p => p.Name);
        }

        /// <summary>
        /// Tranforms a list of years from the specified <see cref="LibraryData"/> into a list of <see cref="YearViewData"/>.
        /// Results are sorted by <see cref="YearViewData.Year"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="YearViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<YearViewData> GetYearsViewData(LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.Years
                            .Select(y => new YearViewData(y, library))
                            .OrderBy(p => p.Year);
        }

        /// <summary>
        /// Filters and transforms a list of <see cref="TrackData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="TrackViewData"/>.
        /// Results are sorted by <see cref="AlbumArtistData.Name"/>,
        /// then by <see cref="AlbumData.Name"/>,
        /// and finally by <see cref="TrackData.Number"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <param name="albumArtistFilter">Optionnal; <see cref="AlbumArtistData"/> filter.</param>
        /// <param name="albumFilter">Optionnal; <see cref="AlbumData"/> filter.</param>
        /// <param name="performerFilter">Optionnal; <see cref="PerformerData"/> filter.</param>
        /// <param name="genreFilter">Optionnal; <see cref="GenreData"/> filter.</param>
        /// <param name="yearFilter">Optionnal; year filter.</param>
        /// <returns>List of <see cref="TrackViewData"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public static IEnumerable<TrackViewData> GetTracksViewData(LibraryData library,
            AlbumArtistData albumArtistFilter = null,
            AlbumData albumFilter = null,
            PerformerData performerFilter = null,
            GenreData genreFilter = null,
            uint? yearFilter = null)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            return library.Tracks
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
