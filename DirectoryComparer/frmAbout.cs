using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DirectoryComparer
{
    internal partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            textBoxDescription.Text = "This is just a simple application that can be used to compare 2 directories."
                                      + Environment.NewLine
                                      + Environment.NewLine
                                      + " For updates visit http://comparer.thekfactor.info";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var siteLauncher = new Process();
            siteLauncher.StartInfo.FileName = "iexplore.exe";
            siteLauncher.StartInfo.Arguments = ((LinkLabel)sender).Text;
            siteLauncher.Start();
        }
    }
}