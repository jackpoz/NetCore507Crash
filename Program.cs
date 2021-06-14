using NAudio.Wave;
using System;
using System.IO;

namespace NetCore507Crash
{
    class Program
    {
        const string filePath = "Dee Yan-Key - Allegretto assai.mp3";
        static void Main()
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File '{filePath}' is missing");
                return;
            }

            Console.WriteLine("Playing some music...");
            var player = new MusicPlayer();
            player.Play(filePath);
        }
    }

    class MusicPlayer
    {
        IWavePlayer waveOut;
        IMp3FrameDecompressor decompressor;
        BufferedWaveProvider bufferedWaveProvider;

        public void Play(string filePath)
        {
            byte[] buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            using FileStream stream = File.OpenRead(filePath);
            for (; ; )
            {
                Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
                if (frame == null)
                    continue;
                if (decompressor == null)
                {
                    WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate);
                    decompressor = new AcmMp3FrameDecompressor(waveFormat);
                    bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                }

                try
                {
                    int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                    if (decompressed > 0)
                        bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                }
                catch (NAudio.MmException)
                {
                    // Just ignore the frame if a MmException occurs
                }

                if (waveOut == null)
                {
                    waveOut = new WaveOutEvent();
                    VolumeWaveProvider16 volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    volumeProvider.Volume = 0.5f;
                    waveOut.Init(volumeProvider);
                    waveOut.Play();
                }
            }
        }
    }
}
