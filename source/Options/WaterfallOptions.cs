﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace sam.Options
{
    public partial class WaterfallOptions : Form
    {
        ExpSweepMeasurement expSweepMeasurement;

        public WaterfallOptions()
        {
            InitializeComponent();
        }

        public void Init(ExpSweepMeasurement expSweepMeasurement, WterfallGenerateOptions waterfallGenerateOptions)
        {
            this.expSweepMeasurement = expSweepMeasurement;

            numericSampleRate.Value = expSweepMeasurement.SampleRate;

            numericWindow.Value = waterfallGenerateOptions.Window;
            numericSlices.Value = waterfallGenerateOptions.SliceCount;
            numericStep.Value = waterfallGenerateOptions.Step;
            numericCaptureTime.Value = (decimal)CalcCapturedTime;

            numericLeftWindow.Value = waterfallGenerateOptions.LeftTukeyWindow;
            numericRightWindow.Value = waterfallGenerateOptions.RightTukeyWindow;

            numericdBRange.Value = waterfallGenerateOptions.dBRange;

            numericSmothInvOctaves.Value = (decimal)waterfallGenerateOptions.SmothInvOctaves;

            numericOffset.Value = waterfallGenerateOptions.Offset;
        }

        public void SetOptions(WterfallGenerateOptions waterfallGenerateOptions)
        {
            waterfallGenerateOptions.Window = (int)numericWindow.Value;
            waterfallGenerateOptions.SliceCount = (int)numericSlices.Value;
            waterfallGenerateOptions.Step = (int)numericStep.Value;

            waterfallGenerateOptions.LeftTukeyWindow = (int)numericLeftWindow.Value;
            waterfallGenerateOptions.RightTukeyWindow = (int)numericRightWindow.Value;

            waterfallGenerateOptions.dBRange = (int)numericdBRange.Value;

            waterfallGenerateOptions.SmothInvOctaves = (double)numericSmothInvOctaves.Value;

            waterfallGenerateOptions.Offset = (int)numericOffset.Value;
        }

        private double CalcCapturedTime => (((double)numericSlices.Value * (double)numericStep.Value / (double)(expSweepMeasurement.SampleRate) * 1000.0));

        private void numericSlices_ValueChanged(object sender, EventArgs e)
        {
            numericCaptureTime.Value = (decimal)CalcCapturedTime;
        }

        private void numericStep_ValueChanged(object sender, EventArgs e)
        {
            numericCaptureTime.Value = (decimal)CalcCapturedTime;
        }

        private void numericWindow_ValueChanged(object sender, EventArgs e)
        {
            numericLeftWindow.Maximum = (int)numericWindow.Value / 2;
            numericRightWindow.Maximum = (int)numericWindow.Value / 2;
        }
    }
}
