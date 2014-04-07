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
            System.Console.WriteLine(ver);
            Regex rx = new Regex(@"\.(?<rev>\d+?)$");
            Match m = rx.Match(ver);
            ver = rx.Replace(ver, "");
            if (m.Success && Int32.Parse(m.Groups["rev"].Value) > 0) ver += " rev." + m.Groups["rev"].Value;
            labelVersion.Text += ver;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
