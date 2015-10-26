using System;
using WhisperingAudioMusicLibrary;

namespace WhisperingAudioMusicEngine
{
    public class SongChangedEventArgs
    {

        private Track t;

        public SongChangedEventArgs(Track song)
        {
            t = song;
        }

        public Track Song
        {
            get { return t; }
        }
    }
}
