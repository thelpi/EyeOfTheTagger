using System;
using System.Collections.Generic;
using EyeOfTheTagger.Data.Enum;

namespace EyeOfTheTagger.Data
{
    /// <summary>
    /// Represents a log.
    /// </summary>
    public class LogData
    {
        private const string _DEFAULT = "<unknown>";

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
        public LogLevel Level { get; private set; }
        /// <summary>
        /// Additional datas.
        /// Keys cannot be <c>Null</c> or trimmable to empty, and are case-sensitive.
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalDatas { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"><see cref="Message"/></param>
        /// <param name="level"><see cref="Level"/></param>
        /// <param name="additionalDatas"><see cref="AdditionalDatas"/></param>
        public LogData(string message, LogLevel level, params KeyValuePair<string, string>[] additionalDatas)
        {
            Message = message ?? _DEFAULT;
            Date = DateTime.Now;
            Level = level;

            var datas = new Dictionary<string, string>();
            if (additionalDatas != null)
            {
                foreach (KeyValuePair<string, string> kvp in additionalDatas)
                {
                    if (!string.IsNullOrWhiteSpace(kvp.Key) && !datas.ContainsKey(kvp.Key.Trim()))
                    {
                        datas.Add(kvp.Key.Trim(), kvp.Value ?? _DEFAULT);
                    }
                }
            }

            AdditionalDatas = datas;
        }
    }
}
