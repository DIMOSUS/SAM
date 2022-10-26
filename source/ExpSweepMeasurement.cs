using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Math;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace audioAnalizer
{
    public class ExpSweepMeasurement
    {
        public delegate void CompleteHandler(bool Succes);
        public event CompleteHandler? CompleteNotify;

        ExponentialSineSweep? exponentialSineSweep;
        SoundRecorder? soundRecorder;
        WaveOutEvent? waveOutEvent;
        System.Threading.Tasks.Task? Measurement;

        CancellationTokenSource? cancellationTokenSource;

        public Complex[]? ImpulseResponce
        {
            get; private set;
        }

        public bool InProgress
        {
            get; private set;
        }

        public int SampleRate
        {
            get; private set;
        }

        public int Octaves
        {
            get; private set;
        }

        public int Bits
        {
            get; private set;
        }

        public Chanels Chanels
        {
            get; private set;
        }

        public int RecordedSamples
        {
            get
            {
                if (soundRecorder == null) return 0;
                return soundRecorder.ReadSamples;
            }
        }

        public int MaxMagnitudeInd
        {
            get; private set;
        }

        public void Init(int octaves, int sampleRate, int bits, float desireDuration, Chanels chanels)
        {
            SampleRate = sampleRate;
            Bits = bits;
            Chanels = chanels;
            Octaves = octaves;
            InProgress = false;
            ImpulseResponce = null;

            exponentialSineSweep?.Dispose();
            exponentialSineSweep = new ExponentialSineSweep();
            exponentialSineSweep.FillData(octaves, desireDuration, bits, sampleRate, chanels);

            soundRecorder?.Dispose();
            soundRecorder = new SoundRecorder();
            soundRecorder.Init(sampleRate, bits, 1);
        }

        public void Run()
        {
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            ImpulseResponce = null;

            Measurement = new Task(() =>
            {
                InProgress = true;

                waveOutEvent?.Dispose();
                waveOutEvent = new WaveOutEvent();
                waveOutEvent.Init(exponentialSineSweep.rawSourceWaveStream);
                exponentialSineSweep.rawSourceWaveStream.Position = 0;
                waveOutEvent.Play();

                soundRecorder.StartRecording();

                while (waveOutEvent.PlaybackState == PlaybackState.Playing)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        waveOutEvent.Stop();
                        waveOutEvent.Dispose();
                        soundRecorder.StopRecording();
                        InProgress = false;
                        exponentialSineSweep.rawSourceWaveStream.Position = 0;
                        CompleteNotify?.Invoke(false);
                        return;
                    }

                    Thread.Sleep(1);
                }
                exponentialSineSweep.rawSourceWaveStream.Position = 0;
                waveOutEvent.Dispose();

                while (soundRecorder.ReadSamples < exponentialSineSweep.SweepSamples + SampleRate)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        soundRecorder.StopRecording();
                        InProgress = false;
                        CompleteNotify?.Invoke(false);
                        return;
                    }
                    Thread.Sleep(1);
                }
                soundRecorder.StopRecording();

                ProcessedIR();

                InProgress = false;

                CompleteNotify?.Invoke(true);

            }, cancellationToken);

            Measurement.Start();
        }

        public void Abort()
        {
            if (InProgress)
            {
                cancellationTokenSource.Cancel();
                while(InProgress)
                {
                    Thread.Sleep(1);
                }
            }
        }

        public double HarmonicIROffset(double haarmonic)
        {
            if (exponentialSineSweep == null)
            {
                return 0;
            }
            else
            {
                return exponentialSineSweep.SweepSamples * Log(haarmonic) / Log(Pow(2, Octaves));
            }
        }

        private void ProcessedIR()
        {
            int fftLength = (int)Pow(2, Ceiling(Log2(soundRecorder.ReadSamples)));

            Complex[] inData = new Complex[fftLength * 2];
            Complex[] invFltData = new Complex[fftLength * 2];

            for (int i = 0; i < soundRecorder.Samples[0].Count; i++)
            {
                inData[i] = new Complex(soundRecorder.Samples[0][i], 0);
            }

            for (int i = 0; i < exponentialSineSweep.InverseFiltere.Length; i++)
            {
                invFltData[i] = new Complex(exponentialSineSweep.InverseFiltere[i], 0);
            }

            Fourier.Forward(inData);
            Fourier.Forward(invFltData);

            for (int i = 0; i < inData.Length; i++)
                inData[i] *= invFltData[i];

            Fourier.Inverse(inData);

            ImpulseResponce = inData;

            double maxMag = 0;
            int maxMagInd = 0;
            for (int i = 0; i < ImpulseResponce.Length; i++)
            {
                if (ImpulseResponce[i].Magnitude > maxMag)
                {
                    maxMag = ImpulseResponce[i].Magnitude;
                    maxMagInd = i;
                }
            }
            MaxMagnitudeInd = maxMagInd;
        }
    }
}
