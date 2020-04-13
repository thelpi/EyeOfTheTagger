using EyeOfTheTaggerLib.Datas.Abstractions;

namespace EyeOfTheTaggerLib.Datas
{
    /// <summary>
    /// Represents a musical genre.
    /// </summary>
    /// <seealso cref="BaseData"/>
    public class GenreData : BaseData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal GenreData(string name, bool isDefault)
            : base(name, isDefault) { }
    }
}
