namespace EyeOfTheTagger.Data
{
    public class ArtistData
    {
        public string Name { get; private set; }

        public ArtistData(string name)
        {
            Name = name;
        }

        public static ArtistData Unknown { get; } = new ArtistData(Constants.UnknownInfo);
    }
}
