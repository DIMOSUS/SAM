using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CheckBox = System.Windows.Forms.CheckBox;

namespace sam
{
    public class OverlayCollection
    {
        List<Overlay> AllOverlays = new List<Overlay>();
        OxyPlot.WindowsForms.PlotView plotView;

        public OverlayCollection(Form1 form, Panel overlays, OxyPlot.WindowsForms.PlotView plotView)
        {
            this.plotView = plotView;
            Panel overlayPanel1 = null;
            Button button1 = null;
            NumericUpDown numericUpDown1 = null;
            CheckBox checkBox1 = null;

            if (overlays.Controls.Count > 0)
            {
                var el = overlays.Controls[0];
                if (el != null)
                {
                    if (el is Panel)
                    {
                        overlayPanel1 = (Panel)el;
                        foreach (var c in overlayPanel1.Controls)
                        {
                            if (c is Button) button1 = (Button)c;
                            if (c is NumericUpDown) numericUpDown1 = (NumericUpDown)c;
                            if (c is CheckBox) checkBox1 = (CheckBox)c;
                        }
                    }
                }
            }

            AllOverlays.Add(new Overlay(overlayPanel1, button1, numericUpDown1, checkBox1, 1));

            form.SuspendLayout();
            overlays.SuspendLayout();

            Random r = new Random(3);

            for (int i = 2; i <= 12; i++)
            {
                Button button = new Button();
                CheckBox checkBox = new CheckBox();
                NumericUpDown numericUpDown = new NumericUpDown();
                Panel overlayPanel = new Panel();

                overlayPanel.SuspendLayout();

                //
                overlayPanel.BackColor = Color.FromArgb(r.Next(255), r.Next(255), r.Next(255));

                overlayPanel.Controls.Add(numericUpDown);
                overlayPanel.Controls.Add(checkBox);
                overlayPanel.Controls.Add(button);
                overlayPanel.Size = new Size(130, 25);
                overlayPanel.Location = new Point(3, overlayPanel1.Location.Y + (overlayPanel.Size.Height + overlayPanel1.Margin.Top) * (i - 1));
                overlayPanel.Name = $"overlayPanel{i}";

                //
                numericUpDown.BorderStyle = System.Windows.Forms.BorderStyle.None;
                numericUpDown.Location = numericUpDown1.Location;
                numericUpDown.Maximum = numericUpDown1.Maximum;
                numericUpDown.Minimum = numericUpDown1.Minimum;
                numericUpDown.Name = $"numericUpDown{i}";
                numericUpDown.Size = numericUpDown1.Size;
                numericUpDown.TextAlign = numericUpDown1.TextAlign;
                numericUpDown.Value = numericUpDown1.Value;

                //
                checkBox.AutoSize = checkBox1.AutoSize;
                checkBox.Location = checkBox1.Location;
                checkBox.Name = $"checkBox{i}";
                checkBox.Size = checkBox1.Size;

                //
                button.FlatStyle = button1.FlatStyle;
                button.Location = button1.Location;
                button.Name = $"button{i}";
                button.Size = button1.Size;
                button.Text = $"{i}";

                overlayPanel.ResumeLayout(false);
                overlayPanel.PerformLayout();

                overlays.Controls.Add(overlayPanel);
                AllOverlays.Add(new Overlay(overlayPanel, button, numericUpDown, checkBox, i));
            }
            overlays.ResumeLayout(false);
            form.ResumeLayout(false);
        }

        public void Prepare(Mode mode)
        {

        }
    }

    public class Overlay
    {
        public int Index
        {
            get; private set; 
        }

        Panel panel;
        NumericUpDown numericUpDown;
        Button button;
        CheckBox checkBox;

        public Overlay(Panel panel, Button button, NumericUpDown numericUpDown, CheckBox checkBox, int index)
        {
            this.panel = panel;
            this.numericUpDown = numericUpDown;
            this.button = button;
            this.checkBox = checkBox;

            panel.Click += new EventHandler(panelClick);
            checkBox.CheckedChanged += new EventHandler(checkBoxUpdate);
            button.Click += new EventHandler(buttonClick);
            numericUpDown.ValueChanged += new EventHandler(numericValueChanged);
            Index = index;
        }

        public void Update()
        {

        }

        private void panelClick(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            // Keeps the user from selecting a custom color.
            MyDialog.AllowFullOpen = false;

            // Sets the initial color select to the current text color.
            MyDialog.Color = panel.BackColor;

            if (MyDialog.ShowDialog() == DialogResult.OK)
                panel.BackColor = MyDialog.Color;

            Update();
        }

        private void checkBoxUpdate(object sender, EventArgs e)
        {
            Update();
        }

        private void buttonClick(object sender, EventArgs e)
        {
            Update();
        }

        private void numericValueChanged(object sender, EventArgs e)
        {
            Update();
        }
    }
}
