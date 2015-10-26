using System;
using System.Collections.Generic;
using System.Linq;
using NAJAudio.Wave;
using NAJAudio.Wave.WaveFormats;
using NAJAudio.Wave.WaveOutputs;
using NAJAudio.Wave.WaveStreams;

namespace WhisperingAudioMusicEngine
{
    public delegate void MemoryFileNewFinishedEventHandler(object sender, EventArgs e);

    class MemoryReaderNew : WaveStream, ISampleProvider
    {
        public event MemoryFileNewFinishedEventHandler MemoryFileNewFinishedEvent;
        private readonly WaveFormat waveFormat;
        private readonly long length;
        private long position;
        private bool isDisposed;


        public float[] AudioData { get; private set; }


        public MemoryReaderNew(string audioFileName)
        {
            //GC.Collect();

            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                this.waveFormat = audioFileReader.WaveFormat;
                this.length = audioFileReader.Length;

                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer= new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while((samplesRead = audioFileReader.Read(readBuffer,0,readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
            //GC.Collect();
        }


        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;

            var availableSamples = AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            if ((int)samplesToCopy == 0)
            {
                isDisposed = true;
                AudioData = null;

                OnRaiseFileReaderFinishedEvent(new EventArgs());
            }
            return (int)samplesToCopy;
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get
            {
                return waveFormat;
            }
        }

        public override long Length
        {
            get
            {
                return length;
            }
        }

        public override long Position
        {
            get
            {
                return position * 4;
            }
            set
            {
                value = Math.Min(value, Length);
                // make sure we don't get out of sync
                value -= (value % WaveFormat.BlockAlign);
                position = value / 4;
            }
        }



        /// <summary>
        /// Reads bytes from the Wave File
        /// <see cref="Stream.Read"/>
        /// </summary>
        public override int Read(byte[] array, int offset, int count)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The current position in the stream in Time format
        /// </summary>
        public override TimeSpan CurrentTime
        {
            get
            {
                return TimeSpan.FromSeconds((double)Position / WaveFormat.AverageBytesPerSecond);
            }
            set
            {
                Position = (long)(value.TotalSeconds * WaveFormat.AverageBytesPerSecond);
            }
        }

        /// <summary>
        /// Total length in real-time of the stream (may be an estimate for compressed files)
        /// </summary>
        public override TimeSpan TotalTime
        {
            get
            {
                return TimeSpan.FromSeconds((double)Length / WaveFormat.AverageBytesPerSecond);
            }
        }

        public void OnRaiseFileReaderFinishedEvent(EventArgs ea)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            MemoryFileNewFinishedEventHandler handler = MemoryFileNewFinishedEvent;
            // Raise the event
            if (handler != null)
                handler(this, ea);
        }

        protected override void Dispose(bool disposing)
        {
            isDisposed = true;
            if (AudioData != null)
            {
                AudioData = null;
            }
        }

    }    
    
}
