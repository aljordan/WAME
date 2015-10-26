using System;

namespace WhisperingAudioMusicEngine
{
    [Serializable()]
    public class AudioOutput
    {
        private AudioOutputs.AudioDeviceType deviceType;
        private string deviceName;
        private int chosenLatency;
        private int deviceNumber; // for WaveOut device
        private NAJAudio.CoreAudioApi.MMDevice mmDevice;

        public AudioOutput() { }

        /// <summary>
        /// Constructor for Asio Devices
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceName"></param>
        public AudioOutput(AudioOutputs.AudioDeviceType deviceType, string deviceName)
        {
            this.deviceType = deviceType;
            this.deviceName = deviceName;
        }

        /// <summary>
        /// Constructor for Wasapi devices
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceName"></param>
        /// <param name="mmDevice"></param>
        public AudioOutput(AudioOutputs.AudioDeviceType deviceType, string deviceName, NAJAudio.CoreAudioApi.MMDevice mmDevice)
        {
            this.deviceType = deviceType;
            this.deviceName = deviceName;
            this.mmDevice = mmDevice;
        }

        /// <summary>
        /// Constructor for WaveOut devices
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="deviceName"></param>
        /// <param name="deviceNumber"></param>
        public AudioOutput(AudioOutputs.AudioDeviceType deviceType, string deviceName, int deviceNumber)
        {
            this.deviceType = deviceType;
            this.deviceName = deviceName;
            this.deviceNumber = deviceNumber;
        }

        public AudioOutputs.AudioDeviceType DeviceType
        {
            get { return deviceType; }
            set { deviceType = value; }
        }

        public string DeviceName
        {
            get { return deviceName; }
            set { deviceName = value; }
        }

        public int ChosenLatency
        {
            get { return chosenLatency; }
            set { chosenLatency = value; }
        }

        public NAJAudio.CoreAudioApi.MMDevice MMDevice
        {
            get { return mmDevice; }
        }

        public int DeviceNumber
        {
            get { return deviceNumber; }
        }

        public override string ToString()
        {
            return "(" + deviceType + ") " + deviceName;
        }
        
    }
}
