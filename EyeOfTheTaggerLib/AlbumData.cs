using System;
using EyeOfTheTaggerLib.Abstractions;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents an album.
    /// </summary>
    /// <seealso cref="BaseData"/>
    public class AlbumData : BaseData
    {
        /// <summary>
        /// <see cref="AlbumArtistData"/>.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public AlbumArtistData AlbumArtist { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="albumArtist"><see cref="AlbumArtist"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="albumArtist"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal AlbumData(AlbumArtistData albumArtist, string name, bool isDefault)
            : base(name, isDefault)
        {
            AlbumArtist = albumArtist ?? throw new ArgumentNullException(nameof(albumArtist));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} - {AlbumArtist.Name}";
        }
    }
}
