namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents an album.
    /// </summary>
    public class AlbumData
    {
        /// <summary>
        /// <see cref="AlbumArtistData"/>; cannot be <c>Null</c>.
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
        public AlbumData(AlbumArtistData albumArtist, string name)
        {
            AlbumArtist = albumArtist ?? AlbumArtistData.Unknown;
            Name = name ?? Constants.UnknownInfo;
        }

        /// <summary>
        /// Default instance for unknown datas.
        /// </summary>
        public static AlbumData Unknown { get; } = new AlbumData(AlbumArtistData.Unknown, Constants.UnknownInfo);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} - {AlbumArtist.Name}";
        }
    }
}
