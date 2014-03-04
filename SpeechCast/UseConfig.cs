using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SpeechCast
{
    public class UserConfig
    {
        public UserConfig()
        {
        }

        public UserConfig(string filePath)
        {
            filePath_ = filePath;
        }

        public void Initialize()
        {
            Pronounciations.AddRange(new string[]
                {
                    @"(http|ttp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)/リンク",
                    @"ww+/ワラワラ",
                    @"ｗｗ+/ワラワラ",
                    @"(w|ｗ)\s*$/ワラ",
                    @"(w|ｗ)\s*\n/ワラ",
                    @"～/ー",
                    @"下手/ヘタ",
                    @"リア充/リアジュウ",
                    @"今年/コトシ",
                    @"今日/キョウ",
                    @"今回/コンカイ",
                    @"今月/コンゲツ",
                    @"今週/コンシュウ",
                    @"今/イマ",
                    @"(tktk|ｔｋｔｋ)/テケテケ",
                    @"(wktk|ｗｋｔｋ)/ワクテカ",
                    @"(gdgd|ｇｄｇｄ)/グダグダ",
                    @"(gthm|ｇｔｈｍ)/ガチホモ",
                    @"(ktkr|ｋｔｋｒ)/キタコレ",
                    @"(kwsk|ｋｗｓｋ)/クワシク",
                    @"あああ+/アー",
                    @"ううう+/ウー",
                    @"えええ+/エー",
                    @"いいい+/イー",
                    @"おおお+/オー",
                    @"(\(笑\)|（笑）)/ワラ",
                    @"(wiki|ｗｉｋｉ)/ウィキ/i",
                    @"(wiki|ｗｉｋｉ)/ウィキ/i",
                    @"（ｒｙ|\(ry/イカリャク",
                    @"厨/チュウ",
                    @"正直/ショウジキ",

                }
            );
            AAModeConditions.AddRange(new string[]
                {
                    @"д",
                    @"（●）",
                    @"从",
                    @"）)ﾉヽ",
                    @"∀",
                    @"<●>",
                    @"（__人__）",
                }

            );
            SupplementSettings();
        }

        /// <summary>
        /// 設定されていなかったら、保管する
        /// </summary>
        public void SupplementSettings()
        {
            if (string.IsNullOrEmpty(this.EditorFilePath))
            {
                string editorFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"..\notepad.exe");

                editorFilePath = System.IO.Path.GetFullPath(editorFilePath);

                if (System.IO.File.Exists(editorFilePath))
                {
                    EditorFilePath = editorFilePath;
                }
            }

            if (CaptionFont.IsInitialized == false)
            {
                CaptionFont.Colors.HighForeColor = System.Drawing.Color.White;
                CaptionFont.Colors.LowForeColor = System.Drawing.Color.FromArgb(151, 151, 255);
                CaptionFont.Colors.BorderColor = System.Drawing.Color.Navy;
                CaptionFont.Colors.TransparentColor = System.Drawing.Color.FromArgb(8, 8, 8);
            }

            if (AACaptionFont.IsInitialized == false)
            {
                AACaptionFont.Colors.HighForeColor = System.Drawing.Color.White;
                AACaptionFont.Colors.LowForeColor = System.Drawing.Color.FromArgb(192, 255, 192);
                AACaptionFont.Colors.BorderColor = System.Drawing.Color.FromArgb(0, 64, 0);
                AACaptionFont.Colors.TransparentColor = System.Drawing.Color.FromArgb(8, 8, 8);
            }

        }

        static private string filePath_;

        /// <summary>
        /// メインフォームのウィンドウ位置
        /// </summary>
        public Rectangle FormMainRect;

        /// <summary>
        /// 字幕フォントのウィンドウ位置
        /// </summary>
        public Rectangle FormCaptionRect;

        /// <summary>
        /// 読み上げONOFF
        /// </summary>
        public bool SpeakMode = false;

        /// <summary>
        /// リロード音ONOFF
        /// </summary>
        public bool NavigationSound = true;
        
        /// <summary>
        /// 読み上げ間隔
        /// </summary>
        public int SpeakingInvervalMillsec = 3000;

        /// <summary>
        /// 読み上げ間隔(ターボモード)
        /// </summary>
        public int TurboSpeakingInvervalMillsec = 500;

        /// <summary>
        /// AAモードで字幕が消える時間
        /// </summary>
        public int AAModeInvervalMillsec = 6000;

        /// <summary>
        /// 読み上げ速度
        /// </summary>
        public int SpeakingRate = 0;

        /// <summary>
        /// 読み上げ速度(ターボモード)
        /// </summary>
        public int TurboSpeakingRate = 4;

        /// <summary>
        /// ターボモードを実行するレスキュー数
        /// </summary>
        public int TurboThreshold = 3;

        public bool TurboMode = false;
        /// <summary>
        /// 読み上げボリューム
        /// </summary>
        public int SpeakingVolume = 100;

        /// <summary>
        /// 音楽ファイル再生ボリューム
        /// </summary>
        public int PlayVolume = 100;

        /// <summary>
        /// レス番号を読み上げる？
        /// </summary>
        public bool SpeaksResNumber = true;

        /// <summary>
        /// AAモード時にレス番号を読み上げる？
        /// </summary>
        public bool SpeaksResNumberWhenAAMode = true;

        /// <summary>
        /// AAモード時に固定で読み上げる文字列
        /// </summary>
        public string SpeakingTextWhenAAMode = "アスキーアート";

        /// <summary>
        /// WEB自動更新間隔
        /// </summary>
        public int AutoGettingWebInvervalMillsec = 10000;

        /// <summary>
        /// 次スレ自動開始
        /// </summary>
        public bool AutoOpenNextThread = false;

        /// <summary>
        /// 文字数制限
        /// </summary>
        public int MaxSpeakingCharacterCount = 100;

        /// <summary>
        /// NGワード
        /// </summary>
        public List<string> NGWords = new List<string>();

        /// <summary>
        /// 読み方リスト
        /// </summary>
        public List<string> Pronounciations = new List<string>();

        /// <summary>
        /// AAモードになる条件の含まれる文字列リスト
        /// </summary>
        public List<string> AAModeConditions = new List<string>();

        /// <summary>
        /// AAモードになる条件の文字数
        /// </summary>
        public int AAModeTextLength = 200;

        /// <summary>
        /// AAモードの時でも「」の中は読み上げる
        /// </summary>
        public bool SpeakTextBetweenBracesEvenIfAAMode = true;

        /// <summary>
        /// AAモードのときウィンドウ枠の幅にあわせてフォントを縮小する
        /// </summary>
        public bool ReduceFontSizeByWindowWidth = true;

        /// <summary>
        /// AAモードのときウィンドウ枠の高さにあわせてフォントを縮小する
        /// </summary>
        public bool ReduceFontSizeByWindowHeight = true;


        /// <summary>
        /// GZip圧縮を使うか？
        /// </summary>
        public bool GZipCompressionEnabled = true;

        /// <summary>
        /// 字幕ウィンドウ表示するか？
        /// </summary>
        public bool CaptionVisible = false;


        /// <summary>
        /// 字幕ウィンドウのタイトル(SpeechCast)を隠すか？
        /// </summary>
        public bool HideCaptionTitle = false;

        /// <summary>
        /// インターネットオプションで設定したプロキシを使用？
        /// </summary>
        public bool UseDefaultProxy = true;

        /// <summary>
        /// 字幕を瞬間表示する？
        /// </summary>
        public bool ShowCaptionImmediately = false;

        /// <summary>
        /// 新着レスがあったときサウンドを鳴らす？
        /// </summary>
        public bool PlaySoundNewResponse = false;

        /// <summary>
        /// サウンド再生終了後に、読み上げを開始する
        /// </summary>
        public bool PlaySoundNewResponseSync = false;

        /// <summary>
        /// 新着レスファイル
        /// </summary>
        public List<string> NewResponseSoundFilePathes = new List<string>();

        /// <summary>
        /// エディターのファイルパス
        /// </summary>
        public string EditorFilePath = "";

        public void UpdatePronounciations()
        {
            replacedStrings = null;
        }

        /// <summary>
        /// URL
        /// </summary>
        public string URL = "";

        /// <summary>
        /// ボイス名
        /// </summary>
        public string VoiceName = "VW Misaki";

        /// <summary>
        /// 字幕フォント
        /// </summary>
        public CaptionFont CaptionFont = new CaptionFont();

        /// <summary>
        /// AA字幕フォント
        /// </summary>
        public CaptionFont AACaptionFont = new CaptionFont();

        /// <summary>
        /// 字幕インデントONOFF
        /// </summary>
        //public bool CaptionIndent = true;

        /// <summary>
        /// 字幕表示位置調整
        /// </summary>
        public int CaptionIndentTopPadding = 0;
        public int CaptionIndentLeftPadding = 0;

        /// <summary>
        /// 字幕の時計表記設定
        /// </summary>
        public bool MilitaryTime = false;
        public bool ShowSecond = false;
        /// <summary>
        /// 字幕表示エリア自動縮小(ネットアクセスタイムアウト時に字幕が塗りつぶされる場合の暫定対応)
        /// </summary>
        public bool CaptionAutoSmall;

        /// <summary>
        /// レス書き込み画面での名前
        /// </summary>
        public string FormWriteName = "";

        /// <summary>
        /// レス書き込み画面でのメールアドレス
        /// </summary>
        public string FormWriteMailAddress = "";

        /// <summary>
        /// レス書き込み画面でのメールアドレスをsageにする
        /// </summary>
        public bool FormWriteSage;


        public void Serialize()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(UserConfig));

            string dirPath = System.IO.Path.GetDirectoryName(filePath_);

            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            CaptionFont.Colors.Serialize();
            AACaptionFont.Colors.Serialize();
            CaptionFont.IsInitialized = true;
            AACaptionFont.IsInitialized = true;

            using (System.IO.FileStream fs = new System.IO.FileStream(filePath_, System.IO.FileMode.Create))
            {
                serializer.Serialize(fs, this);
            }
        }

        public static void SetRectToForm(Rectangle rect, Form form)
        {
            form.Top = rect.Top;
            form.Height = rect.Height;
            form.Left = rect.Left;
            form.Width = rect.Width;
        }

        public static void SetFormToRect(ref Rectangle rect, Form form)
        {
            rect.Y = form.Top;
            rect.Height = form.Height;
            rect.X = form.Left;
            rect.Width = form.Width;
        }


        public static UserConfig Deserialize(string filePath)
        {
            UserConfig userConfig;

            filePath_ = filePath;

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(UserConfig));
            using (System.IO.FileStream fs = new System.IO.FileStream(filePath_, System.IO.FileMode.Open))
            {
                userConfig = (UserConfig) serializer.Deserialize(fs);
            }

            userConfig.CaptionFont.Colors.Deserialize();
            userConfig.AACaptionFont.Colors.Deserialize();
            userConfig.SupplementSettings();
            return userConfig;
        }

        public bool IncludesNGWord(string text)
        {
            foreach (string ngword in NGWords)
            {
                if (text.IndexOf(ngword) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private List<int> replacementIndices;
        private Dictionary<Regex, string> replacedStrings = null;

        private void CreateReplacedStrings()
        {
            if (replacedStrings == null)
            {
                replacedStrings = new Dictionary<Regex, string>();

                foreach (string pro in Pronounciations)
                {
                    bool backSlash = false;
                    string regexString = null;
                    string regexOption = "";
                    for (int i = 0; i < pro.Length; i++)
                    {
                        if (backSlash == false && pro[i] == '/')
                        {
                            regexString = pro.Substring(0, i);
                            newText = pro.Substring(i + 1);
                            int idx = newText.IndexOf('/');

                            if (idx >= 0 && idx < newText.Length - 1)
                            {
                                regexOption = newText.Substring(idx + 1, newText.Length - idx - 1);
                                newText = newText.Substring(0, idx);
                            }
                            break;
                        }

                        if (pro[i] == '\\')
                        {
                            backSlash = true;
                        }
                        else
                        {
                            backSlash = false;
                        }
                    }

                    if (newText != null)
                    {
                        try
                        {
                            RegexOptions opt = RegexOptions.None;

                            for (int i = 0; i < regexOption.Length; i++)
                            {
                                char c = char.ToLower(regexOption[i]);

                                switch (c)
                                {
                                    case 'i':
                                        opt |= RegexOptions.IgnoreCase;
                                        break;
                                    case 's':
                                        opt |= RegexOptions.Singleline;
                                        break;
                                    case 'm':
                                        opt |= RegexOptions.Multiline;
                                        break;

                                }
                            }

                            Regex rx = new Regex(regexString, opt);

                            replacedStrings.Add(rx, newText);
                        }
                        catch //(Exception e)
                        {
                        }
                    }
                }
            }
        }

        public bool IncludeAAConditionText(string text)
        {
            foreach (string word in this.AAModeConditions)
            {
                if (text.IndexOf(word, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public string ReplacePronouncationWords(string text, out List<int> indices)
        {
            CreateReplacedStrings();

            speakingText = text;
            replacementIndices = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                replacementIndices.Add(i);
            }

            foreach (KeyValuePair<Regex, string> kvp in replacedStrings)
            {
                Regex rx = kvp.Key;
                newText = kvp.Value;

                matches.Clear();
                text = rx.Replace(text, matchDelegate);

                matches.Reverse();
                foreach (Match match in matches)
                {
                    int index = match.Index;

                    int endIndex = match.Index + match.Length;

                    if (endIndex >= replacementIndices.Count)
                    {
                        endIndex = replacementIndices.Count - 1;
                    }

                    int start = replacementIndices[endIndex] - 1;


                    for (int i = 0; i < match.Length; i++)
                    {
                        if (index < replacementIndices.Count)
                        {
                            replacementIndices.RemoveAt(index);
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (int i = 0; i < newText.Length; i++)
                    {
                        if (replacementIndices.Count - 1 > index)
                        {
                            replacementIndices.Insert(index, start);
                        }
                        else
                        {
                            replacementIndices.Add(start);
                        }
                    }
                }                
            }
        

            indices = replacementIndices;
            return text;
        }

        public void SetProxy(System.Net.HttpWebRequest webRequest)
        {
            if (UseDefaultProxy)
            {
                webRequest.Proxy = System.Net.WebRequest.DefaultWebProxy;
            }
            else
            {
                webRequest.Proxy = null;
            }
        }

        public System.Net.IWebProxy getProxy()
        {
            if (UseDefaultProxy)
            {
                return System.Net.WebRequest.DefaultWebProxy;
            }
            else
            {
                return null;
            }
        }

        List<Match> matches = new List<Match>();
        private string speakingText;
        private string newText;
        private string matchDelegate(Match match)
        {
            matches.Add(match);
            return newText;
        }
    }
}
