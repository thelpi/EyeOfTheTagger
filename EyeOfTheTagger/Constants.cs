using System.Collections.Generic;

namespace EyeOfTheTagger
{
    public static class Constants
    {
        public const string AppName = "EyeOfTheTagger";
        public static readonly string ErrorLabel = $"{AppName} - Error";
        public static readonly List<string> Extensions = new List<string>
        {
            "mp3",
            "wma"
        };
        public const string LibraryPath = @"D:\Ma musique";
        public const string UnknownInfo = "[unset]";
    }
}
