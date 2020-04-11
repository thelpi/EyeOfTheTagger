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
        /// Tranforms list of <see cref="AlbumArtistData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="AlbumArtistViewData"/>.
        /// Results are sorted by <see cref="AlbumArtistData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="AlbumArtistViewData"/>.</returns>
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
        /// Tranforms list of <see cref="AlbumData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="AlbumViewData"/>.
        /// Results are sorted by <see cref="AlbumData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="AlbumViewData"/>.</returns>
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
        /// Tranforms list of <see cref="GenreData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="GenreViewData"/>.
        /// Results are sorted by <see cref="GenreData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="GenreViewData"/>.</returns>
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
        /// Tranforms list of <see cref="PerformerData"/> from the specified <see cref="LibraryData"/> into a list of <see cref="PerformerViewData"/>.
        /// Results are sorted by <see cref="PerformerData.Name"/>.
        /// </summary>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <returns>List of <see cref="PerformerViewData"/>.</returns>
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
    }
}
