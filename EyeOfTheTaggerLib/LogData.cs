using System;
using System.Collections.Generic;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a log.
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Message.
        /// Cannot be <c>Null</c>.
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// Date.
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// <see cref="LogLevel"/>
        /// </summary>
        public Enums.LogLevel Level { get; private set; }
        /// <summary>
        /// Additional datas.
        /// Keys cannot be <c>Null</c> or trimmable to empty, and are case-sensitive.
        /// Values cannot be <c>Null</c>.
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalDatas { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="level"><see cref="Level"/></param>
        /// <param name="additionalDatas"><see cref="AdditionalDatas"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <c>Null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="additionalDatas"/> contains some <c>Null</c> or duplicated informations.</exception>
        internal LogData(string message, Enums.LogLevel level, params KeyValuePair<string, string>[] additionalDatas)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Date = DateTime.Now;
            Level = level;

            var datas = new Dictionary<string, string>();
            if (additionalDatas != null)
            {
                foreach (KeyValuePair<string, string> kvp in additionalDatas)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key) || kvp.Value == null
                        || datas.ContainsKey(kvp.Key.Trim()))
                    {
                        throw new ArgumentException(nameof(additionalDatas));
                    }
                    datas.Add(kvp.Key.Trim(), kvp.Value);
                }
            }

            AdditionalDatas = datas;
        }
    }
}
