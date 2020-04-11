using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Performer view data.
    /// </summary>
    /// <seealso cref="BaseViewData"/>
    internal class PerformerViewData : BaseViewData
    {
        private readonly PerformerData _sourceData;

        /// <summary>
        /// <see cref="PerformerData.Name"/>
        /// </summary>
        public string Name { get { return _sourceData.Name; } }
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
        /// <param name="sourceData"><see cref="PerformerData"/></param>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public PerformerViewData(PerformerData sourceData, LibraryData library)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            _sourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));

            IEnumerable<TrackData> tracks = library.Tracks.Where(t => t.Performers.Contains(sourceData));
            
            TracksCount = tracks.Count();
            TracksLength = new TimeSpan(0, 0, (int)tracks.Sum(t => t.Length.TotalSeconds));
        }
    }
}
