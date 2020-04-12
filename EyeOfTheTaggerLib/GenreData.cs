using System;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a musical genre.
    /// </summary>
    public class GenreData
    {
        /// <summary>
        /// Name.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal GenreData(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
