using System;

namespace EyeOfTheTaggerLib.Datas.Abstractions
{
    /// <summary>
    /// Represents the base class for every kind of data.
    /// </summary>
    public abstract class BaseData
    {
        /// <summary>
        /// Name.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Indicates it's the default instance (for the subtype, not globally).
        /// </summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        protected BaseData(string name, bool isDefault)
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
