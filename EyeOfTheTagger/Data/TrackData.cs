namespace EyeOfTheTagger.Data
{
    public class TrackData
    {
        public uint TrackNumber { get; private set; }
        public string Title { get; private set; }
        public AlbumData Album { get; private set; }
        public GenreData Genre { get; private set; }
        public uint Year { get; private set; }
        public string FilePath { get; private set; }

        public TrackData(uint trackNumber, string title, AlbumData album, GenreData genre, uint year, string filePath)
        {
            TrackNumber = trackNumber;
            Title = title;
            Album = album;
            Genre = genre;
            Year = year;
            FilePath = filePath;
        }

        public TrackData(string filePath)
        {
            TrackNumber = 0;
            Title = Constants.UnknownInfo;
            Album = AlbumData.Unknown;
            Genre = GenreData.Unknown;
            Year = 0;
            FilePath = filePath;
        }

        public override string ToString()
        {
            return $"{TrackNumber} - {Title} - {Album.Name} - {Album.AlbumArtist.Name} - {Year} - {Genre.Name}";
        }
    }
}
