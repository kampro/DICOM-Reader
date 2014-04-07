using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using Ionic.Zip;

namespace DICOMReader
{
    public partial class Form1 : Form
    {
        private Reader reader;
        private FileInformation fileInformation;
        private Dictionary<string, string> dictionary;
        private FileHelper fileHelper;
        private RunWorkerCompletedEventHandler workerCompleted;
        private ProgressChangedEventHandler progressChanged;

        public Form1()
        {
            InitializeComponent();

            this.label1.Visible = false;
            this.tableLayoutPanel1.Visible = false;
            this.pictureBox1.MouseHover += this.Control_MouseHover;
            this.pictureBox1.MouseWheel += this.pictureBox1_MouseWheel;
            this.treeView1.MouseHover += this.Control_MouseHover;
            this.treeView1.DragEnter += this.treeView1_DragEnter;
            this.treeView1.DragDrop += this.treeView1_DragDrop;
            this.treeView1.AfterSelect += this.treeView1_AfterSelect;
            this.dataGridView1.MouseHover += this.Control_MouseHover;
            this.dataGridView1.Columns.Add("tagName", "Tag name");
            this.dataGridView1.Columns.Add("tagValue", "Tag value");
            this.dataGridView1.Columns["tagName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["tagValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.RowHeadersWidth = 14;

            this.workerCompleted = (s, e) =>
            {
                this.tableLayoutPanel1.Visible = false;
                this.progressBar1.Value = 0;
            };
            this.progressChanged = (s, e) =>
            {
                this.progressBar1.Value = e.ProgressPercentage;
            };

            this.fileHelper = new FileHelper(this.treeView1);
            this.fileHelper.zipCompleted = this.workerCompleted;
            this.fileHelper.zipProgressChanged = this.progressChanged;

            this.reader = new Reader();
        }

        private void ReadFromReader()
        {
            this.label1.Visible = true;

            if (this.reader.FilePath != null && this.reader.Read())
            {
                this.pictureBox1.Image = this.reader.BitmapImage;
                this.fileInformation = this.reader.FileInf;
                this.dictionary = this.fileInformation.GetTagsDictionary();

                int[] dims = this.reader.Dims;

                StringBuilder stringBuilder = new StringBuilder("Patient: ");
                stringBuilder.Append(this.fileInformation.PatientName);
                stringBuilder.Append(" | Body part: ");
                stringBuilder.Append(this.fileInformation.BodyPart);
                stringBuilder.AppendFormat(" | Image dimensions: {0}x{1}", dims[0], dims[1]);
                this.toolStripStatusLabel1.Text = stringBuilder.ToString();

                this.dataGridView1.Rows.Clear();

                foreach (KeyValuePair<string, string> kv in this.dictionary)
                    this.dataGridView1.Rows.Add(kv.Key, kv.Value);
            }
            else
                MessageBox.Show("File has not been found. File name has been changed, file has been deleted or file is corrupted.", "DICOM Reader error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            this.label1.Visible = false;
        }

        private void BuildTree()
        {
            this.label2.Text = "Building images' hierarchy...";

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += (s, e) =>
            {
                string[] ids;
                List<FileInfo> entries = new List<FileInfo>();
                TreeNode treeNode;

                // Get entries from TreeView
                foreach (TreeNode n in this.treeView1.Nodes)
                {
                    if (n.Tag != null)
                        entries.Add(new FileInfo(n.Tag.ToString()));
                }

                double total = entries.Count;

                // Clear tree
                this.treeView1.Invoke((MethodInvoker)(() => this.treeView1.Nodes.Clear()));

                // Build new tree
                int i = 0;

                foreach (FileInfo fi in entries)
                {
                    this.reader.FilePath = fi.FullName;

                    if (this.reader.FilePath != null)
                    {
                        ids = this.reader.GetIDs();

                        // 1st level
                        if (!this.treeView1.Nodes.ContainsKey(ids[0]))
                            this.treeView1.Invoke((MethodInvoker)(() => this.treeView1.Nodes.Add(ids[0], ids[0])));

                        // 2nd level
                        if (!this.treeView1.Nodes[ids[0]].Nodes.ContainsKey(ids[1]))
                            this.treeView1.Invoke((MethodInvoker)(() => this.treeView1.Nodes[ids[0]].Nodes.Add(ids[1], ids[1])));

                        // 3rd level
                        treeNode = new TreeNode(fi.Name);
                        treeNode.Tag = fi.FullName;

                        if (!this.treeView1.Nodes[ids[0]].Nodes[ids[1]].Nodes.ContainsKey(fi.Name))
                            this.treeView1.Invoke((MethodInvoker)(() => this.treeView1.Nodes[ids[0]].Nodes[ids[1]].Nodes.Add(treeNode)));

                        i++;

                        backgroundWorker.ReportProgress((int)(i / total * 100));
                    }
                }
            };
            backgroundWorker.RunWorkerCompleted += this.workerCompleted;
            backgroundWorker.ProgressChanged += this.progressChanged;
            backgroundWorker.RunWorkerAsync();
        }

        private void Control_MouseHover(object sender, EventArgs e)
        {
            ((Control)sender).Focus();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            TreeNode selectedNode = this.treeView1.SelectedNode;

            if (e.Delta > 0)
            {
                if (selectedNode != null)
                    this.treeView1.SelectedNode = selectedNode.PrevVisibleNode;
            }
            else if (e.Delta < 0)
            {
                if (selectedNode != null)
                    this.treeView1.SelectedNode = selectedNode.NextVisibleNode;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                this.reader.FilePath = e.Node.Tag.ToString();
                this.ReadFromReader();
            }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array filesArray = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (filesArray != null)
                {
                    foreach (object o in filesArray)
                        this.AddFile(o.ToString());
                }
            }
            catch (Exception ex)
            {
                // DEBUG
                System.Diagnostics.Debug.WriteLine("DragDrop exception: " + ex.Message);
            }
        }

        private void AddFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);

