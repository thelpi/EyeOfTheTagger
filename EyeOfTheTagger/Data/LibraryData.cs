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
                return _tracks.Select(t => t.Album.AlbumArtist).Distinct().ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="AlbumData"/>.
        /// </summary>
        public IReadOnlyCollection<AlbumData> Albums
        {
            get
            {
                return _tracks.Select(t => t.Album).Distinct().ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="GenreData"/>.
        /// </summary>
        public IReadOnlyCollection<GenreData> Genres
        {
            get
            {
                return _tracks.SelectMany(t => t.Genres).Distinct().ToList();
            }
        }

        /// <summary>
        /// List of every <see cref="ArtistData"/>.
        /// </summary>
        public IReadOnlyCollection<ArtistData> Artists
        {
            get
            {
                return _tracks.SelectMany(t => t.Artists).Distinct().ToList();
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

                        int i = 1;
                        foreach (string file in files)
                        {
                            TreatSingleFile(i, file);
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

        private void TreatSingleFile(int i, string file)
        {
            try
            {
                TagFile tagFile = TagFile.Create(file);
                if (!_tracks.Any(t => t.FilePath.TrueEquals(tagFile.Name)))
                {
                    TrackData track;
                    if (tagFile.Tag != null)
                    {
                        bool multipleAlbumArtists = tagFile.Tag.AlbumArtists.Length > 1;
                        if (multipleAlbumArtists)
                        {
                            LoadingLogHandler?.Invoke(this, new LoadingLogEventArgs(new LogData($"Multiple album artists for the file : {file}.", Enum.LogLevel.Warning), i));
                        }

                        AlbumArtistData albumArtist = AlbumArtists.SingleOrDefault(aa => aa.Name.TrueEquals(tagFile.Tag.FirstAlbumArtist));
                        if (albumArtist == null)
                        {
                            albumArtist = new AlbumArtistData(tagFile.Tag.FirstAlbumArtist);
                        }

                        var artists = new List<ArtistData>();
                        foreach (string artistString in tagFile.Tag.Performers)
                        {
                            ArtistData artist = Artists.SingleOrDefault(a => a.Name.TrueEquals(artistString));
                            if (artist == null)
                            {
                                artist = new ArtistData(artistString);
                            }
                            artists.Add(artist);
                        }

                        var genres = new List<GenreData>();
                        foreach (string genreString in tagFile.Tag.Genres)
                        {
                            GenreData genre = Genres.SingleOrDefault(g => g.Name.TrueEquals(genreString));
                            if (genre == null)
                            {
                                genre = new GenreData(genreString);
                            }
                            genres.Add(genre);
                        }

                        AlbumData album = Albums.SingleOrDefault(a => a.Name.TrueEquals(tagFile.Tag.Album) && a.AlbumArtist.Name.TrueEquals(tagFile.Tag.FirstAlbumArtist));
                        if (album == null)
                        {
                            album = new AlbumData(albumArtist, tagFile.Tag.Album);
                        }

                        track = new TrackData(tagFile.Tag.Track, tagFile.Tag.Title, album, artists, genres, tagFile.Tag.Year, tagFile.Name, multipleAlbumArtists);
                    }
                    else
                    {
                        track = new TrackData(tagFile.Name);
                    }
                    _tracks.Add(track);
                    LoadingLogHandler?.Invoke(this, new LoadingLogEventArgs(new LogData($"The file {file} has been processed.", Enum.LogLevel.Information), i));
                }
            }
            catch (Exception exLocal)
            {
                LoadingLogHandler?.Invoke(this, new LoadingLogEventArgs(new LogData($"Error while processing {file}.", Enum.LogLevel.Error, new KeyValuePair<string, string>("Error message", exLocal.Message)), i));
            }
        }
    }
}
