using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhisperingAudioMusicEngine;

namespace WAMEConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (AudioOutputs.IsAsioSupported())
            {
                Console.WriteLine("Asio Outputs:");
                List<string> asioOutputs = AudioOutputs.GetAsioDriverNames();
                foreach (string output in asioOutputs)
                    Console.WriteLine("\t" + output);
            }

            if (AudioOutputs.IsDirectSoundSupported())
            {
                Console.WriteLine("Direct Sound Outputs:");
                List<string> dsOutputs = AudioOutputs.GetDirectSoundDriverNames();
                foreach (string output in dsOutputs)
                    Console.WriteLine("\t" + output);
            }

            if (AudioOutputs.IsWasapiSupported())
            {
                Console.WriteLine("Wasapi Outputs:");
                List<string> wOutputs = AudioOutputs.GetWasapiDeviceNames();
                foreach (string output in wOutputs)
                    Console.WriteLine("\t" + output);
            }

            if (AudioOutputs.IsWaveOutSupported())
            {
                Console.WriteLine("WaveOut Outputs:");
                List<string> woOutputs = AudioOutputs.GetWaveOutDeviceNames();
                foreach (string output in woOutputs)
                    Console.WriteLine("\t" + output);
            }


            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

        }
    }
}
