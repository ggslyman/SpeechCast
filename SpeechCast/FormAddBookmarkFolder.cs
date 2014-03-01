using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpeechCast
{
    public partial class FormAddBookmarkFolder : Form
    {
        public FormAddBookmarkFolder()
        {
            InitializeComponent();
            
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public string FolderName
        {
            get
            {
                return this.textBoxFolderName.Text;
            }
        }
    }
}
