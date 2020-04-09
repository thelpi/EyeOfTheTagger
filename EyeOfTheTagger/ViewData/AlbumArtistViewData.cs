using System;
using System.Linq;
using EyeOfTheTagger.Data;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Album artist view data.
    /// </summary>
    public class AlbumArtistViewData
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

            Name = sourceData.Name;
            AlbumsCount = library.Albums.Count(a => a.AlbumArtist.Name == sourceData.Name);
            TracksCount = library.Tracks.Count(t => t.Album.AlbumArtist.Name == sourceData.Name);
            TracksLength = new TimeSpan(0, 0, (int)library.Tracks.Where(t => t.Album.AlbumArtist.Name == sourceData.Name).Sum(t => t.Length.TotalSeconds));
        }
    }
}
