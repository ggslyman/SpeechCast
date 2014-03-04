using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace SpeechCast
{
    public class Bookmarks
    {
        #region サブクラス
        [XmlInclude(typeof(Folder))]
        [XmlInclude(typeof(Bookmark))]

        public abstract class BookmarkBase
        {
            public string Name = "";
        }

        public class Folder : BookmarkBase
        {
            public List<BookmarkBase> Items = new List<BookmarkBase>();
        }
        public class Bookmark : BookmarkBase
        {
            public string URL = "";
        }
        #endregion

        public Folder RootFolder = new Folder();

        public Bookmarks()
        {
        }

        public Bookmarks(string filePath)
        {
            filePath_ = filePath;
        }

        static string filePath_;

        public static Bookmarks Deserialize(string filePath)
        {
            Bookmarks bookmarks;

            filePath_ = filePath;

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Bookmarks));
            using (System.IO.FileStream fs = new System.IO.FileStream(filePath_, System.IO.FileMode.Open))
            {
                bookmarks = (Bookmarks)serializer.Deserialize(fs);
            }

            return bookmarks;
        }

        public void Serialize()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Bookmarks));

            string dirPath = System.IO.Path.GetDirectoryName(filePath_);

            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }

            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath_, System.IO.FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception e)
            {
                FormMain.Instance.AddLog(e.Message);
            }
        }

    }
}
