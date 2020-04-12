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
        private const string _NULL = "<<<null>>>";
        private const string _EMPTY_MIMETYPE = "<<<unknown>>>";

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
        /// List of every <see cref="PerformerData"/>.
        /// </summary>
        public IReadOnlyCollection<PerformerData> Performers
        {
            get
            {
                return _tracks
                        .SelectMany(t => t.Performers)
                        .GroupBy(p => p)
                        .Select(pg => pg.First())
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

                        var localAlbumArtistDatas = new Dictionary<KeyValuePair<string, bool>, AlbumArtistData>
                        {
                            { new KeyValuePair<string, bool>(_NULL, true), new AlbumArtistData(_NULL, true) }
                        };
                        var localAlbumDatas = new Dictionary<KeyValuePair<string, bool>, AlbumData>
                        {
                            { new KeyValuePair<string, bool>(_NULL, true), new AlbumData(localAlbumArtistDatas[new KeyValuePair<string, bool>(_NULL, true)], _NULL, true) }
                        };
                        var localGenreDatas = new Dictionary<KeyValuePair<string, bool>, GenreData>
                        {
                            { new KeyValuePair<string, bool>(_NULL, true), new GenreData(_NULL, true) }
                        };
                        var localPerformerDatas = new Dictionary<KeyValuePair<string, bool>, PerformerData>
                        {
                            { new KeyValuePair<string, bool>(_NULL, true), new PerformerData(_NULL, true) }
                        };
                        
                        int i = 1;
                        foreach (string file in files)
                        {
                            TreatSingleFile(i, file,
                                localAlbumArtistDatas, localAlbumDatas, localGenreDatas, localPerformerDatas);
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
            Dictionary<KeyValuePair<string, bool>, AlbumArtistData> localAlbumArtistDatas,
            Dictionary<KeyValuePair<string, bool>, AlbumData> localAlbumDatas,
            Dictionary<KeyValuePair<string, bool>, GenreData> localGenreDatas,
            Dictionary<KeyValuePair<string, bool>, PerformerData> localPerformerDatas)
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

                        AlbumArtistData albumArtist = localAlbumArtistDatas[new KeyValuePair<string, bool>(_NULL, true)];
                        AlbumData album = localAlbumDatas[new KeyValuePair<string, bool>(_NULL, true)];
                        List<PerformerData> performers = new List<PerformerData>();
                        List<GenreData> genres = new List<GenreData>();
                        List<string> sourceAlbumArtists = new List<string>();

                        if (tag.AlbumArtists?.Length > 1)
                        {
                            LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Multiple album artists for the file : {file}.", LogLevel.Warning), i), null, null);
                        }

                        if (tag.AlbumArtists != null)
                        {
                            sourceAlbumArtists = tag.AlbumArtists.Where(aa => aa != null).ToList();
                        }

                        if (tag.FirstAlbumArtist != null)
                        {
                            if (!localAlbumArtistDatas.TryGetValue(new KeyValuePair<string, bool>(tag.FirstAlbumArtist, false), out albumArtist))
                            {
                                albumArtist = new AlbumArtistData(tag.FirstAlbumArtist, false);
                                localAlbumArtistDatas.Add(new KeyValuePair<string, bool>(tag.FirstAlbumArtist, false), albumArtist);
                            }
                        }

                        if (tag.Album != null)
                        {
                            string key = string.Concat(tag.Album, " - ", albumArtist.Name);
                            if (!localAlbumDatas.TryGetValue(new KeyValuePair<string, bool>(key, false), out album))
                            {
                                album = new AlbumData(albumArtist, tag.Album, false);
                                localAlbumDatas.Add(new KeyValuePair<string, bool>(key, false), album);
                            }
                        }

                        if (tag.Performers != null)
                        {
                            foreach (string performer in tag.Performers)
                            {
                                PerformerData performerd = localPerformerDatas[new KeyValuePair<string, bool>(_NULL, true)];
                                if (performer != null)
                                {
                                    if (!localPerformerDatas.TryGetValue(new KeyValuePair<string, bool>(performer, false), out performerd))
                                    {
                                        performerd = new PerformerData(performer, false);
                                        localPerformerDatas.Add(new KeyValuePair<string, bool>(performer, false), performerd);
                                    }
                                }
                                if (!performers.Contains(performerd))
                                {
                                    performers.Add(performerd);
                                }
                            }
                        }

                        if (tag.Genres != null)
                        {
                            foreach (string genre in tag.Genres)
                            {
                                GenreData genred = localGenreDatas[new KeyValuePair<string, bool>(_NULL, true)];
                                if (genre != null)
                                {
                                    if (!localGenreDatas.TryGetValue(new KeyValuePair<string, bool>(genre, false), out genred))
                                    {
                                        genred = new GenreData(genre, false);
                                        localGenreDatas.Add(new KeyValuePair<string, bool>(genre, false), genred);
                                    }
                                }
                                if (!genres.Contains(genred))
                                {
                                    genres.Add(genred);
                                }
                            }
                        }

                        string frontCoverMimeType = string.Empty;
                        List<byte> frontCoverDatas = new List<byte>();
                        if (tag.Pictures != null && tag.Pictures.Length > 0)
                        {
                            TagLib.IPicture pic = tag.Pictures.FirstOrDefault(p => p != null && p.Type == TagLib.PictureType.FrontCover);
                            if (pic != null)
                            {
                                frontCoverMimeType = GetMimeType(pic.MimeType);
                                if (pic.Data?.Data != null)
                                {
                                    frontCoverDatas.AddRange(pic.Data.Data);
                                }
                            }
                        }

                        track = new TrackData(tag.Track, tag.Title ?? _NULL, album,
                            performers, genres, tag.Year,
                            (tagFile.Properties?.Duration).GetValueOrDefault(TimeSpan.Zero),
                            tagFile.Name, sourceAlbumArtists, GetMimeType(tagFile.MimeType),
                            frontCoverMimeType, frontCoverDatas);
                    }
                    else
                    {
                        track = new TrackData(tagFile.Name, (tagFile.Properties?.Duration).GetValueOrDefault(TimeSpan.Zero), localAlbumDatas[new KeyValuePair<string, bool>(_NULL, false)], GetMimeType(tagFile.MimeType));
                    }
                }
                else
                {
                    track = new TrackData(file, TimeSpan.Zero, localAlbumDatas[new KeyValuePair<string, bool>(_NULL, false)], tagFile.MimeType);
                }
                _tracks.Add(track);
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"The file {file} has been processed.", LogLevel.Information), i), null, null);
            }
            catch (Exception exLocal)
            {
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Error while processing {file}.", LogLevel.Error, new KeyValuePair<string, string>("Error message", exLocal.Message)), i), null, null);
            }
        }

        private static string GetMimeType(string sourceMimeType)
        {
            return sourceMimeType?.Replace("taglib/", "audio/") ?? _EMPTY_MIMETYPE;
        }
    }
}
