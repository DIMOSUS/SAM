using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace sam
{

    public sealed class NoiseSignal : IDisposable
    {

        public byte[][] ByteData
        {
            get; private set;
        }

        public int SampleRate
        {
            get; private set;
        }

        public int ChanelsCount
        {
            get; private set;
        }

        public int Samples
        {
            get; private set;
        }

        public int BitsPerSample
        {
            get; private set;
        }

        public double DesireDuration
        {
            get; private set;
        }

        public MemoryStream[] memoryStream
        {
            get; private set;
        }

        public RawSourceWaveStream[] rawSourceWaveStream
        {
            get; private set;
        }

        public void FillData(double desireDuration, int bitsPerSample = 24, int sampleRate = 44100)
        {
            if (bitsPerSample != 16 &&
                bitsPerSample != 24)
            {
                throw new Exception("Unsupported");
            }

            BitsPerSample = bitsPerSample;
            SampleRate = sampleRate;
            DesireDuration = desireDuration;

            Samples = (int)(sampleRate * desireDuration);

            Dispose();
            ByteData = new byte[(int)Chanels.Count][];
            memoryStream = new MemoryStream[(int)Chanels.Count];
            rawSourceWaveStream = new RawSourceWaveStream[(int)Chanels.Count];

            Random random= new Random(42);

            for (int c = 0; c < (int)Chanels.Count; c++)
            {
                ChanelsCount = (Chanels)c == Chanels.Mono ? 1 : 2;
                int bytesPerSample = bitsPerSample / 8;
                int maxVal = Int32.MaxValue >> (32 - bitsPerSample);

                ByteData[c] = new byte[Samples * bytesPerSample * ChanelsCount];

                for (int n = 0; n < Samples; n++)
                {
                    int sample = (int)((random.NextDouble() - 0.5) * maxVal);

                    sample *= (int)Math.Pow(256, (4 - bytesPerSample));

                    var bytes = BitConverter.GetBytes(sample);

                    if ((Chanels)c == Chanels.Mono)
                    {
                        for (int b = 0; b < bytesPerSample; b++)
                        {
                            int byteOffset = n * bytesPerSample;
                            ByteData[c][byteOffset + b] = bytes[b + (4 - bytesPerSample)];
                        }
                    }
                    else
                    {
                        for (int b = 0; b < bytesPerSample; b++)
                        {
                            int byteOffset = n * bytesPerSample * ChanelsCount;

                            if ((Chanels)c == Chanels.Left || (Chanels)c == Chanels.Stereo)
                            {
                                ByteData[c][byteOffset + b] = bytes[b + (4 - bytesPerSample)];
                            }
                            if ((Chanels)c == Chanels.Right || (Chanels)c == Chanels.Stereo)
                            {
                                ByteData[c][byteOffset + bytesPerSample + b] = bytes[b + (4 - bytesPerSample)];
                            }
                        }
                    }
                }

                memoryStream[c]?.Dispose();
                rawSourceWaveStream[c]?.Dispose();

                memoryStream[c] = new MemoryStream(ByteData[c]);
                rawSourceWaveStream[c] = new RawSourceWaveStream(memoryStream[c], new WaveFormat(SampleRate, BitsPerSample, ChanelsCount));
            }
        }
        
        ~NoiseSignal()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(memoryStream != null)
            {
                foreach(var ms in memoryStream)
                {
                    ms?.Dispose();
                }
            }
            if (rawSourceWaveStream != null)
            {
                foreach (var rsws in rawSourceWaveStream)
                {
                    rsws?.Dispose();
                }
            }
        }
    }
}
