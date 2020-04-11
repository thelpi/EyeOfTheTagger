using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EyeOfTheTaggerLib.Enum;
using EyeOfTheTaggerLib.Event;
using TagFile = TagLib.File;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a library.
    /// </summary>
    public class LibraryData
    {
        // TODO : not perfect because a real tag might have this value, even it's unlikely.
        private const string _NULL = "<<<null>>>";

        /// <summary>
        /// Event sent when something noticeable occurs while loading files tags.
        /// </summary>
        public event EventHandler<LoadingLogEventArgs> LoadingLogHandler;

        /// <summary>
        /// Number of files retrieved; default value is <c>-1</c>.
        /// Might be different than <see cref="Tracks"/> count in two cases :
        /// <list type="bullet">
        /// <item>Loading is not set yet.</item>
        /// <item>Tags extraction of the file has failed.</item>
        /// </list>
        /// </summary>
        public int TotalFilesCount { get; private set; }

        private readonly List<string> _extensions = new List<string>();
        private readonly List<string> _paths = new List<string>();
        private readonly List<TrackData> _tracks = new List<TrackData>();

        /// <summary>
        /// List of every <see cref="TrackData"/>.
        /// </summary>
        public IReadOnlyCollection<TrackData> Tracks
        {
            get
            {
                return _tracks;
            }
        }

        /// <summary>
        /// List of every <see cref="AlbumArtistData"/>.
        /// </summary>
        public IReadOnlyCollection<AlbumArtistData> AlbumArtists
        {
            get
            {
                return _tracks
                        .GroupBy(t => t.Album.AlbumArtist)
                        .Select(tg => tg.First().Album.AlbumArtist)
                        .ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="AlbumData"/>.
        /// </summary>
        public IReadOnlyCollection<AlbumData> Albums
        {
            get
            {
                return _tracks
                        .GroupBy(t => t.Album)
                        .Select(tg => tg.First().Album)
                        .ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="GenreData"/>.
        /// </summary>
        public IReadOnlyCollection<GenreData> Genres
        {
            get
            {
                return _tracks
                        .SelectMany(t => t.Genres)
                        .GroupBy(g => g)
                        .Select(gg => gg.First())
                        .ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="ArtistData"/>.
        /// </summary>
        public IReadOnlyCollection<ArtistData> Artists
        {
            get
            {
                return _tracks
                        .SelectMany(t => t.Artists)
                        .GroupBy(a => a)
                        .Select(ag => ag.First())
                        .ToList();
            }
        }

        /// <summary>
        /// List of every years with at least one track.
        /// </summary>
        public IReadOnlyCollection<uint> Years
        {
            get
            {
                return _tracks.Select(t => t.Year).Distinct().ToList();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="paths">List of folder paths to analyze.</param>
        /// <param name="extensions">List of file extensions to consider.</param>
        /// <param name="instantLoad"><c>True</c> to load the library immediately.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensions"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="paths"/> is <c>Null</c>.</exception>
        public LibraryData(IEnumerable<string> paths, IEnumerable<string> extensions, bool instantLoad)
        {
            PrepareLoad(paths, extensions);
            if (instantLoad)
            {
                Load();
            }
        }

        /// <summary>
        /// Loads or reloads the library.
        /// </summary>
        /// <param name="paths">List of folder paths to analyze.</param>
        /// <param name="extensions">List of file extensions to consider.</param>
        /// <exception cref="ArgumentNullException"><paramref name="extensions"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="paths"/> is <c>Null</c>.</exception>
        public void Reload(IEnumerable<string> paths, IEnumerable<string> extensions)
        {
            PrepareLoad(paths, extensions);
            Load();
        }

        private void PrepareLoad(IEnumerable<string> paths, IEnumerable<string> extensions)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            _paths.Clear();
            _paths.AddRange(paths);

            _extensions.Clear();
            _extensions.AddRange(extensions);

            TotalFilesCount = -1;
            _tracks.Clear();
        }

        private void Load()
        {
            foreach (string path in _paths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        List<string> files = new List<string>();
                        foreach (string extension in _extensions)
                        {
                            string[] extensionFiles = Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories);
                            if (extensionFiles != null)
                            {
                                files.AddRange(extensionFiles);
                            }
                        }

                        files = files.Distinct().ToList();

                        TotalFilesCount = files.Count;

                        var localAlbumArtistDatas = new Dictionary<string, AlbumArtistData>
                        {
                            { _NULL, new AlbumArtistData(_NULL) }
                        };
                        var localAlbumDatas = new Dictionary<string, AlbumData>
                        {
                            { _NULL, new AlbumData(localAlbumArtistDatas[_NULL], _NULL) }
                        };
                        var localGenreDatas = new Dictionary<string, GenreData>
                        {
                            { _NULL, new GenreData(_NULL) }
                        };
                        var localArtistDatas = new Dictionary<string, ArtistData>
                        {
                            { _NULL, new ArtistData(_NULL) }
                        };
                        
                        int i = 1;
                        foreach (string file in files)
                        {
                            TreatSingleFile(i, file,
                                localAlbumArtistDatas, localAlbumDatas, localGenreDatas, localArtistDatas);
                            i++;
                        }
                    }
                }
                catch (Exception exGlobal)
                {
                    LoadingLogHandler?.Invoke(this, new LoadingLogEventArgs(new LogData($"An error has occured, the process has been stopped.", LogLevel.Critical, new KeyValuePair<string, string>("Error message", exGlobal.Message)), -1));
                }
            }
        }

        private void TreatSingleFile(int i, string file,
            Dictionary<string, AlbumArtistData> localAlbumArtistDatas,
            Dictionary<string, AlbumData> localAlbumDatas,
            Dictionary<string, GenreData> localGenreDatas,
            Dictionary<string, ArtistData> localArtistDatas)
        {
            try
            {
                TagFile tagFile = TagFile.Create(file);

                TrackData track;
                if (tagFile != null)
                {
                    if (tagFile.Tag != null)
                    {
                        TagLib.Tag tag = tagFile.Tag;

                        AlbumArtistData albumArtist = localAlbumArtistDatas[_NULL];
                        AlbumData album = localAlbumDatas[_NULL];
                        List<ArtistData> artists = new List<ArtistData>();
                        List<GenreData> genres = new List<GenreData>();
                        bool multipleAlbumArtistsTag = false;

                        if (tag.AlbumArtists?.Length > 1)
                        {
                            multipleAlbumArtistsTag = true;
                            LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Multiple album artists for the file : {file}.", LogLevel.Warning), i), null, null);
                        }

                        if (tag.FirstAlbumArtist != null)
                        {
                            if (!localAlbumArtistDatas.TryGetValue(tag.FirstAlbumArtist, out albumArtist))
                            {
                                albumArtist = new AlbumArtistData(tag.FirstAlbumArtist);
                                localAlbumArtistDatas.Add(tag.FirstAlbumArtist, albumArtist);
                            }
                        }

                        if (tag.Album != null)
                        {
                            string key = string.Concat(tag.Album, " - ", albumArtist.Name);
                            if (!localAlbumDatas.TryGetValue(key, out album))
                            {
                                album = new AlbumData(albumArtist, tag.Album);
                                localAlbumDatas.Add(key, album);
                            }
                        }

                        if (tag.Performers != null)
                        {
                            foreach (string performer in tag.Performers)
                            {
                                ArtistData artist = localArtistDatas[_NULL];
                                if (performer != null)
                                {
                                    if (!localArtistDatas.TryGetValue(performer, out artist))
                                    {
                                        artist = new ArtistData(performer);
                                        localArtistDatas.Add(performer, artist);
                                    }
                                }
                                if (!artists.Contains(artist))
                                {
                                    artists.Add(artist);
                                }
                            }
                        }

                        if (tag.Genres != null)
                        {
                            foreach (string genre in tag.Genres)
                            {
                                GenreData genred = localGenreDatas[_NULL];
                                if (genre != null)
                                {
                                    if (!localGenreDatas.TryGetValue(genre, out genred))
                                    {
                                        genred = new GenreData(genre);
                                        localGenreDatas.Add(genre, genred);
                                    }
                                }
                                if (!genres.Contains(genred))
                                {
                                    genres.Add(genred);
                                }
                            }
                        }

                        track = new TrackData(tag.Track, tag.Title ?? _NULL, album, artists, genres, tag.Year,
                            (tagFile.Properties?.Duration).GetValueOrDefault(TimeSpan.Zero),
                            tagFile.Name, multipleAlbumArtistsTag);
                    }
                    else
                    {
                        track = new TrackData(tagFile.Name, (tagFile.Properties?.Duration).GetValueOrDefault(TimeSpan.Zero), localAlbumDatas[_NULL]);
                    }
                }
                else
                {
                    track = new TrackData(file, TimeSpan.Zero, localAlbumDatas[_NULL]);
                }
                _tracks.Add(track);
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"The file {file} has been processed.", LogLevel.Information), i), null, null);
            }
            catch (Exception exLocal)
            {
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Error while processing {file}.", LogLevel.Error, new KeyValuePair<string, string>("Error message", exLocal.Message)), i), null, null);
            }
        }
    }
}
