using System;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.Events
{
    /// <summary>
    /// Add log event.
    /// </summary>
    /// <seealso cref="EventArgs"/>
    internal class AddLogEvent : EventArgs
    {
        /// <summary>
        /// <see cref="LogData"/>
        /// </summary>
        public LogData Log { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="log"><see cref="Log"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="log"/> is <c>Null</c>.</exception>
        public AddLogEvent(LogData log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }
    }
}
