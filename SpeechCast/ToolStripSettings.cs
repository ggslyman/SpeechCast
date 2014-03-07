using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class ToolStripManager2
{
    internal class ToolStripSettings
        : System.Configuration.ApplicationSettingsBase
    {
        /// <summary>
        /// ToolStripPanelにあるToolStripを列に分けて保存する
        /// </summary>
        [System.Configuration.UserScopedSetting,
        System.Configuration.DefaultSettingValue("")]
        public List<List<ToolStripInfo>> Rows
        {
            get
            {
                return (List<List<ToolStripInfo>>)this["Rows"];
            }
            set
            {
                this["Rows"] = value;
            }
        }

        public ToolStripSettings(string settingsKey)
            : base(settingsKey)
        {
        }
    }

    /// <summary>
    /// 保存するToolStripの情報
    /// </summary>
    public class ToolStripInfo : IComparable<ToolStripInfo>
    {
        public string Name = "";
        public Point Location = Point.Empty;

        public ToolStripInfo(ToolStrip ts)
        {
            this.Name = ts.Name;
            this.Location = ts.Location;
        }

        public ToolStripInfo()
        {
        }

        public int CompareTo(ToolStripInfo other)
        {
            if (this.Location.X == other.Location.X)
            {
                return this.Location.Y - other.Location.Y;
            }
            return this.Location.X - other.Location.X;
        }
    }

    /// <summary>
    /// sourceFormに配置されたToolStripPanel内のToolStripの位置を保存する
    /// </summary>
    /// <param name="sourceForm"></param>
    public static void SaveSettings(Form sourceForm)
    {
        ToolStripManager.SaveSettings(sourceForm);
        ToolStripManager2.InternalSaveSettings(sourceForm);
    }

    /// <summary>
    /// sourceFormに配置されたToolStripPanel内のToolStripの位置を復元する
    /// </summary>
    /// <param name="sourceForm"></param>
    public static void LoadSettings(Form sourceForm)
    {
        ToolStripManager.LoadSettings(sourceForm);
        ToolStripManager2.InternalLoadSettings(sourceForm);
    }

    internal static void InternalSaveSettings(Control owner)
    {
        //owner内のToolStripPanelを探す
        List<Control> toolStripPanels = new List<Control>();
        FindControls(typeof(ToolStripPanel), owner.Controls,
            ref toolStripPanels);

        foreach (ToolStripPanel tsp in toolStripPanels)
        {
            if (string.IsNullOrEmpty(tsp.Name)) continue;

            List<List<ToolStripInfo>> rowsList =
                new List<List<ToolStripInfo>>();
            foreach (ToolStripPanelRow r in tsp.Rows)
            {
                //ToolStripPanelの列内のToolStripの情報を収集
                List<ToolStripInfo> toolStripNames =
                    new List<ToolStripInfo>();
                foreach (Control con in r.Controls)
                {
                    if (con is ToolStrip &&
                        !string.IsNullOrEmpty(con.Name))
                    {
                        toolStripNames.Add(
                            new ToolStripInfo((ToolStrip)con));
                    }
                }
                //列内の順番を並び替え
                toolStripNames.Sort();
                rowsList.Add(toolStripNames);
            }

            //ToolStripPanelごとに保存する
            string skey = owner.GetType().FullName + "." + tsp.Name;
            ToolStripSettings settings = new ToolStripSettings(skey);
            settings.Rows = rowsList;
            settings.Save();
        }
    }

    internal static void InternalLoadSettings(Control owner)
    {
        //owner内のToolStripPanelを探す
        List<Control> toolStripPanels = new List<Control>();
        FindControls(typeof(ToolStripPanel), owner.Controls,
            ref toolStripPanels);

        foreach (ToolStripPanel tsp in toolStripPanels)
        {
            if (string.IsNullOrEmpty(tsp.Name)) continue;

            //ToolStripPanelの情報を読み込む
            string skey = owner.GetType().FullName + "." + tsp.Name;
            ToolStripSettings settings = new ToolStripSettings(skey);
            List<List<ToolStripInfo>> rowsList = settings.Rows;

            //ToolStripPanel内のToolStripを一時的にすべて削除する
            Dictionary<string, ToolStrip> toolstrips =
                new Dictionary<string, ToolStrip>();
            foreach (Control c in tsp.Controls)
            {
                toolstrips.Add(c.Name, (ToolStrip)c);
            }
            tsp.Controls.Clear();

            for (int i = 0; i < rowsList.Count; i++)
            {
                foreach (ToolStripInfo info in rowsList[i])
                {
                    //位置を設定するToolStripを探す
                    ToolStrip ts = null;
                    if (toolstrips.ContainsKey(info.Name))
                    {
                        ts = toolstrips[info.Name];
                    }
                    else
                    {
                        Control[] tss =
                            owner.Controls.Find(info.Name, true);
                        if ((tss != null) && (tss.Length == 1)
                            && (tss[0] is ToolStrip))
                        {
                            ts = (ToolStrip)tss[0];
                        }
                    }
                    //ToolStripの位置を変更する
                    if (ts != null)
                    {
                        tsp.Join(ts, info.Location);
                    }
                }
            }
        }
    }

    internal static void FindControls(Type findType,
        Control.ControlCollection conts, ref List<Control> foundList)
    {
        foreach (Control c in conts)
        {
            if (findType.IsAssignableFrom(c.GetType()))
            {
                foundList.Add(c);
            }
            if (c.Controls.Count > 0)
            {
                FindControls(findType, c.Controls, ref foundList);
            }
        }
    }
}