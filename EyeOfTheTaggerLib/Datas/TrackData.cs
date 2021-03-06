﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EyeOfTheTaggerLib.Datas.Abstractions;

namespace EyeOfTheTaggerLib.Datas
{
    /// <summary>
    /// Represents a track.
    /// </summary>
    /// <seealso cref="BaseData"/>
    public class TrackData : BaseData
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
#pragma warning disable IDE1006 // Extern method cannot be renamed.
        private static extern int memcmp(byte[] b1, byte[] b2, long count);
#pragma warning restore IDE1006

        private readonly List<PerformerData> _performers;
        private readonly List<GenreData> _genres;
        private readonly List<string> _sourceAlbumArtists;
        private readonly List<byte> _frontCoverDatas;

        /// <summary>
        /// Number (on the album).
        /// </summary>
        public uint Number { get; private set; }
        /// <summary>
        /// <see cref="AlbumData"/>.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public AlbumData Album { get; private set; }
        /// <summary>
        /// List of <see cref="PerformerData"/>.
        /// Cannot be (or containing) <c>Null</c>.
        /// </summary>
        public IReadOnlyCollection<PerformerData> Performers { get { return _performers; } }
        /// <summary>
        /// List of <see cref="GenreData"/>.
        /// Cannot be (or containing) <c>Null</c>.
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
        /// Is necessarily a valid folder path.
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// Mime type.
        /// </summary>
        public string MimeType { get; private set; }
        /// <summary>
        /// Front cover mime type.
        /// Cannot be <c>Null</c>, but is empty if the track has no front cover.
        /// </summary>
        public string FrontCoverMimeType { get; private set; }
        /// <summary>
        /// Front cover datas.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public IReadOnlyCollection<byte> FrontCoverDatas { get { return _frontCoverDatas; } }
        /// <summary>
        /// List of album artists as tagged in source file.
        /// Cannot be (or containing) <c>Null</c>.
        /// </summary>
        public IReadOnlyCollection<string> SourceAlbumArtists { get { return _sourceAlbumArtists; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackNumber"><see cref="Number"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="album"><see cref="Album"/></param>
        /// <param name="performers"><see cref="Performers"/></param>
        /// <param name="genres"><see cref="Genres"/></param>
        /// <param name="year"><see cref="Year"/></param>
        /// <param name="length"><see cref="Length"/></param>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <param name="sourceAlbumArtists"><see cref="SourceAlbumArtists"/></param>
        /// <param name="frontCoverDatas"><see cref="FrontCoverDatas"/></param>
        /// <param name="frontCoverMimeType"><see cref="FrontCoverMimeType"/></param>
        /// <param name="mimeType"><see cref="MimeType"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="album"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="performers"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="genres"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="sourceAlbumArtists"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceAlbumArtists"/> contains at least one <c>Null</c> value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="mimeType"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="frontCoverMimeType"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="frontCoverDatas"/> is <c>Null</c>.</exception>
        internal TrackData(uint trackNumber, string name, AlbumData album,
            List<PerformerData> performers, List<GenreData> genres, uint year,
            TimeSpan length, string filePath, List<string> sourceAlbumArtists,
            string mimeType, string frontCoverMimeType, List<byte> frontCoverDatas)
            : this(name, filePath, length, album, mimeType)
        {
            if (sourceAlbumArtists == null)
            {
                throw new ArgumentNullException(nameof(sourceAlbumArtists));
            }
            else if (sourceAlbumArtists.Contains(null))
            {
                throw new ArgumentException(nameof(sourceAlbumArtists));
            }

            Number = trackNumber;
            _performers = performers ?? throw new ArgumentNullException(nameof(performers));
            _genres = genres ?? throw new ArgumentNullException(nameof(genres));
            Year = year;
            FilePath = filePath;
            _sourceAlbumArtists = sourceAlbumArtists;
            FrontCoverMimeType = frontCoverMimeType ?? throw new ArgumentNullException(nameof(frontCoverMimeType));
            _frontCoverDatas = frontCoverDatas ?? throw new ArgumentNullException(nameof(frontCoverDatas));
        }

        /// <summary>
        /// Constructor for tracks without <see cref="TagLib.File.Tag"/>.
        /// </summary>
        /// <param name="filePath"><see cref="FilePath"/></param>
        /// <param name="length"><see cref="Length"/></param>
        /// <param name="album"><see cref="Album"/></param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is not a valid path.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="album"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="mimeType"/> is <c>Null</c>.</exception>
        internal TrackData(string filePath, TimeSpan length, AlbumData album, string mimeType)
            : this(string.Empty, filePath, length, album, mimeType) { }

        private TrackData(string name, string filePath, TimeSpan length, AlbumData album, string mimeType)
            : base(name, false)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            Number = 0;
            Album = album ?? throw new ArgumentNullException(nameof(album));
            _performers = new List<PerformerData>();
            _genres = new List<GenreData>();
            Year = 0;
            Length = length;
            FilePath = filePath;
            _sourceAlbumArtists = new List<string>();
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            FrontCoverMimeType = string.Empty;
            _frontCoverDatas = new List<byte>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Number} - {Name} - {Album.Name} - {Album.AlbumArtist.Name} - {Year} - {_genres.First().Name}";
        }

        /// <summary>
        /// Checks if <see cref="FrontCoverDatas"/> is equal (every bytes), to <paramref name="other"/>.
        /// </summary>
        /// <param name="other">Second cover datas for comparison.</param>
        /// <returns><c>True</c> if datas are the same; <c>False</c> otherwise.</returns>
        public bool CompareFrontCoverDatas(IEnumerable<byte> other)
        {
            if (_frontCoverDatas == null)
            {
                return other == null;
            }

            if (other == null)
            {
                return false;
            }

            return _frontCoverDatas.Count() == other.Count()
                && memcmp(_frontCoverDatas.ToArray(), other.ToArray(), other.Count()) == 0;
        }

        /// <summary>
        /// Sets a new front cover on the instance.
        /// </summary>
        /// <param name="imageFilePath">Path to the new front cover image file.</param>
        /// <exception cref="ArgumentException">The specified <paramref name="imageFilePath"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Tags are in an invalid state on the track; the action cannot be performed.</exception>
        public void SetFrontCover(string imageFilePath)
        {
            if (!System.IO.File.Exists(imageFilePath))
            {
                throw new ArgumentException($"The specified path is invalid.", nameof(imageFilePath));
            }

            TagLib.File file = TagLib.File.Create(FilePath);
            if (file?.Tag == null)
            {
                throw new InvalidOperationException("Tags are in an invalid state on the track; the action cannot be performed.");
            }

            List<TagLib.IPicture> currentPictures = new List<TagLib.IPicture>();
            if (file.Tag.Pictures != null)
            {
                currentPictures.AddRange(file.Tag.Pictures);
                currentPictures.RemoveAll(p => p.Type == TagLib.PictureType.FrontCover);
            }

            currentPictures.Add(new TagLib.Picture(imageFilePath));

            file.Tag.Pictures = currentPictures.ToArray();
            file.Save();

            // reload the instance informations.
            _frontCoverDatas.Clear();
            _frontCoverDatas.AddRange(LibraryEngine.ExtractFrontCoverInformations(file?.Tag, out string mimeType));
            FrontCoverMimeType = mimeType;
        }
    }
}
