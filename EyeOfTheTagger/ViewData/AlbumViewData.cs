using System;
using System.Collections.Generic;
using System.Linq;
using EyeOfTheTaggerLib;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Album view data.
    /// </summary>
    internal class AlbumViewData
    {
        private readonly List<TrackData> _tracks;

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
        /// Checks if the instance has an empty or unknown name.
        /// </summary>
        /// <returns><c>True</c> if empty name; <c>False</c> otherwise.</returns>
        public bool HasEmptyName()
        {
            return Name.Trim() == string.Empty || SourceData.IsDefault;
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
    }
}
