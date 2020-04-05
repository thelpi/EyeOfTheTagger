namespace EyeOfTheTagger.Data
{
    public class AlbumArtistData
    {
        public string Name { get; private set; }

        public AlbumArtistData(string name)
        {
            Name = name;
        }

        public static AlbumArtistData Unknown { get; } = new AlbumArtistData(Constants.UnknownInfo);
    }
}
