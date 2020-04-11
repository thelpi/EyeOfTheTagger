namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents an album.
    /// </summary>
    public class AlbumData
    {
        /// <summary>
        /// <see cref="AlbumArtistData"/>.
        /// </summary>
        public AlbumArtistData AlbumArtist { get; private set; }
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="albumArtist"><see cref="AlbumArtist"/></param>
        /// <param name="name"><see cref="Name"/></param>
        internal AlbumData(AlbumArtistData albumArtist, string name)
        {
            AlbumArtist = albumArtist;
            Name = name;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} - {AlbumArtist.Name}";
        }
    }
}
