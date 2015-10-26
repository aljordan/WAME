using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using WhisperingAudioMusicLibrary;

// TODO: Decide if we want to keep reshuffling when it is time to repeat playlist, or just 
// tack on to the end in original shuffled order.  The smaller the playlist, the more the 
// chances that we might repeat the same song after tacking a new shuffle onto the end.
namespace WhisperingAudioMusicEngine
{
    public class Shuffler
    {
        private Playlist unshuffledPlaylist;
        private Playlist shuffledPlaylist;
        private int currentIndex;
        private bool hasPlaylistStarted;
        private int unrepeatedPlaylistLength;

        public Shuffler(Playlist playlist)
        {
            currentIndex = -1;
            unshuffledPlaylist = playlist;
            init();
        }

        public void RestartPlaylist()
        {
            currentIndex = -1;
        }

        /// <summary>
        /// Returns what the next track will be, but does not move to it.
        /// Useful to preload the next track.
        /// Returns null if at end of playlist
        /// </summary>
        /// <returns></returns>
        public Track SeeNextTrack()
        {
            if (currentIndex < shuffledPlaylist.Count() - 1)
                return shuffledPlaylist.ElementAt(currentIndex + 1);
            else
                return null;
        }

        /// <summary>
        /// Returns what the next track will be, but does not move to it.
        /// Useful to preload the next track.
        /// Returns first track if at end of playlist.
        /// </summary>
        /// <returns></returns>
        public Track SeeNextTrackRepeat()
        {
            if (currentIndex < shuffledPlaylist.Count() - 1)
                return shuffledPlaylist.ElementAt(currentIndex + 1);
            else
            {
                return shuffledPlaylist.ElementAt(0);
            }
        }


        /// <summary>
        /// Moves to and returns the next track
        /// Returns null if at end of playlist
        /// </summary>
        /// <returns></returns>
        public Track MoveToNextTrack()
        {
            if (currentIndex < shuffledPlaylist.Count() - 1)
            {
                currentIndex += 1;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
            else
                return null;
        }

        /// <summary>
        /// Moves to and returns the next track and restarts from beginning if at end of playlist
        /// </summary>
        /// <returns></returns>
        public Track MoveToNextTrackRepeat()
        {
            if (currentIndex < shuffledPlaylist.Count() - 1)
            {
                currentIndex += 1;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
            else
            {
                currentIndex = 0;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
        }


        /// <summary>
        /// Moves to and returns the previous track
        /// returns null if at the beginning of the playlist.
        /// </summary>
        /// <returns></returns>
        public Track MoveToPreviousTrack()
        {
            if (currentIndex > 0)
            {
                currentIndex -= 1;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
            else
                return null;
        }


        /// <summary>
        /// Moves to and returns the previous track
        /// returns last track in playlist if at the beginning of the playlist.
        /// </summary>
        /// <returns></returns>
        public Track MoveToPreviousTrackRepeat()
        {
            if (currentIndex > 0)
            {
                currentIndex -= 1;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
            else
            {
                currentIndex = shuffledPlaylist.Count() -1 ;
                return shuffledPlaylist.ElementAt(currentIndex);
            }
        }


        private void init()
        {
            unrepeatedPlaylistLength = unshuffledPlaylist.Count();
            currentIndex = -1;
            //create and initialize array with number of elements equal to items in the playlist
            int[] shuffledPlaylistIndexes = new int[unrepeatedPlaylistLength];
            for (int counter = 0; counter < unrepeatedPlaylistLength; counter++)
                shuffledPlaylistIndexes[counter] = counter;

            // To shuffle an array a of n elements (indices 0..n-1):
            for (int i = unrepeatedPlaylistLength - 1; i >= 0; i--)
            {
                Random r = new Random();
                int j = r.Next(0, i);
                int num1 = shuffledPlaylistIndexes[j];
                int num2 = shuffledPlaylistIndexes[i];
                shuffledPlaylistIndexes[j] = num2;
                shuffledPlaylistIndexes[i] = num1;
            }

            shuffledPlaylist = new Playlist();
            for (int counter = 0; counter < shuffledPlaylistIndexes.Length; counter++)
                shuffledPlaylist.AddSong(unshuffledPlaylist.ElementAt(shuffledPlaylistIndexes[counter]));
        }

        public void AddTracksToPlaylist(List<Track> tracks)
        {
            foreach (Track t in tracks)
            {
                unshuffledPlaylist.AddSong(t);
            }

            int[] shuffledPlaylistIndexes = new int[tracks.Count];
            for (int counter = 0; counter < tracks.Count; counter++)
                shuffledPlaylistIndexes[counter] = counter;

            // To shuffle an array a of n elements (indices 0..n-1):
            for (int i = unrepeatedPlaylistLength - 1; i >= 0; i--)
            {
                Random r = new Random();
                int j = r.Next(0, i);
                int num1 = shuffledPlaylistIndexes[j];
                int num2 = shuffledPlaylistIndexes[i];
                shuffledPlaylistIndexes[j] = num2;
                shuffledPlaylistIndexes[i] = num1;
            }

            for (int counter = 0; counter < tracks.Count; counter++)
                shuffledPlaylist.AddSong(tracks.ElementAt(shuffledPlaylistIndexes[counter]));
        }


        //private void AddMoreToEnd()
        //{
        //    int[] newBunch = new int[unrepeatedPlaylistLength];
        //    for (int counter = 0; counter < unrepeatedPlaylistLength; counter++)
        //        newBunch[counter] = counter;

        //    // To shuffle an array a of n elements (indices 0..n-1):
        //    for (int i = unrepeatedPlaylistLength - 1; i >= 0; i--)
        //    {
        //        Random r = new Random();
        //        int j = r.Next(0, i);
        //        int num1 = newBunch[j];
        //        int num2 = newBunch[i];
        //        newBunch[j] = num2;
        //        newBunch[i] = num1;
        //    }

        //    // now add new bunch to end of previously shuffled playlist indexes
        //    int[] bothArraysAdded = new int[shuffledPlaylistIndexes.Count() + unrepeatedPlaylistLength];

        //    shuffledPlaylistIndexes.CopyTo(bothArraysAdded, 0);
        //    newBunch.CopyTo(bothArraysAdded, shuffledPlaylistIndexes.Length);
        //    shuffledPlaylistIndexes = bothArraysAdded;
        //}
    }
}
