using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTagger.ItemDatas.Abstractions;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ItemDatas
{
    /// <summary>
    /// Performer item data.
    /// </summary>
    /// <seealso cref="BaseItemData"/>
    internal class PerformerItemData : BaseItemData
    {
        /// <summary>
        /// <see cref="PerformerData"/>
        /// </summary>
        public new PerformerData SourceData { get { return (PerformerData)base.SourceData; } }
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
        public PerformerItemData(PerformerData sourceData, LibraryEngine library) : base(sourceData)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Performers.Contains(sourceData));
            
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}
