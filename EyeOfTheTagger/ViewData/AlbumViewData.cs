using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Album view data.
    /// </summary>
    /// <seealso cref="BaseViewData"/>
    internal class AlbumViewData : BaseViewData
    {
        /// <summary>
        /// <see cref="AlbumData"/>
        /// </summary>
        public AlbumData SourceData { get; private set; }
        /// <summary>
        /// <see cref="AlbumData.Name"/>
        /// </summary>
        public string Name { get { return SourceData.Name; } }
        /// <summary>
        /// <see cref="AlbumData.AlbumArtist"/> name.
        /// </summary>
        public string AlbumArtist { get { return SourceData.AlbumArtist.Name; } }
        /// <summary>
        /// Release year.
        /// If several, takes the more likely.
        /// </summary>
        public uint Year { get; private set; }
        /// <summary>
        /// Main genre name.
        /// Empty if the album has no genred tracks.
        /// </summary>
        public string Genre { get; private set; }
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
        /// <param name="sourceData"><see cref="SourceData"/></param>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public AlbumViewData(AlbumData sourceData, LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            SourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Album == sourceData);
            
            Year = tracks.Select(t => t.Year).GroupBy(y => y).OrderByDescending(y => y.Count()).First().Key;
            Genre = tracks.SelectMany(t => t.Genres).GroupBy(g => g).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key?.Name ?? string.Empty;
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}