            if (this.fileHelper.IsDCM(fileInfo))
                this.fileHelper.AddDCM(fileInfo);
            else if (this.fileHelper.IsDir(fileInfo))
                this.fileHelper.AddDir(new DirectoryInfo(path));
            else if (this.fileHelper.IsZip(fileInfo))
            {
                this.label2.Text = "Extracting ZIP file...";
                this.tableLayoutPanel1.Visible = true;
                this.fileHelper.AddZip(fileInfo);
            }
        }

        private void openDICOMsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "DICOM file (*.dcm)|*.dcm;*.DCM";
            fileDialog.Multiselect = true;
            fileDialog.FilterIndex = 1;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo fileInfo;

                foreach (string path in fileDialog.FileNames)
                {
                    fileInfo = new FileInfo(path);
                    this.fileHelper.AddDCM(fileInfo);
                }
            }
        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
                this.fileHelper.AddDir(new DirectoryInfo(folderDialog.SelectedPath));
        }

        private void openZIPArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "ZIP archive (*.zip)|*.zip;*.ZIP";
            fileDialog.Multiselect = false;
            fileDialog.FilterIndex = 1;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.label2.Text = "Extracting ZIP file...";
                this.tableLayoutPanel1.Visible = true;
                FileInfo fileInfo = new FileInfo(fileDialog.FileName);

                this.fileHelper.AddZip(fileInfo);
            }
        }

        private void showRawDICOMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RAWDataForm rawDataForm = new RAWDataForm();

            rawDataForm.Focus();
            rawDataForm.Content = this.reader.ListAllTags();
            rawDataForm.ShowDialog();
        }

        private void exportImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.pictureBox1.Image != null)
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.AddExtension = true;
                fileDialog.Filter = "Portable Network Graphics (*.png)|*.png|Bitmap (*.bmp)|*.bmp";
                fileDialog.FilterIndex = 1;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.Drawing.Imaging.ImageFormat format;

                    if (fileDialog.FilterIndex == 1)
                        format = System.Drawing.Imaging.ImageFormat.Png;
                    else
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    this.pictureBox1.Image.Save(fileDialog.FileName, format);
                }
            }
            else
                MessageBox.Show("Image does not exist.", "DICOM Reader information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Copyright \u00a9 2014 Martyna Donocik, Kamil Prokop\n\nThis program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.\n\nThis program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.\n\nYou should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.\n\nContact: kprokop24@gmail.com", "About program", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(FileHelper.tempDirPath);

            if (directoryInfo.Exists)
            {
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception ex)
                {
                    // DEBUG
                    System.Diagnostics.Debug.WriteLine("Directory delete exception: " + ex.Message);
                }
            }
        }

        private void removeSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = this.treeView1.SelectedNode;

            if (selectedNode != null)
                this.treeView1.Nodes.Remove(selectedNode);
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                this.removeSelectedToolStripMenuItem_Click(sender, e);
        }

        private void buildHierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.BuildTree();
        }
    }
}
