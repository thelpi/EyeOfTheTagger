using System;

namespace EyeOfTheTagger.Data.Event
{
    public class TrackLoadingErrorEventArgs : EventArgs
    {
        public string FileName { get; private set; }
        public string ErrorMessage { get; private set; }
        public int CurrentCount { get; private set; }

        public TrackLoadingErrorEventArgs(string fileName, int currentCount, Exception ex) : base()
        {
            FileName = fileName ?? string.Empty;
            ErrorMessage = ex?.Message ?? Constants.UnknownInfo;
            CurrentCount = currentCount;
        }
    }
}
