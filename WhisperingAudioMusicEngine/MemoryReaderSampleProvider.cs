using System;
using NAJAudio.Wave;
using NAJAudio.Wave.WaveFormats;
using NAJAudio.Wave.WaveOutputs;

namespace WhisperingAudioMusicEngine
{
    class MemoryReaderSampleProvider : ISampleProvider
    {
        private readonly MemoryReader memoryFile;
        private long position;

        public MemoryReaderSampleProvider(MemoryReader memoryFile)
        {
            this.memoryFile = memoryFile;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = memoryFile.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(memoryFile.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            if ((int)samplesToCopy == 0)
                memoryFile.OnRaiseFileReaderFinishedEvent(new EventArgs());

            return (int)samplesToCopy;
        }

        public long Position
        {
            get { return position; }
            set
            {
                value = Math.Min(value, memoryFile.Length);
                // make sure we don't get out of sync
                value -= (value % memoryFile.WaveFormat.BlockAlign);
                position = value;
            }

        }

        public WaveFormat WaveFormat { get { return memoryFile.WaveFormat; } }
    }
}
