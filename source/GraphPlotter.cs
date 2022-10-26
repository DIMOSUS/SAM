using MathNet.Numerics.IntegralTransforms;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using OxyPlot.WindowsForms;

namespace sam
{
    public static class GraphPlotter
    {
        public static float todB(double val)
        {
            return 20.0f * (float)Math.Log10(Math.Max(val, 0.00000001));
        }

        static List<DataPoint> Smooth(List<DataPoint> data, double smooth)
        {
            double lanczosKernel(double x, double a = 1)
            {
                if (x < 0.00001f)
                {
                    return 1.0f;
                }
                if (x <= a)
                {
                    return (a * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * x / a)) / (Math.PI * Math.PI * x * x);
                }
                return 0.0f;
            }

            double sample(int sampleInd)
            {
                if (sampleInd < 0)
                    return data[0].Y;

                if (sampleInd >= data.Count)
                    return data[data.Count - 1].Y;

                return data[sampleInd].Y;
            }

            List<DataPoint> dataSmooth = new List<DataPoint> { };

            for (int i = 0; i < data.Count; i++)
            {
                int kernelSize = (int)Math.Round(data[i].X * smooth / (data[1].X - data[0].X));
                if (kernelSize > 1)
                {
                    double sumVal = 0;
                    double sumF = 0;
                    for (int k = -kernelSize; k <= kernelSize; k++)
                    {
                        double F = lanczosKernel(k / kernelSize);
                        sumVal += sample(i + k) * F;
                        sumF += F;
                    }
                    dataSmooth.Add(new DataPoint(data[i].X, sumVal / sumF));
                }
                else
                {
                    dataSmooth.Add(data[i]);
                }
            }
            return dataSmooth;
        }

        public static void DrawImp(OxyPlot.WindowsForms.PlotView plotView1, Complex[] ir)
        {
            List<DataPoint> data = new List<DataPoint> { };
            for (int i = 0; i < ir.Length; i++)
                data.Add(new DataPoint(i, (ir[i].Real)));

            var model = new PlotModel { Title = "Impulse " };

            LineSeries LS_ = new LineSeries();
            LS_.Points.AddRange(data);
            LS_.Color = OxyColor.FromRgb(255, 127, 0);
            model.Series.Add(LS_);

            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
            //LogarithmicAxis
            //LinearAxis
            plotView1.Model = model;
        }

        public static List<DataPoint> getSequenceData(ExpSweepMeasurement measurement, int Start, int End, int Length, double smooth = 0.0, bool Hann = false, bool normalize = true)
        {
            double[] windowFunc = new double[Length];
            for (int i = 0; i < Length; i++)
            {
                // Hann
                double angle = -Math.PI + Math.PI * 2 * i / (double)(Length - 1);
                windowFunc[i] = Math.Cos(angle) * 0.5 + 0.5;
            }

            Complex[] fftSet = new Complex[Length];
            for (int i = 0; i < Length; i++)
                fftSet[i] = measurement.ImpulseResponce[i + Start] * (Hann ? windowFunc[i] : 1.0);

            Fourier.Inverse(fftSet);

            double norm = 1.0;
            if (normalize)
            {
                norm = Math.Pow(10, 0.5) * Math.Sqrt(Length / (double)measurement.SampleRate);
            }

            List<DataPoint> data = new List<DataPoint> { };
            for (int i = 1; i < fftSet.Length / 2; i++)
            {
                float f = (float)i * ((float)measurement.SampleRate / (float)Length);
                //if (f >= 20 && f <= 20000)
                data.Add(new DataPoint(f, todB(fftSet[i].Magnitude * norm)));
            }

            //-- smooth
            if (smooth > 0.0)
            {
                return Smooth(data, smooth);
            }

            return data;
        }

        public static List<LineSeries> DrawSpectrum(OxyPlot.WindowsForms.PlotView plotView1, ExpSweepMeasurement measurement)
        {
            List<LineSeries> series = new List<LineSeries> { };

            int maxMagInd = measurement.MaxMagnitudeInd;

            {
                int h1Start = maxMagInd - (int)measurement.HarmonicIROffset(1.01);
                int h1End = maxMagInd + (int)(measurement.RecordedSamples * 0.1);
                int h1Length = h1End - h1Start;

                LineSeries LS = new LineSeries();
                LS.Points.AddRange(getSequenceData(measurement, h1Start, h1End, h1Length, 0.01));
                LS.Color = OxyColor.FromRgb(255, 127, 0);
                series.Add(LS);
            }

            for (int h = 2; h < 5; h++)
            {
                int hStart = maxMagInd - (int)measurement.HarmonicIROffset(h + 0.2);
                int hEnd = maxMagInd - (int)measurement.HarmonicIROffset(h - 0.2);
                int hLength = hEnd - hStart;

                LineSeries LS = new LineSeries();
                LS.Title = "h" + h.ToString();
                LS.Points.AddRange(getSequenceData(measurement, hStart, hEnd, hLength, 0.07, true));
                LS.Color = OxyColor.FromRgb((byte)(255 - 127 * (h - 2)), 64, (byte)(127 * (h - 2)));
                series.Add(LS);
            }

            {
                int hStart = maxMagInd - (int)measurement.HarmonicIROffset(5.5);
                int hEnd = maxMagInd - (int)measurement.HarmonicIROffset(1.5);
                int hLength = hEnd - hStart;

                LineSeries LS = new LineSeries();
                LS.Title = "hAll";
                LS.Points.AddRange(getSequenceData(measurement, hStart, hEnd, hLength, 0.1));
                LS.Color = OxyColor.FromRgb(255, 255, 255);
                series.Add(LS);
            }
            return series;
        }

