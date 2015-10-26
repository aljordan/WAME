using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAJAudio.Wave;
using NAJAudio.Wave.SampleProviders;
using NAJAudio.Wave.WaveFormats;
using NAJAudio.Wave.WaveOutputs;
using NAJAudio.Wave.WaveStreams;


namespace WhisperingAudioMusicEngine
{
    public delegate void FileReaderFinishedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// An NAudio AudioFilereader that will auto dipose when the end of file is reached.
    /// For use with the MixingSampleProvider only.
    /// </summary>
    class AutoDisposeFileReader : ISampleProvider
    {
        public event FileReaderFinishedEventHandler FileReaderFinishedEvent;
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
                OnRaiseFileReaderFinishedEvent(new EventArgs());
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }

        protected virtual void OnRaiseFileReaderFinishedEvent(EventArgs ea)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            FileReaderFinishedEventHandler handler = FileReaderFinishedEvent;
            // Raise the event
            if (handler != null)
                handler(this, ea);
        }

    }
}
