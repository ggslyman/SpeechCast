using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace SpeechCast.Controls
{
    public partial class FormWaitForEdit : Form
    {
        public FormWaitForEdit()
        {
            InitializeComponent();
        }

        private Process process;

        public void SetProcess(Process process)
        {
            this.process = process;
            process.SynchronizingObject = this.buttonClose;
            process.Exited += new EventHandler(process_Exited);
        }

        void process_Exited(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }        

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("エディターを終了する前に閉じると、編集結果が反映されませんがよろしいですか？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
