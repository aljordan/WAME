using System;
using WhisperingAudioMusicLibrary;

namespace WhisperingAudioMusicEngine
{
    public class PlayerStateChangedEventArgs
    {

        private PlayerState playerState;
        //private Track t;


        public PlayerStateChangedEventArgs(PlayerState state)
        {
            playerState = state;
            //t = null;
        }

        //public PlayerStateChangedEventArgs(PlayerState state, Track song)
        //{
        //    playerState = state;
        //    t = song;
        //}

        public PlayerState State
        {
            get { return playerState; }
        }

        //public Track Song
        //{
        //    get { return t; }
        //}
    }
}
