using System;

namespace WhisperingAudioMusicEngine
{
    public class InvalidOutputDeviceException : Exception
    {
        private AudioOutput output;
        public InvalidOutputDeviceException(AudioOutput output)
        {
            this.output = output;
        }

        public InvalidOutputDeviceException(string message, AudioOutput output)
            : base(message)
        {
            this.output = output;
        }

        public InvalidOutputDeviceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AudioOutput Output
        {
            get { return output; }
        }

    }
}
