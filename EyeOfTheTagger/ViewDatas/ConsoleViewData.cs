using System;
using System.Collections.Generic;
using EyeOfTheTagger.Events;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger.ViewDatas
{
    /// <summary>
    /// Console view data.
    /// </summary>
    internal class ConsoleViewData
    {
        private static ConsoleViewData _default;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static ConsoleViewData Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ConsoleViewData();
                }
                return _default;
            }
        }

        /// <summary>
        /// Event triggered when a log is added to <see cref="Logs"/>.
        /// When <see cref="Logs"/> is cleard, a <c>Null</c> event is sent.
        /// </summary>
        public EventHandler<AddLogEvent> AddLogHandler;

        private readonly List<LogData> _logs;

        /// <summary>
        /// Collection of <see cref="LogData"/>.
        /// </summary>
        public IReadOnlyCollection<LogData> Logs
        {
            get
            {
                return _logs;
            }
        }
        
        private ConsoleViewData()
        {
            _logs = new List<LogData>();
        }

        /// <summary>
        /// Adds a log to <see cref="Logs"/>
        /// </summary>
        /// <param name="log"><see cref="LogData"/></param>
        public void AddLog(LogData log)
        {
            if (log != null)
            {
                _logs.Add(log);
                AddLogHandler?.BeginInvoke(this, new AddLogEvent(log), null, null);
            }
        }

        /// <summary>
        /// Clears all the log.
        /// </summary>
        public void ClearLogs()
        {
            _logs.Clear();
            AddLogHandler?.BeginInvoke(this, null, null, null);
        }
    }
}
