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
using OxyPlot;

namespace sam
{
    public class NoiseMeasurement
    {
        public delegate void CompleteHandler(bool Succes);
        public event CompleteHandler? CompleteNotify;

        public NoiseSignal? noiseSignal;
        SoundRecorder? soundRecorder;
        WaveOutEvent? waveOutEvent;
        System.Threading.Tasks.Task? Measurement;

        CancellationTokenSource? cancellationTokenSource;

        public double[]? AccData;

        public bool InProgress
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

        public Chanels PlayCanels
        {
            get; private set;
        }

        public int SequenceLength
        {
            get; private set;
        }

        int SequencesCounter = 0;

        public void Init(int sampleRate, int bits, double desireDuration, Chanels playCanels, int sequenceLength = 2048)
        {
            SequenceLength = sequenceLength;
            PlayCanels = playCanels;
            SampleRate = sampleRate;
            Bits = bits;
            InProgress = false;

            noiseSignal?.Dispose();
            noiseSignal = new NoiseSignal();
            noiseSignal.FillData(desireDuration, bits, sampleRate);

            soundRecorder?.Dispose();
            soundRecorder = new SoundRecorder();
            soundRecorder.Init(sampleRate, bits, 1);
            soundRecorder.Sequence = sequenceLength;
            soundRecorder.SequenceReadyNotify += ProcessedSequence;
        }

        public void Run()
        {
            AccData = null;
            SequencesCounter = 0;

            int c = (int)PlayCanels;
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Measurement = new Task(() =>
            {
                if (noiseSignal == null || soundRecorder == null)
                {
                    return;
                }

                InProgress = true;

                waveOutEvent?.Dispose();
                waveOutEvent = new WaveOutEvent();
                waveOutEvent.Init(noiseSignal.rawSourceWaveStream[c]);
                noiseSignal.rawSourceWaveStream[c].Position = 0;
                waveOutEvent.Play();

                soundRecorder.StartRecording();

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        waveOutEvent.Stop();
                        waveOutEvent.Dispose();
                        soundRecorder.StopRecording();
                        InProgress = false;
                        noiseSignal.rawSourceWaveStream[c].Position = 0;
                        CompleteNotify?.Invoke(false);
                        return;
                    }
                    if (waveOutEvent.PlaybackState != PlaybackState.Playing)
                    {
                        noiseSignal.rawSourceWaveStream[c].Position = 0;
                        waveOutEvent.Play();
                    }

                    Thread.Sleep(1);
                }
            }, cancellationToken);

            Measurement.Start();
        }

        public void Abort()
        {
            if (InProgress)
            {
                cancellationTokenSource?.Cancel();
                while(InProgress)
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void ProcessedSequence(float[] Sequence)
        {
            int length = SequenceLength;

            Complex[] inData = new Complex[length];
            for (int i = 0; i < length; i++)
            {
                inData[i] = new Complex(Sequence[i], 0);
            }

            Fourier.Forward(inData, FourierOptions.Matlab);

            double lerpCof = 0.01;

            double Lerp(double v0, double v1)
            {
                return (1 - lerpCof) * v0 + lerpCof * v1;
            }

            if (AccData == null)
            {
                if (SequencesCounter > 2)
                {
                    AccData = new double[length / 2];
                    lock (AccData)
                    {
                        for (int i = 0; i < length / 2; i++)
                        {
                            AccData[i] = inData[i].Magnitude;
                        }
                    }
                }
            }
            else
            {
                lock (AccData)
                {
                    for (int i = 0; i < length / 2; i++)
                    {
                        AccData[i] = Lerp(AccData[i], inData[i].Magnitude);
                    }
                }
            }

            SequencesCounter++;
        }
    }
}
