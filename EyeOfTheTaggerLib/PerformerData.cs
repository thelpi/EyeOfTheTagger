using System;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a performer.
    /// </summary>
    public class PerformerData
    {
        /// <summary>
        /// Name.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Indicates it's the default instance.
        /// </summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal PerformerData(string name, bool isDefault)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsDefault = isDefault;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
