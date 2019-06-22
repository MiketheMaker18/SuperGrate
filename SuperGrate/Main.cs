﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SuperGrate
{
    public partial class Main : Form
    {
        public static Form Form;
        public static RichTextBox LoggerBox;
        public static ProgressBar Progress;
        public static string SourceComputer;
        public static string DestinationComputer;
        public static ListSource CurrentListSource = ListSource.Unknown;
        public Main()
        {
            InitializeComponent();
            Form = this;
            LoggerBox = LogBox;
            Progress = pbMain;
            lbxUsers.Tag = new string[0];
        }
        private void Main_Load(object sender, EventArgs e)
        {
            Config.LoadConfig();
            Logger.Success("Welcome to Super Grate!");
            Logger.Information("Enter some information to get started!");
            UpdateFormRestrictions();
        }
        private async void BtStartStop_Click(object sender, EventArgs e)
        {
            tblMainLayout.Enabled = false;
            if(CurrentListSource == ListSource.SourceComputer)
            {
                await USMT.CopyUSMT(SourceComputer);
                foreach (int index in lbxUsers.SelectedIndices)
                {
                    await USMT.Do(USMTMode.ScanState, ((string[])lbxUsers.Tag)[index]);
                }
                await USMT.CleaupUSMT(SourceComputer);
            }
            if(tbDestinationComputer.Text != "")
            {
                await USMT.CopyUSMT(DestinationComputer);
                foreach (int index in lbxUsers.SelectedIndices)
                {
                    await USMT.Do(USMTMode.LoadState, ((string[])lbxUsers.Tag)[index]);
                }
                await USMT.CleaupUSMT(DestinationComputer);
            }
            tblMainLayout.Enabled = true;
        }
        private async void BtnListSource_Click(object sender, EventArgs e)
        {
            tblMainLayout.Enabled = false;
            lbxUsers.Items.Clear();
            lblUserList.Text = "Users on Source Computer:";
            Dictionary<string, string> results = await Misc.GetUsersFromHost(tbSourceComputer.Text);
            if (results != null)
            {
                lbxUsers.Tag = results.Keys.ToArray();
                lbxUsers.Items.AddRange(results.Values.ToArray());
                CurrentListSource = ListSource.SourceComputer;
                Logger.Success("Done!");
            }
            else
            {
                Logger.Error("Failed to list users from the source computer.");
            }
            tblMainLayout.Enabled = true;
            UpdateFormRestrictions();
        }
        private async void BtnListStore_Click(object sender, EventArgs e)
        {
            tblMainLayout.Enabled = false;
            lbxUsers.Items.Clear();
            lblUserList.Text = "Users in Migration Store:";
            Dictionary<string, string> results = await Misc.GetUsersFromStore(Config.MigrationStorePath);
            if(results != null)
            {
                lbxUsers.Tag = results.Keys.ToArray();
                lbxUsers.Items.AddRange(results.Values.ToArray());
                CurrentListSource = ListSource.MigrationStore;
            }
            else
            {
                Logger.Error("Failed to list users from the migration store.");
            }
            tblMainLayout.Enabled = true;
            UpdateFormRestrictions();
        }
        private void LogBox_DoubleClick(object sender, EventArgs e)
        {
            if(Logger.VerboseEnabled)
            {
                Logger.VerboseEnabled = false;
                Logger.Information("Verbose mode disabled.");
            }
            else
            {
                Logger.VerboseEnabled = true;
                Logger.Information("Verbose mode enabled.");
            }
        }
        private void UpdateFormRestrictions(object sender = null, EventArgs e = null)
        {
            if (lbxUsers.SelectedIndices.Count == 0 || (tbDestinationComputer.Text == "" && CurrentListSource == ListSource.MigrationStore))
            {
                btStartStop.Enabled = false;
            }
            else
            {
                btStartStop.Enabled = true;
            }
            if (lbxUsers.SelectedIndices.Count != 0 && CurrentListSource == ListSource.MigrationStore)
            {
                tbSourceComputer.Enabled = false;
                btnDelete.Enabled = true;
            }
            else
            {
                tbSourceComputer.Enabled = true;
                btnDelete.Enabled = false;
            }
            if(tbSourceComputer.Text == "")
            {
                btnListSource.Enabled = false;
            }
            else
            {
                btnListSource.Enabled = true;
            }
        }
        private void TbSourceComputer_TextChanged(object sender, EventArgs e)
        {
            SourceComputer = tbSourceComputer.Text;
            UpdateFormRestrictions();
        }
        private void TbDestinationComputer_TextChanged(object sender, EventArgs e)
        {
            DestinationComputer = tbDestinationComputer.Text;
            UpdateFormRestrictions();
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            foreach (int index in lbxUsers.SelectedIndices)
            {
                Misc.DeleteFromStore(((string[])lbxUsers.Tag)[index]);
            }
            btnListStore.PerformClick();
        }
    }
    public enum ListSource
    {
        Unknown = -1,
        SourceComputer = 1,
        MigrationStore = 2
    }
}
