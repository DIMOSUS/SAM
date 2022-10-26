using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sam
{
    public enum Chanels : byte
    {
        Mono = 0,
        Left = 1,
        Right = 2,
        Stereo = 3
    }

    public sealed class ExponentialSineSweep : IDisposable
    {
        public float[] SweepData
        {
            get; private set;
        }

        public float[] InverseFiltere
        {
            get; private set;
        }

        public float[] Frequence
        {
            get; private set;
        }

        public byte[] SweepByteData
        {
            get; private set;
        }

        public int SampleRate
        {
            get; private set;
        }

        /// <summary>
        /// Number of sweep samples
        /// </summary>
        public int SweepSamples
        {
            get; private set;
        }

        public int ChanelsCount
        {
            get; private set;
        }

        public int BitsPerSample
        {
            get; private set;
        }

        public int Octaves
        {
            get; private set;
        }

        public MemoryStream memoryStream
        {
            get; private set;
        }

        public RawSourceWaveStream rawSourceWaveStream
        {
            get; private set;
        }

        public void FillData(int octaves, float desireDuration, int bitsPerSample = 24, int sampleRate = 44100, Chanels chanels = Chanels.Mono)
        {
            if (bitsPerSample != 16 &&
                bitsPerSample != 24)
            {
                throw new Exception("Unsupported");
            }

            Octaves = octaves;
            BitsPerSample = bitsPerSample;
            SampleRate = sampleRate;

            double P2 = Math.Pow(2, octaves);
            double LogP2 = Math.Log(P2);

            double targetLen = sampleRate * desireDuration;

            double constFactor = (Math.PI / P2) / LogP2;

            double M = Math.Max(1, Math.Round((constFactor * targetLen) / (Math.PI * 2)));

            double L = (M * Math.PI * 2) / constFactor;

            int N = Math.Max(1, (int)Math.Round(L));

            SweepSamples = N;

            SweepData = new float[N];
            InverseFiltere = new float[N];
            Frequence = new float[N];

            double ocraveLen = N / (double)octaves;

            for (int i = 0; i < N; i++)
            {
                double logPow = Math.Pow(Math.E, (double)i / N * LogP2);
                Frequence[i] = (float)((sampleRate / 2 / P2) * (L / N) * logPow);

                SweepData[i] = (float)Math.Sin(constFactor * L * logPow) * (float)Math.Min(i / ocraveLen, 1.0);
            }

            for (int i = 0; i < N; i++)
            {
                InverseFiltere[i] = (float)(SweepData[N - i - 1] *
                    Math.Pow(Math.Pow(2, octaves / (double)N), -i) *
                    (octaves * Math.Log(2) / (1 - Math.Pow(2, -octaves))));
            }

            ChanelsCount = chanels == Chanels.Mono ? 1 : 2;
            int bytesPerSample = bitsPerSample / 8;
            int maxVal = Int32.MaxValue >> (32 - bitsPerSample);

            SweepByteData = new byte[SweepSamples * bytesPerSample * ChanelsCount];

            for (int n = 0; n < SweepSamples; n++)
            {
                int sample = (int)(SweepData[n] * maxVal);

                sample *= (int)Math.Pow(256, (4 - bytesPerSample));

                var bytes = BitConverter.GetBytes(sample);

                if (chanels == Chanels.Mono)
                {
                    for (int b = 0; b < bytesPerSample; b++)
                    {
                        int byteOffset = n * bytesPerSample;
                        SweepByteData[byteOffset + b] = bytes[b + (4 - bytesPerSample)];
                    }
                }
                else
                {
                    for (int b = 0; b < bytesPerSample; b++)
                    {
                        int byteOffset = n * bytesPerSample * ChanelsCount;

                        if (chanels == Chanels.Left || chanels == Chanels.Stereo)
                        {
                            SweepByteData[byteOffset + b] = bytes[b + (4 - bytesPerSample)];
                        }
                        if (chanels == Chanels.Right || chanels == Chanels.Stereo)
                        {
                            SweepByteData[byteOffset + bytesPerSample + b] = bytes[b + (4 - bytesPerSample)];
                        }
                    }
                }
            }

            memoryStream?.Dispose();
            rawSourceWaveStream?.Dispose();

            memoryStream = new MemoryStream(SweepByteData);
            rawSourceWaveStream = new RawSourceWaveStream(memoryStream, new WaveFormat(SampleRate, BitsPerSample, ChanelsCount));
        }
        
        ~ExponentialSineSweep()
        {
            Dispose();
        }

        public void Dispose()
        {
            memoryStream?.Dispose();
            rawSourceWaveStream?.Dispose();
        }
    }
}
