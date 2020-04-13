using System;
using System.Linq;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ViewData
{
    /// <summary>
    /// Track view data.
    /// </summary>
    internal class TrackViewData
    {
        /// <summary>
        /// <see cref="TrackData"/>
        /// </summary>
        public TrackData SourceData { get; private set; }
        /// <summary>
        /// <see cref="TrackData.Number"/>
        /// </summary>
        public uint Number { get { return SourceData.Number; } }
        /// <summary>
        /// <see cref="EyeOfTheTaggerLib.Datas.Abstractions.BaseData.Name"/>
        /// </summary>
        public string Name { get { return SourceData.Name; } }
        /// <summary>
        /// <see cref="TrackData.Album"/> name.
        /// </summary>
        public string Album { get { return SourceData.Album.Name; } }
        /// <summary>
        /// <see cref="TrackData.Album"/> artist name.
        /// </summary>
        public string AlbumArtist { get { return SourceData.Album.AlbumArtist.Name; } }
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
        public uint Year { get { return SourceData.Year; } }
        /// <summary>
        /// <see cref="TrackData.Length"/>
        /// </summary>
        public TimeSpan Length { get; private set; }
        /// <summary>
        /// <see cref="TrackData.FilePath"/>
        /// </summary>
        public string FilePath { get { return SourceData.FilePath; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="SourceData"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="sourceData"/> is <c>Null</c>.</exception>
        public TrackViewData(TrackData sourceData)
        {
            SourceData = sourceData ?? throw new ArgumentNullException(nameof(sourceData));

            Performers = string.Join(", ", SourceData.Performers.Select(p => p.Name).OrderBy(p => p));
            Genres = string.Join(", ", SourceData.Genres.Select(p => p.Name).OrderBy(p => p));
            Length = new TimeSpan(0, 0, (int)SourceData.Length.TotalSeconds);
        }
    }
}
