using System;
using System.IO;

namespace WhisperingAudioMusicEngine
{

    public class FlacDecoder
    {
        enum DecodeResult
        {
            Succeeded,
            Failed
        }

        public static void DecodeFlacToWav(string inputFilePath, string outputFilePath)
        {
            // FLAC -> WAV
            if (!File.Exists(inputFilePath))
                throw new ApplicationException("Input file " + inputFilePath + " cannot be found!");

            using (WavWriter wav = new WavWriter(outputFilePath))
            using (FlacReader flac = new FlacReader(inputFilePath, wav))
                flac.Process();
        }
    }
}
