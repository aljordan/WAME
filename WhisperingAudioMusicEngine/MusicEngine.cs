using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAJAudio.Wave;
using NAJAudio.Wave.SampleProviders;
using NAJAudio.Wave.WaveOutputs;
using NAJAudio.Wave.WaveStreams;

//TODO: When moving readahead wav to temp wav, if the flac file is still decompressing, an exception is thrown
// fix this by offering some retry behavior on that specificexception.  You can generate the exception by
// changing the number of seconds ahead in ucPlayer.xaml.cs HandleTimerEvent to a very short number of seconds.

// TODO: An exception is thrown and the player quits if yo try to use an ASIO output whose hardware or software is
// not running.  Trap that exception and put up an alert instead of crashing. 

//TODO: Find a way to make the UI responsive while decoding first flac file in playlist
// possibly by decoding on a thread that has a callback to continue playing when decoding is finished
namespace WhisperingAudioMusicEngine
{
    //public delegate void PlaybackTimerEventHandler(object sender, PlaybackTimerEventArgs e);
    public delegate void SongFinishedEventHandler(object sender, EventArgs ea);


    public class MusicEngine : IDisposable
    {
        //public event PlaybackTimerEventHandler PlaybackTimerEvent;
        public event SongFinishedEventHandler SongFinishedEvent;
        private IWavePlayer waveOut;
        private AudioFileReader audioFileReader;
        private MixingSampleProvider mixingSampleProvider;
        private VolumeSampleProvider volumeSampleProvider = null;
        private bool isVolumeEnabled;
        private float currentVolume;
        private int sampleRate;
        private int bitDepth;
        private int previousSampleRate;
        private int previousBitDepth;
        private bool disposed;
        private string currentFile; //used to determine if play button is just pressed twice
        private bool memoryPlay;
        private bool scheduleMemoryPlaySettingsChange;
        private MemoryReaderNew memoryFile;
        private bool memoryFileTooBig;
        //private BufferedWaveProvider silentBuffer;
        //private System.Timers.Timer playbackTimer;

        public MusicEngine()
        {
            currentVolume = 1;
            //playbackTimer = new System.Timers.Timer();
            //playbackTimer.Interval = 500;
            //playbackTimer.Elapsed += HandlePlaybackTimerEvent;
        }

        public static void Cleanup()
        {
            foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp"))
            {
                if (file.ToLower().EndsWith(".wav"))
                    File.Delete(file);
            }
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                audioFileReader = null;
                waveOut = null;
                foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp"))
                {
                    if (file.ToLower().EndsWith(".wav"))
                        File.Delete(file);
                }

                disposed = true;
            }
        }


