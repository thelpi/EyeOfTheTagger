namespace EyeOfTheTagger.Data
{
    public class AlbumData
    {
        public AlbumArtistData AlbumArtist { get; private set; }
        public string Name { get; private set; }

        public AlbumData(AlbumArtistData albumArtist, string name)
        {
            Name = name;
            AlbumArtist = albumArtist;
        }

        public static AlbumData Unknown { get; } = new AlbumData(AlbumArtistData.Unknown, Constants.UnknownInfo);
    }
}
