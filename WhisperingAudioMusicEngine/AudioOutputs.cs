using System;
using System.Collections.Generic;
using System.Linq;
using NAJAudio.Wave;
using NAJAudio.CoreAudioApi;
using NAJAudio.Wave.WaveOutputs;


namespace WhisperingAudioMusicEngine
{
    public static class AudioOutputs
    {
        public enum AudioDeviceType { Asio, DirectSound, Wasapi, WaveOut } ;

        public static List<AudioOutput> GetDeviceList()
        {
            List<AudioOutput> results = new List<AudioOutput>();

            try
            {
                if (IsAsioSupported())
                {
                    foreach (string name in GetAsioDriverNames())
                        results.Add(new AudioOutput(AudioDeviceType.Asio, name));
                }
            } catch (Exception e) 
            {
                Console.WriteLine("Issue querying ASIO: " + e.Message);
            }


            //try
            //{
            //    if (IsDirectSoundSupported())
            //    {
            //        foreach (string name in GetDirectSoundDriverNames())
            //            results.Add(new AudioOutput(AudioDeviceType.DirectSound, name));
            //    }
            //} catch (Exception e) 
            //{
            //    Console.WriteLine("Issue querying Direct Sound: " + e.Message);
            //}

            try
            {
                if (IsWasapiSupported())
                {
                    foreach (NAJAudio.CoreAudioApi.MMDevice device in GetWasapiDevices())
                        results.Add(new AudioOutput(AudioDeviceType.Wasapi, device.FriendlyName, device));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Issue querying Wasapi: " + e.Message);
            }

            try
            {
                if (IsWaveOutSupported())
                {
                    int deviceNumber = 0;
                    foreach (string name in GetWaveOutDeviceNames())
                    {
                        results.Add(new AudioOutput(AudioDeviceType.WaveOut, name, deviceNumber));
                        deviceNumber += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Issue querying Wave Out: " + e.Message);
            }

            return results;
        }

        private static bool IsAsioSupported()
        {
            return AsioOut.isSupported();
        }

        private static List<string> GetAsioDriverNames()
        {
            if (IsAsioSupported())
                return AsioOut.GetDriverNames().ToList();
            else
                return new List<string>();
        }

        private static bool IsDirectSoundSupported()
        {
            return DirectSoundOut.Devices.Count() > 0;
        }

        private static List<string> GetDirectSoundDriverNames()
        {
            List<string> results = new List<string>();
            if (IsDirectSoundSupported())
            {
                foreach (DirectSoundDeviceInfo device in DirectSoundOut.Devices)
                    results.Add(device.Description);
            }
            return results;
        }

        private static bool IsWasapiSupported()
        {
            // supported on Vista and above
            return Environment.OSVersion.Version.Major >= 6;
        }


        private static List<NAJAudio.CoreAudioApi.MMDevice> GetWasapiDevices()
        {
            List<NAJAudio.CoreAudioApi.MMDevice> results = new List<NAJAudio.CoreAudioApi.MMDevice>();

            if (IsWasapiSupported())
            {
                var enumerator = new MMDeviceEnumerator();
                var endPoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (var endPoint in endPoints)
                    results.Add(endPoint);
            }

            return results;
        }


        private static bool IsWaveOutSupported()
        {
            return WaveOut.DeviceCount > 0;

        }

        private static List<string> GetWaveOutDeviceNames()
        {
            List<string> results = new List<string>();
            if (IsWaveOutSupported())
            {
                for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
                {
                    var capabilities = WaveOut.GetCapabilities(deviceId);
                    results.Add(String.Format("Device {0} ({1})", deviceId, capabilities.ProductName));
                }
            }

            return results;
        }

    }
}
