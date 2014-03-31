using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Speech.Synthesis;
using System.Globalization; 
using System.Text.RegularExpressions;
using System.IO;
using System.Media;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SpeechCast
{
    public partial class FormMain : Form
    {
        // webブラウザコントロールのリロード音を消すためのAPI登録
        [DllImport("urlmon.dll"), PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(int FeatureEntry, [In, MarshalAs(UnmanagedType.U4)]uint dwFlags, bool fEnable);
        private const uint SET_FEATURE_ON_PROCESS = 0x00000002;
        private const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;

        DateTime objDate = new DateTime();
        public FormMain()
        {
            // ログのファイル出力周り処理(デバッグビルド時のみ)
            //  出力ファイルを指定して、StreamWriterオブジェクトを作成
            StreamWriter sw = new StreamWriter("debug.log");
            //  自動的にフラッシュされるようにする
            sw.AutoFlush = true;
            //  スレッドセーフラッパを作成
            TextWriter tw = TextWriter.Synchronized(sw);
            //  DefaultTraceListenerが必要なければ削除する
            Trace.Listeners.Remove("Default");
            //  名前を LogFile としてTextWriterTraceListenerオブジェクトを作成
            TextWriterTraceListener twtl = new TextWriterTraceListener(tw, "LogFile");
            //  リスナコレクションに追加する
            Trace.Listeners.Add(twtl);

            // コンポーネントの初期化
            InitializeComponent();

            // インストール済みSSAPI5のボイスリストを取得
            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                toolStripComboBoxSelectVoice.Items.Add(voice.VoiceInfo.Name);
            }

            // 音声再生関連のコールバック関数の登録
            synthesizer.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synthesizer_SpeakProgress);
            synthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synthesizer_SpeakCompleted);

            // 生成されたインスタンスを変数へ代入(別フォームからの操作のため)
            Instance = this;

            FormCaption.Instance = new FormCaption();

            // Webブラウザオブジェクトへの新規イベント追加
            this.webBrowser.StatusTextChanged += new EventHandler(webBrowser_StatusTextChanged);
            this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
            // JavaScriptでの差分取得用HTMLをセット
            string html = Properties.Resources.resView.ToString();
            webBrowser.DocumentText = html;
        }

        static Regex youtubeId = new Regex(@"http[s]?://www.youtube.com/watch\?v=([a-zA-Z0-9]*)", RegexOptions.IgnoreCase);
        // ブラウザ内リンクのイベント追加
        void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.AbsoluteUri;
            if (url.StartsWith(Response.AnchorUrl))
            {
                e.Cancel = true;
                string resNoStr = url.Substring(Response.AnchorUrl.Length, url.Length - Response.AnchorUrl.Length);
                int resNo = System.Convert.ToInt32(resNoStr);

                if (resNo > 0 && resNo <= responses.Count)
                {
                    webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(resNo));
                }
            }
            else if (url.EndsWith("jpg") || url.EndsWith("png") || url.EndsWith("gif"))
            {
                e.Cancel = true;
                oepnFormViewNewtabImg(url);
            }
            else if (url.IndexOf("youtube.com/watch?v=")>0)
            {
                e.Cancel = true;
                Match m = youtubeId.Match(url);
                if(m.Success){
                    var id = m.Groups[1].Value;
                    oepnFormViewNewtabYoutube(id);
                }
            }
            else if (url.StartsWith("http:"))
            {
                System.Diagnostics.Process.Start(url);
                e.Cancel = true;
            }
        }


        static public FormMain Instance;
        private string captionTextBuffer = "";
        // 代替テキスト
        public string CaptionTextBuffer{
            // 数値と時計の自動変換
            get { return captionTextBuffer.Replace("#1#", comboBoxCaptionNum1.SelectedIndex.ToString()).Replace("#2#", comboBoxCaptionNum2.SelectedIndex.ToString()).Replace("#CLOCK#", objDate.ToString(dateformat)); }
            set {captionTextBuffer = value;}
        }
        // ステータスバーの表示テキスト
        private string communicationStatusString
        {
            get
            {
                return this.toolStripStatusLabelCommunication.Text;
            }
            set
            {
                if (this.toolStripStatusLabelCommunication.Text != value)
                {
                    this.toolStripStatusLabelCommunication.Text = value;
                    this.statusStrip1.Update();
                }
            }
        }
        // 読み上げ完了時の処理
        void synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            speakingCompletedTime = System.DateTime.Now;
            isSpeaking = false;

            if (e.Cancelled)
            {
                FormCaption.Instance.CaptionText = "";
            }
            else
            {
                FormCaption.Instance.CaptionText = speakingText;

                // 自動次スレオープン時のフラグ更新と現在のレス番号のリセット
                if (openNextThread == SpeakAnnounce)
                {
                    openNextThread = ReadThread;
                    CurrentResNumber = 0;
                }
                else if (CurrentResNumber <= Response.MaxResponseCount && speakClipboard == false)
                {
                    CurrentResNumber++;
                }
            }
        }

        System.DateTime speakingCompletedTime;
        System.DateTime gettingWebTime;


        void synthesizer_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            string speackStr;

            if (!UserConfig.ShowCaptionImmediately && !FormCaption.Instance.IsAAMode)
            {
                int index = e.CharacterPosition + e.CharacterCount;

                if (replacementIndices != null)
                {
                    if (index > 0)
                    {
                        index--;
                    }

                    if (index < replacementIndices.Count)
                    {
                        index = replacementIndices[index] + 1;
                    }
                    else
                    {
                        index = speakingText.Length;
                    }
                }

                if (index > speakingText.Length)
                {
                    index = speakingText.Length;
                }
                speackStr = speakingText.Substring(0, index);
            }
            else
            {
                speackStr = speakingText;
            }

            FormCaption.Instance.CaptionText = speackStr;
        }

        public SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        private void toolStripComboBoxSelectVoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = toolStripComboBoxSelectVoice.SelectedIndex;

            if (idx >= 0)
            {
                string voiceName = toolStripComboBoxSelectVoice.Items[idx].ToString();
                synthesizer.SelectVoice(voiceName);
                UserConfig.VoiceName = voiceName;
            }
        }

        private void toolStripButtonEnter_Click(object sender, EventArgs e)
        {
            GetFromURL();
        }

        private void toolStripTextBoxURL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                GetFromURL();
            }
        }

        private string rawURL = null;

        private async void GetFromURL()
        {
            if (CheckBaseURL())
            {
                if (ShowFormBBSThreads() == false)
                {
                    return;
                }
                else
                {
                    CurrentResNumber = 0;
                }
            }
            try
            {
                await GetFromURL(false);
            }
            catch (Exception ex)
            {
                AddLog("Exception: {0}", ex.Message);
            }
            AutoUpdate = false;
        }

        private async Task GetFromURLNext()
        {
            this.endWebRequest = false;
            if (!await GetFromURL(true))
            {
                if (needsRetry)
                {
                   await GetFromURL(false);
                }
            }
            this.endWebRequest = true;
        }
       
        string encodingName = "";

        int datSize = 0;
        DateTime lastModifiedDateTime;

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


        Regex jbbsBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://(jbbs.livedoor.jp|jbbs.shitaraba.net)/\w+/\d+/)");
        Regex nichanBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://.+2ch\.net/\w+/)\s*$");
        Regex yyBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://(yy.+\.60\.kg|yy.+\.kakiko\.com|bbs\.aristocratism\.info|www.+\.atchs\.jp)/\w+/$)");

        private bool CheckBaseURL()
        {
            Match m = jbbsBaseRegex.Match(toolStripTextBoxURL.Text);

            if (m.Success)
            {
                baseURL = m.Groups[1].Value;
                Response.Style = Response.BBSStyle.jbbs;
                return true;
            }
            else
            {
                m = nichanBaseRegex.Match(toolStripTextBoxURL.Text);
                if (m.Success)
                {
                    baseURL = m.Groups[1].Value;
                    Response.Style = Response.BBSStyle.nichan;
                    return true;
                }
                else
                {
                    m = yyBaseRegex.Match(toolStripTextBoxURL.Text);
                    if (m.Success)
                    {
                        baseURL = m.Groups[1].Value;
                        Response.Style = Response.BBSStyle.yykakiko;
                        return true;
                    }
                }
            }
            return false;
        }


        string baseURL = null;
        bool needsRetry;
        string threadTitle = "";

