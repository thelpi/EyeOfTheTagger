using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EyeOfTheTagger.Data.Event;
using TagFile = TagLib.File;

namespace EyeOfTheTagger.Data
{
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

        public IReadOnlyCollection<TrackData> Tracks
        {
            get
            {
                return _tracks;
            }
        }

        public IReadOnlyCollection<AlbumArtistData> AlbumArtists
        {
            get
            {
                return _tracks.Select(t => t.Album.AlbumArtist).Distinct().ToList();
            }
        }

        public IReadOnlyCollection<AlbumData> Albums
        {
            get
            {
                return _tracks.Select(t => t.Album).Distinct().ToList();
            }
        }

        public IReadOnlyCollection<GenreData> Genres
        {
            get
            {
                return _tracks.Select(t => t.Genre).Distinct().ToList();
            }
        }

        public IReadOnlyCollection<uint> Years
        {
            get
            {
                return _tracks.Select(t => t.Year).Distinct().ToList();
            }
        }

        public LibraryData(List<string> paths, bool instantLoad)
        {
            TotalFilesCount = -1;
            _tracks = new List<TrackData>();
            _paths = (paths ?? new List<string>()).Distinct().ToList();
            if (instantLoad)
            {
                Load();
            }
        }

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
                        foreach (string extension in Constants.Extensions)
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
                            try
                            {
                                TagFile tagFile = TagFile.Create(file);
                                if (!_tracks.Any(t => t.FilePath.TrueEquals(tagFile.Name)))
                                {
                                    TrackData track;
                                    if (tagFile.Tag != null)
                                    {
                                        AlbumArtistData albumArtist = AlbumArtists.SingleOrDefault(aa => aa.Name.TrueEquals(tagFile.Tag.FirstAlbumArtist));
                                        if (albumArtist == null)
                                        {
                                            albumArtist = new AlbumArtistData(tagFile.Tag.FirstAlbumArtist);
                                        }

                                        GenreData genre = Genres.SingleOrDefault(g => g.Name.TrueEquals(tagFile.Tag.FirstGenre));
                                        if (genre == null)
                                        {
                                            genre = new GenreData(tagFile.Tag.FirstGenre);
                                        }

                                        AlbumData album = Albums.SingleOrDefault(a => a.Name.TrueEquals(tagFile.Tag.Album) && a.AlbumArtist.Name.TrueEquals(tagFile.Tag.FirstAlbumArtist));
                                        if (album == null)
                                        {
                                            album = new AlbumData(albumArtist, tagFile.Tag.Album);
                                        }

                                        track = new TrackData(tagFile.Tag.Track, tagFile.Tag.Title, album, genre, tagFile.Tag.Year, tagFile.Name);
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
    }
}
