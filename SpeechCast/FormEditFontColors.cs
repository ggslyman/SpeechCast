using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpeechCast
{
    public partial class FormEditFontColors : Form
    {
        public FormEditFontColors()
        {
            InitializeComponent();
        }

        public CaptionFont.CaptionColors Colors
        {
            get
            {
                CaptionFont.CaptionColors colors = new CaptionFont.CaptionColors();

                colors.HighForeColor = this.panelHighForeColor.BackColor;
                colors.LowForeColor = this.panelLowForeColor.BackColor;
                colors.BorderColor = this.panelBorderColor.BackColor;
                colors.TransparentColor = this.panelTransparentColor.BackColor;

                return colors;
            }

            set
            {
                this.panelHighForeColor.BackColor = value.HighForeColor;
                this.panelLowForeColor.BackColor = value.LowForeColor;
                this.panelBorderColor.BackColor = value.BorderColor;
                this.panelTransparentColor.BackColor = value.TransparentColor;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            Panel panel = sender as Panel;

            if (panel != null)
            {
                ColorDialog dialog = new ColorDialog();

                dialog.Color = panel.BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    panel.BackColor = dialog.Color;
                }
            }
        }
    }
}
