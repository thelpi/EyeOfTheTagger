using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTagger.ItemDatas.Abstractions;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ItemDatas
{
    /// <summary>
    /// Genre item data.
    /// </summary>
    /// <seealso cref="BaseItemData"/>
    internal class GenreItemData : BaseItemData
    {
        /// <summary>
        /// <see cref="GenreData"/>
        /// </summary>
        public new GenreData SourceData { get { return (GenreData)base.SourceData; } }
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
        /// <param name="sourceData"><see cref="BaseItemData.SourceData"/></param>
        /// <param name="library"><see cref="LibraryEngine"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public GenreItemData(GenreData sourceData, LibraryEngine library) : base(sourceData)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Genres.Contains(sourceData));
            
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}