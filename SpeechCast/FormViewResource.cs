using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace SpeechCast
{
    public partial class FormViewResource : Form
    {
        static public FormViewResource Instance;
        private ImageList imgList = new ImageList();
        public Rectangle rect;
        public FormViewResource(Rectangle loadRect) 
        {
            rect = loadRect;
            InitializeComponent();
            Instance = this;
            this.tabControlContainer.ImageList = imgList;
            Size thumbSize = new Size(32, 32);
            this.tabControlContainer.ImageList.ImageSize = thumbSize;
        }

        private void FormViewResouce_Load(object sender, EventArgs e)
        {
            this.Top = rect.Top;
            this.Left = rect.Left;
            this.Width = rect.Width;
            this.Height = rect.Height;
        }

        public void addTabYoutube(string id)
        {
            string widgetCode = "<iframe width=\"480\" height=\"360\" src=\"https://www.youtube.com/embed/" + id + "\" frameborder=\"0\" allowfullscreen></iframe>";
            // コンテンツ格納用のWebブラウザを作成
            WebBrowser wb = new WebBrowser();
            wb.Dock = DockStyle.Fill;
            wb.DocumentText = "<body>" + widgetCode + "</body>";

            // タブを追加
            TabPage tp = new TabPage();
            tp.Controls.Add(wb);
            this.tabControlContainer.TabPages.Add(tp);
        }
        public void addTabImg(string url)
        {
            // コンテンツ格納用のWebブラウザを作成
            WebBrowser wb = new WebBrowser();
            wb.Url = new Uri(url);
            wb.Dock = DockStyle.Fill;
            // タブに表示するアイコン用画像ストリーム取得
            WebClient wc = new WebClient();
            Stream stream = wc.OpenRead(url);
            Bitmap bmp = new Bitmap(stream);
            imgList.Images.Add(url,bmp);
            
            // タブを追加
            TabPage tp = new TabPage();
            tp.Controls.Add(wb);
            tp.ImageIndex = imgList.Images.IndexOfKey(url);
            this.tabControlContainer.TabPages.Add(tp);
        }

        // タブクリック処理
        private void tabControlContainer_MouseDown(object sender, MouseEventArgs e)
        {
//            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < this.tabControlContainer.TabCount; i++)
                {
                    //タブとマウス位置を比較し、クリックしたタブを選択
                    if (this.tabControlContainer.GetTabRect(i).Contains(e.X, e.Y))
                    {
                        TabPage rmPage = this.tabControlContainer.TabPages[i];
                        this.tabControlContainer.TabPages.Remove(this.tabControlContainer.TabPages[i]);
                        // タブを消しただけだとページがメモリ上に残るのでdisposeする
                        rmPage.Dispose();
                        // タブが無くなったらフォーム自体を閉じる
                        if (this.tabControlContainer.TabCount == 0)this.Close();
                        break;
                    }
                }
            }
        }

        // ウィンドウ位置の保存
        private void FormViewResource_Move(object sender, EventArgs e)
        {
            FormMain.Instance.saveViewerPos(this);
        }
        private void FormViewResource_Resize(object sender, EventArgs e)
        {
            FormMain.Instance.saveViewerPos(this);
        }

    }
}
