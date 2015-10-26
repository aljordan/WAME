using System;

namespace WhisperingAudioMusicEngine
{
    public class PlaybackStatusEventArgs : EventArgs
    {
        private MusicEngine.PlaybackStatus status;

        public PlaybackStatusEventArgs()
        {
        }

        public PlaybackStatusEventArgs(MusicEngine.PlaybackStatus status)
        {
            this.status = status;
        }

        public MusicEngine.PlaybackStatus Status
        {
            get { return status; }
        }
    }
}
