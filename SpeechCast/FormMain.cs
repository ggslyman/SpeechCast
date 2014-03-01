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
        [DllImport("urlmon.dll"), PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(int FeatureEntry, [In, MarshalAs(UnmanagedType.U4)]uint dwFlags, bool fEnable);
        private const uint SET_FEATURE_ON_PROCESS = 0x00000002;
        private const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;

        public FormMain()
        {
#if DEBUG
            //出力ファイルを指定して、StreamWriterオブジェクトを作成
            StreamWriter sw = new StreamWriter("debug.log");
            //自動的にフラッシュされるようにする
            sw.AutoFlush = true;
            //スレッドセーフラッパを作成
            TextWriter tw = TextWriter.Synchronized(sw);
            //DefaultTraceListenerが必要なければ削除する
            Trace.Listeners.Remove("Default");
            //名前を LogFile としてTextWriterTraceListenerオブジェクトを作成
            TextWriterTraceListener twtl =
                new TextWriterTraceListener(tw, "LogFile");
            //リスナコレクションに追加する
            Trace.Listeners.Add(twtl);
#endif

            InitializeComponent();

            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                toolStripComboBoxSelectVoice.Items.Add(voice.VoiceInfo.Name);
            }

            synthesizer.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synthesizer_SpeakProgress);
            synthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synthesizer_SpeakCompleted);

            Instance = this;

            FormCaption.Instance = new FormCaption();

            this.webBrowser.StatusTextChanged += new EventHandler(webBrowser_StatusTextChanged);
            this.webBrowser.Navigating += new WebBrowserNavigatingEventHandler(webBrowser_Navigating);
        }

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
            else if (url.StartsWith("http:"))
            {
                System.Diagnostics.Process.Start(url);
                e.Cancel = true;
            }
        }


        static public FormMain Instance;


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

                if (CurrentResNumber <= Response.MaxResponseCount && speakClipboard == false)
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

        private void GetFromURL()
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
                GetFromURL(false);
            }
            catch (Exception ex)
            {
                AddLog("Exception: {0}", ex.Message);
            }
            AutoUpdate = false;
        }

        private void GetFromURLNext()
        {
            this.endWebRequest = false;
            if (!GetFromURL(true))
            {
                if (needsRetry)
                {
                   GetFromURL(false);
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
        Regex yyBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://(yy.+\.60\.kg|yy.+\.kakiko\.com|bbs\.aristocratism\.info)/\w+/$)");

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

        private bool GetFromURL(bool next)
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
            }

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            communicationStatusString = "通信中・・・・";
            PushAndSetWaitCursor();
            try
            {
                int oldResponseCount = responses.Count;

                System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                webReq.KeepAlive = false;
                FormMain.UserConfig.SetProxy(webReq);

                if (UserConfig.GZipCompressionEnabled && useRangeHeader == false)
                {
                    webReq.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                }
#if DEBUG
                AddLog("datSize={0} lastModifiedTime={1} useRangeHeader={2}",
                    datSize, lastModifiedDateTime.ToLongTimeString(), useRangeHeader);
#endif
                if (useRangeHeader)
                {
                    webReq.AddRange(datSize - 1);
                    webReq.IfModifiedSince = lastModifiedDateTime;
                }

                System.Net.HttpWebResponse webRes = null;

                gettingWebTime = System.DateTime.Now; //例外が発生した場合、連続してwebアクセスが起こるのを防ぐ

                long responseTime = 0, readTime = 0, listViewTime = 0, documetnTime = 0, encodingTime = 0, setTime = 0;
                try
                {
                    stopWatch.Start();
                    //サーバーからの応答を受信するためのWebResponseを取得
                    webReq.Timeout=10000;
                    webRes = (System.Net.HttpWebResponse)webReq.GetResponse();

                    lastModifiedDateTime = webRes.LastModified;
                    responseTime = stopWatch.ElapsedMilliseconds;
                    //if (useRangeHeader)
                    //{
                    //    throw (new Exception(" 416 "));
                    //}
                }
                catch (Exception e)
                {
                    if (e.Message.IndexOf("304") < 0)
                    {
                        AddLog("GetResponse Exception: {0}", e.Message);
                    }

                    if (e.Message.IndexOf("416") >= 0)
                    {
                        //autoUpdate = false;
                        //
                        //MessageBox.Show(this, "多分、あぼ～んされています。Enterを押して再取得してください。");
                        AddLog("誰かのレスがあぼ～んされてるかも？　全レス再取得します。");
                        needsRetry = true;
                    }

                    return false;
                }

                try
                {
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

                            string datDirName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ,"Logs");

                            if(!Directory.Exists(datDirName))
                            {
                                Directory.CreateDirectory(datDirName);
                            }
                            string fullPath = Path.Combine(datDirName,debugDatFileName);
                            FileMode fileMode = FileMode.Create;

                            if (File.Exists(fullPath) && next)
                            {
                                fileMode = FileMode.Append;
                            }

                            using (FileStream fs = new FileStream(fullPath, fileMode))
                            {
                                Byte[] bytes = new Byte[memStream.Length - 1];
                                memStream.Read(bytes, 0, (int)memStream.Length - 1);
                                fs.Write(bytes,0,bytes.Length);
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
                                PlaySoundNewResponse();
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
                
                if (responses.Count != 0)
                {
                    threadTitle = responses[0].ThreadTitle;
                    this.Text = string.Format("SpeechCast - {0}", threadTitle);
                }

                if (updated)
                {
                    string[] htmlStrings = new string[responses.Count];

                    int i = 0;
                    foreach (Response res in responses)
                    {
                        htmlStrings[i] = res.Html;
                        i++;
                    }

                    string html = string.Format("<html><body>{0}</body></html>"
                                        , string.Concat(htmlStrings)
                                        );

                    try
                    {
                        webBrowser.DocumentText = html;

                        documetnTime = stopWatch.ElapsedMilliseconds;

#if DEBUG
                        AddLog("Elasped res={0} read={1} enc={2} list={3} doc={4} set={5}",
                            responseTime, readTime, encodingTime, listViewTime, documetnTime, setTime);
#endif
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        AddLog("webBrowser Exception: {0}", ex.Message);
                    }
                }
            }
            finally
            {
                PopCursor();
                communicationStatusString = "";
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
            Trace.WriteLine(dtNow.ToString()+":"+str);
            textBoxLog.AppendText(str + "\r\n");
        }

        private void listViewResponses_Click(object sender, EventArgs e)
        {
            Response res = GetSelectedResonse();

            if (res != null)
            {
                webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(res.Number));
            }
        }

        private SoundPlayer soundPlayerNewResponse = null;

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
            ScrollToDocumentEnd();
        }

        private int currentResNumber_ = 0;

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
            isSpeaking = true;


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
                StartSpeaking(text);
                listViewResponses.Items[CurrentResNumber - 1].Selected = true;
                webBrowser.Document.Window.ScrollTo(0, GetResponsesScrollY(CurrentResNumber));
            }
            else if (CurrentResNumber > Response.MaxResponseCount)
            {
                speakingText = string.Format("レス{0}を超えました。\nこれ以上は表示できません。\n次スレを立ててください。", Response.MaxResponseCount);

                replacementIndices = null;
                isSpeakingWarningMessage = true;
                FormCaption.Instance.IsAAMode = false;
                synthesizer.Rate = UserConfig.SpeakingRate;
                synthesizer.SpeakAsync(speakingText);
            }
            else
            {
                speakingCompletedTime = System.DateTime.Now;
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
            synthesizer.SpeakAsync(pronounciationText);
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
        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan diff = System.DateTime.Now - speakingCompletedTime;
            TimeSpan diffWeb = System.DateTime.Now - gettingWebTime;
            if (threadTitle.Length>0) {
                FormCaption.Instance.setTitle(threadTitle + "[" + (CurrentResNumber - 1) + "/" + responses.Count + "]");
                FormCaption.Instance.Invalidate();
            }
            if (isSpeaking)
            {
                communicationStatusString = string.Format("話し中・・・（{0}/{1}）", CurrentResNumber,responses.Count);
            }
            else
            {
                if (AutoUpdate)
                {

                    if (CurrentResNumber > responses.Count)
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
                else
                {
                    communicationStatusString = "";
                }
            }
            
            int speakingInvervalMillsec;
            if ((responses.Count - CurrentResNumber) < UserConfig.TurboThreshold || UserConfig.TurboMode == false)
            {
                speakingInvervalMillsec = UserConfig.SpeakingInvervalMillsec;
            }
            else
            {
                speakingInvervalMillsec = UserConfig.TurboSpeakingInvervalMillsec;
            }

            if (isSpeakingWarningMessage)
            {
                speakingInvervalMillsec = 20 * 1000;
            }
            else if (FormCaption.Instance.IsAAMode)
            {
                speakingInvervalMillsec = UserConfig.AAModeInvervalMillsec;
            }
            if (isSpeaking == false && timerTickEnabled)
            {
                if (diff.TotalMilliseconds >= speakingInvervalMillsec)
                {
                    if (CurrentResNumber <= responses.Count && AutoUpdate)
                    {
                        StartSpeaking();
                    }
                    else if (!this.richTextBoxDefaultCaption.Focused)
                    {
                        FormCaption.Instance.CaptionText = this.richTextBoxDefaultCaption.Text;
                    }else{
                        FormCaption.Instance.CaptionText = "";
                    }
                }


                if (CurrentResNumber > responses.Count && AutoUpdate && diffWeb.TotalMilliseconds >= UserConfig.AutoGettingWebInvervalMillsec)
                {
                    if (CurrentResNumber <= Response.MaxResponseCount)
                    {
                        if (this.endWebRequest) {
                            orgWidth = FormCaption.Instance.Width;
                            orgHeight = FormCaption.Instance.Height;
                            FormCaption.Instance.Height = FormCaption.Instance.drawRect.Height;
                            FormCaption.Instance.Width = FormCaption.Instance.drawRect.Width;
                            GetFromURLNext();
                            FormCaption.Instance.Height = orgHeight;
                            FormCaption.Instance.Width = orgWidth;

                        }
                    }
                    else
                    {
                        if (diff.TotalMilliseconds >= speakingInvervalMillsec)
                        {
                            StartSpeaking();
                        }
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

                    toolStripButtonClickSound.Checked = UserConfig.NavigationSound;
                    CoInternetSetFeatureEnabled(FEATURE_DISABLE_NAVIGATION_SOUNDS, SET_FEATURE_ON_PROCESS, !UserConfig.NavigationSound);

                    FormCaption.Instance.Visible = UserConfig.CaptionVisible;
                    toolStripButtonCaption.Checked = UserConfig.CaptionVisible;
                    toolStripButtonTurbo.Checked = UserConfig.TurboMode;
                    toolStripButtonSpeech.Checked = UserConfig.SpeakMode;
                    this.splitContainer3.SplitterDistance = 2000;
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

        private void toolStripTrackBarPlayVolume_ValueChanged(object sender, EventArgs e)
        {
            UserConfig.PlayVolume = toolStripTrackBarPlayVolume.Value;
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
            if (this.splitContainer3.Panel2.Height > 0)
            {
                this.splitContainer3.SplitterDistance = 2000;
                this.webBrowser.Focus();
            }
            else
            {
                this.splitContainer3.SplitterDistance = this.splitContainer3.Height - 130;
            }
        }

    }
}
