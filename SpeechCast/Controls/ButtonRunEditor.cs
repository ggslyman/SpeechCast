using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SpeechCast.Controls
{
    public partial class ButtonRunEditor : UserControl
    {
        public ButtonRunEditor()
        {
            InitializeComponent();

            this.buttonEdit.Click += new EventHandler(buttonEdit_Click);
        }

        void buttonEdit_Click(object sender, EventArgs e)
        {
            if (GetEditorFilePath == null)
            {
                MessageBox.Show(editorErrorMessage);
            }
            else
            {
                EventEditorArgs ee = new EventEditorArgs();
                GetEditorFilePath(sender, ee);

                if (ee.EditorFilePath != null)
                {
                    try
                    {

                        tempFilePath = Path.Combine(Path.Combine(Path.GetTempPath(), "SpeechCast"), DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".txt");

                        string tempDir = Path.GetDirectoryName(tempFilePath);

                        if (!Directory.Exists(tempDir))
                        {
                            Directory.CreateDirectory(tempDir);
                        }

                        using (StreamWriter sw = new StreamWriter(tempFilePath, false, Encoding.GetEncoding("Shift_JIS")))
                        {
                            sw.Write(textBox.Text);
                        }


                        processEditor = new Process();
                        processEditor.StartInfo.FileName = ee.EditorFilePath;
                        processEditor.StartInfo.Arguments = "\"" + tempFilePath + "\"";
                        processEditor.StartInfo.UseShellExecute = false;
                        processEditor.EnableRaisingEvents = true;

                        formWaitForEdit = new FormWaitForEdit();
                        formWaitForEdit.SetProcess(processEditor);

                        processEditor.Start();

                        if (formWaitForEdit.ShowDialog() == DialogResult.OK)
                        {
                            using (StreamReader sr = new StreamReader(tempFilePath, Encoding.GetEncoding("Shift_JIS")))
                            {
                                textBox.Text = sr.ReadToEnd();
                            }
                        }

                        formWaitForEdit = null;
                        processEditor = null;
                        File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
                }
            }

        }

        private FormWaitForEdit formWaitForEdit;
        private Process processEditor = null;
        private string tempFilePath = null;

        private string editorErrorMessage = "";

        public string EditorErrorMessage
        {
            get
            {
                return this.editorErrorMessage;
            }
            set
            {
                this.editorErrorMessage = value;
            }
        }

        public event GetEditorFilePathEventHandler GetEditorFilePath;

        private TextBox textBox;

        public TextBox TextBox
        {
            get
            {
                return this.textBox;
            }
            set
            {
                this.textBox = value;
            }
        }

    }


    public class EventEditorArgs :  EventArgs
    {
        public string EditorFilePath = null;
    }

    public delegate void GetEditorFilePathEventHandler(object sender, EventEditorArgs e);
}
