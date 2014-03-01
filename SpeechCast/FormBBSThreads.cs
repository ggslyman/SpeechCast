using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace SpeechCast
{
    public partial class FormBBSThreads : Form
    {
        public FormBBSThreads()
        {
            InitializeComponent();

            formThreadWrite.IsThreadCreation = true;
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


        private void UpdateThreads()
        {
            string subjectURL = BaseURL + "subject.txt";
            System.Console.WriteLine(subjectURL);
            System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(subjectURL);
            FormMain.UserConfig.SetProxy(webReq);

            string encodingName = null;


            switch (Response.Style)
            {
                case Response.BBSStyle.jbbs:
                    encodingName = "EUC-JP";
                    break;
                case Response.BBSStyle.yykakiko:
                case Response.BBSStyle.nichan:
                    encodingName = "Shift_JIS";
                    break;
            }
            //encodingName = "Shift_JIS";
            System.Net.HttpWebResponse webRes = null;
            bBSThreads.Clear();

            PushAndSetWaitCursor();
            try
            {
                webRes = (System.Net.HttpWebResponse)webReq.GetResponse();
                Dictionary<string, object> lines = new Dictionary<string, object>();

                using (StreamReader reader = new StreamReader(webRes.GetResponseStream(), Encoding.GetEncoding(encodingName)))
                {
                    while (true)
                    {
                        string s = reader.ReadLine();
                        if (s == null)
                        {
                            break;
                        }

                        if (lines.ContainsKey(s) == false)
                        {
                            lines.Add(s, null);
                            BBSThread thread = new BBSThread();

                            if (thread.SetRawText(s))
                            {
                                bBSThreads.Add(thread);
                            }
                        }
                    }
                }


                filteredBBSThreads = new List<BBSThread>(bBSThreads);
                UpdateListView();
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("エラーが発生しました:{0}", e.Message));
            }
            finally
            {
                PopCursor();
            }
        }

        void UpdateListView()
        {
            listViewThreads.VirtualListSize = filteredBBSThreads.Count;
            virtualListViewItems = new ListViewItem[filteredBBSThreads.Count];
            listViewThreads.Refresh();
        }

        List<BBSThread> bBSThreads = new List<BBSThread>();

        List<BBSThread> filteredBBSThreads = new List<BBSThread>();
        ListViewItem[] virtualListViewItems = null;

        public string BaseURL = null;

        public string ThreadURL = null;

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (this.listViewThreads.SelectedIndices.Count > 0)
            {
                if (SetThreadURL())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else
            {
                MessageBox.Show("スレを選択してください");
            }
        }



        private bool SetThreadURL()
        {
            if (this.listViewThreads.SelectedIndices.Count > 0)
            {
                BBSThread thread = filteredBBSThreads[this.listViewThreads.SelectedIndices[0]];
                if (thread != null)
                {
                    Match m = Communicator.JBBSBaseRegex.Match(BaseURL);

                    if (m.Success)
                    {
                        ThreadURL = string.Format("{0}/bbs/read.cgi/{1}/{2}/{3}/"
                            , m.Groups[1].Value
                            , m.Groups[2].Value
                            , m.Groups[3].Value
                            , thread.ThreadID);
                        return true;
                    }
                    m = Communicator.NichanBaseRegex.Match(BaseURL);
                    if (m.Success)
                    {
                        ThreadURL = string.Format("{0}/test/read.cgi/{1}/{2}/"
                            , m.Groups[1].Value
                            , m.Groups[2].Value
                            , thread.ThreadID);
                        return true;
                    }
                    m = Communicator.YYBaseRegex.Match(BaseURL);
                    if (m.Success)
                    {
                        ThreadURL = string.Format("{0}/test/read.cgi/{1}/{2}/"
                            , m.Groups[1].Value
                            , m.Groups[2].Value
                            , thread.ThreadID);
                        return true;
                    }
                }
            }
            return false;
        }


        private void FormBBSThreads_Load(object sender, EventArgs e)
        {
            UpdateThreads();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            UpdateThreads();
        }

        private void listViewThreads_DoubleClick(object sender, EventArgs e)
        {
            if (SetThreadURL())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private FormWrite formThreadWrite = new FormWrite();
        static Regex threadTitleRegex = new System.Text.RegularExpressions.Regex(@"(.+)(\d+)(\D*)", RegexOptions.CultureInvariant);

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            //if (Response.Style == Response.BBSStyle.nichan)
            //{
            //    MessageBox.Show("2chのスレ作成機能は未実装です。");
            //    return;
            //}

            Communicator.Instance.BaseURL = BaseURL;
            Communicator.Instance.ThreadURL = ThreadURL;

            if (this.listViewThreads.Items.Count > 0)
            {
                BBSThread thread = this.listViewThreads.Items[0].Tag as BBSThread;
                if (thread != null)
                {
                    Match m = threadTitleRegex.Match(thread.Text);
                    if (m.Success)
                    {
                        //タイトルに数字が含まれる場合、＋１したモノをデフォルト値とする
                        try
                        {
                            int threadSeq = System.Convert.ToInt32(m.Groups[2].Value) + 1;

                            formThreadWrite.ThreadTitle = m.Groups[1].Value + threadSeq.ToString() + m.Groups[3].Value;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (formThreadWrite.ShowDialog() == DialogResult.OK)
            {
                UpdateThreads();
            }
        }

        private void listViewThreads_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            BBSThread thread = filteredBBSThreads[e.ItemIndex];
            ListViewItem item = virtualListViewItems[e.ItemIndex];

            if (item == null)
            {
                item = thread.CreateListViewItem();
                virtualListViewItems[e.ItemIndex] = item;
            }
            e.Item = item;            
        }

        private void listViewThreads_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            for (int i = e.StartIndex; i <= e.EndIndex; i++)
            {
                ListViewItem item = virtualListViewItems[i];

                if (item == null)
                {
                    item = filteredBBSThreads[i].CreateListViewItem();
                    virtualListViewItems[i] = item;
                }                
            }
        }

        private void textBoxFilterWord_TextChanged(object sender, EventArgs e)
        {
            UpdateFilter();
        }


        private void UpdateFilter()
        {
            filteredBBSThreads.Clear();
            string text = textBoxFilterWord.Text;

            foreach (BBSThread thread in bBSThreads)
            {
                if (thread.Text.IndexOf(text) >= 0)
                {
                    filteredBBSThreads.Add(thread);
                }
            }
            UpdateListView();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.ActiveControl == textBoxFilterWord)
            {
                if (keyData == Keys.Enter)
                {
                    UpdateFilter();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
