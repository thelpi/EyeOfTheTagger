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

        public static IEnumerable<AlbumArtistViewData> GetAlbumArtistsViewData(LibraryData library)
        {
            return library.AlbumArtists
                            .Select(aa => new AlbumArtistViewData(aa, library))
                            .OrderBy(aa => aa.Name);
        }

        public static IEnumerable<AlbumViewData> GetAlbumsViewData(LibraryData library)
        {
            return library.Albums
                            .Select(aa => new AlbumViewData(aa, library))
                            .OrderBy(aa => aa.Name);
        }
    }
}
