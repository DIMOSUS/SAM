using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sam
{
    public class Overlay
    {
        public int Index
        {
            get; private set; 
        }

        public Panel panel;
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
