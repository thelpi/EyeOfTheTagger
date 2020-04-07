using System.Collections.Generic;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Application name.
        /// </summary>
        public const string AppName = "EyeOfTheTagger"; // TODO : get from reflection.
        /// <summary>
        /// File extensions managed.
        /// </summary>
        public static IReadOnlyCollection<string> Extensions { get; } = new List<string>
        {
            // TODO : configuration.
            "mp3",
            "wma"
        };
        /// <summary>
        /// Library directory path.
        /// </summary>
        public const string LibraryPath = @"D:\Ma musique"; // TODO : configuration.
        /// <summary>
        /// Default label for unknown data.
        /// </summary>
        public const string UnknownInfo = "[unset]";
    }
}
