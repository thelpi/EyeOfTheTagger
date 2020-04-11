﻿using System;
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
        /// <see cref="AlbumData.Name"/>
        /// </summary>
        public string Name { get; private set; }
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
        /// <param name="sourceData"><see cref="AlbumData"/></param>
        /// <param name="library"><see cref="LibraryData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public AlbumViewData(AlbumData sourceData, LibraryData library)
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
            TracksCount = library.Tracks.Count(t => t.Album == sourceData);
            TracksLength = new TimeSpan(0, 0, (int)library.Tracks.Where(t => t.Album == sourceData).Sum(t => t.Length.TotalSeconds));
        }
    }
}