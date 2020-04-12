using System;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents an album.
    /// </summary>
    public class AlbumData
    {
        /// <summary>
        /// <see cref="AlbumArtistData"/>.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public AlbumArtistData AlbumArtist { get; private set; }
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
        /// <param name="albumArtist"><see cref="AlbumArtist"/></param>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="albumArtist"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal AlbumData(AlbumArtistData albumArtist, string name, bool isDefault)
        {
            AlbumArtist = albumArtist ?? throw new ArgumentNullException(nameof(albumArtist));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsDefault = isDefault;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} - {AlbumArtist.Name}";
        }
    }
}
