namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a performer.
    /// </summary>
    public class PerformerData
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        internal PerformerData(string name)
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
