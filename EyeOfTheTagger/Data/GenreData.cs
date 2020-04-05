namespace EyeOfTheTagger.Data
{
    public class GenreData
    {
        public string Name { get; private set; }

        public GenreData(string name)
        {
            Name = name;
        }

        public static GenreData Unknown { get; } = new GenreData(Constants.UnknownInfo);
    }
}
