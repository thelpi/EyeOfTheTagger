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
        /// Length.
        /// </summary>
        public TimeSpan Length { get; private set; }
        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Indicates if the original tag file has multiple album artists.
        /// </summary>
        public bool MultipleAlbumArtistsTag { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackNumber"><see cref="Number"/></param>
        /// <param name="title"><see cref="Title"/></param>
        /// <param name="album"><see cref="Album"/></param>
        /// <param name="artists"><see cref="Artists"/></param>
        /// <param name="genres"><see cref="Genres"/></param>
        /// <param name="year"><see cref="Year"/></param>
        /// <param name="length"><see cref="Length"/></param>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <param name="multipleAlbumArtistsTag"><see cref="MultipleAlbumArtistsTag"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        internal TrackData(uint trackNumber, string title, AlbumData album, List<ArtistData> artists,
            List<GenreData> genres, uint year, TimeSpan length, string filePath, bool multipleAlbumArtistsTag)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            Number = trackNumber;
            Title = title;
            Album = album;
            _artists = artists;
            _genres = genres;
            Year = year;
            Length = length;
            FilePath = filePath;
            MultipleAlbumArtistsTag = multipleAlbumArtistsTag;
        }

        /// <summary>
        /// Constructor for track without <see cref="TagLib.File.Tag"/>.
        /// </summary>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <param name="length"><see cref="Length"/></param>
        /// <param name="album"><see cref="Album"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        internal TrackData(string filePath, TimeSpan length, AlbumData album)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            Number = 0;
            Title = null;
            Album = album;
            _artists = new List<ArtistData>();
            _genres = new List<GenreData>();
            Year = 0;
            Length = length;
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
