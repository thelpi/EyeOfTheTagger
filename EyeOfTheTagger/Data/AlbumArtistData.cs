namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents an album artist.
    /// </summary>
    public class AlbumArtistData
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        public AlbumArtistData(string name)
        {
            Name = name ?? Constants.UnknownInfo;
        }

        /// <summary>
        /// Default instance for unknown datas.
        /// </summary>
        public static AlbumArtistData Unknown { get; } = new AlbumArtistData(Constants.UnknownInfo);

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
