using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DirectoryComparer.Interfaces;
using DirectoryComparer.Objects;
using DirectoryComparer.Services;

namespace DirectoryComparer
{
    public partial class frmCompareResults : Form
    {
        public List<CompareResult> CompareResults;

        public frmMain mainReference;

        public IResults Results;

        private ListViewItem selectedItem;

        public frmCompareResults()
        {
            InitializeComponent();
            InitializeList();
            InitializeListOperations();
        }

        private void InitializeListOperations()
        {
            listView1.MouseDown += listView1_MouseDown;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            selectedItem = listView1.GetItemAt(e.X, e.Y);
            if (selectedItem != null && e.Button == MouseButtons.Right)
            {
                ManipulateContextMenuItems(selectedItem);
                contextMenuStrip1.Show(listView1.PointToScreen(e.Location));
            }
        }

        private void ManipulateContextMenuItems(ListViewItem selectedItem)
        {
            foreach (ToolStripItem item in contextMenuStrip1.Items)
                item.Enabled = true;

            var serialNo = int.Parse(selectedItem.SubItems[0].Text);
            var fileName = selectedItem.SubItems[1].Text;
            var leftPath = selectedItem.SubItems[2].Text;
            var rightPath = selectedItem.SubItems[3].Text;

            var currentItem = CompareResults.Single(s => s.SerialNo == serialNo);

            if (currentItem.IsFile)
            {
                if (leftPath == string.Empty)
                    DisableContextMenuItems(0, 2, 5);
                if (rightPath == string.Empty)
                    DisableContextMenuItems(1, 3, 6);
            }
            else
            {
                if (leftPath == string.Empty)
                    DisableContextMenuItems(0, 1, 2, 5);
                if (rightPath == string.Empty)
                    DisableContextMenuItems(0, 1, 3, 6);
                if (leftPath != string.Empty && rightPath != string.Empty)
                    DisableContextMenuItems(0, 1);
            }
        }

        private void DisableContextMenuItems(params int[] list)
        {
            foreach (var index in list)
                contextMenuStrip1.Items[index].Enabled = false;
        }

        private void InitializeList()
        {
            listView1.SmallImageList = ImagesManager.GetImages();

            var serialNo = new ColumnHeader();
            serialNo.Text = "Serial No.";
            serialNo.Width = 50;
            serialNo.TextAlign = HorizontalAlignment.Center;

            listView1.Columns.Add(serialNo);
            listView1.Columns.Add("File/Folder Name").Width = 100;
            listView1.Columns.Add("Left Folder").Width = 300;
            listView1.Columns.Add("Right Folder").Width = 300;
            listView1.Columns.Add("Match").Width = 100;

            var columnItems = DirectoryComparerBaseInfo.Preferences.Columns;
            foreach (var item in columnItems.Where(c => c.IsVisible))
                listView1.Columns.Add(item.ColumnCaption).Width = 100;
        }

        public void AddItems()
        {
            AddItems(CompareResults);
        }

        public void AddItems(List<CompareResult> compareResults)
        {
            listView1.Items.Clear();
            listView1.BeginUpdate();
            foreach (var item in compareResults)
            {
                var listItem = new ListViewItem(item.SerialNo.ToString());

                if (!item.IsFile)
                    listItem.ImageIndex = 0;

                listItem.SubItems.Add(item.GetFileOrFolderName());
                listItem.SubItems.Add(item.LeftFilePath != string.Empty
                    ? Path.GetDirectoryName(item.LeftFilePath)
                    : string.Empty);
                listItem.SubItems.Add(item.RightFilePath != string.Empty
                    ? Path.GetDirectoryName(item.RightFilePath)
                    : string.Empty);
                listItem.SubItems.Add(GetMatchStatus(item));

                var columnItems = DirectoryComparerBaseInfo.Preferences.Columns;
                foreach (var cItem in columnItems.Where(c => c.IsVisible))
                    listItem.SubItems.Add(item.GetValue(cItem.ColumnCaption));

                listView1.Items.Add(listItem);
            }

            listView1.EndUpdate();
        }

        private string GetMatchStatus(CompareResult item)
        {
            if (item.ExistsLeft && !item.ExistsRight)
                return "Exists on the left";
            if (!item.ExistsLeft && item.ExistsRight)
                return "Exists on the right";
            if (item.Match)
                return "Matches";
            return "Does not match";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Environment.Exit(0);
        }

        private void frmCompareResults_Load(object sender, EventArgs e)
        {
            CompareResults = AddSerialNumbers(Results.CoalescedResults());
            AddItems();
        }

