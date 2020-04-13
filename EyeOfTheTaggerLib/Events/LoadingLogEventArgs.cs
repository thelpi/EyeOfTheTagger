using System;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTaggerLib.Events
{
    /// <summary>
    /// Datas for log event while loading the library.
    /// </summary>
    /// <seealso cref="EventArgs"/>
    public class LoadingLogEventArgs : EventArgs
    {
        /// <summary>
        /// Instance of <see cref="LogData"/>.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public LogData Log { get; private set; }
        /// <summary>
        /// Indicates the track position, starting to <c>1</c>, in the files list when the log has occured.
        /// <c>-1</c> if the log is <see cref="Enum.LogLevel.Critical"/>.
        /// </summary>
        public int TrackIndex { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="log"><see cref="Log"/></param>
        /// <param name="trackIndex"><see cref="TrackIndex"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="log"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="log"/> is not <see cref="Enum.LogLevel.Critical"/>, <paramref name="trackIndex"/> must be greater than <c>0</c>.</exception>
        internal LoadingLogEventArgs(LogData log, int trackIndex)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
            if (log.Level == Enums.LogLevel.Critical)
            {
                TrackIndex = -1;
            }
            else
            {
                if (trackIndex < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(trackIndex));
                }
                TrackIndex = trackIndex;
            }
        }
    }
}
