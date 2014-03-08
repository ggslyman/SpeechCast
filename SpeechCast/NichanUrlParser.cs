using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Web;

namespace NichanUrlParser
{
    public class NichanUrlParser
    {
        // <summary>
        // 設定ファイル名定義
        // </summary>
        private static string settingFile = "parserSetting.xml";
        // <summary>
        // 設定ファイル解析用オブジェクト
        // </summary>
        private static System.Xml.XmlDocument xml = new System.Xml.XmlDocument();

        // <summary>
        // 解析元URL
        // </summary>
        private string parseUrl = "";
        // <summary>
        // XML設定ファイル上の識別ID(現状、処理には使っていない)
        // </summary>
        private int bbsId = 0;
        // <summary>
        // したらば、わいわいなどの運営サイト名
        // </summary>
        private string bbsType = "";
        // <summary>
        // 板名
        // </summary>
        private string bbsName = "";
        // <summary>
        // 板URL
        // </summary>
        private string baseUrl = "";
        // <summary>
        // スレッドIDを除いた全スレッド共通部分のURL
        // </summary>
        private string threadRootUrl = "";
        // <summary>
        // 現在開いているスレッド名
        // </summary>
        private string threadName = "";
        // <summary>
        // 現在開いているスレッドURL
        // </summary>
        private string threadUrl = "";
        // <summary>
        // 現在開いているスレッドID
        // </summary>
        private string threadId = "";
        // <summary>
        // 現在開いているスレッドのdatファイルURL
        // </summary>
        private string datUrl = "";
        // <summary>
        // subject.txtの区切り文字
        // </summary>
        private string subjectDelimStrings = "";
        // <summary>
        // datファイルの区切り文字
        // </summary>
        private string datDelimStrings = "";
        // <summary>
        // スレッドルートURLの変換用フォーマット
        // </summary>
        private string threadRootUrlFormat = "";
        // <summary>
        // datファイルの文字エンコード
        // </summary>
        private string encoding = "";
        // <summary>
        // datファイルの正規表現パーシング定義
        // </summary>
        private string datRegex = "";
        // <summary>
        // 板INDEXの文字エンコード(ほぼあっとちゃんねるず用)
        // </summary>
        private string bbsEncoding = "";

        // <summary>
        // subject.txtの内容を格納するコレクション
        // </summary>
        private ObservableCollection<Subject> listSubjects = new ObservableCollection<Subject>();
        // <summary>
        // スレッドのレス内容を格納するコレクション
        // </summary>
        private ObservableCollection<ThreadLine> listTreadLines = new ObservableCollection<ThreadLine>();

