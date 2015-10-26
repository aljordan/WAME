using System;
using WhisperingAudioMusicLibrary;

namespace WhisperingAudioMusicEngine
{
    public class PlaylistChangedEventArgs : EventArgs
    {
        private Playlist p;

        public PlaylistChangedEventArgs(Playlist newPlaylist)
        {
            p = newPlaylist;
        }

        public Playlist NewPlaylist
        {
            get { return p; }
        }
    }
}
