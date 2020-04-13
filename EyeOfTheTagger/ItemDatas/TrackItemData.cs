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
    }
}
