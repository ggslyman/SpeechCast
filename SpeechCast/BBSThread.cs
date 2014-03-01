using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpeechCast
{
    class BBSThread
    {
        public string Text = null;
        public string RawText = null;
        public string ResponseCount = null;
        public string ThreadID = null;
        public string Power = "";

        static Regex jbbsRegex = new Regex(@"^(\d+).cgi,(.+)\((\d+)\)");
        static Regex nichanRegex = new Regex(@"^(\d+).dat<>(.+)\((\d+)\)");


        public ListViewItem CreateListViewItem()
        {
            ListViewItem item = new ListViewItem(this.Text);
            item.SubItems.Add(this.ResponseCount);
            item.SubItems.Add(this.Power);
            item.Tag = this;
            return item;
        }

        public bool SetRawText(string rawText)
        {
            this.RawText = rawText;
            bool success = false;

            switch (Response.Style)
            {
                case Response.BBSStyle.jbbs:
                    {
                        Match m = jbbsRegex.Match(rawText);

                        if (m.Success)
                        {
                            Text = m.Groups[2].Value;
                            ResponseCount = m.Groups[3].Value;
                            ThreadID = m.Groups[1].Value;
                            success = true;
                        }
                        break;
                    }
                case Response.BBSStyle.yykakiko:
                case Response.BBSStyle.nichan:
                    {
                        Match m = nichanRegex.Match(rawText);

                        if (m.Success)
                        {
                            Text = m.Groups[2].Value;
                            ResponseCount = m.Groups[3].Value;
                            ThreadID = m.Groups[1].Value;
                            success = true;
                        }
                        break;
                    }

            }

            if (success)
            {
                DateTime orgTime = DateTime.Parse("1970/1/1 00:00:00");

                double unixTime = (double)((DateTime.Now.ToFileTimeUtc() - orgTime.ToFileTimeUtc()) / 10000000) - System.Convert.ToDouble(ThreadID); 
                if (unixTime > 0.01f)
                {
                    double power = System.Convert.ToDouble(ResponseCount) / unixTime * 60.0f * 60.0f * 24.0f;

                    Power = power.ToString("#0.0");
                }
            }


            return success;
        }


    }
}
