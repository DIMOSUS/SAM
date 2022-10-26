using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static audioAnalizer.ExponentialSineSweep;

namespace audioAnalizer
{
    public sealed class SoundRecorder : IDisposable
    {
        private WaveInEvent waveSource = null;

        public List<float>[] Samples;

        public int ReadSamples
        {
            get; private set;
        }

        public int SampleRate
        {
            get; private set;
        }

        public int Bits
        {
            get; private set;
        }

        public int Chanels
        {
            get; private set;
        }
        
        public void Init(int sampleRate, int bits, int chanels)
        {
            ReadSamples = 0;
            waveSource?.Dispose();
            if (bits != 16 &&
                bits != 24)
            {
                throw new Exception("Unsupported");
            }

            SampleRate = sampleRate;
            Bits = bits;
            Chanels = chanels;

            Samples = new List<float>[chanels];
            for (int i = 0; i < chanels; i++)
            {
                Samples[i] = new List<float>(sampleRate);
            }
        }

        private void ReciveData(WaveInEventArgs args)
        {
            int bytesPerSample = Bits / 8;

            int buffPtr = 0;

            int intMaxVal = Int32.MaxValue >> (32 - Bits);

            Int32 readInt()
            {
                Int32 outVal = 0;

                for(int b = 0; b < bytesPerSample; b++)
                {
                    byte readByte = args.Buffer[buffPtr];

                    outVal |= readByte << (8 * b);

                    buffPtr++;
                }

                outVal <<= (32 - Bits);
                outVal /= 1 << (32 - Bits);

                return outVal;
            }

            for (int i = 0; i < args.BytesRecorded / bytesPerSample; i++)
            {
                float fVal = readInt() / (float)intMaxVal;
                //
                Samples[0].Add(fVal);
                ReadSamples++;
            }
        }

        private void RecordingStopped(StoppedEventArgs args)
        {
            waveSource?.Dispose();
        }

        public void StartRecording()
        {
            for (int i = 0; i < Chanels; i++)
            {
                Samples[i] = new List<float>(SampleRate);
            }

            waveSource?.Dispose();
            ReadSamples = 0;

            waveSource = new WaveInEvent();
            waveSource.WaveFormat = new WaveFormat(SampleRate, Bits, Chanels);

            waveSource.DataAvailable += (sender, args) => ReciveData(args);
            waveSource.RecordingStopped += (sender, args) => RecordingStopped(args);

            waveSource.StartRecording();
        }

        public void StopRecording()
        {
            waveSource?.StopRecording();
        }

        ~SoundRecorder()
        {
            Dispose();
        }

        public void Dispose()
        {
            waveSource?.Dispose();
        }
    }
}
