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

        public void addTabYoutube(string id,int width,int height)
        {
            // コンテンツ格納用のWebブラウザを作成
            WebBrowser wb = new WebBrowser();
            // ウィジェットHTMLを生成
            string widgetCode = "<iframe width=\"" + width.ToString() + "\" height=\"" + height.ToString() + "\" src=\"https://www.youtube.com/embed/" + id + "\" frameborder=\"0\" allowfullscreen></iframe>";
            wb.DocumentText = "<body>" + widgetCode + "</body>";
            wb.Dock = DockStyle.Fill;

            // タブに表示するアイコン用画像ストリーム取得
            WebClient wc = new WebClient();
            string url = "http://img.youtube.com/vi/" + id + "/1.jpg";
            // FormMain.Instance.AddLog(url);
            Stream stream = wc.OpenRead(url);
            Bitmap bmp = new Bitmap(stream);
            imgList.Images.Add(url, bmp);

            // タブを追加
            TabPage tp = new TabPage();
            tp.Controls.Add(wb);
            tp.ImageIndex = imgList.Images.IndexOfKey(url);
            this.tabControlContainer.TabPages.Add(tp);
        }
        public void addTabImg(string url)
        {
            // コンテンツ格納用のWebブラウザを作成
            WebBrowser wb = new WebBrowser();
            string imgHtml = "<img id=\"mainImg\" src=\"" + url + "\" />";
            string js = @"<script type=""text/javascript"">
image = document.getElementById(""mainImg"");
var w = image.width , h = image.height ;
if ( typeof image.naturalWidth !== 'undefined' ) {  // for Firefox, Safari, Chrome
    w = image.naturalWidth;
    h = image.naturalHeight;
} else if ( typeof image.runtimeStyle !== 'undefined' ) {    // for IE
    var run = image.runtimeStyle;
    var mem = { w: run.width, h: run.height };  // keep runtimeStyle
    run.width  = ""auto"";
    run.height = ""auto"";
    w = image.width;
    h = image.height;
    run.width  = mem.w;
    run.height = mem.h;
} else {         // for Opera
    var mem = { w: image.width, h: image.height };  // keep original style
    image.removeAttribute(""width"");
    image.removeAttribute(""height"");
    w = image.width;
    h = image.height;
    image.width  = mem.w;
    image.height = mem.h;
}
var ImgAspectRatio = w/h;
ImageChange();
function ImageChange(){
    var AspectRatio = document.body.clientWidth / document.body.clientHeight;
    if(AspectRatio < ImgAspectRatio){
        document.getElementById(""mainImg"").style.width = ""100%"";
        document.getElementById(""mainImg"").style.height = """";
    }else{
        document.getElementById(""mainImg"").style.width = """";
        document.getElementById(""mainImg"").style.height = ""100%"";
    }
}
window.onresize = ImageChange;
</script>
";
            wb.DocumentText = "<body>" + imgHtml + js + "</body>";
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

        private void tabControlContainer_DoubleClick(object sender, EventArgs e)
        {
            TabPage rmPage = this.tabControlContainer.TabPages[tabControlContainer.TabIndex];
            this.tabControlContainer.TabPages.Remove(this.tabControlContainer.TabPages[tabControlContainer.TabIndex]);
            // タブを消しただけだとページがメモリ上に残るのでdisposeする
            rmPage.Dispose();
            // タブが無くなったらフォーム自体を閉じる
            if (this.tabControlContainer.TabCount == 0) this.Close();
        }

    }
}
