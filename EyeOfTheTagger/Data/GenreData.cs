namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents a musical genre.
    /// </summary>
    public class GenreData
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        public GenreData(string name)
        {
            Name = name ?? Constants.UnknownInfo;
        }

        /// <summary>
        /// Default instance for unknown datas.
        /// </summary>
        public static GenreData Unknown { get; } = new GenreData(Constants.UnknownInfo);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
