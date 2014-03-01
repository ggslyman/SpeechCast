using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace SpeechCast
{
    public partial class FormWrite : Form
    {
        public FormWrite()
        {
            InitializeComponent();


        }

        private bool isThreadCreation = true;
        /// <summary>
        /// スレッド作成モード？
        /// </summary>
        public bool IsThreadCreation
        {
            get { return isThreadCreation; }

            set
            {
                if (isThreadCreation != value)
                {
                    isThreadCreation = value;
                    labelThreadTitle.Enabled = isThreadCreation;
                    textBoxThreadTitle.Enabled = isThreadCreation;

                    if (isThreadCreation)
                    {
                        this.Text = "スレッド作成";
                    }
                    else
                    {
                        this.Text = "書き込み";
                    }
                }

            }
        }

        public string ThreadTitle
        {
            get { return textBoxThreadTitle.Text; }
            set
            {
                textBoxThreadTitle.Text = value;
            }
        }

        private Cursor pushedCursor = null;

        public void PushAndSetWaitCursor()
        {
            pushedCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
        }

        public void PopCursor()
        {
            Cursor = pushedCursor;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            PushAndSetWaitCursor();
            try
            {
                if (isThreadCreation)
                {
                    if (Communicator.Instance.CreateThread(textBoxThreadTitle.Text, textBoxName.Text, textBoxMailAddress.Text, textBoxContents.Text))
                    {
                        MessageBox.Show(Communicator.Instance.ReturnText, "作成結果");
                        textBoxContents.Text = "";
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(Communicator.Instance.ReturnText, "エラー");
                    }

                }
                else
                {
                    if (Communicator.Instance.WriteResponse(textBoxName.Text, textBoxMailAddress.Text, textBoxContents.Text))
                    {
                        //MessageBox.Show(Communicator.Instance.ReturnText, "書き込み結果");
                        textBoxContents.Text = "";
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(Communicator.Instance.ReturnText, "エラー");
                    }
                }
            }
            finally
            {
                PopCursor();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Shift | Keys.Enter))
            {
                buttonOK_Click(null, null);
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void buttonRunEditor_GetEditorFilePath(object sender, SpeechCast.Controls.EventEditorArgs e)
        {
            e.EditorFilePath = FormMain.UserConfig.EditorFilePath;
        }


        private void buttonPlay_Click(object sender, EventArgs e)
        {
            FormMain.Instance.StopSpeaking();
            FormMain.Instance.StartSpeaking(this.textBoxContents.Text);
        }

        bool autoUpdate = false;

        private void FormWrite_Shown(object sender, EventArgs e)
        {
            autoUpdate = FormMain.Instance.AutoUpdate;
            textBoxName.Text = FormMain.UserConfig.FormWriteName;
            checkBoxSage.Checked = FormMain.UserConfig.FormWriteSage;
            textBoxMailAddress.Text = FormMain.UserConfig.FormWriteMailAddress;
        }

        private void FormWrite_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormMain.Instance.AutoUpdate = autoUpdate;
            FormMain.UserConfig.FormWriteName = textBoxName.Text;
            FormMain.UserConfig.FormWriteMailAddress = textBoxMailAddress.Text;
            FormMain.UserConfig.FormWriteSage = checkBoxSage.Checked;
        }

        private void checkBoxSage_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSage.Checked)
            {
                textBoxMailAddress.Enabled = false;
                textBoxMailAddress.Text = "sage";
            }
            else
            {
                textBoxMailAddress.Enabled = true;
                textBoxMailAddress.Text = "";
            }
        }

    }
}
