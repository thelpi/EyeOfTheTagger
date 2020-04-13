using System;
using System.Linq;
using EyeOfTheTaggerLib.Datas;
using EyeOfTheTagger.ItemDatas.Abstractions;

namespace EyeOfTheTagger.ItemDatas
{
    /// <summary>
    /// Track item data.
    /// </summary>
    /// <seealso cref="BaseItemData"/>
    internal class TrackItemData : BaseItemData
    {
        /// <summary>
        /// <see cref="TrackData"/>
        /// </summary>
        public new TrackData SourceData { get { return (TrackData)base.SourceData; } }
        /// <summary>
        /// <see cref="TrackData.Number"/>
        /// </summary>
        public uint Number { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Album"/> name.
        /// </summary>
        public string Album { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Album"/> artist name.
        /// </summary>
        public string AlbumArtist { get; private set; }
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
        public uint Year { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Length"/>
        /// </summary>
        public TimeSpan Length { get; private set; }
        /// <summary>
        /// <see cref="TrackData.FilePath"/>
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="BaseItemData.SourceData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public TrackItemData(TrackData sourceData) : base(sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            Number = sourceData.Number;
            AlbumArtist = sourceData.Album.AlbumArtist.Name;
            Album = sourceData.Album.Name;
            Year = sourceData.Year;
            FilePath = sourceData.FilePath;
            Performers = string.Join(", ", sourceData.Performers.Select(p => p.Name).OrderBy(p => p));
            Genres = string.Join(", ", sourceData.Genres.Select(p => p.Name).OrderBy(p => p));
            Length = new TimeSpan(0, 0, (int)sourceData.Length.TotalSeconds);
        }

        /// <summary>
        /// Gets if the instance has several album artists.
        /// </summary>
        /// <returns><c>True</c> if several; <c>False</c> otherwise.</returns>
        public bool HasSeveralAlbumArtists()
        {
            return SourceData.SourceAlbumArtists.Count > 1;
        }

        /// <summary>
        /// Gets if the instance has no or empty performers.
        /// </summary>
        /// <returns><c>True</c> if no performers; <c>False</c> otherwise.</returns>
        public bool HasEmptyPerformer()
        {
            return SourceData.Performers.Count == 0
                || SourceData.Performers.All(p => string.IsNullOrWhiteSpace(p.Name));
        }

        /// <summary>
        /// Gets if the instance has several times the same performer.
        /// </summary>
        /// <returns><c>True</c> if duplicate performers; <c>False</c> otherwise.</returns>
        public bool HasDuplicatePerformers()
        {
            return SourceData.Performers
                                .GroupBy(p => p.Name.Trim().ToLower())
                                .Any(p => p.Count() > 1);
        }

        /// <summary>
        /// Gets if the instance has no or empty genres.
        /// </summary>
        /// <returns><c>True</c> if no genres; <c>False</c> otherwise.</returns>
        public bool HasEmptyGenre()
        {
            return SourceData.Genres.Count == 0
                || SourceData.Genres.All(g => string.IsNullOrWhiteSpace(g.Name));
        }

        /// <summary>
        /// Gets if the instance has several times the same genre.
        /// </summary>
        /// <returns><c>True</c> if duplicate genres; <c>False</c> otherwise.</returns>
        public bool HasDuplicateGenres()
        {
            return SourceData.Genres
                                .GroupBy(g => g.Name.Trim().ToLower())
                                .Any(g => g.Count() > 1);
        }

        /// <summary>
        /// Gets if the instance has no front cover.
        /// </summary>
        /// <returns><c>True</c> if no front cover; <c>False</c> otherwise.</returns>
        public bool HasNoFrontCover()
        {
            return SourceData.FrontCoverDatas.Count == 0;
        }
    }
}
