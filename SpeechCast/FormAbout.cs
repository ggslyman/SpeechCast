using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;

namespace SpeechCast
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();

            Assembly asm = Assembly.GetEntryAssembly();

            string ver = asm.GetName().Version.ToString();

            Regex rx = new Regex(@"\.\d+$");

            ver = rx.Replace(ver, "");
            labelVersion.Text += ver;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
