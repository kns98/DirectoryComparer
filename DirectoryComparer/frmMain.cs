using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DirectoryComparer.Comparers;
using DirectoryComparer.Interfaces;
using DirectoryComparer.Objects;
using DirectoryComparer.RegistryManager;

namespace DirectoryComparer
{
    public partial class frmMain : Form
    {
        private frmCompareResults _frmCompareResults;

        public frmMain()
        {
            InitializeComponent();
            MaximizeBox = false;

            comparerWorker.DoWork += comparerWorker_DoWork;
            comparerWorker.ProgressChanged += comparerWorker_ProgressChanged;
            comparerWorker.RunWorkerCompleted += comparerWorker_RunWorkerCompleted;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnChoose1_Click(object sender, EventArgs e)
        {
            var result = folderChooser.ShowDialog();
            if (result == DialogResult.OK) txtFolder1.Text = folderChooser.SelectedPath;
        }

        private void bnChoose2_Click(object sender, EventArgs e)
        {
            var result = folderChooser.ShowDialog();
            if (result == DialogResult.OK) txtFolder2.Text = folderChooser.SelectedPath;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var status = ValidateInputs();

            if (status != string.Empty)
            {
                MessageBox.Show(status, "Directory Comparer");
                return;
            }

            DirectoryComparerBaseInfo.Preferences = GetPreferences();
            DirectoryComparerBaseInfo.LeftPath = txtFolder1.Text;
            DirectoryComparerBaseInfo.RightPath = txtFolder2.Text;
            DirectoryComparerBaseInfo.Recursive = chkRecursive.Checked;

            comparerWorker.RunWorkerAsync();
        }

        private CompareResultsPreferences GetPreferences()
        {
            var regManager = RegManager.getInstance();
            var columns = regManager.getColumnPreferences();
            var prefs = new CompareResultsPreferences();
            var finalCols = columns != string.Empty ? columns : "0,0,0,0,0,0";
            prefs.DefaultLeftPath = regManager.getDefaultLeftDir();
            prefs.DefaultRightPath = regManager.getDefaultRightDir();
            prefs.Columns = ColumnItemHelper.GetColumns(finalCols);
            return prefs;
        }

        private void comparerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ITwoPassComparer comparer = new RecursiveComparer(this);
            IDirectoryComparer recursiveComparer = new RecursiveDirectoryComparer(comparer);
            IResults results = recursiveComparer.CompareDirectories();

            ReportProgress(100);

            Thread.Sleep(1000);

            e.Result = results;
        }

        public void ClearProgress()
        {
            progressBar.Value = 0;
        }

        public void ReportProgress(int percentage)
        {
            comparerWorker.ReportProgress(percentage);
        }

        private void comparerWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void comparerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var results = (IResults)e.Result;

            _frmCompareResults = new frmCompareResults();
            _frmCompareResults.Results = results;
            _frmCompareResults.mainReference = this;
            Hide();
            _frmCompareResults.Show();
        }

        private string ValidateInputs()
        {
            if (string.IsNullOrEmpty(txtFolder1.Text))
                return "Please choose folder 1";

            if (string.IsNullOrEmpty(txtFolder2.Text))
                return "Please choose folder 2";

            if (File.Exists(txtFolder1.Text))
                return string.Format("{0} is a file, please choose a folder.", txtFolder1.Text);

            if (File.Exists(txtFolder2.Text))
                return string.Format("{0} is a file, please choose a folder.", txtFolder2.Text);

            if (!Directory.Exists(txtFolder1.Text))
                return string.Format("{0} does not exist", txtFolder1.Text);

            if (!Directory.Exists(txtFolder2.Text))
                return string.Format("{0} does not exist", txtFolder2.Text);

            return "";
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            var prefs = GetPreferences();
            txtFolder1.Text = prefs.DefaultLeftPath;
            txtFolder2.Text = prefs.DefaultRightPath;
        }
    }
}