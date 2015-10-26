using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperingAudioMusicEngine
{
    public class PlaybackStoppedEventArgs : EventArgs
    {
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of StoppedEventArgs
        /// </summary>
        /// <param name="exception">An exception to report (null if no exception)</param>
        public PlaybackStoppedEventArgs(Exception exception = null)
        {
            this.exception = exception;
        }

        /// <summary>
        /// An exception. Will be null if the playback operation stopped normally
        /// </summary>
        public Exception Exception { get { return exception; } }

    }
}