        public static List<LineSeries> DrawPhase(OxyPlot.WindowsForms.PlotView plotView1, ExpSweepMeasurement measurement)
        {
            List<LineSeries> series = new List<LineSeries> { };

            int length = 4096;
            int offset = -0;
            int start = measurement.MaxMagnitudeInd + offset;

            Complex[] fftSet = new Complex[length];
            for (int i = 0; i < length; i++)
                fftSet[i] = measurement.ImpulseResponce[i + start];

            Fourier.Inverse(fftSet);

            List<DataPoint> data = new List<DataPoint> { };
            for (int i = 1; i < fftSet.Length / 2; i++)
            {
                float f = (float)i * ((float)measurement.SampleRate / (float)length);
                //if (f >= 20 && f <= 20000)
                data.Add(new DataPoint(f, fftSet[i].Phase / Math.PI * 180));
            }

            LineSeries LS = new LineSeries();
            LS.Points.AddRange(data);
            LS.Color = OxyColor.FromRgb(255, 127, 0);
            series.Add(LS);

            return series;
        }

        public static List<ScatterSeries> DrawWaterFall(OxyPlot.WindowsForms.PlotView plotView, ExpSweepMeasurement measurement)
        {
            List<ScatterSeries> series = new List<ScatterSeries>();

            double windowScale = 0.1;
            double timeWindow = 20;
            double offsetTime = -5;

            int window = 1024;
            int slices = 256;

            int step = (int)Math.Floor(((timeWindow + timeWindow / 2.0) / 1000.0 * measurement.SampleRate) / slices);
            int offset = (int)(offsetTime / 1000.0 * (double)measurement.SampleRate);

            double invalidFrequenceThreshold = (measurement.SampleRate / (double)window) * 2 / windowScale;

            double[] windowFunc = new double[window];
            for (int i = 0; i < window; i++)
            {
                // Hann
                /*
                double angle = -Math.PI + Math.PI * 2 * i / (double)(window - 1);
                windowFunc[i] = Math.Cos(angle) * 0.5 + 0.5;
                */

                double n = i / (double)(window - 1);

                n -= 0.5 * (1.0 - windowScale);
                n /= windowScale;
                n = Math.Clamp(n, 0, 1);

                //Nuttall window
                double a0 = 0.355768;
                double a1 = 0.487396;
                double a2 = 0.144232;
                double a3 = 0.012604;
                windowFunc[i] =
                      a0
                    - a1 * Math.Cos(2 * Math.PI * n)
                    + a2 * Math.Cos(4 * Math.PI * n)
                    - a3 * Math.Cos(6 * Math.PI * n);
            }

            double norm = Math.Sqrt(window / (double)measurement.SampleRate);
            
            Complex[] fftSet = new Complex[window];

            ScatterSeries scatterSeries = new ScatterSeries();

            for (int s = 0; s < slices; s++)
            {
                int center = measurement.MaxMagnitudeInd + s * step + offset;

                for (int i = 0; i < window; i++)
                {
                    fftSet[i] = 
                        measurement.ImpulseResponce[center + i - window / 2]
                        * windowFunc[i];
                }
                Fourier.Inverse(fftSet);

                List<DataPoint> data_ = new List<DataPoint>(window);
                for (int i = 1; i < fftSet.Length / 2; i++)
                {
                    double f = (double)i * ((double)measurement.SampleRate / (double)window);
                    //if (f >= 20 && f <= 20000)
                    double magnitude = Math.Max(-80, todB(fftSet[i].Magnitude * norm));

                    if (f > invalidFrequenceThreshold)
                     data_.Add(new DataPoint(f, magnitude));

                }

                List<DataPoint> data = Smooth(data_, 0.05);

                double time = (center - measurement.MaxMagnitudeInd) / (double)measurement.SampleRate * 1000.0;

                for (int i = 0; i < data.Count; i++)
                {
                    scatterSeries.Points.Add(new ScatterPoint(data[i].X, time, double.NaN, data[i].Y));
                }
            }
            series.Add(scatterSeries);

            return series;
        }

        public static List<LineSeries> DrawGroupDelay(OxyPlot.WindowsForms.PlotView plotView, ExpSweepMeasurement measurement)
        {
            List<LineSeries> series = new List<LineSeries> { };

            int length = 4096;
            int offset = -0;
            int start = measurement.MaxMagnitudeInd + offset;

            Complex[] fftSet = new Complex[length];
            for (int i = 0; i < length; i++)
                fftSet[i] = measurement.ImpulseResponce[i + start];

            Fourier.Inverse(fftSet);

            double deltaOmega = (measurement.SampleRate / (double)length) * 2 * Math.PI;

            List<DataPoint> data = new List<DataPoint> { };
            for (int i = 2; i < fftSet.Length / 2; i++)
            {
                double f = (double)i * ((double)measurement.SampleRate / (double)length);

                double phaseDelta = fftSet[i].Phase - fftSet[i - 1].Phase;

                if (Math.Abs(phaseDelta) > Math.PI)
                {
                    phaseDelta -= Math.Floor(phaseDelta / Math.PI) * Math.PI;
                }

                data.Add(new DataPoint(f, phaseDelta / deltaOmega * 1000)); //-- ms
            }

            LineSeries LS = new LineSeries();
            LS.Points.AddRange(data);
            LS.Color = OxyColor.FromRgb(255, 127, 0);
            
            series.Add(LS);

            return series;
        }
    }
}
