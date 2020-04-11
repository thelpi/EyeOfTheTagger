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
        internal ArtistData(string name)
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
