namespace EyeOfTheTaggerLib
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
        internal AlbumArtistData(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
