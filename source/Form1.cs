using NAudio.Wave;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System.Numerics;
using System.Diagnostics.Metrics;
using MathNet.Numerics.IntegralTransforms;
using System.Reflection;
using OxyPlot.WindowsForms;
using System.Drawing.Drawing2D;

namespace sam
{
    public partial class Form1 : Form
    {
        public WaveIn waveSource = null;
        DirectSoundOut waveOut = null;
        private WaveOut player;

        ExpSweepMeasurement expSweepMeasurement = new ExpSweepMeasurement();

        public Form1()
        {
            InitializeComponent();
            expSweepMeasurement.Init(12, 44100, 24, 1.0f, Chanels.Mono);
            expSweepMeasurement.CompleteNotify += (bool Succes) =>
            {
                Invoke((MethodInvoker)delegate {
                    // Running on the UI thread
                    if (Succes)
                    {
                        button1.Text = "Ready";
                    }
                    else
                    {
                        button1.Text = "Aborted";
                    }
                });

            };

            button2_Click(this, new EventArgs());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (expSweepMeasurement.InProgress)
            {
                expSweepMeasurement.Abort();
            }
            else
            {
                expSweepMeasurement.Run();
                button1.Text = "Running...";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var model = new PlotModel { Title = "Frequency Response" };

            if (expSweepMeasurement.ImpulseResponce != null && !expSweepMeasurement.InProgress)
            {
                var series = GraphPlotter.DrawSpectrum(plotView1, expSweepMeasurement);
                foreach (var s in series)
                {
                    model.Series.Add(s);
                }
            }

            model.Axes.Add(new LogarithmicAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 20,
                AbsoluteMaximum = 20000,
                Minimum = 20,
                Maximum = 20000,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = -120,
                AbsoluteMaximum = 0,
                MajorStep = 10,
                Minimum = -90,
                Maximum = 0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "dB",
            });

            plotView1.Model = model;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var model = new PlotModel { Title = "Phase Response" };

            if (expSweepMeasurement.ImpulseResponce != null && !expSweepMeasurement.InProgress)
            {
                var series = GraphPlotter.DrawPhase(plotView1, expSweepMeasurement);
                foreach (var s in series)
                {
                    model.Series.Add(s);
                }
            }

            model.Axes.Add(new LogarithmicAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 20,
                AbsoluteMaximum = 20000,
                Minimum = 20,
                Maximum = 20000,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = -180,
                AbsoluteMaximum = 180,
                Minimum = -180,
                Maximum = 180,
                MajorStep = 45,
                MajorGridlineStyle = LineStyle.Solid,
                MinorStep = 15,
                MinorGridlineStyle = LineStyle.Dot,
            });

            plotView1.Model = model;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Cumulative cml = new Cumulative();
            //cml.Owner = this;
            //cml.ShowDialog();
            if (expSweepMeasurement.ImpulseResponce != null && !expSweepMeasurement.InProgress)
            {
                GraphPlotter.DrawWaterFall(plotView1, expSweepMeasurement);
            }

            var model = new PlotModel { Title = "Cumulative Spectrum Decay" };

            var axis1 = new LinearColorAxis();
            axis1.Key = "ColorAxis";
            // axis1.Maximum = 2 * Math.PI;
            // axis1.Minimum = 0;
            axis1.IsZoomEnabled = false;
            axis1.Position = AxisPosition.Right;
            model.Axes.Add(axis1);

            /*
            var s1 = new ScatterSeries();

            s1.ColorAxisKey = "ColorAxis";
            s1.MarkerSize = 8;
            s1.MarkerType = MarkerType.Circle;

            /or (double x = 0; x <= 2 * Math.PI; x += 0.1)
                s1.Points.Add(new ScatterPoint(x, Math.Sin(x), double.NaN, x));
            model.Series.Add(s1);
            */

            if (expSweepMeasurement.ImpulseResponce != null && !expSweepMeasurement.InProgress)
            {
                List<ScatterSeries> series = GraphPlotter.DrawWaterFall(plotView1, expSweepMeasurement);
                ScatterSeries s0 = series[0];
                s0.ColorAxisKey = "ColorAxis";
                s0.MarkerSize = 5;
                s0.MarkerType = MarkerType.Square;
                model.Series.Add(s0);

            }
            
            model.Axes.Add(new LinearAxis //LogarithmicAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 20,
                AbsoluteMaximum = 20000,
                Minimum = 20,
                Maximum = 20000,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
            });
            
            
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                //AbsoluteMinimum = -20,
                //AbsoluteMaximum = 20,
                //Minimum = -10,
                //Maximum = 10,
                //MajorStep = 1,
                //MajorGridlineStyle = LineStyle.Solid,
                StartPosition = 1,
                EndPosition = 0,
                IsZoomEnabled = false,
                Title = "Time, ms"
            });
            
            plotView1.Model = model;
			
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var model = new PlotModel { Title = "Group Delay" };

            if (expSweepMeasurement.ImpulseResponce != null && !expSweepMeasurement.InProgress)
            {
                var series = GraphPlotter.DrawGroupDelay(plotView1, expSweepMeasurement);
                foreach (var s in series)
                {
                    model.Series.Add(s);
                }
            }

            model.Axes.Add(new LogarithmicAxis
            {
                Position = AxisPosition.Bottom,
                AbsoluteMinimum = 20,
                AbsoluteMaximum = 20000,
                Minimum = 20,
                Maximum = 20000,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
            });
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = -20,
                AbsoluteMaximum = 20,
                Minimum = -10,
                Maximum = 10,
                MajorStep = 1,
                MajorGridlineStyle = LineStyle.Solid,
                Title = "ms"
            });

            plotView1.Model = model;
        }
    }
}