using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTagger.ItemDatas.Abstractions;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ItemDatas
{
    /// <summary>
    /// Year item data.
    /// </summary>
    /// <seealso cref="BaseItemData"/>
    internal class YearItemData : BaseItemData
    {
        /// <summary>
        /// Release year.
        /// </summary>
        public uint Year { get; private set; }
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
        /// <param name="year"><see cref="Year"/></param>
        /// <param name="library"><see cref="LibraryEngine"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        public YearItemData(uint year, LibraryEngine library) : base(null)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Year == year);

            Year = year;
            AlbumsCount = tracks.GroupBy(t => t.Album).Count();
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}