        // 取得するURLの設定関数
        public void setUrl(string url)
        {
            parseUrl = url;
        }

        
        // <summary>
        // コンストラクタ
        // </summary>
        public NichanUrlParser()
        {
            if (System.IO.File.Exists(@settingFile))
            {
                try
                {
                    Console.WriteLine("読込OK");
                    xml.PreserveWhitespace = true;
                    xml.Load(settingFile);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                throw new NichanUrlParserException("エラーが出たよ");
            }
        }

        //以下各種URLのゲッター
        public string BaseUrl
        {
            get { return baseUrl; }
        }
        public string ThreadRootUrl
        {
            get { return threadRootUrl; }
        }
        public string ThreadUrl
        {
            get { return threadUrl; }
        }
        public string ThreadId
        {
            get { return threadId; }
        }
        public string DatUrl
        {
            get { return datUrl; }
        }
        public string ThreadName
        {
            get { return threadName; }
        }

        public string DatRegex
        {
            set { datRegex = value; }
            get { return datRegex; }
        }
        // 現在のURLのメタデータ取得
        public string getBbsType
        {
            get { return bbsType; }
        }
        public string BbsName
        {
            get { return bbsName; }
        }

        // <summary>
        // サブジェクトリストを返すゲッター
        // </summary>
        public ObservableCollection<Subject> ListSubjects
        {
            get { return listSubjects; }
        }
        // <summary>
        // スレッドラインリストを返すゲッター
        // </summary>
        public ObservableCollection<ThreadLine> ListTreadLines
        {
            get { return listTreadLines; }
        }

        
        // セッティングURLの生出力(デバッグ用)
        public string getParseSettingText()
        {
            return xml.InnerXml;//テキストエリアにタグごとXMLを表示
        }

        // subject.txtのパーサパターン
        private static string threadTitleRegex = "(?<id>\\d+?)\\.\\w+...*{0}(?<title>.*?)\\((?<resCount>\\d+?)\\)$";


        // URLパースのログを文字列で返す(デバッグ用)
        public string getSetUrlsLog(){
            return bbsId + ":" + bbsType + "\n" + baseUrl + "\n" + threadUrl + "\n" + threadRootUrl + "\n";
        }

        public async Task setNamesAndSubjectsAsync()
        {
            await setBbsNameAsync();
            await setThreadNameAsync();
            await setSubjectsAsync();
        }
        // <summary>
        // 板名の取得
        // </summary>
        public async Task setBbsNameAsync()
        {
            if (baseUrl != "")
            {
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                { 
                    bbsName = "";
                    // スレトップをすべて読み込み板名を取得
                    Stream stream = await httpClient.GetStreamAsync(baseUrl);
                    string baseHtml = "";
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(bbsEncoding)))
                    {
                        while (!reader.EndOfStream)
                        {
                            baseHtml += reader.ReadLine();
                        }
                    }
                    // タイトルタグを正規表現パース
                    string titleRegex = "<title>(?<title>.*?)</title>";
                    Regex retitleRegex = new Regex(titleRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    for (Match m = retitleRegex.Match(baseHtml); m.Success; m = m.NextMatch())
                    {
                        bbsName = m.Groups["title"].Value.Trim();
                    }
                }
            }
        }

