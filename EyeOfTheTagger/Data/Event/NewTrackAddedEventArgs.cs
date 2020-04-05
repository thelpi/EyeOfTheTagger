using System;

namespace EyeOfTheTagger.Data.Event
{
    public class NewTrackAddedEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public int CurrentCount { get; private set; }

        public NewTrackAddedEventArgs(string fileName, int currentCount) : base()
        {
            FileName = fileName ?? string.Empty;
            CurrentCount = currentCount;
        }
    }
}
