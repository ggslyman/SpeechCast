using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpeechCast
{
    public partial class FormAddBookmark : Form
    {
        public FormAddBookmark()
        {
            InitializeComponent();
        }

        public void Initialize(string baseURL)
        {
            if (baseURL != null)
            {
                textBoxURL.Text = baseURL;

                Communicator.Instance.BaseURL = baseURL;
                textBoxTitle.Text = Communicator.Instance.GetTitle();


            }

            textBoxURL.Focus();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public Bookmarks.Bookmark GetBookmark()
        {
            Bookmarks.Bookmark bookmark = new Bookmarks.Bookmark();

            bookmark.Name = textBoxTitle.Text;
            bookmark.URL = textBoxURL.Text;

            return bookmark;
        }
    }
}