#if DEBUG
        string debugDatFileName = null;
#endif
        // 元ソースより非同期に変更
        private int oldResCount = 0;
        private string oldUrl = "";
        private async Task<bool> GetFromURL(bool next)
        {

            string url = null;
            bool clearItems = true;
            bool result = true;
            bool updated = false;
            bool useRangeHeader = false;
            needsRetry = false;
            if (next && rawURL != null)
            {
                switch (Response.Style)
                {
                    case Response.BBSStyle.jbbs:
                        url = string.Format("{0}{1}-", rawURL, responses.Count + 1);
                        clearItems = false;
                        break;
                    case Response.BBSStyle.yykakiko:
                    case Response.BBSStyle.nichan:
                        url = rawURL;
                        clearItems = false;
                        useRangeHeader = true;
                        break;

                }
            }
            else
            {
#if DEBUG
                debugDatFileName = null;
#endif
                Match m = Communicator.JBBSRegex.Match(toolStripTextBoxURL.Text);
                if (m.Success)
                {
                    rawURL = m.Groups[1].Value + "/bbs/rawmode.cgi" + m.Groups[2];
                    AddLog("jbbs rawmode: {0}", rawURL);
                    Response.Style = Response.BBSStyle.jbbs;
                    encodingName = "EUC-JP";

                    baseURL = string.Format("{0}/{1}/{2}/", m.Groups[1], m.Groups[3], m.Groups[4]);
                }
                else
                {
                    m = Communicator.YYRegex.Match(toolStripTextBoxURL.Text);
                    if (m.Success)
                    {
                        rawURL = m.Groups[1].Value + "/" + m.Groups[2].Value + "/dat/" + m.Groups[3].Value + ".dat";
                        Response.Style = Response.BBSStyle.yykakiko;

                        AddLog("yykakiko dat mode: {0}", rawURL);

                        encodingName = "Shift_JIS";
                        baseURL = string.Format("{0}/{1}/", m.Groups[1], m.Groups[2]);
                    }
                    else
                    {
                        m = Communicator.NichanRegex.Match(toolStripTextBoxURL.Text);
                        if (m.Success)
                        {
                            rawURL = m.Groups[1].Value + "/" + m.Groups[2].Value + "/dat/" + m.Groups[3].Value + ".dat";
                            Response.Style = Response.BBSStyle.nichan;

                            AddLog("2ch dat mode: {0}", rawURL);

                            encodingName = "Shift_JIS";
                            baseURL = string.Format("{0}/{1}/", m.Groups[1], m.Groups[2]);
#if DEBUG
                            debugDatFileName = m.Groups[3].Value + ".dat";
#endif
                        }
                    }
                }


                if (rawURL == null)
                {
                    AutoUpdate = false;
                    MessageBox.Show("サポートしていないＵＲＬです");
                    return false;
                }
                url = rawURL;
                datSize = 1;
                // レス差分取得関係の初期化処理
                if (oldUrl != rawURL)
                {
                    responses.Clear();
                    oldUrl = rawURL;
                    oldResCount = 0;
                    Object[] objArray = new Object[1];
                    webBrowser.Document.InvokeScript("clearRes", objArray);
                }
            }

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            // 完全にバックグラウンド処理になったので、メッセージなどはコメントアウト
            //communicationStatusString = "通信中・・・・";
            //PushAndSetWaitCursor();
            int oldResponseCount = responses.Count;
            long responseTime = 0, readTime = 0, listViewTime = 0, documetnTime = 0, encodingTime = 0, setTime = 0;
            System.Net.HttpWebResponse webRes = null;
            // タイムアウト等の結果判別用真偽値
            bool webReqResult = true;
            try
            {
                // Webアクセス部分を非同期化するためTask.Runで囲む
                await Task.Run(() =>
                {
                    System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    webReq.KeepAlive = false;
                    FormMain.UserConfig.SetProxy(webReq);

                    if (UserConfig.GZipCompressionEnabled && useRangeHeader == false)
                    {
                        webReq.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                    }
#if DEBUG
                    //AddLog("datSize={0} lastModifiedTime={1} useRangeHeader={2}",
                    //    datSize, lastModifiedDateTime.ToLongTimeString(), useRangeHeader);
#endif
                    if (useRangeHeader)
                    {
                        webReq.AddRange(datSize - 1);
                        webReq.IfModifiedSince = lastModifiedDateTime;
                    }


                    gettingWebTime = System.DateTime.Now; //例外が発生した場合、連続してwebアクセスが起こるのを防ぐ

                    stopWatch.Start();
                    //サーバーからの応答を受信するためのWebResponseを取得
                    webReq.Timeout = 10000;
                    webRes = (System.Net.HttpWebResponse)webReq.GetResponse();

                    lastModifiedDateTime = webRes.LastModified;
                    responseTime = stopWatch.ElapsedMilliseconds;
                    //if (useRangeHeader)
                    //{
                    //    throw (new Exception(" 416 "));
                    //}
                });// 非同期処理終了
            }
            catch (System.Net.WebException e)
            {
                if (e.Message.IndexOf("304") < 0)
                {
                    AddLog("GetResponse Exception: {0}", e.Message);
                }
                if (e.Message.IndexOf("416") >= 0)
                {
                    //autoUpdate = false;
                    //
                    MessageBox.Show(this, "多分、あぼ～んされています。Enterを押して再取得してください。");
                    AddLog("誰かのレスがあぼ～んされてるかも？　全レス再取得します。");
                    needsRetry = true;
                }
                webReqResult = false;
            }
                if (webReqResult)
                {
                    try
                    {
                        // datの差分取得の為の生データバイト数計測
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            stopWatch.Reset();
                            stopWatch.Start();

                            Stream data = webRes.GetResponseStream();

                            byte[] buf = new byte[1024];

                            while (true)
                            {
                                int size = data.Read(buf, 0, buf.Length);

                                if (size == 0)
                                {
                                    break;
                                }
                                memStream.Write(buf, 0, size);
                            }

                            data.Close();

                            gettingWebTime = System.DateTime.Now;



                            List<Response> tempResponses = new List<Response>(responses);

                            int number = responses.Count + 1;
                            if (clearItems)
                            {
                                tempResponses.Clear();
                                number = 1;
                            }

                            int incRes = 0;
                            memStream.Seek(0, SeekOrigin.Begin);

#if DEBUG
                            if (Response.Style == Response.BBSStyle.nichan && string.IsNullOrEmpty(debugDatFileName) == false && memStream.Length > 1)
                            {
                                memStream.ReadByte();

                                string datDirName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Logs");

                                if (!Directory.Exists(datDirName))
                                {
                                    Directory.CreateDirectory(datDirName);
                                }
                                string fullPath = Path.Combine(datDirName, debugDatFileName);
                                FileMode fileMode = FileMode.Create;

                                if (File.Exists(fullPath) && next)
                                {
                                    fileMode = FileMode.Append;
                                }

                                using (FileStream fs = new FileStream(fullPath, fileMode))
                                {
                                    Byte[] bytes = new Byte[memStream.Length - 1];
                                    memStream.Read(bytes, 0, (int)memStream.Length - 1);
                                    fs.Write(bytes, 0, bytes.Length);
                                }

                                memStream.Seek(0, SeekOrigin.Begin);
                            }
#endif

                            if (useRangeHeader && memStream.Length > 0)
                            {
                                int c = memStream.GetBuffer()[0];

                                //AddLog("c={0}", c);

                                if (c != 10) // 10 == '\n'
                                {
                                    //autoUpdate = false;
                                    AddLog("誰かのレスがあぼ～んされてるかも？　全レス再取得します。(char={0},datSize={1})", c, datSize);
                                    needsRetry = true;
                                    return false;
                                }
                            }
                            readTime = stopWatch.ElapsedMilliseconds;
                            stopWatch.Reset();
                            stopWatch.Start();

                            System.Diagnostics.Stopwatch stopWatchSet = new System.Diagnostics.Stopwatch();

                            // 実データ取得
                            using (StreamReader reader = new StreamReader(memStream, Encoding.GetEncoding(encodingName)))
                            {
                                while (true)
                                {
                                    string s = reader.ReadLine();
                                    if (s == null)
                                    {
                                        break;
                                    }

                                    Response res = new Response();


                                    res.Number = number; //先に入れておかないとHTMLが正しく作成されない
                                    stopWatchSet.Start();
                                    bool ret = res.SetRawText(s);
                                    stopWatchSet.Stop();

                                    if (ret)
                                    {
                                        if (Response.Style == Response.BBSStyle.jbbs)
                                        {
                                            //途中の番号が抜ける場合の対策
                                            while (res.Number > number)
                                            {
                                                AddLog("途中のレスが削除された？ No.{0}", number);
                                                Response emptyRes = new Response();

                                                emptyRes.Number = number++;
                                                emptyRes.SetEmpty();
                                                tempResponses.Add(emptyRes);
                                            }
                                        }

                                        tempResponses.Add(res);
                                        number++;
                                    }

                                    incRes++;
                                }

                                //AddLog("memStream.Size={0} incRes={1}", memStream.Length, incRes);

                                datSize += (int)memStream.Length - 1;
                            }
                            setTime = stopWatchSet.ElapsedMilliseconds;
                            encodingTime = stopWatch.ElapsedMilliseconds;
                            stopWatch.Reset();
                            stopWatch.Start();

                            if (tempResponses.Count != responses.Count || !next)
                            {
                                //レスが増えた

                                updated = true;
                                // 差分用処理
                                if (responses.Count == 0)
                                {
                                    oldResCount = 0;
                                }
                                else
                                {
                                    oldResCount = responses.Count;
                                }
                                listViewResponses.BeginUpdate();
                                int startIndex;
                                if (clearItems)
                                {
                                    responses.Clear();
                                    listViewResponses.Items.Clear();
                                    startIndex = 0;
                                }
                                else
                                {
                                    startIndex = responses.Count;
                                }
                                //AddLog("startIndex={0} tempResponses={1} responses={2}", startIndex, tempResponses.Count, responses.Count);

                                for (int j = startIndex; j < tempResponses.Count; j++)
                                {
                                    Response res = tempResponses[j];

                                    res.Number = j + 1;
                                    responses.Add(res);
                                    listViewResponses.Items.Add(res.CreateListViewItem());
                                }
                                listViewResponses.EndUpdate();


                                if (next == true)
                                {
                                    NewResponseNumber = oldResponseCount + 1;
                                    //AddLog(NewResponseNumber.ToString());
                                    //PlaySoundNewResponse();
                                }
                            }
                            listViewTime = stopWatch.ElapsedMilliseconds;
                            stopWatch.Reset();
                            stopWatch.Start();
                        }
                    }
                    catch (System.Net.WebException we)
                    {
                        result = false;

                        AddLog("WebException: status={0}", we.Status.ToString());
                        if (we.Response != null && we.Response.Headers != null)
                        {
                            for (int j = 0; j < we.Response.Headers.Count; j++)
                            {
                                AddLog("WebException: header {0}={1}", we.Response.Headers.Keys[j], we.Response.Headers[j]);

                            }
                        }
                    }
                }                

                if (responses.Count != 0)
                {
                    threadTitle = responses[0].ThreadTitle;
                    this.Text = string.Format("SpeechCast - {0}", threadTitle);
                }

                if (updated)
                {
                    // JavaScriptでの差分追加処理
                    if (webBrowser.Document != null)
                    {
                        for (int idx = oldResCount; idx < responses.Count; idx++)
                        {
                            Object[] objArray = new Object[1];
                            objArray[0] = responses[idx].Html;
                            webBrowser.Document.InvokeScript("addRes", objArray);
                        }
                    }                    
                }
                return result;
        }

        private List<Response> responses = new List<Response>();

        public void AddLog(string format, params object[] args)
        {
            string str = string.Format(format, args);

            AddLog(str);
        }

        private void AddLog(string str)
        {
            DateTime dtNow = DateTime.Now;
            if (UserConfig.OutputDebugLog)
            {
                Trace.WriteLine(dtNow.ToString() + ":" + str);
                textBoxLog.AppendText(str + "\r\n");
            }
        }

        private void listViewResponses_Click(object sender, EventArgs e)
        {
            Response res = GetSelectedResonse();

            if (res != null)
            {
                webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(res.Number));
            }
        }

        public SoundPlayer soundPlayerNewResponse = null;

        private void PlaySoundNewResponse()
        {
            if (UserConfig.NewResponseSoundFilePathes.Count > 0 && UserConfig.PlaySoundNewResponse)
            {
                Random cRandom = new System.Random();
                var soundIdx = cRandom.Next(UserConfig.NewResponseSoundFilePathes.Count);
                var NewResponseSoundFilePath = UserConfig.NewResponseSoundFilePathes[soundIdx];
                //System.Console.WriteLine(NewResponseSoundFilePath);
                //System.Console.WriteLine(soundIdx);
                if (File.Exists(NewResponseSoundFilePath))
                {
                    try
                    {
                        //MP3対応したかったが、同期再生が出来ずに挫折。誰か実装してくれ
                        //WMPLib.WindowsMediaPlayer mp = new WMPLib.WindowsMediaPlayer(); /* WMP */
                        //mp.URL = NewResponseSoundFilePath; /* 再生したい音声ファイルのパス */
                        //mp.settings.volume = UserConfig.PlayVolume;
                        //if (UserConfig.PlaySoundNewResponseSync)
                        //{
                        //    mp.controls.play();
                        //    while((int)mp.playState == 1) /* 停止しているか判定 */
                        //    {
                        //        System.Threading.Thread.Sleep(1000);
                        //    }
                        //    mp.close();　/* 音声ファイルを切り替える前に使うといいらしい */
                        //}
                        //else
                        //{
                        //    mp.controls.play();
                        //}
                        if (soundPlayerNewResponse == null)
                        {
                            soundPlayerNewResponse = new SoundPlayer();
                        }
                        soundPlayerNewResponse.SoundLocation = NewResponseSoundFilePath;
                        soundPlayerNewResponse.Load();

                        if (UserConfig.PlaySoundNewResponseSync)
                        {
                            soundPlayerNewResponse.PlaySync();
                        }
                        else
                        {
                            soundPlayerNewResponse.Play();
                        }
                        soundPlayerNewResponse.Dispose();

                    }
                    catch (Exception ex)
                    {
                        AddLog("PlaySoundNewResponse:{0}", ex.Message);
                    }
                }
            }

        }

        private int GetResponsesScrollY(int no)
        {
            int scrollY = 0;

            if (no >= 1 && no <= responses.Count)
            {
                if (responses[no - 1].ScrollY < 0)
                {
                    string name = string.Format("res{0}", no);

                    foreach (HtmlElement elem in webBrowser.Document.GetElementsByTagName("a"))
                    {
                        if (name == elem.Name)
                        {
                            scrollY = elem.OffsetRectangle.Y;
                            responses[no - 1].ScrollY = scrollY;
                            break;
                        }
                    }
                }
                else
                {
                    scrollY = responses[no - 1].ScrollY;
                }
            }
            return scrollY;
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //ScrollToDocumentEnd();
        }

        private int currentResNumber_ = 0;
        private int NewResponseNumber = 0;

        public int CurrentResNumber
        {
            get
            {
                return this.currentResNumber_;
            }
            set
            {
                this.currentResNumber_ = value;
                this.toolStripStatusLabelResNumber.Text = string.Format("レス番号{0}", value);
            }
        }
        public bool AutoUpdate
        {
            get
            {
                return toolStripButtonAutoUpdate.Checked;
            }

            set
            {
                if (toolStripButtonAutoUpdate.Checked != value)
                {

                    toolStripButtonAutoUpdate.Checked = value;

                    if (value)
                    {
                        Response res = GetSelectedResonse();

                        if (res != null)
                        {
                            CurrentResNumber = res.Number;
                        }
                        else
                        {
                            CurrentResNumber = responses.Count + 1;
                        }

                        StartSpeaking();
                    }
                    else
                    {
                        communicationStatusString = "";
                    }
                }
            }
        }

        private bool timerTickEnabled
        {
            get
            {
                return (this.formSettings == null && !isformThreadsShowed && !isformWriteResponse);
            }
        }

        private Response GetSelectedResonse()
        {
            if (listViewResponses.SelectedItems.Count != 0)
            {
                Response res = listViewResponses.SelectedItems[0].Tag as Response;

                return res;
            }
            return null;
        }

        private void StartSpeaking(int resNumber)
        {
            StopSpeaking();

            CurrentResNumber = resNumber;
            StartSpeaking();
        }

        private bool isSpeaking = false;
        private string speakingText = "";
        private bool isSpeakingWarningMessage = false;
        private void StartSpeaking()
        {
            speakClipboard = false;

            if (CurrentResNumber <= 0)
            {
                CurrentResNumber = 1;
            }

            if (CurrentResNumber <= responses.Count && CurrentResNumber <= Response.MaxResponseCount)
            {
                Response res = responses[CurrentResNumber - 1];

                string text = res.Text;

                if (UserConfig.IncludesNGWord(res.RawText))
                {
                    text = "(NG Word)";
                }

                if (UserConfig.SpeaksResNumber)
                {
                    text = string.Format("レス{0}\n{1}", res.Number, text);
                }

                isSpeakingWarningMessage = false;
                isSpeaking = true;
                StartSpeaking(text);
                listViewResponses.Items[CurrentResNumber - 1].Selected = true;
                //webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(CurrentResNumber));
            }
            else if (openNextThread == OpenNextThread)
            {
                openNextThread = SpeakAnnounce;
                speakingText = string.Format("次スレ候補、{0}を開きます。", threadTitle);

                replacementIndices = null;
                isSpeakingWarningMessage = true;
                FormCaption.Instance.IsAAMode = false;
                synthesizer.Rate = UserConfig.SpeakingRate;
                isSpeaking = true;
                synthesizer.SpeakAsync(speakingText);
            }
            else if (CurrentResNumber > Response.MaxResponseCount && openNextThread == ReadThread)
            {
                speakingText = string.Format("レス{0}を超えました。\nこれ以上は表示できません。\n次スレを立ててください。", Response.MaxResponseCount);

                replacementIndices = null;
                isSpeakingWarningMessage = true;
                FormCaption.Instance.IsAAMode = false;
                synthesizer.Rate = UserConfig.SpeakingRate;
                isSpeaking = true;
                synthesizer.SpeakAsync(speakingText);
            }
            else
            {
                if(isSpeaking)speakingCompletedTime = System.DateTime.Now;
                isSpeaking = false;
            }
        }

        bool speakClipboard = false;

        public const string LONG_SENTENCE_STRING = "\n(長文のため以下省略)";

        public static Regex RegexBetweenBraces = new Regex("「(.+?)」", RegexOptions.None);

        public void StartSpeaking(string text)
        {
            speakingText = text;
            bool isAAMode = false;

            pronounciationText = UserConfig.ReplacePronouncationWords(text, out replacementIndices);

            if (!isSpeakingWarningMessage)
            {
                if (pronounciationText.Length >= UserConfig.AAModeTextLength)
                {
                    isAAMode = true;
                }
                else if (UserConfig.IncludeAAConditionText(text))
                {
                    isAAMode = true;
                }
            }

            if (isAAMode)
            {
                //AAモード

                if (UserConfig.SpeakTextBetweenBracesEvenIfAAMode)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Match m in RegexBetweenBraces.Matches(text))
                    {
                        sb.AppendLine(m.Groups[1].Value);
                    }
                    pronounciationText = sb.ToString();
                }
                else
                {
                    pronounciationText = "";
                }

                if (UserConfig.SpeaksResNumberWhenAAMode)
                {
                    pronounciationText = string.Format("レス{0}\n", CurrentResNumber) + pronounciationText;
                }
                pronounciationText += UserConfig.SpeakingTextWhenAAMode;
            }
            else
            {
                //通常モード
                int max = UserConfig.MaxSpeakingCharacterCount;
                if (pronounciationText.Length > max)
                {
                    pronounciationText = pronounciationText.Substring(0, max);
                    pronounciationText += LONG_SENTENCE_STRING;


                    if (max < replacementIndices.Count)
                    {
                        int len = replacementIndices[max];

                        if (len < speakingText.Length)
                        {
                            speakingText = speakingText.Substring(0, len);
                        }
                    }

                    speakingText += LONG_SENTENCE_STRING;

                    int startIndex = speakingText.Length;
                    for (int i = 0; i < LONG_SENTENCE_STRING.Length; i++)
                    {
                        replacementIndices.Add(startIndex++);
                    }
                }
            }

            FormCaption.Instance.IsAAMode = isAAMode;
            FormCaption.Instance.CaptionText = "";
            if ((responses.Count - CurrentResNumber) < UserConfig.TurboThreshold || UserConfig.TurboMode == false)
            {
                synthesizer.Rate = UserConfig.SpeakingRate;
            }
            else
            {
                synthesizer.Rate = UserConfig.TurboSpeakingRate;
            }
            if (UserConfig.SpeakMode)
            {
                synthesizer.Volume = UserConfig.SpeakingVolume;
            }else{
                synthesizer.Volume = 0;
            }
            if (CurrentResNumber == NewResponseNumber) PlaySoundNewResponse();
            synthesizer.SpeakAsync(MMFrame.Text.Language.Japanese.ToKatakanaFromKatakanaHalf(pronounciationText));
        }

        public void StopSpeaking()
        {
            StopSpeakingCore();
            AutoUpdate = false;
        }

        private void StopSpeakingCore()
        {
            FormCaption.Instance.CaptionText = "";
            synthesizer.SpeakAsyncCancelAll();

            this.Enabled = false; //UIをOFF
            try
            {
                while (isSpeaking)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(1);
                }
            }
            finally
            {
                this.Enabled = true;
            }            
            isSpeaking = false;
        }

        private List<int> replacementIndices;
        private string pronounciationText;

        private void toolStripButtonCaption_Click(object sender, EventArgs e)
        {
            FormCaption.Instance.Visible = !FormCaption.Instance.Visible;
            toolStripButtonCaption.Checked = FormCaption.Instance.Visible;
            UserConfig.CaptionVisible = FormCaption.Instance.Visible;
        }

        private void toolStripButtonBorder_Click(object sender, EventArgs e)
        {
            FormCaption.Instance.BorderVisible = !FormCaption.Instance.BorderVisible;

            toolStripButtonBorder.Checked = FormCaption.Instance.BorderVisible;
        }

        private void listViewResponses_DoubleClick(object sender, EventArgs e)
        {
            Response res = GetSelectedResonse();

            if (res != null)
            {
                CurrentResNumber = res.Number;

                if (synthesizer.State == SynthesizerState.Ready)
                {
                    StartSpeaking();
                }
            }
        }

        private void toolStripButtonAutoUpdate_Click(object sender, EventArgs e)
        {
            if (AutoUpdate == false)
            {
                StopSpeakingCore();
            }
            AutoUpdate = !AutoUpdate;
        }
        private bool endWebRequest = true;
        private int orgWidth;
        private int orgHeight;
        private int openNextThread;
        // openNextThreadのフラグ定義
        // レス読み上げ中
        private const int ReadThread = 0;
        // 次スレ発見
        private const int OpenNextThread = 1;
        // 次スレへ移動中(移動中アナウンス管理用)
        private const int SpeakAnnounce = 2;

        private TimeSpan diff;
        private TimeSpan diffWeb;
        // 読み上げ管理用バックグラウンドプロセス
        private void timer_Tick(object sender, EventArgs e)
        {
            if (UserConfig.EnableAutoScroll) webBrowser.Document.Window.ScrollTo(0, (webBrowser.Document.Body.ScrollTop + UserConfig.AutoScrollSpeed));
            diff = System.DateTime.Now - speakingCompletedTime;
            diffWeb = System.DateTime.Now - gettingWebTime;
            // スレタイトル更新処理
            if (threadTitle.Length > 0)
            {
                FormCaption.Instance.setTitle(threadTitle + " [" + (CurrentResNumber - 1) + "/" + responses.Count + "]");
                FormCaption.Instance.Invalidate();
            }
            // ステータステキスト更新処理
            if (isSpeaking)
            {
                communicationStatusString = string.Format("話し中・・・（{0}/{1}）", CurrentResNumber,responses.Count);
            }
            // 読み上げが止まっていたら、次の読み上げ条件の検索と読み上げ実行
            else
            {
                // 代替テキスト更新処理
                if (diff.TotalMilliseconds >= UserConfig.DefaultCaptinoDispInvervalMillsec)
                { 
                    objDate = System.DateTime.Now;
                    FormCaption.Instance.CaptionText = CaptionTextBuffer;
                }
                // 自動更新がONならば
                if (AutoUpdate)
                {
                    // 別スレッドのレス取得状態の確認
                    if (!this.endWebRequest)
                    {
                        communicationStatusString = string.Format("取得中・・・");
                    }
                    // レス取得が終わっていて、かつ読み上げ対象レスがなければ、次のレス取得までのカウントダウンをステータスに表示
                    else if (CurrentResNumber > responses.Count)
                    {
                        int leftSeconds = (UserConfig.AutoGettingWebInvervalMillsec - (int)diffWeb.TotalMilliseconds) / 1000 + 1;

                        if (leftSeconds < 1)
                        {
                            leftSeconds = 1;
                        }

                        StringBuilder sb = new StringBuilder(leftSeconds);
                        for (int i = 0; i < leftSeconds; i++)
                        {
                            sb.Append("・");
                        }
                        communicationStatusString = string.Format("レス取得まで後{0}秒{1}", leftSeconds, sb.ToString());
                    }
                    else
                    {
                        communicationStatusString = "待機中・・・";
                    }
                }
                // 自動更新がOFFならステータスを消去
                else
                {
                    communicationStatusString = "";
                }
            }
            
            // 読み上げ処理
            if (
                !isSpeaking
                && timerTickEnabled
               )
            {
                // 読み上げボイスステータスの設定
                int speakingInvervalMillsec;
                // 読み上げ間隔の設定
                if ((responses.Count - CurrentResNumber) < UserConfig.TurboThreshold || UserConfig.TurboMode == false)
                {
                    speakingInvervalMillsec = UserConfig.SpeakingInvervalMillsec;
                }
                else
                {
                    speakingInvervalMillsec = UserConfig.TurboSpeakingInvervalMillsec;
                }
                // 
                if (isSpeakingWarningMessage && !UserConfig.AutoOpenNextThread)
                {
                    // 次スレを立てるのを促すアナウンスの場合強制的に20秒間隔にする
                    speakingInvervalMillsec = 20 * 1000;
                }
                else if (FormCaption.Instance.IsAAMode)
                {
                    // AAモードの時のインターバルの設定
                    speakingInvervalMillsec = UserConfig.AAModeInvervalMillsec;
                }

                // 通常レス読み上げ
                if (
                    diff.TotalMilliseconds >= speakingInvervalMillsec
                    && AutoUpdate
                    //&& responses.Count >= CurrentResNumber
                    )
                {
                    StartSpeaking();
                }
            }
        }

        // レス取得用バックグラウンドプロセス
        private async void timerWeb_Tick(object sender, EventArgs e)
        {
            // 自動Webアクセス条件がTrueなら
            if(
                    diffWeb.TotalMilliseconds >= UserConfig.AutoGettingWebInvervalMillsec
                    && AutoUpdate
                    && timerTickEnabled
              ){
                // 自動次スレ取得の条件判別
                if (
                        CurrentResNumber > Response.MaxResponseCount
                        && UserConfig.AutoOpenNextThread
                        && openNextThread == ReadThread
                    )
                {
                    openNextThread = openNextThreadUrl();
                }

                // レス取得の条件判定
                if (
                        responses.Count <= Response.MaxResponseCount
                        && this.endWebRequest
                    )
                {
                    if (UserConfig.CaptionAutoSmall)
                    {
                        orgWidth = FormCaption.Instance.Width;
                        orgHeight = FormCaption.Instance.Height;
                        FormCaption.Instance.Height = FormCaption.Instance.drawRect.Height;
                        FormCaption.Instance.Width = FormCaption.Instance.drawRect.Width;
                    }
                    await GetFromURLNext();
                    if (UserConfig.CaptionAutoSmall)
                    {
                        FormCaption.Instance.Height = orgHeight;
                        FormCaption.Instance.Width = orgWidth;
                    }
                }
            }
        }

        public static UserConfig UserConfig;
        public static Bookmarks Bookmarks;

        private void FormMain_Load(object sender, EventArgs e)
        {
            string userConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpeechCast\Application.cfg");
            string bookmarksConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"SpeechCast\Bookmarks.cfg");

            if (File.Exists(userConfigPath))
            {
                try
                {
                    UserConfig = UserConfig.Deserialize(userConfigPath);

                    UserConfig.SetRectToForm(UserConfig.FormMainRect, this);
                    UserConfig.SetRectToForm(UserConfig.FormCaptionRect, FormCaption.Instance);

                    string[] args = Environment.GetCommandLineArgs();

                    if (args.Length > 1)
                    {
                        toolStripTextBoxURL.Text = args[1];
                        GetFromURL();
                    }
                    else
                    {
                        toolStripTextBoxURL.Text = UserConfig.URL;
                    }

                    for (int cnt = 0; cnt <= 100; cnt++)
                    {
                        comboBoxCaptionNum1.Items.Add(cnt);
                        comboBoxCaptionNum2.Items.Add(cnt);
                    }
                    comboBoxCaptionNum1.SelectedIndex = 0;
                    comboBoxCaptionNum2.SelectedIndex = 0;
                    toolStripButtonClickSound.Checked = UserConfig.NavigationSound;
                    CoInternetSetFeatureEnabled(FEATURE_DISABLE_NAVIGATION_SOUNDS, SET_FEATURE_ON_PROCESS, !UserConfig.NavigationSound);

                    FormCaption.Instance.Visible = UserConfig.CaptionVisible;
                    toolStripButtonAutoNextThread.Checked = UserConfig.AutoOpenNextThread;
                    toolStripButtonCaption.Checked = UserConfig.CaptionVisible;
                    toolStripButtonTurbo.Checked = UserConfig.TurboMode;
                    toolStripButtonSpeech.Checked = UserConfig.SpeakMode;
                    toolStripButtonShowCaptionImmediately.Checked = UserConfig.ShowCaptionImmediately;
                    toolStripButtonCaptionAutoSmall.Checked = UserConfig.CaptionAutoSmall;
                    checkBoxClockMilitaryTime.Checked = UserConfig.MilitaryTime;
                    checkBoxShowSecond.Checked = UserConfig.MilitaryTime;
                    toolStripButtonPlaySoundNewResponse.Checked = UserConfig.PlaySoundNewResponse;
                    this.splitContainerResCaption.SplitterDistance = 2000;
                    toolStripButtonAutoScroll.Checked = UserConfig.EnableAutoScroll;
                }
                catch (Exception ex)
                {
                    AddLog("Deserialze error :{0}", ex.Message);
                    UserConfig = new UserConfig(userConfigPath);
                }
            }
            else
            {
                UserConfig = new UserConfig(userConfigPath);
                UserConfig.Initialize();
            }


            if (File.Exists(bookmarksConfigPath))
            {
                try
                {
                    Bookmarks = Bookmarks.Deserialize(bookmarksConfigPath);
                    UpdateBookmarkMenu();
                }
                catch (Exception ex)
                {
                    AddLog("Deserialze error :{0}", ex.Message);
                    Bookmarks = new Bookmarks(bookmarksConfigPath);
                }
            }
            else
            {
                Bookmarks = new Bookmarks(bookmarksConfigPath);
            }


            int selectedIndex = 0;
            int idx = toolStripComboBoxSelectVoice.Items.IndexOf(UserConfig.VoiceName);
            if (idx >= 0)
            {
                selectedIndex = idx;
            }

            if (selectedIndex < toolStripComboBoxSelectVoice.Items.Count)
            {
                toolStripComboBoxSelectVoice.SelectedIndex = selectedIndex;
            }

            selectedIndex = 0;
            toolStripTrackBarVoiceVolume.Value = UserConfig.SpeakingVolume;
            synthesizer.Volume = UserConfig.SpeakingVolume;
            myToolStripPlay.GripStyle = ToolStripGripStyle.Hidden;
            myToolStripUrl.GripStyle = ToolStripGripStyle.Hidden;
            myToolStripVoice.GripStyle = ToolStripGripStyle.Hidden;
            //idx = toolStripComboBoxVolume.Items.IndexOf(UserConfig.SpeakingVolume);
            //if (idx >= 0)
            //{
            //    selectedIndex = idx;
            //}

            //if (selectedIndex < toolStripComboBoxVolume.Items.Count)
            //{
            //    toolStripComboBoxVolume.SelectedIndex = selectedIndex;
            //}
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            UserConfig.SetFormToRect(ref UserConfig.FormMainRect, this);
            UserConfig.SetFormToRect(ref UserConfig.FormCaptionRect, FormCaption.Instance);
            UserConfig.URL = toolStripTextBoxURL.Text;
            UserConfig.Serialize();
            Bookmarks.Serialize();
        }

        FormSettings formSettings = null;

        private void toolStripButtonSettings_Click(object sender, EventArgs e)
        {
            formSettings = new FormSettings();
            try
            {
                formSettings.SetUserConfig(UserConfig);
                if (formSettings.ShowDialog() == DialogResult.OK)
                {
                    formSettings.GetUserConfig(UserConfig);
                    FormCaption.Instance.Refresh();
                    if (soundPlayerNewResponse != null)
                    {
                        soundPlayerNewResponse.Dispose();
                        soundPlayerNewResponse = null;
                    }
                }
            }
            finally
            {
                formSettings = null;
            }
        }

        //private void toolStripComboBoxVolume_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    int vol = (int)toolStripComboBoxVolume.SelectedItem;
        //    UserConfig.SpeakingVolume = vol;
        //    synthesizer.Volume = vol;            
        //}

        FormBBSThreads formBBSThreads = new FormBBSThreads();
        bool isformThreadsShowed = false;

        private void toolStripButtonThreads_Click(object sender, EventArgs e)
        {
            if (ShowFormBBSThreads())
            {
                GetFromURL(false);
                CurrentResNumber = 0;
                isSpeakingWarningMessage = false;
            }
        }

        private bool ShowFormBBSThreads()
        {
            isformThreadsShowed = true;
            try
            {
                if (baseURL != null)
                {
                    AddLog(baseURL);
                    formBBSThreads.BaseURL = baseURL;
                    formBBSThreads.ThreadURL = toolStripTextBoxURL.Text;
                    if (formBBSThreads.ShowDialog() == DialogResult.OK)
                    {
                        toolStripTextBoxURL.Text = formBBSThreads.ThreadURL;
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show("URLを入力してEnterを押してください");
                }
            }
            finally
            {
                isformThreadsShowed = false;
            }
            return false;
        }

        public void ScrollToDocumentEnd()
        {
            if (responses.Count > 0)
            {
                webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(responses.Count));
            }
        }


        private FormWrite formWriteResponse = new FormWrite();

        bool isformWriteResponse = false;

        private void toolStripButtonResponse_Click(object sender, EventArgs e)
        {
            formWriteResponse.IsThreadCreation = false;

            isformWriteResponse = true;
            try
            {
                if (baseURL != null)
                {
                    Communicator.Instance.ThreadURL = this.toolStripTextBoxURL.Text;
                    formWriteResponse.ThreadTitle = this.threadTitle;
                    if (formWriteResponse.ShowDialog() == DialogResult.OK)
                    {
                        AddLog("書き込み結果>>" + Communicator.Instance.ReturnText);
                        GetFromURLNext();
                    }
                }
                else
                {
                    MessageBox.Show("URLを入力してEnterを押してください");
                }
            }
            finally
            {
                isformWriteResponse = false;
            }

        }

        FormBookmarks formBookmarks = new FormBookmarks();
        

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            formBookmarks.Initialize(Bookmarks);            
            formBookmarks.ShowDialog();
            UpdateBookmarkMenu();
            Bookmarks.Serialize();
        }

        private void webBrowser_SizeChanged(object sender, EventArgs e)
        {
            PushAndSetWaitCursor();
            foreach (Response res in responses)
            {
                res.ScrollY = -1;
            }
            PopCursor();
        }


        private void toolStripMenuItemAddBookmark_Click(object sender, EventArgs e)
        {
            FormAddBookmark formAddBookmark = new FormAddBookmark();
            CheckBaseURL();

            PushAndSetWaitCursor();
            formAddBookmark.Initialize(baseURL);
            PopCursor();

            if (formAddBookmark.ShowDialog() == DialogResult.OK)
            {
                Bookmarks.RootFolder.Items.Add(formAddBookmark.GetBookmark());
                UpdateBookmarkMenu();
                Bookmarks.Serialize();
            }
        }

        private void UpdateBookmarkMenu()
        {
            List<ToolStripItem> removedItems = new List<ToolStripItem>();
            bool remove = false;

            foreach (ToolStripItem item in this.toolStripMenuItemBookmarks.DropDownItems)
            {
                if (remove)
                {
                    removedItems.Add(item);
                }
                else
                {
                    if (item is ToolStripSeparator)
                    {
                        remove = true;
                    }
                }
            }

            foreach (ToolStripItem item in removedItems)
            {
                toolStripMenuItemBookmarks.DropDownItems.Remove(item);
            }

            ToolStripItemCollection menuItems = toolStripMenuItemBookmarks.DropDownItems;

            AddBookmarkMenu(menuItems, Bookmarks.RootFolder);
        }

        private void AddBookmarkMenu(ToolStripItemCollection menuItems , Bookmarks.Folder folder)
        {
            foreach (Bookmarks.BookmarkBase bkBase in folder.Items)
            {
                Bookmarks.Bookmark bk = bkBase as Bookmarks.Bookmark;

                if (bk != null)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(bk.Name);
                    menuItem.Tag = bk.URL;
                    menuItem.Click += toolStripMenuItemBookmark_Click;

                    menuItems.Add(menuItem);

                }

                Bookmarks.Folder childFolder = bkBase as Bookmarks.Folder;

                if (childFolder != null)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(childFolder.Name);

                    menuItems.Add(menuItem);

                    AddBookmarkMenu(menuItem.DropDownItems, childFolder);

                }

            }
        }

        private void toolStripMenuItemBookmark_Click(object sender, EventArgs e)
        {
            string url = (string) ((sender as ToolStripMenuItem).Tag);

            toolStripTextBoxURL.Text = url;

            GetFromURL();
        }

        private void toolStripMenuItemFirst_Click(object sender, EventArgs e)
        {
            StartSpeaking(1);
        }

        private void toolStripMenuItemPrev_Click(object sender, EventArgs e)
        {
            if (CurrentResNumber > 1)
            {
                CurrentResNumber--;

                if (isSpeaking == false && CurrentResNumber > 1)
                {
                    CurrentResNumber--;
                }

                StartSpeaking(CurrentResNumber);
            }
        }

        private void toolStripMenuItemNext_Click(object sender, EventArgs e)
        {
            if (CurrentResNumber <= responses.Count)
            {
                if (isSpeaking && CurrentResNumber < responses.Count)
                {
                    CurrentResNumber++;
                }
                StartSpeaking(CurrentResNumber);
            }
        }

        private void toolStripMenuItemLast_Click(object sender, EventArgs e)
        {
            StartSpeaking(responses.Count);
        }

        private void toolStripMenuItemStop_Click(object sender, EventArgs e)
        {
            int resNum = CurrentResNumber;

            if (isSpeaking == false && FormCaption.Instance.CaptionText != "" && resNum > 1)
            {
                resNum--;
            }

            StopSpeaking();
            CurrentResNumber = resNum;
        }

        private void toolStripMenuItemCopyboard_Click(object sender, EventArgs e)
        {
            string text = Clipboard.GetText();

            if (!string.IsNullOrEmpty(text))
            {
                StopSpeaking();
                isSpeaking = true;
                speakClipboard = true;
                isSpeakingWarningMessage = false;
                StartSpeaking(text);
            }
        }

        private void toolStripButtonOpenAsBrowser_Click(object sender, EventArgs e)
        {
            string url = toolStripTextBoxURL.Text;

            if (url.StartsWith("http://"))
            {
                PushAndSetWaitCursor();
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                finally
                {
                    PopCursor();
                }
            }
        }

        private void toolStripButtonOpenURLFromClipboard_Click(object sender, EventArgs e)
        {
            string url = Clipboard.GetText();

            if (url.StartsWith("http://"))
            {
                toolStripTextBoxURL.Text = url;
                GetFromURL();
            }
        }

        private void toolStripTrackBarVoiceVolume_ValueChanged(object sender, EventArgs e)
        {
            UserConfig.SpeakingVolume = toolStripTrackBarVoiceVolume.Value;
            synthesizer.Volume = toolStripTrackBarVoiceVolume.Value;            
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            FormAbout formAbout = new FormAbout();

            formAbout.ShowDialog();
        }

        ToolTip ToolTip1 = new ToolTip();


        void webBrowser_StatusTextChanged(object sender, EventArgs e)
        {
            string statusText = webBrowser.StatusText;

#if DEBUG
            this.toolStripStatusLabelCommunication.Text = statusText;
#endif
            Point pt = this.PointToClient(Control.MousePosition);

            pt.Y += 32;
            if (statusText.StartsWith(Response.AnchorUrl))
            {
                string resNoStr = statusText.Substring(Response.AnchorUrl.Length, statusText.Length - Response.AnchorUrl.Length);
                int resNo = System.Convert.ToInt32(resNoStr);
                resNo--;

                if (resNo >= 0 && resNo < responses.Count)
                {
                    Response res = responses[resNo];


                    string toolTipText = string.Format("{0} {1} {2} {3} {4}\n\n{5}"
                                                        , res.Number
                                                        , res.Name
                                                        , res.MailAddress
                                                        , res.DateTime
                                                        , res.ID
                                                        , res.Text);

                    ToolTip1.Show(toolTipText, this, pt);
                }
            }
            else if (statusText.StartsWith("mailto:"))
            {
                int len = statusText.Length - 7;

                if (len > 0)
                {
                    string str = statusText.Substring(7, len);

                    ToolTip1.Show(str, this, pt);
                }
            }                
            else
            {
                ToolTip1.Hide(this);
            }
        }

        private void toolStripMenuItemGoSupportBBS_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("サポート掲示板に移動します。よろしいですか？", "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                toolStripTextBoxURL.Text = "http://bbs.aristocratism.info/slyman/";
                GetFromURL();
            }
        }

        private void toolStripButtonTurbo_Click(object sender, EventArgs e)
        {
            UserConfig.TurboMode = !UserConfig.TurboMode;
        }

        private void toolStripButtonSpeech_Click(object sender, EventArgs e)
        {
            UserConfig.SpeakMode = !UserConfig.SpeakMode;
        }

        private void toolStripButtonClickSound_Click(object sender, EventArgs e)
        {
            UserConfig.NavigationSound = !UserConfig.NavigationSound;
            CoInternetSetFeatureEnabled(FEATURE_DISABLE_NAVIGATION_SOUNDS, SET_FEATURE_ON_PROCESS, !UserConfig.NavigationSound);

        }

        private void toolStripButtonPlaySoundNewResponse_Click(object sender, EventArgs e)
        {
            UserConfig.PlaySoundNewResponse = !UserConfig.PlaySoundNewResponse;
        }

        private void toolStripStatusLabelCommunication_Click(object sender, EventArgs e)
        {
            if (this.splitContainerResCaption.Panel2.Height > 0)
            {
                this.splitContainerResCaption.SplitterDistance = 2000;
                this.webBrowser.Focus();
            }
            else
            {
                this.splitContainerResCaption.SplitterDistance = this.splitContainerResCaption.Height - 130;
            }
        }

        private void toolStripButtonCaptionAutoSmall_Click(object sender, EventArgs e)
        {
            UserConfig.CaptionAutoSmall = this.toolStripButtonCaptionAutoSmall.Checked;
        }


        private void toolStripButtonAutoNextThread_Click(object sender, EventArgs e)
        {
            UserConfig.AutoOpenNextThread = !UserConfig.AutoOpenNextThread;
        }

        // 次スレ自動オープン機能
        // 現在の検索条件は、「現在のスレタイの最初に出てくる数字(全角半角問わず)に1足した数字をスレタイに含むスレを開く」
        // その他条件は要望があれば追加
        // 案としては、
        // 「現在の条件＋現在のスレより後に立てられたスレに絞り込む」
        // 「現在のスレより後に立てられたスレが1件のみであれば数字の一致を確認しない」
        // 等を想定。そういえばゴミスレはどうすりゃいいんだ・・・
        private int openNextThreadUrl(){
            // 最初の数字検索用の正規表現オブジェクト
            System.Text.RegularExpressions.Regex r =
                new System.Text.RegularExpressions.Regex(
                    @"[0-9]+",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            // スレタイのレス数の削除用正規表現オブジェクト
            System.Text.RegularExpressions.Regex rTitle =
                new System.Text.RegularExpressions.Regex(
                    @"(?<title>\\d+?)\([0-9]+\)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            string nextUrl = null;
            try
            {
                if (baseURL != null && threadTitle != null)
                {
                    // 全角半角変換処理を挟む
                    string searchTitle = zen2han(threadTitle);
                    // 現在のスレタイから連番と思われる部分を抽出
                    System.Text.RegularExpressions.Match m = r.Match(searchTitle);
                    string nextNumber = null;
                    AddLog(searchTitle);
                    if (m.Success)
                    {
                        // 取得した数字をインクリメントして文字列として格納
                        nextNumber = (Int32.Parse(m.Value) + 1).ToString();
                    }
                    //AddLog(nextNumber);
                    // スレッド一覧の取得
                    string subjectURL = baseURL + "subject.txt";
                    //AddLog(subjectURL);
                    System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(subjectURL);
                    FormMain.UserConfig.SetProxy(webReq);
                    string encodingName = null;
                    // 運営サイトごとに文字エンコーディングを取得
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
                    System.Net.HttpWebResponse webRes = null;
                    try
                    {
                        string[] parseSubject;
                        webRes = (System.Net.HttpWebResponse)webReq.GetResponse();
                        gettingWebTime = System.DateTime.Now; //例外が発生した場合、連続してwebアクセスが起こるのを防ぐ
                        using (StreamReader reader = new StreamReader(webRes.GetResponseStream(), Encoding.GetEncoding(encodingName)))
                        {
                            // subject.txtを1行ずつ全件検索
                            while (true)
                            {
                                string s = reader.ReadLine();
                                if (s == null)
                                {
                                    // 全件検索してもHITしなければ、次スレ検索状況をレス読み中のままにする
                                    return ReadThread;
                                }

                                // スレタイを取得
                                parseSubject = s.Replace("<>", ",").Split(',');
                                string searchSubject = zen2han(parseSubject[1]);
                                // スレタイの連番部分を取得
                                System.Text.RegularExpressions.Match m2 = r.Match(searchSubject);
                                if(m2.Success)
                                {
                                    // 連番部分が次スレ連番候補と一致したら
                                    if (m2.Value == nextNumber)
                                    {
                                        // 一致データをメンバ変数に入れてループを抜ける
                                        nextUrl = parseSubject[0];
                                        threadTitle = parseSubject[1];
                                        break;
                                    }
                                }
                            }
                        }
                        System.Text.RegularExpressions.Match m3 = r.Match(nextUrl);
                        string threadId = null;
                        if (m3.Success)
                        {
                            //一致した対象が見つかったときキャプチャした部分文字列を表示
                            threadId = m3.Value;
                        }
                        // スレッドURLを生成
                        string threadUrl = Communicator.Instance.getThreadUrl(baseURL, threadId);
                        if (threadUrl.Length > 0)
                        {
                            // スレッドURLを生成出来たら、URL入力テキストボックスにそのURLをセット、
                            // 現在開いてるDatのURLをクリア
                            // スレタイを更新して、フラグを次スレオープン状態に変更
                            toolStripTextBoxURL.Text = threadUrl;
                            rawURL = null;

                            System.Text.RegularExpressions.Match mtitle = r.Match(parseSubject[1]);
                            if (mtitle.Success) threadTitle = mtitle.Groups["title"].Value;
                            return OpenNextThread;
                        }
                    }
                    catch (Exception e)
                    {
                        AddLog(string.Format("エラーが発生しました:{0}", e.Message + e.StackTrace));
                    }
                }
            }
            catch (Exception e)
            {
                AddLog(string.Format("エラーが発生しました:{0}", e.Message + e.StackTrace));
            }
            return ReadThread;
        }

        // ググって拾っきた全角半角変換スクリプト
        private static string zen2han(string aStr)
        {

            Regex reZen = new Regex(@"[！＃＄％＆（）＊＋，－．／０-９：；＜＝＞？＠Ａ-Ｚ［］＾＿｀ａ-ｚ｛｜｝～]", RegexOptions.Compiled);
            string ret = reZen.Replace(aStr, (m) =>
            {
                return ((char)((int)(m.Value.ToCharArray())[0] - 65248)).ToString();
            });
            return ret.Replace("　", " ");
        }

        // 代替字幕関連の処理
        private void buttonCaptionNum1_Click(object sender, EventArgs e)
        {
            insertTextBox(this.textBoxDefaultCaption, "#1#");
            this.CaptionTextBuffer = this.textBoxDefaultCaption.Text;
        }

        private void buttonCaptionNum2_Click(object sender, EventArgs e)
        {
            insertTextBox(this.textBoxDefaultCaption, "#2#");
            this.CaptionTextBuffer = this.textBoxDefaultCaption.Text;
        }

        private string dateformat = "hh:mm";
        private void buttonCaptionClock_Click(object sender, EventArgs e)
        {
            insertTextBox(this.textBoxDefaultCaption,"#CLOCK#");
            this.CaptionTextBuffer = this.textBoxDefaultCaption.Text;
        }

        private void buttonNum1Dec_Click(object sender, EventArgs e)
        {
            if (comboBoxCaptionNum1.SelectedIndex > 0) comboBoxCaptionNum1.SelectedIndex--;
        }

        private void buttonNum1Inc_Click(object sender, EventArgs e)
        {
            if (comboBoxCaptionNum1.SelectedIndex < (comboBoxCaptionNum1.Items.Count-1)) comboBoxCaptionNum1.SelectedIndex++;
        }

        private void buttonNum2Dec_Click(object sender, EventArgs e)
        {
            if (comboBoxCaptionNum2.SelectedIndex > 0) comboBoxCaptionNum2.SelectedIndex--;
        }

        private void buttonNum2Inc_Click(object sender, EventArgs e)
        {
            if (comboBoxCaptionNum2.SelectedIndex < (comboBoxCaptionNum2.Items.Count - 1)) comboBoxCaptionNum2.SelectedIndex++;
        }

        private void checkBoxClock24_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxClockMilitaryTime.Checked)
            {
                dateformat = dateformat.Replace("hh", "HH");
                UserConfig.MilitaryTime = true;
            }
            else
            {
                dateformat = dateformat.Replace("HH", "hh");
                UserConfig.MilitaryTime = false;
            }
        }

        private void checkBoxShowSecond_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowSecond.Checked)
            {
                dateformat = dateformat.Replace(":ss", "");
                dateformat += (":ss");
                UserConfig.ShowSecond= true;
            }
            else
            {
                dateformat = dateformat.Replace(":ss", "");
                UserConfig.ShowSecond= false;
            }
        }

        private void buttonSpeakCaptionText_Click(object sender, EventArgs e)
        {
            if (CaptionTextBuffer.Replace("#1#", comboBoxCaptionNum1.SelectedIndex.ToString()).Replace("#2#", comboBoxCaptionNum2.SelectedIndex.ToString()).Replace("#CLOCK#", objDate.ToString(dateformat)).Length>0)
                StartSpeaking(CaptionTextBuffer.Replace("#1#", comboBoxCaptionNum1.SelectedIndex.ToString()).Replace("#2#", comboBoxCaptionNum2.SelectedIndex.ToString()).Replace("#CLOCK#", objDate.ToString(dateformat)));
        }

        private void textBoxDefaultCaption_Changed(object sender, EventArgs e)
        {
            CaptionTextBuffer = textBoxDefaultCaption.Text;
        }
        private void insertTextBox(TextBox targetText, string msg)
        {
            if (targetText.SelectedText.Length == 0)
            {
                targetText.SelectedText = msg;
            }
            else {
                targetText.Text = targetText.Text.Substring(0, targetText.SelectionStart)
                + msg
                + targetText.Text.Substring(targetText.SelectionStart + targetText.SelectionLength);
            }
            targetText.SelectionLength = 0;　//なくてもいい
            targetText.SelectionStart = targetText.Text.Length;
        }

        // 字幕瞬間表示のオプション
        private void toolStripButtonShowCaptionImmediately_Click(object sender, EventArgs e)
        {
            UserConfig.ShowCaptionImmediately = toolStripButtonShowCaptionImmediately.Checked;
        }

        public static string ConvertKana(string src)
        {
            return System.Text.RegularExpressions.Regex.Replace(src, "[\uFF61-\uFF9F+]", MatchKanaEvaluator);
        }

        private static string MatchKanaEvaluator(System.Text.RegularExpressions.Match m)
        {
            return Microsoft.VisualBasic.Strings.StrConv(m.Value, Microsoft.VisualBasic.VbStrConv.Wide, 0);
        }

        // イメージビューア起動
        public FormViewResource formViewResource = null;
        private void oepnFormViewNewtabImg(string url)
        {
            if ((formViewResource == null) || formViewResource.IsDisposed)
            {
                formViewResource = new FormViewResource(UserConfig.FormViewToRect);
                formViewResource.Show();
            }
            formViewResource.addTabImg(url);
            formViewResource.Activate();
        }
        private void oepnFormViewNewtabYoutube(string id)
        {
            if ((formViewResource == null) || formViewResource.IsDisposed)
            {
                formViewResource = new FormViewResource(UserConfig.FormViewToRect);
                formViewResource.Show();
            }
            formViewResource.addTabYoutube(id,UserConfig.ViewerYoutubePlayerWidth,UserConfig.ViewerYoutubePlayerHeight);
            formViewResource.Activate();
        }
        // ビューア位置の保存
        public void saveViewerPos(FormViewResource frm)
        {
            UserConfig.SetFormToRect(ref UserConfig.FormViewToRect, frm);
        }

        // オートスクロールのONOFF
        private void toolStripButtonAutoScroll_Click(object sender, EventArgs e)
        {
            UserConfig.EnableAutoScroll = !UserConfig.EnableAutoScroll;
            toolStripButtonAutoScroll.Checked = UserConfig.EnableAutoScroll;
        }

    }
}
