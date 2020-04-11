using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EyeOfTheTagger.Data.Event;
using TagFile = TagLib.File;

namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents a library.
    /// </summary>
    public class LibraryData
    {
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

        private readonly List<string> _paths;
        private readonly List<TrackData> _tracks;

        private readonly object _albumArtistLock = new object();
        private readonly object _albumLock = new object();
        private readonly object _genreLock = new object();
        private readonly object _artistLock = new object();

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
                        .GroupBy(t => t.Album.AlbumArtist.Name)
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
                        .GroupBy(t => t.Album.Name)
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
                        .GroupBy(g => g.Name)
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
                        .GroupBy(a => a.Name)
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
        /// <param name="instantLoad"><c>True</c> to load the library immediately.</param>
        public LibraryData(bool instantLoad)
        {
            TotalFilesCount = -1;
            _tracks = new List<TrackData>();
            _paths = Tools.ParseConfigurationList(Properties.Settings.Default.LibraryDirectories);
            if (instantLoad)
            {
                Load();
            }
        }

        /// <summary>
        /// Loads or reloads the library.
        /// </summary>
        public void Reload()
        {
            TotalFilesCount = -1;
            _tracks.Clear();
            Load();
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
                        foreach (string extension in Tools.ParseConfigurationList(Properties.Settings.Default.LibraryExtensions))
                        {
                            string[] extensionFiles = Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories);
                            if (extensionFiles != null)
                            {
                                files.AddRange(extensionFiles);
                            }
                        }

                        files = files.Distinct().ToList();

                        TotalFilesCount = files.Count;

                        int filesCountByProcessor = TotalFilesCount / Environment.ProcessorCount;
                        List<List<string>> filesBalancer =
                            Enumerable.Range(0, Environment.ProcessorCount)
                                .Select(_ => _ == Environment.ProcessorCount - 1 ?
                                    files.Skip(filesCountByProcessor * (Environment.ProcessorCount - 1)).ToList()
                                    : files.GetRange(_ * filesCountByProcessor, filesCountByProcessor))
                                .ToList();

                        var localAlbumArtistDatas = new Dictionary<string, AlbumArtistData>();
                        var localAlbumDatas = new Dictionary<string, AlbumData>();
                        var localGenreDatas = new Dictionary<string, GenreData>();
                        var localArtistDatas = new Dictionary<string, ArtistData>();
                        
                        DateTime startProcessTime = DateTime.Now;
                        int i = 1;
                        if (Properties.Settings.Default.ParallelLibraryProcess)
                        {
                            Parallel.ForEach(filesBalancer, (List<string> subFiles) =>
                            {
                                foreach (string file in subFiles)
                                {
                                    TreatSingleFile(i, file,
                                        localAlbumArtistDatas, localAlbumDatas, localGenreDatas, localArtistDatas);
                                    i++;
                                }
                            });
                        }
                        else
                        {
                            foreach (string file in files)
                            {
                                TreatSingleFile(i, file,
                                    localAlbumArtistDatas, localAlbumDatas, localGenreDatas, localArtistDatas);
                                i++;
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"Exec total : {(DateTime.Now - startProcessTime).TotalSeconds}");
                    }
                }
                catch (Exception exGlobal)
                {
                    LoadingLogHandler?.Invoke(this, new LoadingLogEventArgs(new LogData($"An error has occured, the process has been stopped.", Enum.LogLevel.Critical, new KeyValuePair<string, string>("Error message", exGlobal.Message)), -1));
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
                        int albumArtistsCount = tagFile.Tag.AlbumArtists == null ? 0 : tagFile.Tag.AlbumArtists.Length;
                        if (albumArtistsCount > 1)
                        {
                            LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Multiple album artists for the file : {file}.", Enum.LogLevel.Warning), i), null, null);
                        }

                        AlbumArtistData albumArtist = AlbumArtistData.Unknown;
                        if (albumArtistsCount > 0)
                        {
                            string albumArtistKey = tagFile.Tag.FirstAlbumArtist?.Trim()?.ToLowerInvariant() ?? string.Empty;
                            lock (_albumArtistLock)
                            {
                                if (!localAlbumArtistDatas.TryGetValue(albumArtistKey, out albumArtist))
                                {
                                    albumArtist = new AlbumArtistData(tagFile.Tag.FirstAlbumArtist);
                                    localAlbumArtistDatas.Add(albumArtistKey, albumArtist);
                                }
                            }
                        }

                        var artists = new List<ArtistData>();
                        foreach (string artistString in tagFile.Tag.Performers.Select(p => p ?? string.Empty))
                        {
                            ArtistData artist;
                            string artistKey = artistString.Trim().ToLowerInvariant();
                            lock (_artistLock)
                            {
                                if (!localArtistDatas.TryGetValue(artistKey, out artist))
                                {
                                    artist = new ArtistData(artistString);
                                    localArtistDatas.Add(artistKey, artist);
                                }
                            }
                            artists.Add(artist);
                        }

                        var genres = new List<GenreData>();
                        foreach (string genreString in tagFile.Tag.Genres.Select(g =>  g ?? string.Empty))
                        {
                            GenreData genre;
                            lock (_genreLock)
                            {
                                string genreKey = genreString.Trim().ToLowerInvariant();
                                if (!localGenreDatas.TryGetValue(genreKey, out genre))
                                {
                                    genre = new GenreData(genreString);
                                    localGenreDatas.Add(genreKey, genre);
                                }
                            }
                            genres.Add(genre);
                        }

                        AlbumData album = AlbumData.Unknown;
                        if (tagFile.Tag.Album != null)
                        {
                            string albumKey = string.Concat(tagFile.Tag.Album.Trim().ToLowerInvariant(), '|', albumArtist.Name.Trim().ToLowerInvariant());
                            lock (_albumLock)
                            {
                                if (!localAlbumDatas.TryGetValue(albumKey, out album))
                                {
                                    album = new AlbumData(albumArtist, tagFile.Tag.Album);
                                    localAlbumDatas.Add(albumKey, album);
                                }
                            }
                        }

                        track = new TrackData(tagFile.Tag.Track, tagFile.Tag.Title, album, artists, genres,
                            tagFile.Tag.Year, tagFile.Properties?.Duration, tagFile.Name, albumArtistsCount > 1);
                    }
                    else
                    {
                        track = new TrackData(tagFile.Name, tagFile.Properties?.Duration);
                    }
                }
                else
                {
                    track = new TrackData(file, TimeSpan.Zero);
                }
                _tracks.Add(track);
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"The file {file} has been processed.", Enum.LogLevel.Information), i), null, null);
            }
            catch (Exception exLocal)
            {
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Error while processing {file}.", Enum.LogLevel.Error, new KeyValuePair<string, string>("Error message", exLocal.Message)), i), null, null);
            }
        }
    }
}
