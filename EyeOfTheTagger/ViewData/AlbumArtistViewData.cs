using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Album artist view data.
    /// </summary>
    /// <seealso cref="BaseViewData"/>
    internal class AlbumArtistViewData : BaseViewData
    {
        /// <summary>
        /// <see cref="AlbumArtistData.Name"/>
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Albums count.
        /// </summary>
        public int AlbumsCount { get; private set; }
        /// <summary>
        /// Tracks count.
        /// </summary>
        public int TracksCount { get; private set; }
        /// <summary>
        /// Tracks length.
        /// </summary>
        public TimeSpan TracksLength { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="AlbumArtistData"/></param>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public AlbumArtistViewData(AlbumArtistData sourceData, LibraryData library)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Album.AlbumArtist == sourceData);

            Name = sourceData.Name;
            AlbumsCount = tracks.Select(t => t.Album).Distinct().Count();
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}
