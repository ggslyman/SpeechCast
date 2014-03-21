using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SpeechCast
{
    class Response
    {
        public string Text = null;
        public string RawText = null;
        public string Html = null;

        public int Number = -1;
        public string Name = null;
        public string MailAddress = null;
        public string DateTime = null;
        public string ThreadTitle = null;
        public string ID = null;
        public int ScrollY = -1;
        public static BBSStyle Style;
        public const string AnchorUrl = "http://res/";

        public static int MaxResponseCount
        {
            get
            {
                switch (Style)
                {
                    case BBSStyle.jbbs:
                        return 1000;
                    case BBSStyle.yykakiko:
                    case BBSStyle.nichan:
                        return 1001;
                }
                return 1000;
            }
        }


        //static Regex jbbsRegex = new Regex(@"^(\d+)<>(.*)<>(.*)<>(.+)<>(.*)<>(.*)<>(.*)");
        //static Regex yyRegex = new Regex(@"^(.+)<>(.*)<>(.*)\s*(.*)<>(.*)<>(.*)");

        public enum BBSStyle
        {
            /// <summary>
            /// jbbs(したらば)形式
            /// </summary>
            jbbs,
            /// <summary>
            /// yyかきこ形式
            /// </summary>
            yykakiko,
            /// <summary>
            /// 2ch形式
            /// </summary>
            nichan,
        }

        public Response()
        {
        }

        static string[] delimStrings = new string[] { "<>" };
        static string[] idStrings = new string[] { " ID:" };

        public bool SetRawText(string rawText)
        {
            this.RawText = rawText;

            switch (Style)
            {
                case BBSStyle.jbbs:
                    {
                        string[] values = rawText.Split(delimStrings, StringSplitOptions.None);


                        if (values.Length > 6)
                        {
                            this.Name = ConvertToText(values[1]);
                            this.MailAddress = ConvertToText(values[2]);
                            this.DateTime = values[3];
                            this.Text = ConvertToText(values[4]);
                            this.ThreadTitle = values[5];
                            this.ID = values[6];
                            try
                            {
                                this.Number = System.Convert.ToInt32(values[0]);
                            }
                            catch
                            {
                            }
                            //HTMLコンバートは最後に行う
                            this.Html = ConvertToHtml(values[4]);

                            return true;
                        }
                    }

                    break;
                case BBSStyle.yykakiko:
                case BBSStyle.nichan:
                    {
                        string[] values = rawText.Split(delimStrings, StringSplitOptions.None);

                        if (values.Length > 4)
                        {
                            this.Name = ConvertToText(values[0]);
                            this.MailAddress = ConvertToText(values[1]);

                            string dateTime = values[2];
                            string id = "";

                            string[] strs = dateTime.Split(idStrings, StringSplitOptions.None);

                            if (strs.Length > 1)
                            {
                                dateTime = strs[0];
                                id = strs[1];
                            }

                            this.DateTime = dateTime;
                            this.Text = ConvertToText(values[3]);
                            this.ThreadTitle = values[4];
                            this.ID = id;

                            //HTMLコンバートは最後に行う
                            this.Html = ConvertToHtml(values[3]);
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        public ListViewItem CreateListViewItem()
        {
            ListViewItem item = new ListViewItem();

            item.Text = Number.ToString();
            item.SubItems.Add(Name);
            item.SubItems.Add(Text);
            item.Tag = this;
            return item;
        }


        /// <summary>
        /// 空にする
        /// </summary>
        public void SetEmpty()
        {
            this.Name = "＜削除＞";
            this.Text = "＜削除＞";
            this.RawText = "";
        }



        static Regex toTextRegexBr = new Regex("<br>", RegexOptions.IgnoreCase);
        static Regex toTextRegexHref = new Regex("<a\\s+href=\".+?\".*?>(.+?)</a>", RegexOptions.IgnoreCase);
        static Regex toTextRegexFont = new Regex("<font.+?>(.+?)</font>", RegexOptions.IgnoreCase);
        static Regex toTextRegexUl = new Regex("<ul>(.+?)</ul>", RegexOptions.IgnoreCase);
        static Regex toTextRegexB = new Regex("<b>(.*?)</b>", RegexOptions.IgnoreCase);
        //static Regex toTextRegexRevB = new Regex("</b>(.*?)<b>", RegexOptions.IgnoreCase);
        static Regex toTextRegexComment = new Regex("<!--.+?-->", RegexOptions.IgnoreCase);
        static Regex toTextRegexScript = new Regex("<script.*?>.*?</script>", RegexOptions.IgnoreCase);
        static Regex toTextRegexHr = new Regex("<hr.*?>", RegexOptions.IgnoreCase);
        static Regex toTextRegexDiv = new Regex("<div.*?>.*?</div>", RegexOptions.IgnoreCase);

        static Regex toHtmlRegexGtGt = new Regex(@"&gt;&gt;(\d+)", RegexOptions.IgnoreCase);
        static Regex toHtmlRegexUrl = new Regex(@"(http|ttp)(s)?(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)");

        static MatchEvaluator toTextMatchEvalGroupRemoveLink = new MatchEvaluator(ReplaceGroupRemoveLink);
        static MatchEvaluator toTextMatchEvalGroupAddLink = new MatchEvaluator(ReplaceGroupResAnchor);
        static MatchEvaluator toTextMatchEvalGroupResAnchor = new MatchEvaluator(ReplaceGroupAddLink);
        //static MatchEvaluator toTextMatchEvalGroupReverseBold = new MatchEvaluator(ReplaceGroupReverseBold);

        public static string ConvertToText(string content)
        {
            content = toTextRegexComment.Replace(content, "");
            content = toTextRegexScript.Replace(content, "");
            content = toTextRegexHr.Replace(content, "");
            content = toTextRegexDiv.Replace(content, "");
            content = toTextRegexHref.Replace(content, toTextMatchEvalGroupRemoveLink);
            content = toTextRegexFont.Replace(content, toTextMatchEvalGroupRemoveLink);
            content = toTextRegexB.Replace(content, toTextMatchEvalGroupRemoveLink);
            //content = toTextRegexRevB.Replace(content, toTextMatchEvalGroupReverseBold);
            content = toTextRegexUl.Replace(content, toTextMatchEvalGroupRemoveLink);
            content = toTextRegexBr.Replace(content, "\n");
            try
            {
                content = HttpUtility.HtmlDecode(content);
            }
            catch //(Exception ex)
            {
                //Decodeできない文字列がある?
                //AddLog("cannot decode:{0}", content);
            }

            return content;
        }

        private string ConvertToHtml(string content)
        {
            content = toTextRegexHref.Replace(content, toTextMatchEvalGroupRemoveLink); //リンクははずす
            content = toHtmlRegexUrl.Replace(content, toTextMatchEvalGroupResAnchor); //リンクをつける
            content = toHtmlRegexGtGt.Replace(content, toTextMatchEvalGroupAddLink); //レスアンカー

            //string id = "ID:" + this.ID;

            //string headerHtml = string.Format("<a name=\"res{0}\"</a><table border=0 bgcolor=\"#CCCCCC\"><tr><td><font color=\"#000000\">{0}: <a href=\"mailto:{1}\">{2}</a>: {3} {4} </font></td></tr></table>"
            //    , this.Number
            //    , this.MailAddress
            //    , this.Name
            //    , this.DateTime
            //    , id);

            StringBuilder sb = new StringBuilder();
            sb.Append("<a name=\"res");
            sb.Append(this.Number);
            sb.Append("\"></a><table border=0 bgcolor=\"#CCCCCC\"><tr><td><font color=\"#000000\">");
            sb.Append(this.Number);
            sb.Append(": ");
            if (this.MailAddress != "")
            {
                sb.Append("<a href=\"mailto:");
                sb.Append(this.MailAddress);
                sb.Append("\"><b>");
                sb.Append(this.Name);
                sb.Append("</b></a>: ");
            }
            else
            {
                sb.Append("<b>");
                sb.Append(this.Name);
                sb.Append("</b>: ");
            }
            sb.Append(this.DateTime);
            sb.Append(" ID:");
            sb.Append(this.ID);
            sb.Append("  </font></td></tr></table>");

            sb.Append("<blockquote>");
            sb.Append(content);
            sb.Append("</blockquote>");

            string txt = sb.ToString();
            return txt;
        }

        private static string ReplaceGroupRemoveLink(Match m)
        {
            return m.Groups[1].Value;
        }

        private static string ReplaceGroupResAnchor(Match m)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<a href=\"http://res/");
            sb.Append(m.Groups[1].Value);
            sb.Append("\">");
            sb.Append(m.Groups[0].Value);
            sb.Append("</a>");

            return sb.ToString();
        }

        private static string ReplaceGroupAddLink(Match m)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<a href=\"http");
            sb.Append(m.Groups[3].Value);
            sb.Append("\">");
            //sb.Append("\" target=\"_blank\">");
            sb.Append(m.Groups[0].Value);
            sb.Append("</a>");

            return sb.ToString();
        }

        //private static string ReplaceGroupReverseBold(Match m)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    sb.Append("<b>");
        //    sb.Append(m.Groups[1].Value);
        //    sb.Append("</b>");

        //    return sb.ToString();
        //}
        
    }
}
