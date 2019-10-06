using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GlobalC
{
    public partial class MainForm : Form
    {
        private MainVault vault;

        public MainForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowOpenDialog();
        }

        private void ShowOpenDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Supported files|GlobalC.lzc|All files|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                CreateVault(openFileDialog.FileName);
        }

        private void CreateVault(string fileName)
        {
            if (vault != null)
                vault.Dispose();
            vault = new MainVault(fileName);
            archiveList.DataSource = vault.Archives;
        }

        private void archiveList_Click(object sender, EventArgs e)
        {
            if (archiveList.SelectedItems.Count > 0)
                propertyGrid1.SelectedObject = archiveList.SelectedItems[0];
        }

        private void buttonUnpack_Click(object sender, EventArgs e)
        {
            if (archiveList.SelectedItems.Count > 0)
                (archiveList.SelectedItems[0] as Archive).Unpack();
            RefreshForm();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            vault.Write();
        }

        private void unpackAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                vault.Archives.FindAll(i => i.Type == ArchiveType.JDLZ).ForEach(i => i.Unpack());
                archiveList.DataSource = null;
                archiveList.DataSource = vault.Archives;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Something went wrong: {0}", exception.Message));
            }
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (vault !=  null)
                CreateVault(vault.FileName);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (archiveList.SelectedItems.Count > 0)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                    (archiveList.SelectedItems[0] as Archive).Export(dialog.FileName);
            }
        }

        private void buttonCompress_Click(object sender, EventArgs e)
        {
            if (archiveList.SelectedItems.Count > 0)
                (archiveList.SelectedItems[0] as Archive).Compress();
            RefreshForm();
        }

        private void RefreshForm()
        {
            propertyGrid1.Refresh();
            archiveList.DataSource = null;
            archiveList.DataSource = vault.Archives;
        }

//        private void buttonReplace_Click(object sender, EventArgs e)
//        {
//            var dialog = new OpenFileDialog();
//            if (dialog.ShowDialog() == DialogResult.OK)
//                (archiveList.SelectedItems[0] as Archive).Replace(dialog.FileName);
//        }
    }
}