        // Use C# destructor syntax for finalization code.
        ~MusicEngine()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }

        public bool MemoryPlay
        {
            set 
            {
                if (waveOut != null && (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused))
                    scheduleMemoryPlaySettingsChange = true;
                else
                    memoryPlay = value; 
            }
        }


        public bool IsVolumeEnabled
        {
            get { return isVolumeEnabled; }
            set { isVolumeEnabled = value; }
        }

        public float Volume
        {
            get 
            {
                if (isVolumeEnabled)
                    return currentVolume;
                else
                    return 1;
            }
            set 
            {
                //if (isVolumeEnabled)
                    if (value >= 0 && value <= 1)
                    {
                        currentVolume = value;
                        if (volumeSampleProvider != null)
                            volumeSampleProvider.Volume = value;
                    }
            }
        }

        public string GetTotalTrackTime()
        {
            try
            {
                if (memoryPlay)
                {
                    if (waveOut != null && memoryFile != null)
                    {
                        TimeSpan totalTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : memoryFile.TotalTime;
                        return String.Format("{0:00}:{1:00}", (int)totalTime.TotalMinutes, totalTime.Seconds);
                    }
                    //else
                    //    return "0:00";
                }

                if ((waveOut != null) && (audioFileReader != null))
                {
                    TimeSpan totalTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.TotalTime;
                    return String.Format("{0:00}:{1:00}", (int)totalTime.TotalMinutes, totalTime.Seconds);
                }
                else
                    return "0:00";
            }
            catch (Exception)
            {
                return "0:00";
            }
        }

        public string GetCurrentTrackTime()
        {
            try
            {
                if (waveOut != null && memoryPlay)
                {
                    if (memoryFile != null)
                    {
                        TimeSpan currentTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : memoryFile.CurrentTime;
                        return String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds);
                    }
                    //else
                    //    return "0:00";
                }

                if ((waveOut != null) && (audioFileReader != null))
                {
                    TimeSpan currentTime = (waveOut.PlaybackState == PlaybackState.Stopped) ? TimeSpan.Zero : audioFileReader.CurrentTime;
                    return String.Format("{0:00}:{1:00}", (int)currentTime.TotalMinutes, currentTime.Seconds);
                }
                else
                    return "0:00";
            }
            catch (Exception)
            {
                return "0:00";
            }
        }


        public int GetTotalTrackTimeInSeconds()
        {
            try
            {
                if (memoryPlay)
                {
                    if ((waveOut != null) && (memoryFile != null))
                        return (int)Math.Round(memoryFile.TotalTime.TotalSeconds);
                    //else
                    //    return 0;
                }

                if ((waveOut != null) && (audioFileReader != null))
                    return (int)Math.Round(audioFileReader.TotalTime.TotalSeconds);
                else
                    return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int GetCurrentTrackTimeInSeconds()
        {
            try
            {
                if (memoryPlay)
                {
                    if ((waveOut != null) && (memoryFile != null))
                    {
                        if (waveOut.PlaybackState == PlaybackState.Stopped)
                            return 0;
                        else
                            return (int)Math.Round(memoryFile.CurrentTime.TotalSeconds);
                    }
                //    else
                //        return 0;
                }

                if ((waveOut != null) && (audioFileReader != null))
                {
                    if (waveOut.PlaybackState == PlaybackState.Stopped)
                        return 0;
                    else
                        return (int)Math.Round(audioFileReader.CurrentTime.TotalSeconds);
                }
                else
                    return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public int SampleRate
        {
            get { return sampleRate; }
        }

        public int BitDepth
        {
            get { return bitDepth; }
        }

        public void DecodeFlacAhead(string filePath)
        {
            string newFileName = filePath.Split('\\').Last().ToLower().Replace(".flac", ".wav");
            string outputFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + newFileName;

            Task.Factory.StartNew(new Action(() => 
                { 
                    FlacDecoder.DecodeFlacToWav(filePath, outputFile);
                }));    
        }

        //public void DecodeMemoryAhead(string filePath)
        //{
        //    if ((memoryPlay && !scheduleMemoryPlaySettingsChange) || (!memoryPlay && scheduleMemoryPlaySettingsChange))
        //    {
        //        try
        //        {
        //            nextMemoryFile = new MemoryReaderNew(filePath);
        //            nextMemoryFile.MemoryFileNewFinishedEvent += HandleFileReaderFinishedEvent;
        //        }
        //        catch (Exception e)
        //        {
        //            nextMemoryFile = null;
        //            GC.Collect();
        //            Console.WriteLine("next File too large to play in memory: " + e.Message);
        //        }
        //    }
        //}


        private void PlayFileFromMemory(string filePath, AudioOutput output)
        {
            memoryFileTooBig = false;
            // first check to see if we are playing the same file or are paused.
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing && filePath.Equals(currentFile))
                {
                    //isDriverStopped = false;
                    return;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    //playbackTimer.Enabled = true;
                    return;
                }
            }

            if (IsFlacFile(filePath))
            {
                string tempWavName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\temp.wav";
                string aheadFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + filePath.Split('\\').Last().ToLower().Replace(".flac", ".wav");
                if (File.Exists(aheadFileName))
                {

                    if (File.Exists(tempWavName))
                    {
                        mixingSampleProvider.RemoveAllMixerInputs();
                        File.Delete(tempWavName);
                    }

                    File.Move(aheadFileName, tempWavName);
                    filePath = tempWavName;
                }
                else
                {
                    FlacDecoder.DecodeFlacToWav(filePath, tempWavName);
                    filePath = tempWavName;
                }
            }

            if (memoryFile != null)
            {
                memoryFile.Dispose();
                memoryFile = null;
            }

            GC.Collect();

            try
            {
                memoryFile = new MemoryReaderNew(filePath);
                memoryFile.MemoryFileNewFinishedEvent += HandleFileReaderFinishedEvent;
            }
            catch (Exception e)
            {
                memoryFileTooBig = true;
                memoryFile = null;
                GC.Collect();
                Console.WriteLine("File to large to play in memory: " + e.Message);
                Play(filePath, output);
            }

            try
            {
                bitDepth = memoryFile.WaveFormat.BitsPerSample;
                sampleRate = memoryFile.WaveFormat.SampleRate;
            }
            catch (Exception createException)
            {
                Console.WriteLine(String.Format("{0}", createException.Message), "Error Loading File");
                return;
            }

            //MemoryFileSampleProvider memorySampleProvider = new MemoryFileSampleProvider(memoryFile);
            if (isVolumeEnabled)
            {
                //volumeSampleProvider = new VolumeSampleProvider(memorySampleProvider);
                volumeSampleProvider = new VolumeSampleProvider(memoryFile);
                volumeSampleProvider.Volume = currentVolume;
            }

            // if we already have an existing output of the same bit depth and sample rate, use it
            if (waveOut != null && bitDepth == previousBitDepth && sampleRate == previousSampleRate)
            {
                if (isVolumeEnabled)
                    mixingSampleProvider.AddMixerInput(volumeSampleProvider);
                else
                    mixingSampleProvider.AddMixerInput((ISampleProvider)memoryFile);
            }
            else //create a new output
            {
                mixingSampleProvider = null;
                mixingSampleProvider = new MixingSampleProvider(memoryFile.WaveFormat);
                mixingSampleProvider.ReadFully = true;
                //mixingSampleProvider.ReadFully = false;

                if (isVolumeEnabled)
                    mixingSampleProvider.AddMixerInput(volumeSampleProvider);
                else
                    //mixingSampleProvider.AddMixerInput(memoryFile.MemorySampleProvider);
                    mixingSampleProvider.AddMixerInput((ISampleProvider)memoryFile);

                try
                {
                    CreateWaveOut(output);
                    waveOut.Init(mixingSampleProvider);
                }
                catch (Exception initException)
                {
                    Console.WriteLine(String.Format("{0}", initException.Message), "Error Initializing Output");
                    return;
                }
                waveOut.Play();
            }

            currentFile = filePath;
            previousBitDepth = bitDepth;
            previousSampleRate = sampleRate;
        }



        public void Play(string filePath, AudioOutput output)
        {
            if (scheduleMemoryPlaySettingsChange)
            {
                if (memoryPlay == true)
                {
                    memoryPlay = false;
                    if (memoryFile != null)
                    {
                        memoryFile.Dispose();
                        memoryFile = null;
                    }
                    GC.Collect();
                }
                else
                    memoryPlay = true;

                scheduleMemoryPlaySettingsChange = false;
            }

            if (memoryPlay)
            {
                if (!memoryFileTooBig)
                {
                    PlayFileFromMemory(filePath, output);
                    return;
                }
                else
                    memoryFileTooBig = false;
            }

            // first check to see if we are playing the same file or are paused.
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing && filePath.Equals(currentFile))
                {
                    //isDriverStopped = false;
                    return;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    //playbackTimer.Enabled = true;
                    return;
                }
            }

            if (IsFlacFile(filePath))
            {
                string tempWavName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\temp.wav";
                string aheadFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\wamp\\" + filePath.Split('\\').Last().ToLower().Replace(".flac", ".wav");
                if (File.Exists(aheadFileName))
                {

                    if (File.Exists(tempWavName))
                    {
                        mixingSampleProvider.RemoveAllMixerInputs();
                        File.Delete(tempWavName);
                    }

                    File.Move(aheadFileName, tempWavName);
                    filePath = tempWavName;
                }
                else
                {
                    FlacDecoder.DecodeFlacToWav(filePath, tempWavName);
                    filePath = tempWavName;
                }
            }

            if (memoryFile != null)
            {
                memoryFile.Dispose();
                memoryFile = null;
            }

            audioFileReader = new AudioFileReader(filePath);

            try
            {
                bitDepth = audioFileReader.WaveFormat.BitsPerSample;
                sampleRate = audioFileReader.WaveFormat.SampleRate;
            }
            catch (Exception createException)
            {
                Console.WriteLine(String.Format("{0}", createException.Message), "Error Loading File");
                return;
            }

            AutoDisposeFileReader autoDisposeFileReader = new AutoDisposeFileReader(audioFileReader);
            autoDisposeFileReader.FileReaderFinishedEvent += HandleFileReaderFinishedEvent;

            if (isVolumeEnabled)
            {
                volumeSampleProvider = new VolumeSampleProvider(autoDisposeFileReader);
                volumeSampleProvider.Volume = currentVolume;
            }

            // if we already have an existing output of the same bit depth and sample rate, use it
            if (waveOut != null && bitDepth == previousBitDepth && sampleRate == previousSampleRate)
            {
                if (isVolumeEnabled)
                    mixingSampleProvider.AddMixerInput(volumeSampleProvider);
                else
                    mixingSampleProvider.AddMixerInput(autoDisposeFileReader);
            }
            else //create a new output
            {
                try
                {
                    CreateWaveOut(output);
                }
                catch (InvalidOutputDeviceException e)
                {
                    audioFileReader.Dispose();
                    audioFileReader = null;
                    throw e;
                }

                mixingSampleProvider = null;
                mixingSampleProvider = new MixingSampleProvider(audioFileReader.WaveFormat);
                mixingSampleProvider.ReadFully = true;

                if (isVolumeEnabled)
                    mixingSampleProvider.AddMixerInput(volumeSampleProvider);
                else
                    mixingSampleProvider.AddMixerInput(autoDisposeFileReader);

                try
                {
                    waveOut.Init(mixingSampleProvider);
                }
                catch (Exception initException)
                {
                    Console.WriteLine(String.Format("{0}", initException.Message), "Error Initializing Output");
                    return;
                }
                waveOut.Play();
            }

            currentFile = filePath;
            previousBitDepth = bitDepth;
            previousSampleRate = sampleRate;
            //playbackTimer.Enabled = true;
        }


        /// <summary>
        /// If playing, pause. If paused, play.
        /// </summary>
        /// <returns>bool representing state: true if paused, false if playing.</returns>
        public bool Pause()
        {
            bool result = true; // assume it is paused
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                    result = true;
                    //playbackTimer.Enabled = false;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                    result = false;
                    //playbackTimer.Enabled = true;
                }
            }
            return result;
        }


        public void Stop()
        {
            if (waveOut != null)
            {
                if (audioFileReader != null)
                {
                    try
                    {
                        audioFileReader.Dispose();
                    }
                    catch (Exception) 
                    { 
                        Console.WriteLine("AudioFileReader already disposed."); 
                    }
                    audioFileReader = null;
                }

                if (memoryFile != null)
                {
                    memoryFile.Dispose();
                    memoryFile = null;
                }

                CloseWaveOut();
            }
        }

        public void MoveToPositionInSeconds(int seconds)
        {
            if (memoryPlay)
            {
                if (waveOut != null && memoryFile != null)
                {
                    memoryFile.CurrentTime = TimeSpan.FromSeconds(memoryFile.TotalTime.TotalSeconds * seconds / 100.0);

                    long newPos = (long)(memoryFile.WaveFormat.AverageBytesPerSecond * seconds);
                    // Force it to align to a block boundary
                    if ((newPos % memoryFile.WaveFormat.BlockAlign) != 0)
                        newPos -= newPos % memoryFile.WaveFormat.BlockAlign;
                    // Force new position into valid range
                    newPos = Math.Max(0, Math.Min(memoryFile.Length, newPos));
                    // set position
                    memoryFile.Position = newPos;
                    return;
                }
            }

            if (waveOut != null && audioFileReader != null)
            {
                audioFileReader.CurrentTime = TimeSpan.FromSeconds(audioFileReader.TotalTime.TotalSeconds * seconds / 100.0);

                long newPos = (long)(audioFileReader.WaveFormat.AverageBytesPerSecond * seconds);
                // Force it to align to a block boundary
                if ((newPos % audioFileReader.WaveFormat.BlockAlign) != 0)
                    newPos -= newPos % audioFileReader.WaveFormat.BlockAlign;
                // Force new position into valid range
                newPos = Math.Max(0, Math.Min(audioFileReader.Length, newPos));
                // set position
                audioFileReader.Position = newPos;
            }
        }


        //private ISampleProvider CreateInputStream(string fileName)
        //{
        //    audioFileReader = new AudioFileReader(fileName);
        //    var sampleChannel = new SampleChannel(audioFileReader, true);
        //    sampleChannel.Volume = 1;
        //    return new VolumeSampleProvider(sampleChannel);
        //}


        private void CreateWaveOut(AudioOutput output)
        {
            CloseWaveOut();
            try
            {
                waveOut = CreateDevice(output);
                waveOut.PlaybackStopped += OnPlaybackStopped;
            }
            catch (InvalidOutputDeviceException e)
            {
                throw e;
            }
        }

        public IWavePlayer CreateDevice(AudioOutput output)
        {
            IWavePlayer player = null;
            switch (output.DeviceType) 
            {
                case AudioOutputs.AudioDeviceType.Asio:
                    {
                        try
                        {
                            player = new AsioOut(output.DeviceName);
                        }
                        catch (InvalidOperationException e)
                        {
                            throw new InvalidOutputDeviceException(e.Message, output);
                        }
                        break;
                    }

                case AudioOutputs.AudioDeviceType.WaveOut:
                    {
                        try
                        {
                            WaveOutEvent waveOutput = new WaveOutEvent();
                            waveOutput.DeviceNumber = output.DeviceNumber;
                            waveOutput.NumberOfBuffers = 2;
                            waveOutput.DesiredLatency = output.ChosenLatency;
                            player = waveOutput;
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOutputDeviceException(e.Message, output);
                        }
                        break;
                    }

                    case AudioOutputs.AudioDeviceType.Wasapi:
                    {
                        try
                        {
                            var device = output.MMDevice;
                            // unfortunately, exclusive mode causes repeating buffer on pause, but shared mode sounds awful.
                            NAJAudio.CoreAudioApi.AudioClientShareMode shareMode = NAJAudio.CoreAudioApi.AudioClientShareMode.Exclusive;
                            int latency = output.ChosenLatency;
                            bool useEventSync = false;
                            WasapiOut wasapiOut = new WasapiOut(device, shareMode, useEventSync, latency);
                            player = wasapiOut;
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOutputDeviceException(e.Message, output);
                        }
                        break;
                    }
            }
            
            return player;
        }

        void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            //playbackTimer.Enabled = false;
            if (e.Exception != null)
            {
                Console.WriteLine(e.Exception.Message, "Playback Device Error");
            }
            //if (audioFileReader != null)
            //{
            //    audioFileReader.Position = 0;
            //}
        }


        private void CloseWaveOut()
        {
            if (waveOut != null) //&& (!isDriverStopped))
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                }
            }

            //if (mixingSampleProvider != null)
            //    mixingSampleProvider.RemoveAllMixerInputs();

            // Pulling the following out of this method and putting it into the 
            // play method so that we do not reclose the audiofilereader after
            // opening it to check bit depth and sample rate
            //            if (audioFileReader != null)
            //            {
            //                // this one really closes the file and ACM conversion
            //                audioFileReader.Dispose();
            ////                setVolumeDelegate = null;
            //                audioFileReader = null;
            //            }
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
        }


        private bool IsFlacFile(string filePath)
        {
            return ((filePath.ToLower()).EndsWith(".flac"));
        }


        //protected virtual void OnRaisePlaybackTimerEvent(PlaybackTimerEventArgs ptea)
        //{
        //    // Make a temporary copy of the event to avoid possibility of 
        //    // a race condition if the last subscriber unsubscribes 
        //    // immediately after the null check and before the event is raised.
        //    PlaybackTimerEventHandler handler = PlaybackTimerEvent;
        //    // Raise the event
        //    if (handler != null)
        //        handler(this, ptea);
        //}

        //private void HandlePlaybackTimerEvent(object sender, EventArgs ea)
        //{
        //    OnRaisePlaybackTimerEvent(new PlaybackTimerEventArgs(GetCurrentTrackTimeInSeconds(),GetTotalTrackTimeInSeconds()));
        //}

        protected virtual void OnRaiseSongFinishedEvent(EventArgs ea)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            SongFinishedEventHandler handler = SongFinishedEvent;
            // Raise the event
            if (handler != null)
                handler(this, ea);
        }

        private void HandleFileReaderFinishedEvent(object sender, EventArgs ea)
        {
            //if (sender is MemoryReaderNew)
            //{
            //    silentBuffer = new BufferedWaveProvider(((IWaveProvider)sender).WaveFormat);
            //    silentBuffer.BufferLength = 100;
            //    mixingSampleProvider.AddMixerInput(silentBuffer);
            //}
            OnRaiseSongFinishedEvent(new EventArgs());
        }

    }
}
