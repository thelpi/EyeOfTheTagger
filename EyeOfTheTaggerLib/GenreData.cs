namespace EyeOfTheTaggerLib
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
        internal GenreData(string name)
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
