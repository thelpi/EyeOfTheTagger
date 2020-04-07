using System;
using System.Collections.Generic;
using System.Linq;

namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents a track.
    /// </summary>
    public class TrackData
    {
        private readonly List<ArtistData> _artists;
        private readonly List<GenreData> _genres;

        /// <summary>
        /// Number (on the album).
        /// </summary>
        public uint Number { get; private set; }
        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// <see cref="AlbumData"/>.
        /// </summary>
        public AlbumData Album { get; private set; }
        /// <summary>
        /// List of <see cref="ArtistData"/>.
        /// </summary>
        public IReadOnlyCollection<ArtistData> Artists { get { return _artists; } }
        /// <summary>
        /// List of <see cref="GenreData"/>.
        /// </summary>
        public IReadOnlyCollection<GenreData> Genres { get { return _genres; } }
        /// <summary>
        /// Release year.
        /// </summary>
        public uint Year { get; private set; }
        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Indicates if the original tag file has multiple album artists.
        /// </summary>
        public bool MultipleAlbumArtistsTag { get; private set; }
        /// <summary>
        /// Indicates if the original tag file has genre duplicates.
        /// </summary>
        public bool HasGenreDuplicates { get; private set; }
        /// <summary>
        /// Indicates if the original tag file has artist duplicates.
        /// </summary>
        public bool HasArtistDuplicates { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackNumber"><see cref="Number"/></param>
        /// <param name="title"><see cref="Title"/></param>
        /// <param name="album"><see cref="Album"/></param>
        /// <param name="artists"><see cref="Artists"/></param>
        /// <param name="genres"><see cref="Genres"/></param>
        /// <param name="year"><see cref="Year"/></param>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <param name="multipleAlbumArtistsTag"><see cref="MultipleAlbumArtistsTag"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        public TrackData(uint trackNumber, string title, AlbumData album, IEnumerable<ArtistData> artists,
            IEnumerable<GenreData> genres, uint year, string filePath, bool multipleAlbumArtistsTag)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            Number = trackNumber;
            Title = title ?? Constants.UnknownInfo;
            Album = album ?? AlbumData.Unknown;
            _artists = artists.EnumerableToDistinctList(ArtistData.Unknown, out bool hasArtistDuplicates);
            _genres = genres.EnumerableToDistinctList(GenreData.Unknown, out bool hasGenreDuplicates);
            Year = year;
            FilePath = filePath;
            MultipleAlbumArtistsTag = multipleAlbumArtistsTag;
            HasArtistDuplicates = hasArtistDuplicates;
            HasGenreDuplicates = hasArtistDuplicates;
        }

        /// <summary>
        /// Constructor for track without <see cref="TagLib.File.Tag"/>.
        /// </summary>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        public TrackData(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            Number = 0;
            Title = Constants.UnknownInfo;
            Album = AlbumData.Unknown;
            _artists = new List<ArtistData> { ArtistData.Unknown };
            _genres = new List<GenreData> { GenreData.Unknown };
            Year = 0;
            FilePath = filePath;
            MultipleAlbumArtistsTag = false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Number} - {Title} - {Album.Name} - {Album.AlbumArtist.Name} - {Year} - {_genres.First().Name}";
        }
    }
}
