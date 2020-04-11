using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

                        TotalFilesCount = files.Count;

                        var localAlbumArtistDatas = new Dictionary<string, AlbumArtistData>();
                        var localAlbumDatas = new Dictionary<string, AlbumData>();
                        var localGenreDatas = new Dictionary<string, GenreData>();
                        var localArtistDatas = new Dictionary<string, ArtistData>();

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
                if (!_tracks.Any(t => t.FilePath.TrueEquals(file)))
                {
                    TagFile tagFile = TagFile.Create(file);

                    TrackData track;
                    if (tagFile.Tag != null)
                    {
                        bool multipleAlbumArtists = tagFile.Tag.AlbumArtists.Length > 1;
                        if (multipleAlbumArtists)
                        {
                            LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Multiple album artists for the file : {file}.", Enum.LogLevel.Warning), i), null, null);
                        }
                        
                        string albumArtistKey = tagFile.Tag.FirstAlbumArtist.Trim().ToLowerInvariant();
                        if (!localAlbumArtistDatas.TryGetValue(albumArtistKey, out AlbumArtistData albumArtist))
                        {
                            albumArtist = new AlbumArtistData(tagFile.Tag.FirstAlbumArtist);
                            localAlbumArtistDatas.Add(albumArtistKey, albumArtist);
                        }

                        var artists = new List<ArtistData>();
                        foreach (string artistString in tagFile.Tag.Performers)
                        {
                            string artistKey = artistString.Trim().ToLowerInvariant();
                            if (!localArtistDatas.TryGetValue(artistKey, out ArtistData artist))
                            {
                                artist = new ArtistData(artistString);
                                localArtistDatas.Add(artistKey, artist);
                            }
                            artists.Add(artist);
                        }

                        var genres = new List<GenreData>();
                        foreach (string genreString in tagFile.Tag.Genres)
                        {
                            string genreKey = genreString.Trim().ToLowerInvariant();
                            if (!localGenreDatas.TryGetValue(genreKey, out GenreData genre))
                            {
                                genre = new GenreData(genreString);
                                localGenreDatas.Add(genreKey, genre);
                            }
                            genres.Add(genre);
                        }

                        string albumKey = string.Concat(tagFile.Tag.Album.Trim().ToLowerInvariant(), '|', albumArtist.Name.Trim().ToLowerInvariant());
                        if (!localAlbumDatas.TryGetValue(albumKey, out AlbumData album))
                        {
                            album = new AlbumData(albumArtist, tagFile.Tag.Album);
                            localAlbumDatas.Add(albumKey, album);
                        }

                        track = new TrackData(tagFile.Tag.Track, tagFile.Tag.Title, album, artists, genres,
                            tagFile.Tag.Year, tagFile.Properties?.Duration, tagFile.Name, multipleAlbumArtists);
                    }
                    else
                    {
                        track = new TrackData(tagFile.Name, tagFile.Properties?.Duration);
                    }
                    _tracks.Add(track);
                    LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"The file {file} has been processed.", Enum.LogLevel.Information), i), null, null);
                }
            }
            catch (Exception exLocal)
            {
                LoadingLogHandler?.BeginInvoke(this, new LoadingLogEventArgs(new LogData($"Error while processing {file}.", Enum.LogLevel.Error, new KeyValuePair<string, string>("Error message", exLocal.Message)), i), null, null);
            }
        }
    }
}
