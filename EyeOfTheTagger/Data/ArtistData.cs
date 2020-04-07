namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents an artist.
    /// </summary>
    public class ArtistData
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        public ArtistData(string name)
        {
            Name = name ?? Constants.UnknownInfo;
        }

        /// <summary>
        /// Default instance for unknown datas.
        /// </summary>
        public static ArtistData Unknown { get; } = new ArtistData(Constants.UnknownInfo);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
