﻿using System;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Track view data.
    /// </summary>
    /// <seealso cref="BaseViewData"/>
    internal class TrackViewData : BaseViewData
    {
        private readonly TrackData _sourceData;

        /// <summary>
        /// <see cref="TrackData.Number"/>
        /// </summary>
        public uint Number { get { return _sourceData.Number; } }
        /// <summary>
        /// <see cref="TrackData.Title"/>
        /// </summary>
        public string Title { get { return _sourceData.Title; } }
        /// <summary>
        /// <see cref="TrackData.Album"/> name.
        /// </summary>
        public string Album { get { return _sourceData.Album.Name; } }
        /// <summary>
        /// <see cref="TrackData.Album"/> artist name.
        /// </summary>
        public string AlbumArtist { get { return _sourceData.Album.AlbumArtist.Name; } }
        /// <summary>
        /// <see cref="TrackData.Performers"/> names (alphanumeric sort).
        /// </summary>
        public string Performers { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Genres"/> names (alphanumeric sort).
        /// </summary>
        public string Genres { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Year"/>
        /// </summary>
        public uint Year { get { return _sourceData.Year; } }
        /// <summary>
        /// <see cref="TrackData.Length"/>
        /// </summary>
        public TimeSpan Length { get; private set; }
        /// <summary>
        /// <see cref="TrackData.FilePath"/>
        /// </summary>
        public string FilePath { get { return _sourceData.FilePath; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="TrackData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public TrackViewData(TrackData sourceData)
        {
            _sourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));

            Performers = string.Join(", ", _sourceData.Performers.Select(p => p.Name).OrderBy(p => p));
            Genres = string.Join(", ", _sourceData.Genres.Select(p => p.Name).OrderBy(p => p));
            Length = new TimeSpan(0, 0, (int)_sourceData.Length.TotalSeconds);
        }
    }
}
