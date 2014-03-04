using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;


namespace SpeechCast
{
    class Communicator
    {
        public static Regex JBBSRegex = new System.Text.RegularExpressions.Regex(@"(http://jbbs.livedoor.jp|http://jbbs.shitaraba.net)/bbs/read.cgi(/(\w+)/(\d+)/(\d+)/)");
        public static Regex YYRegex = new System.Text.RegularExpressions.Regex(@"(http://yy.+\..+|http://bbs\.aristocratism\.info|http://www.+\.atchs\.jp)/.+/read.cgi/(\w+)/(\d+)/");
        public static Regex NichanRegex = new System.Text.RegularExpressions.Regex(@"(http://.+2ch\.net)/.+/read.cgi/(\w+)/(\d+)/");

        public static Regex JBBSBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://jbbs.livedoor.jp|http://jbbs.shitaraba.net)/(\w+)/(\d+)/");
        public static Regex YYBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://yy.+\..+|http://bbs\.aristocratism\.info|http://www.+\.atchs\.jp)/(\w+)/");
        public static Regex NichanBaseRegex = new System.Text.RegularExpressions.Regex(@"(http://.+2ch\.net)/(\w+)/");

        public static Regex htmlBodyRegex = new System.Text.RegularExpressions.Regex("<body.*?>(.*)</body>", RegexOptions.IgnoreCase);

        static public Communicator Instance = new Communicator();

        public string BaseURL = null;
        public string ThreadURL = null;

        public string ReturnText = "";

        private Match GetRegexMatchURL()
        {
            Match m = null;
            switch (Response.Style)
            {
                case Response.BBSStyle.jbbs:
                    m = JBBSRegex.Match(ThreadURL);
                    break;
                case Response.BBSStyle.yykakiko:
                    m = YYRegex.Match(ThreadURL);
                    break;
                case Response.BBSStyle.nichan:
                    m = NichanRegex.Match(ThreadURL);
                    break;
            }
            return m;
        }

        private Match GetRegexMatchBaseURL()
        {
            Match m = null;
            switch (Response.Style)
            {
                case Response.BBSStyle.jbbs:
                    m = JBBSBaseRegex.Match(BaseURL);
                    break;
                case Response.BBSStyle.yykakiko:
                    m = YYBaseRegex.Match(BaseURL);
                    break;
                case Response.BBSStyle.nichan:
                    m = NichanBaseRegex.Match(BaseURL);
                    break;
            }
            return m;
        }

        private Encoding GetEncoding()
        {
            string encodingName = "";

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
            return Encoding.GetEncoding(encodingName);
        }

        private string UrlEncode(string srcText)
        {
            return System.Web.HttpUtility.UrlEncode(srcText, GetEncoding());
        }

        public string GetTitle()
        {
            string title = "";

            System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(BaseURL);
            FormMain.UserConfig.SetProxy(webReq);
            System.Net.HttpWebResponse webRes = null;

            try
            {
                webRes = (System.Net.HttpWebResponse)webReq.GetResponse();

                string returnText;
                System.IO.Stream resStream = webRes.GetResponseStream();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(resStream, GetEncoding()))
                {
                    returnText = sr.ReadToEnd();
                }
                Regex rx = new Regex("<title>(.*?)</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                Match m = rx.Match(returnText);
                if (m.Success)
                {
                    title = HttpUtility.HtmlDecode(m.Groups[1].Value);
                }

            }
            catch (Exception e)
            {
                FormMain.Instance.AddLog(e.Message);
            }
            return title;
        }

        public bool WriteResponse(string name, string mailAddress, string contents)
        {
            return this.CreateThread(null, name, mailAddress, contents);
        }

