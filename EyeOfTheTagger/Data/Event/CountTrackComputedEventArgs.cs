using System;

namespace EyeOfTheTagger.Data.Event
{
    public class CountTrackComputedEventArgs : EventArgs
    {
        public int TrackCount { get; private set; }

        public CountTrackComputedEventArgs(int trackCount) : base()
        {
            TrackCount = trackCount;
        }
    }
}
