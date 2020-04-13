using EyeOfTheTaggerLib.Datas.Abstractions;

namespace EyeOfTheTaggerLib.Datas
{
    /// <summary>
    /// Represents an album artist.
    /// </summary>
    /// <seealso cref="BaseData"/>
    public class AlbumArtistData : BaseData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsEmpty"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal AlbumArtistData(string name, bool isDefault)
            : base(name, isDefault) { }
    }
}