        public bool CreateThread(string title, string name, string mailAddress, string contents)
        {
            Match m = GetRegexMatchURL();
            string url = null;
            string postData = null;
            string referer = ThreadURL;
            int jbbsBaseIndex = 1;
            bool isThreadCreation = title != null;

            if (!m.Success)
            {
                if (isThreadCreation)
                {
                    m = GetRegexMatchBaseURL();

                    if (!m.Success)
                    {
                        return false;
                    }
                    jbbsBaseIndex = 0;
                    referer = BaseURL;
                }
                else
                {
                    return false;
                }
            }

            CookieContainer cookieContainer = new CookieContainer();


            DateTime writeTime = DateTime.Now;
            DateTime orgTime = DateTime.Parse("1970/1/1 00:00:00");

            int unixTime = (int)((writeTime.ToFileTimeUtc() - orgTime.ToFileTimeUtc()) / 10000000); 

            for (int i = 0; i < 2; i++)
            {
                switch (Response.Style)
                {
                    case Response.BBSStyle.jbbs:
                        {
                            url = string.Format("{0}/bbs/write.cgi", m.Groups[1].Value);

                            string submitText = "書き込む";
                            string additionalParam = "";

                            if (isThreadCreation)
                            {
                                submitText = "新規スレッド作成";
                                additionalParam += "&SUBJECT=" + UrlEncode(title);
                            }
                            else
                            {
                                additionalParam += "&KEY==" + m.Groups[5].Value;
                            }

                            postData = string.Format("DIR={0}&BBS={1}&TIME={2}&NAME={3}&MAIL={4}&MESSAGE={5}&submit={6}" + additionalParam
                                , m.Groups[2 + jbbsBaseIndex].Value
                                , m.Groups[3 + jbbsBaseIndex].Value
                                , unixTime
                                , UrlEncode(name)
                                , UrlEncode(mailAddress)
                                , UrlEncode(contents)
                                , UrlEncode(submitText)
                                );

                            break;
                        }
                    case Response.BBSStyle.yykakiko:
                    case Response.BBSStyle.nichan:
                        {
                            url = string.Format("{0}/test/bbs.cgi", m.Groups[1].Value);

                            string submitText = "書き込む";
                            string additionalParam = "";
                            string nameText = name;
                            string subject = "";

                            if (isThreadCreation)
                            {
                                submitText = "新規スレッド作成";
                                subject = title;
                                additionalParam += "&subject=" + UrlEncode(subject);
                            }
                            else
                            {
                                additionalParam += "&key=" + m.Groups[3].Value;
                            }



                            if (i == 1)
                            {
                                submitText = "上記全てを承諾して書き込む";
                            }

                            if (Response.Style == Response.BBSStyle.nichan)
                            {
                                additionalParam += "&tepo=don";
                            }
                            else
                            {
                                additionalParam += "&MIRV=kakkoii";
                            }



                            postData = string.Format("bbs={0}&time={1}&FROM={2}&mail={3}&MESSAGE={4}&submit={5}" + additionalParam
                                , m.Groups[2].Value
                                , unixTime
                                , UrlEncode(nameText)
                                , UrlEncode(mailAddress)
                                , UrlEncode(contents)
                                , UrlEncode(submitText)
                                );

                            if (i == 1)
                            {
                                url += "?guid=ON";
                            }

                            if (Response.Style == Response.BBSStyle.nichan && isThreadCreation)
                            {
                                referer = url;
                            }
                            break;
                        }
                }

                byte[] postDataBytes = System.Text.Encoding.ASCII.GetBytes(postData);

                System.Net.HttpWebRequest webReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                FormMain.UserConfig.SetProxy(webReq);

                webReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows XP)";

                //Cookieの設定
                webReq.CookieContainer = new CookieContainer();
                webReq.CookieContainer.Add(cookieContainer.GetCookies(webReq.RequestUri));
                //webReq.UserAgent = "Monazilla / 1.00(monaweb / 1.00)";

                //メソッドにPOSTを指定
                webReq.Method = "POST";

                //ContentTypeを"application/x-www-form-urlencoded"にする
                webReq.ContentType = "application/x-www-form-urlencoded";
                //POST送信するデータの長さを指定
                webReq.ContentLength = postDataBytes.Length;
                //
                webReq.Referer = referer;


                System.Net.HttpWebResponse webRes = null;

                try
                {
                    //データをPOST送信するためのStreamを取得
                    using (System.IO.Stream reqStream = webReq.GetRequestStream())
                    {
                        //送信するデータを書き込む
                        reqStream.Write(postDataBytes, 0, postDataBytes.Length);
                    }

                    webRes = (System.Net.HttpWebResponse)webReq.GetResponse();

                    //受信したCookieのコレクションを取得する
                    System.Net.CookieCollection cookies =
                        webReq.CookieContainer.GetCookies(webReq.RequestUri);
                    //Cookie名と値を列挙する
                    //foreach (System.Net.Cookie cook in cookies)
                    //{
                    //    Console.WriteLine("{0}={1}", cook.Name, cook.Value);
                    //}
                    //取得したCookieを保存しておく
                    cookieContainer.Add(cookies);


                    //応答データを受信するためのStreamを取得
                    System.IO.Stream resStream = webRes.GetResponseStream();
                    //受信して表示
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(resStream, GetEncoding()))
                    {
                        ReturnText = sr.ReadToEnd();
                    }

                    if (ReturnText.IndexOf("書き込み確認") >= 0)
                    {
                        //referer = url;
                        continue;
                    }

                    string temp = ReturnText.Replace("\n", "");
                    m = htmlBodyRegex.Match(temp);

                    if (m.Success)
                    {
                        ReturnText = Response.ConvertToText(m.Groups[1].Value);
                    }



                    if (ReturnText.IndexOf("ＥＲＲＯＲ") >= 0 || ReturnText.IndexOf("ERROR") >= 0)
                    {
                        return false;
                    }


                    return true;
                }
                catch (Exception e)
                {
                    ReturnText = e.Message;
                    return false;
                }
            }

            return false;
        }

        public string getThreadUrl(string baseURL,string ThreadID)
        {
            Match m = Communicator.JBBSBaseRegex.Match(baseURL);
            string ThreadURL;
            if (m.Success)
            {
                ThreadURL = string.Format("{0}/bbs/read.cgi/{1}/{2}/{3}/"
                    , m.Groups[1].Value
                    , m.Groups[2].Value
                    , m.Groups[3].Value
                    , ThreadID);
                return ThreadURL;
            }
            m = Communicator.YYBaseRegex.Match(baseURL);
            if (m.Success)
            {
                ThreadURL = string.Format("{0}/test/read.cgi/{1}/{2}/"
                    , m.Groups[1].Value
                    , m.Groups[2].Value
                    , ThreadID);
                return ThreadURL;
            }
            m = Communicator.NichanBaseRegex.Match(baseURL);
            if (m.Success)
            {
                ThreadURL = string.Format("{0}/test/read.cgi/{1}/{2}/"
                    , m.Groups[1].Value
                    , m.Groups[2].Value
                    , ThreadID);
                return ThreadURL;
            }
            return "";
        }
    
    }
}
