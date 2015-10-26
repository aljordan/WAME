using System;

namespace WhisperingAudioMusicEngine
{
    public class PlaybackTimerEventArgs : EventArgs
    {
        private double totalTimeInSeconds;
        private double currentTimeInSeconds;

        public PlaybackTimerEventArgs()
        {
        }

        public PlaybackTimerEventArgs(double currentTime, double totalTime)
        {
            currentTimeInSeconds = currentTime;
            totalTimeInSeconds = totalTime;
        }

        public int CurrentTimeInSeconds
        {
            get { return (int)Math.Round(currentTimeInSeconds); }
        }

        public int TotalTimeInSeconds
        {
            get { return (int)Math.Round(totalTimeInSeconds); }
        }    
    }
}
