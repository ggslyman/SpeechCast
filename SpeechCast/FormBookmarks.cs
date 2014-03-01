using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SpeechCast
{
    public partial class FormBookmarks : Form
    {
        public FormBookmarks()
        {
            InitializeComponent();
        }


        public void Initialize(Bookmarks bookmarks)
        {
            bookmarks_ = bookmarks;
            UpdateView();
        }


        private Bookmarks bookmarks_;

        public void UpdateView()
        {
            UpdateTreeView();
            UpdateListView();
        }

        private TreeNode rootNode;

        public void UpdateTreeView()
        {
            treeView.Nodes.Clear();

            rootNode = treeView.Nodes.Add("(ルート)");
            rootNode.Tag = bookmarks_.RootFolder;

            AddTreeNodes(rootNode, bookmarks_.RootFolder);

            treeView.SelectedNode = rootNode;
            rootNode.ExpandAll();
        }

        private void AddTreeNodes(TreeNode node, Bookmarks.Folder folder)
        {
            foreach (Bookmarks.BookmarkBase bb in folder.Items)
            {
                Bookmarks.Folder childFolder = bb as Bookmarks.Folder;

                if (childFolder != null)
                {
                    TreeNode childNode = node.Nodes.Add(childFolder.Name);

                    childNode.Tag = childFolder;
                    AddTreeNodes(childNode, childFolder);
                }
            }
        }

        public void UpdateListView()
        {
            listView.Items.Clear();
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode != null)
            {
                Bookmarks.Folder folder = selectedNode.Tag as Bookmarks.Folder;
                if (folder != null)
                {
                    foreach (Bookmarks.BookmarkBase bb in folder.Items)
                    {
                        Bookmarks.Folder childFolder = bb as Bookmarks.Folder;

                        if (childFolder != null)
                        {
                            ListViewItem item = listView.Items.Add("");

                            item.SubItems.Add("");
                            item.Tag = childFolder;
                            UpdateListViewItem(item);
                        }

                        Bookmarks.Bookmark bookmark = bb as Bookmarks.Bookmark;

                        if (bookmark != null)
                        {
                            ListViewItem item = listView.Items.Add("");

                            item.Tag = bookmark;
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            UpdateListViewItem(item);
                        }
                    }
                }
            }
            UpdateUI();
        }

        public bool isUpdatingUI = false;
        public void UpdateUI()
        {
            isUpdatingUI = true;

            try
            {
                bool nameEnabled = false;
                bool urlEnabled = false;

                if (listView.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listView.SelectedItems[0];

                    Bookmarks.Folder childFolder = selectedItem.Tag as Bookmarks.Folder;

                    if (childFolder != null)
                    {
                        nameEnabled = true;
                        textBoxName.Text = childFolder.Name;
                        textBoxURL.Text = "";
                    }

                    Bookmarks.Bookmark bookmark = selectedItem.Tag as Bookmarks.Bookmark;

                    if (bookmark != null)
                    {
                        nameEnabled = true;
                        textBoxName.Text = bookmark.Name;

                        urlEnabled = true;
                        textBoxURL.Text = bookmark.URL;
                    }
                }

                labelName.Enabled = nameEnabled;
                labelURL.Enabled = urlEnabled;
                textBoxName.Enabled = nameEnabled;
                textBoxURL.Enabled = urlEnabled;
            }
            finally
            {
                isUpdatingUI = false;
            }

        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateListView();
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            UpdateUI();
        }

        private void toolStripMenuItemAddBookmak_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode != null)
            {
                Bookmarks.Folder folder = selectedNode.Tag as Bookmarks.Folder;
                if (folder != null)
                {
                    folder.Items.Add(new Bookmarks.Bookmark());

                    UpdateListView();
                    int count = listView.Items.Count;
                    if (count> 0) //0なわけないけど
                    {
                        //今追加したところを選択状態に
                        listView.Items[count - 1].Selected = true;
                    }
                }
            }
        }

        private void SelectNode(Bookmarks.Folder folder)
        {
            TreeNode node = FindNode(folder);

            if (node != null)
            {
                treeView.SelectedNode = node;
            }
        }


        private TreeNode FindNode(Bookmarks.Folder folder)
        {
            return FindNode(treeView.Nodes[0], folder);
        }

        private TreeNode FindNode(TreeNode node, Bookmarks.Folder folder)
        {
            if (node.Tag == folder)
            {
                return node;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                TreeNode n = FindNode(childNode, folder);
                if (n != null)
                {
                    return n;
                }
            }
            return null;
        }


        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode != null)
            {
                Bookmarks.Folder folder = selectedNode.Tag as Bookmarks.Folder;
                if (folder != null)
                {
                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        Bookmarks.BookmarkBase bb = item.Tag as Bookmarks.BookmarkBase;

                        if (bb != null)
                        {
                            folder.Items.Remove(bb);
                        }
                    }

                    UpdateTreeView();
                    SelectNode(folder);
                }
            }
        }

        private void UpdateListViewItem(ListViewItem item)
        {

            Bookmarks.Folder childFolder = item.Tag as Bookmarks.Folder;

            if (childFolder != null)
            {
                item.SubItems[1].Text = childFolder.Name;
                item.ImageIndex = 0;
                TreeNode node = FindNode(childFolder);
                if (node != null)
                {
                    node.Text = childFolder.Name;
                }
            }

            Bookmarks.Bookmark bookmark = item.Tag as Bookmarks.Bookmark;

            if (bookmark != null)
            {
                item.SubItems[1].Text = bookmark.Name;
                item.SubItems[2].Text = bookmark.URL;
            }

        }

        private void UpdateBookmark()
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView.SelectedItems[0];

                Bookmarks.Folder childFolder = selectedItem.Tag as Bookmarks.Folder;

                if (childFolder != null)
                {
                    childFolder.Name = textBoxName.Text;
                }

                Bookmarks.Bookmark bookmark = selectedItem.Tag as Bookmarks.Bookmark;

                if (bookmark != null)
                {
                    bookmark.Name = textBoxName.Text;
                    bookmark.URL = textBoxURL.Text;
                }

                UpdateListViewItem(selectedItem);
            }

        }

        private void textBoxName_TextChanged(object sender, EventArgs e)
        {
            if (!isUpdatingUI)
            {
                UpdateBookmark();
            }
        }

        private void textBoxURL_TextChanged(object sender, EventArgs e)
        {
            if (!isUpdatingUI)
            {
                UpdateBookmark();
            }
        }

        private void toolStripMenuItemAddFolder_Click(object sender, EventArgs e)
        {

            TreeNode selectedNode = treeView.SelectedNode;

            if (selectedNode != null)
            {
                Bookmarks.Folder folder = selectedNode.Tag as Bookmarks.Folder;
                if (folder != null)
                {
                    FormAddBookmarkFolder form = new FormAddBookmarkFolder();

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        Bookmarks.Folder newFolder = new Bookmarks.Folder();

                        newFolder.Name = form.FolderName;
                        folder.Items.Add(newFolder);

                        UpdateTreeView();
                        SelectNode(newFolder);
                    }
                }
            }
        }




        private class DrapDropItem
        {
            public Bookmarks.Folder Folder;
            public List<Bookmarks.BookmarkBase> Items = new List<Bookmarks.BookmarkBase>();
        }


        private ListViewItem GetListViewItem(int x, int y)
        {
            Point p = listView.PointToClient(new Point(x, y));

            return listView.GetItemAt(p.X, p.Y);
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DrapDropItem)))
            {
                if (GetListViewItem(e.X, e.Y) != null)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DrapDropItem)))
            {
                DrapDropItem data = (DrapDropItem) e.Data.GetData(typeof(DrapDropItem));

                ListViewItem item = GetListViewItem(e.X, e.Y);
                if (item != null)
                {
                    Bookmarks.BookmarkBase bookmark = item.Tag as Bookmarks.BookmarkBase;
                    if (bookmark != null)
                    {
                        if (data.Items.IndexOf(bookmark) < 0)
                        {
                            foreach (Bookmarks.BookmarkBase b in data.Items)
                            {
                                data.Folder.Items.Remove(b);

                            }

                            int index = data.Folder.Items.IndexOf(bookmark);

                            foreach (Bookmarks.BookmarkBase b in data.Items)
                            {
                                data.Folder.Items.Insert(index, b);
                            }

                            UpdateTreeView();
                            SelectNode(data.Folder);
                        }
                    }
                }
            }
        }

        private void listView_DragLeave(object sender, EventArgs e)
        {

        }


        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (listView.SelectedItems.Count > 0 && e.Button == MouseButtons.Left)
            {
                DrapDropItem dragDropItem = new DrapDropItem();

                TreeNode selectedNode = treeView.SelectedNode;

                if (selectedNode != null)
                {
                    Bookmarks.Folder folder = selectedNode.Tag as Bookmarks.Folder;
                    if (folder != null)
                    {
                        foreach (ListViewItem item in listView.SelectedItems)
                        {
                            Bookmarks.BookmarkBase bb = item.Tag as Bookmarks.BookmarkBase;

                            if (bb != null)
                            {
                                dragDropItem.Items.Add(bb);
                            }
                        }

                        dragDropItem.Folder = folder;
                        treeView.DoDragDrop(dragDropItem, DragDropEffects.Move);
                    }
                }

            }

        }

        private TreeNode GetTreeNode(int x, int y)
        {
            Point p = treeView.PointToClient(new Point(x, y));

            return treeView.GetNodeAt(p);
        }

        private void treeView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DrapDropItem)))
            {
                if (GetTreeNode(e.X, e.Y) != null)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DrapDropItem)))
            {
                TreeNode node = GetTreeNode(e.X, e.Y);
                if (node != null)
                {
                    DrapDropItem data = (DrapDropItem) e.Data.GetData(typeof(DrapDropItem));

                    Bookmarks.Folder folder = node.Tag as Bookmarks.Folder;
                    if (folder != null && data.Folder != folder)
                    {
                        foreach (Bookmarks.BookmarkBase bb in data.Items)
                        {
                            Bookmarks.Bookmark b = bb as Bookmarks.Bookmark;

                            if (b != null)
                            {
                                data.Folder.Items.Remove(b);
                                folder.Items.Add(b);
                            }
                            else
                            {
                                MessageBox.Show("フォルダはフォルダにドロップできません:" + bb.Name);
                            }
                        }

                        UpdateTreeView();
                        SelectNode(folder);
                    }
                }

            }
        }

        private void treeView_DragLeave(object sender, EventArgs e)
        {

        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView.SelectedItems[0];

                Bookmarks.Folder folder = selectedItem.Tag as Bookmarks.Folder;

                if (folder != null)
                {
                    SelectNode(folder);
                }
            }

        }
    }
}