        private List<CompareResult> AddSerialNumbers(List<CompareResult> list)
        {
            for (var i = 0; i < list.Count; i++) list[i].SerialNo = i + 1;
            return list;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }

        private void SelectMenuItem(object sender)
        {
            ResetMenuItems("View");

            var item = sender as ToolStripMenuItem;
            item.Checked = true;

            SetSelectedFilterChoice(showAllToolStripMenuItem);
        }

        private void SetSelectedFilterChoice(object sender)
        {
            ResetMenuItems("F&ilter");

            var item = sender as ToolStripMenuItem;
            item.Checked = true;
        }

        private void ResetMenuItems(string mnuStart)
        {
            foreach (ToolStripMenuItem menuItem in menuStrip1.Items)
                if (menuItem.Text.Contains(mnuStart) && menuItem.HasDropDownItems)
                    foreach (ToolStripItem mnuItem in menuItem.DropDownItems)
                        if (mnuItem is ToolStripMenuItem)
                            ((ToolStripMenuItem)mnuItem).Checked = false;
        }

        #region View menu related

        private void bothResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareResults = Results.CoalescedResults();
            AddItems();
            SelectMenuItem(sender);
        }

        private void leftSideOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareResults = Results.LeftResults;
            AddItems();
            SelectMenuItem(sender);
        }

        private void rightSideOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareResults = Results.RightResults;
            AddItems();
            SelectMenuItem(sender);
        }

        #endregion

        #region File menu related

        private void compareFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
            mainReference.ClearProgress();
            mainReference.Show();
        }

        private void asXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "xml|*.xml";
            saveFileDialog1.Title = "Export folder compare results";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
                CompareResultExporter.ExportAsXml(CompareResults, saveFileDialog1.FileName);
        }

        private void asCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "csv|*.csv";
            saveFileDialog1.Title = "Export folder compare results";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
                CompareResultExporter.ExportAsCsv(CompareResults, saveFileDialog1.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        #endregion

        #region Filter menu related

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newCompareResults = CompareResults;
            AddItems(newCompareResults);
            SetSelectedFilterChoice(sender);
        }

        private void showMatchesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newCompareResults = CompareResults.Where(c => c.Match).ToList();
            AddItems(newCompareResults);
            SetSelectedFilterChoice(sender);
        }

        private void showMismatchesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newCompareResults = CompareResults.Where(c => c.ExistsLeft == c.ExistsRight && !c.Match).ToList();
            AddItems(newCompareResults);
            SetSelectedFilterChoice(sender);
        }

        private void showLeftFilesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newCompareResults = CompareResults.Where(c => c.ExistsLeft && !c.ExistsRight).ToList();
            AddItems(newCompareResults);
            SetSelectedFilterChoice(sender);
        }

        private void showRightFilesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newCompareResults = CompareResults.Where(c => !c.ExistsLeft && c.ExistsRight).ToList();
            AddItems(newCompareResults);
            SetSelectedFilterChoice(sender);
        }

        #endregion

        #region Context menu items

        private void openLeftFilewNotepa0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = selectedItem.SubItems[2].Text.TrimEnd('\\') + '\\' + selectedItem.SubItems[1].Text;
            FileOrFolderActions.OpenFile(fileName);
        }

        private void openRightFilewNotepa0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = selectedItem.SubItems[3].Text.TrimEnd('\\') + '\\' + selectedItem.SubItems[1].Text;
            FileOrFolderActions.OpenFile(fileName);
        }

        private void copyLeftPathToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var fileOrFolderName = selectedItem.SubItems[2].Text.TrimEnd('\\') + '\\' + selectedItem.SubItems[1].Text;
            Clipboard.SetText(fileOrFolderName);
        }

        private void copyRightPathToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var fileOrFolderName = selectedItem.SubItems[3].Text.TrimEnd('\\') + '\\' + selectedItem.SubItems[1].Text;
            Clipboard.SetText(fileOrFolderName);
        }

        private void openLeftFolderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var folderName = selectedItem.SubItems[2].Text.TrimEnd('\\') + '\\';
            FileOrFolderActions.OpenFolder(folderName);
        }

        private void openRightFolderToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var folderName = selectedItem.SubItems[3].Text.TrimEnd('\\') + '\\';
            FileOrFolderActions.OpenFolder(folderName);
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var preferences = new frmPreferences();
            preferences.ShowDialog();
        }

        #endregion
    }
}