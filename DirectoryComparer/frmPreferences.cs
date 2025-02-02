﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DirectoryComparer.Objects;
using DirectoryComparer.RegistryManager;

namespace DirectoryComparer
{
    public partial class frmPreferences : Form
    {
        public frmPreferences()
        {
            InitializeComponent();
        }

        private void frmPreferences_Load(object sender, EventArgs e)
        {
            textBox1.Text = DirectoryComparerBaseInfo.Preferences.DefaultLeftPath;
            textBox2.Text = DirectoryComparerBaseInfo.Preferences.DefaultRightPath;

            var columnItems = DirectoryComparerBaseInfo.Preferences.Columns;
            var chkBoxes = GetCheckBoxes();

            foreach (var chkBox in chkBoxes)
                chkBox.Checked = columnItems.Single(c => c.ColumnCaption == Regex.Replace(chkBox.Text, @"[&]", ""))
                    .IsVisible;
        }

        private List<CheckBox> GetCheckBoxes()
        {
            var chkBoxes = Controls
                .Cast<Control>()
                .OfType<GroupBox>()
                .Single()
                .Controls
                .Cast<Control>()
                .OfType<CheckBox>()
                .ToList();
            return chkBoxes;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var strStatus = ValidateInputs();

            if (!string.IsNullOrEmpty(strStatus))
            {
                MessageBox.Show(strStatus, "Directory Comparer");
                return;
            }

            var columnStr = "{0},{1},{2},{3},{4},{5}";
            columnStr = string.Format(columnStr,
                checkBox2.Checked.ToInt(),
                checkBox1.Checked.ToInt(),
                checkBox4.Checked.ToInt(),
                checkBox5.Checked.ToInt(),
                checkBox6.Checked.ToInt(),
                checkBox7.Checked.ToInt());

            DirectoryComparerBaseInfo.Preferences.Columns = ColumnItemHelper.GetColumns(columnStr);

            var regManager = RegManager.getInstance();
            var status = regManager.writeColumnPreferences(columnStr);
            status = regManager.writeDefaultLeftDir(textBox1.Text);
            status = regManager.writeDefaultRightDir(textBox2.Text);
            lblStatus.Text = status ? "Preferences saved" : "Errors were encountered";
        }

        private string ValidateInputs()
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                return "Please choose folder 1";

            if (string.IsNullOrEmpty(textBox2.Text))
                return "Please choose folder 2";

            if (File.Exists(textBox1.Text))
                return string.Format("{0} is a file, please choose a folder.", textBox1.Text);

            if (File.Exists(textBox2.Text))
                return string.Format("{0} is a file, please choose a folder.", textBox2.Text);

            if (!Directory.Exists(textBox1.Text))
                return string.Format("{0} does not exist", textBox1.Text);

            if (!Directory.Exists(textBox2.Text))
                return string.Format("{0} does not exist", textBox2.Text);

            return "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = folderChooser.ShowDialog();
            if (result == DialogResult.OK) textBox1.Text = folderChooser.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var result = folderChooser.ShowDialog();
            if (result == DialogResult.OK) textBox2.Text = folderChooser.SelectedPath;
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            var appPath = Application.ExecutablePath;

            var process = new Process();
            process.StartInfo.FileName = appPath;
            process.Start();

            Environment.Exit(0);
        }
    }
}