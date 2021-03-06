﻿using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTagger.ItemDatas.Abstractions;
using EyeOfTheTaggerLib;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ItemDatas
{
    /// <summary>
    /// Album item data.
    /// </summary>
    /// <seealso cref="BaseItemData"/>
    internal class AlbumItemData : BaseItemData
    {
        private readonly List<TrackData> _tracks;

        /// <summary>
        /// <see cref="AlbumData"/>
        /// </summary>
        public new AlbumData SourceData { get { return (AlbumData)base.SourceData; } }
        /// <summary>
        /// <see cref="AlbumData.AlbumArtist"/> name.
        /// </summary>
        public string AlbumArtist { get; private set; }
        /// <summary>
        /// Release year (from the first track).
        /// </summary>
        public uint Year { get { return _tracks.First().Year; } }
        /// <summary>
        /// Genre name (from the first track).
        /// </summary>
        public string Genre { get; private set; }
        /// <summary>
        /// Tracks count.
        /// </summary>
        public int TracksCount { get { return _tracks.Count; } }
        /// <summary>
        /// Tracks length.
        /// </summary>
        public TimeSpan TracksLength { get; private set; }
        /// <summary>
        /// Front cover datas (from the first track).
        /// </summary>
        public IReadOnlyCollection<byte> FrontCoverDatas { get { return _tracks.First().FrontCoverDatas; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="BaseItemData.SourceData"/></param>
        /// <param name="library"><see cref="LibraryEngine"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="library"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public AlbumItemData(AlbumData sourceData, LibraryEngine library) : base(sourceData)
        {
            if (library == null)
            {
                throw new ArgumentNullException(nameof(library));
            }

            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            AlbumArtist = sourceData.AlbumArtist.Name;
            _tracks = library.Tracks.Where(t => t.Album == sourceData).OrderBy(t => t.Number).ToList();
            Genre = _tracks.First().Genres.FirstOrDefault()?.Name ?? string.Empty;
            TracksLength = new TimeSpan(0, 0, (int)_tracks.Sum(t => t.Length.TotalSeconds));
        }

        /// <summary>
        /// Checks if the instance has an invalid tracks number sequence.
        /// </summary>
        /// <returns><c>True</c> if invalid sequence; <c>False</c> otherwise.</returns>
        public bool HasInvalidTrackSequence()
        {
            int numExpected = 1;
            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i].Number != numExpected)
                {
                    return true;
                }
                numExpected++;
            }

            return false;
        }

        /// <summary>
        /// Checks if the instance has multiple years.
        /// </summary>
        /// <returns><c>True</c> if multiple years; <c>False</c> otherwise.</returns>
        public bool HasMultipleYears()
        {
            return _tracks.GroupBy(t =>  t.Year).Count() > 1;
        }

        /// <summary>
        /// Checks if the instance has invalid (several or none) front cover.
        /// </summary>
        /// <returns><c>True</c> if invalid front cover; <c>False</c> otherwise.</returns>
        public bool HasInvalidFrontCover()
        {
            for (int i = 0; i < _tracks.Count - 1; i++)
            {
                for (int j = 1; j < _tracks.Count; j++)
                {
                    if (_tracks[i].FrontCoverDatas.Count == 0
                        || _tracks[j].FrontCoverDatas.Count == 0
                        || !_tracks[i].CompareFrontCoverDatas(_tracks[j].FrontCoverDatas))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to set a new front cover picture on every tracks of the album.
        /// </summary>
        /// <param name="filePath">The path to the front cover image.</param>
        /// <returns>Dictionary of errors; key is <see cref="TrackData.FilePath"/>, value is the exception.</returns>
        public Dictionary<string, Exception> SetFrontCover(string filePath)
        {
            var errors = new Dictionary<string, Exception>();
            foreach (var track in _tracks)
            {
                try
                {
                    track.SetFrontCover(filePath);
                }
                catch (Exception ex)
                {
                    errors.Add(track.FilePath, ex);
                }
            }
            return errors;
        }
    }
}