        // <summary>
        // スレ一覧の取得
        // </summary>
        public async Task setSubjectsAsync()
        {
            if (baseUrl != "")
            {
                // subject.txtを読み込み、スレタイを取得
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    Stream stream = await httpClient.GetStreamAsync(baseUrl + "subject.txt");
                    Regex titleRegex = new Regex(string.Format(threadTitleRegex, subjectDelimStrings), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(encoding)))
                    {
                        if (!reader.EndOfStream)
                        {
                            listSubjects.Clear();
                            var subjectOrder = 1;
                            while (!reader.EndOfStream)
                            {
                                // 1行ごとに正規表現でスレタイを取得
                                var subLine = reader.ReadLine();
                                Match m = titleRegex.Match(subLine);
                                if (m.Success)
                                {
                                    Subject bufSubject = new Subject();
                                    bufSubject.Order = subjectOrder++;
                                    bufSubject.Title = m.Groups["title"].Value;
                                    bufSubject.Url = threadRootUrl + m.Groups["id"].Value + "/";
                                    bufSubject.ResCount = Int32.Parse(m.Groups["resCount"].Value);
                                    DateTime orgTime = DateTime.Parse("1970/1/1 00:00:00");
                                    double unixTime = (double)((DateTime.Now.ToFileTimeUtc() - orgTime.ToFileTimeUtc()) / 10000000) - System.Convert.ToDouble(m.Groups["id"].Value);
                                    if (unixTime > 0.01f)
                                    {
                                        double power = System.Convert.ToDouble(bufSubject.ResCount) / unixTime * 60.0f * 60.0f * 24.0f;
                                        bufSubject.Power = power;
                                    }
                                    listSubjects.Add(bufSubject);
                                    if (m.Groups["id"].Value == threadId) threadName = m.Groups["title"].Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        // <summary>
        // 現在のスレタイの取得
        // </summary>
        public async Task setThreadNameAsync()
        {
            if (threadId != "")
            {
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    threadName = "";
                    Regex titleRegex = new Regex(string.Format(threadTitleRegex, subjectDelimStrings), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    // subject.txtを読み込み、スレタイを取得
                    Stream stream = await httpClient.GetStreamAsync(baseUrl + "subject.txt");
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(encoding)))
                    {
                        while (!reader.EndOfStream)
                        {
                            // 1行ごとに正規表現でスレタイを取得
                            var subLine = reader.ReadLine();
                            Match m = titleRegex.Match(subLine);
                            if (m.Success)
                            {
                                if (m.Groups["id"].Value == threadId)
                                {
                                    threadName = m.Groups["title"].Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        // <summary>
        // 現在のオブジェクトにスレッドURLが設定されているときに、datを取得してコレクションに追加する関数
        // 参考URL：http://www.studyinghttp.net/range
        // 上記URLを参考にdatの差分取得をする
        // Gzipの時はどうなる？
        // 差分取得について：http://sonson.jp/?p=541
        // </summary>
        public async Task getThreadLines()
        {
            if (datUrl != "")
            {
                using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient())
                {
                    // 
                    Regex rdatRegex = new Regex(datRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    // subject.txtを読み込み、スレタイを取得
                    Stream stream = await httpClient.GetStreamAsync(datUrl);
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.GetEncoding(encoding)))
                    {
                        DateTime dt;
                        int idx = 1;
                        while (!reader.EndOfStream)
                        {
                            string sDatetime = "";
                            // 1行ごとに正規表現でthreadLineの各要素を取得
                            var datLine = reader.ReadLine();
                            Match m = rdatRegex.Match(datLine);
                            if (m.Success)
                            {
                                ThreadLine threadLine = new ThreadLine();
                                threadLine.Name = m.Groups["name"].Value;
                                threadLine.Url = m.Groups["url"].Value;
                                if (Int32.Parse(m.Groups["date"].Value.Substring(0,1)) > 7)
                                {
                                    sDatetime = "19" + m.Groups["date"].Value + " " + m.Groups["time"].Value;
                                }
                                else
                                {
                                    sDatetime = "20" + m.Groups["date"].Value + " " + m.Groups["time"].Value;
                                }
                                dt = DateTime.Parse(sDatetime);
                                threadLine.Date = DateTime.Parse(sDatetime);
                                threadLine.Body = stritpTag(HttpUtility.HtmlDecode(m.Groups["body"].Value.Replace("<br>", "\n")));
                                listTreadLines.Add(threadLine);
                            }
                            idx++;
                        }
                    }
                }
            }
        }
        
        // <summary>
        // オブジェクトに設定されたURLから各コンテンツへアクセスするURLやパーシング用パラメータを生成
        // baseUrl              板INDEXのURL
        // threadRootUrl        スレッドのURLからスレッドID部分を除いたもの
        // threadI              スレッドの識別IDL
        // subjectDelimString   subject.txtの1行内の項目区切り文字
        // datDelimStrings      datファイルの1行内の項目区切り文字
        // subjectDelimString   subject.txtの1行区切り文字
        // threadRootUrlFormat  スレッドURLを生成するためのフォーマット(string.Format準拠)
        // </summary>
        public bool setUrls()
        {
            string baseUrlRegex = "";
            string threadUrlRegex = "";
            string datUrlFormat = "";
            baseUrl = "";
            threadRootUrl = "";
            threadUrl = "";
            threadName = "";
            bbsName = "";
            datUrl = "";
            bbsName = "";
            bbsType = "";
            encoding = "";
            bbsEncoding = "";
            threadId = "";
            subjectDelimStrings = "";
            datDelimStrings = "";
            threadRootUrlFormat = "";
            listTreadLines.Clear();
            bool ret = false;
            // 設定ファイルからドメイン判別正規表現を読み込み、渡されたURLのチェック
            using (System.Xml.XmlNodeList eDomain = xml.GetElementsByTagName("domainRegex"))
            {
                for (int i = 0; i < eDomain.Count; i++)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(
                        parseUrl,
                        @eDomain[i].InnerText,
                        System.Text.RegularExpressions.RegexOptions.ECMAScript))
                    {
                        try
                        {
                            // マッチした設定の定義を読み込み
                            System.Xml.XmlNode eBbs = eDomain[i].ParentNode;
                            bbsId = Int32.Parse(eBbs.SelectSingleNode("id").InnerText);
                            bbsType = eBbs.SelectSingleNode("type").InnerText;
                            baseUrlRegex = eBbs.SelectSingleNode("baseUrlRegex").InnerText;
                            threadUrlRegex = eBbs.SelectSingleNode("threadUrlRegex").InnerText;
                            datUrlFormat = eBbs.SelectSingleNode("datUrlFormat").InnerText;
                            encoding = eBbs.SelectSingleNode("encoding").InnerText;
                            bbsEncoding = eBbs.SelectSingleNode("bbsEncoding").InnerText;
                            subjectDelimStrings = eBbs.SelectSingleNode("subjectDelimStrings").InnerText;
                            datDelimStrings = eBbs.SelectSingleNode("datDelimStrings").InnerText;
                            threadRootUrlFormat = eBbs.SelectSingleNode("threadRootUrlFormat").InnerText;
                            datRegex = eBbs.SelectSingleNode("datRegex").InnerText;
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine("設定ファイルが正しくありません。" + e.Message);
                        }catch
                        {
                            System.Console.WriteLine("設定ファイルが正しくありません。");
                        }

                            // URL種別の判別とクラスオブジェクト変数へのURL保存

                        // スレッドURLパースのRegexオブジェクトを作成
                        System.Text.RegularExpressions.Regex rThread =
                            new System.Text.RegularExpressions.Regex(
                                @threadUrlRegex,
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        System.Text.RegularExpressions.Match mThread = rThread.Match(parseUrl);
                        if (mThread.Success)
                        {
                            baseUrl = mThread.Groups[1].Value + mThread.Groups[2].Value + "/";
                            threadUrl = mThread.Groups[0].Value;
                            threadId = mThread.Groups[3].Value;
                            threadRootUrl = string.Format(threadRootUrlFormat, mThread.Groups[1].Value, mThread.Groups[2].Value);
                            datUrl = string.Format(datUrlFormat, mThread.Groups[1].Value, mThread.Groups[2].Value, mThread.Groups[3].Value);
                            ret = true;
                        }
                        else
                        {
                            // ベースURLパースのRegexオブジェクトを作成
                            System.Text.RegularExpressions.Regex rBase =
                                new System.Text.RegularExpressions.Regex(
                                    @baseUrlRegex,
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            System.Text.RegularExpressions.Match mBase = rBase.Match(parseUrl);
                            if (mBase.Success)
                            {
                                //一致した対象が見つかったときキャプチャした部分文字列を表示
                                baseUrl = mBase.Groups[1].Value + mBase.Groups[2].Value + "/";
                                threadRootUrl = string.Format(threadRootUrlFormat, mBase.Groups[1].Value, mBase.Groups[2].Value);
                                ret = true;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public static string stritpTag(string str)
        {
            return Regex.Replace(str, "<.*?>", String.Empty);
        }

        public void clog(string msg)
        {
#if DEBUG
            System.Console.WriteLine(msg);
#endif
        }
    }
    class NichanUrlParserException : System.ApplicationException
    {
        // メッセージの始めにクラス名を付けてみただけ。
        public NichanUrlParserException(string msg)
            : base("NichanUrlParserException : " + msg)
        {
        }

    }
    // <summary>
    // subject.txtを1行に相当するクラス
    // </summary>
    public class Subject
    {
        private string title;
        private string url;
        private int resCount;
        private double power;
        private int order;
        public int Order
        {
            get { return order; }
            set { order = value; }
        }
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        public int ResCount
        {
            get { return resCount; }
            set { resCount = value; }
        }
        public double Power
        {
            get { return power; }
            set { power = value; }
        }
    }
    // <summary>
    // datファイルの1行に相当するクラス
    // </summary>
    public class ThreadLine
    {
        private int number;
        private string name;
        private string url;
        private DateTime date;
        private string body;
        private string id;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        public DateTime Date
        {
            set { date = value; }
        }
        public string DateFormat
        {
            get { return date.ToString(); }
        }
        public string Body
        {
            get { return body; }
            set { body = value; }
        }
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
    }
}
