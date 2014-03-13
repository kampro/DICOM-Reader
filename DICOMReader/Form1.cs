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
        private const string tempDirPath = @".\TEMP\";

        private Reader reader;
        private FileInformation fileInformation;
        private Dictionary<string, string> dictionary;

        public Form1()
        {
            InitializeComponent();

            this.label1.Visible = false;
            this.tableLayoutPanel1.Visible = false;
            this.pictureBox1.MouseHover += this.Control_MouseHover;
            this.pictureBox1.MouseWheel += this.pictureBox1_MouseWheel;
            this.treeView1.MouseHover += this.Control_MouseHover;
            this.treeView1.MouseWheel += this.pictureBox1_MouseWheel;
            this.treeView1.DragEnter += this.treeView1_DragEnter;
            this.treeView1.DragDrop += this.treeView1_DragDrop;
            this.treeView1.AfterSelect += this.treeView1_AfterSelect;
            this.dataGridView1.MouseHover += this.Control_MouseHover;
            this.dataGridView1.Columns.Add("tagName", "Tag name");
            this.dataGridView1.Columns.Add("tagValue", "Tag value");
            this.dataGridView1.Columns["tagName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["tagValue"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.RowHeadersWidth = 14;

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

        private void Control_MouseHover(object sender, EventArgs e)
        {
            ((Control)sender).Focus();
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                // mouse up
            }
            else if (e.Delta < 0)
            {
                // mouse down
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.reader.FilePath = e.Node.Tag.ToString();
            this.ReadFromReader();
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

            if (/*((fileInfo.Attributes & FileAttributes.Normal) == FileAttributes.Normal) &&*/
                (fileInfo.Extension == ".dcm" || fileInfo.Extension == ".DCM"))
                this.AddDCM(fileInfo);
            else if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            { MessageBox.Show("DIR"); }
            else if (((fileInfo.Attributes & FileAttributes.Archive) == FileAttributes.Archive) &&
                (fileInfo.Extension == ".zip" || fileInfo.Extension == ".ZIP"))
                this.AddZip(fileInfo);
        }

        private void AddDir(FileInfo fileInfo)
        {
        }

        private void AddDCM(FileInfo fileInfo)
        {
            TreeNode node = new TreeNode(fileInfo.Name);

            node.Tag = fileInfo.FullName;
            this.treeView1.Nodes.Add(node);
        }

        private void AddZip(FileInfo fileInfo)
        {
            this.tableLayoutPanel1.Visible = true;

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += (s, e) =>
            {
                using (ZipFile zip = ZipFile.Read(fileInfo.FullName))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Form1.tempDirPath);

                    if (!directoryInfo.Exists)
                        directoryInfo.Create();

                    string fileExtension;
                    FileInfo extractedFileInfo;

                    int i = 0;
                    int total = zip.Count;

                    foreach (ZipEntry ze in zip)
                    {
                        fileExtension = ze.FileName.Substring(ze.FileName.Length - 4);

                        if (fileExtension == ".dcm" || fileExtension == ".DCM")
                        {
                            ze.Extract(Form1.tempDirPath, ExtractExistingFileAction.OverwriteSilently);

                            extractedFileInfo = new FileInfo(Form1.tempDirPath + ze.FileName.Replace('/', '\\'));
                            // run in GUI thread
                            this.treeView1.Invoke(
                                new EventHandler(
                                    (s2, e2) =>
                                    {
                                        this.AddDCM(extractedFileInfo);
                                    }
                                )
                            );
                        }

                        i++;

                        backgroundWorker.ReportProgress((int)(i / (double)total * 100));

                        // DEBUG
                        System.Diagnostics.Debug.WriteLine("file: {0} | un.size: {1} | cp.size: {2} | cp.ratio: {3} | encrypt: {4}",
                            ze.FileName,
                            ze.UncompressedSize,
                            ze.CompressedSize,
                            ze.CompressionRatio,
                            (ze.UsesEncryption) ? "Y" : "N");
                    }
                }
            };
            backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                this.tableLayoutPanel1.Visible = false;
                this.progressBar1.Value = 0;
            };
            backgroundWorker.ProgressChanged += (s, e) =>
            {
                this.progressBar1.Value = e.ProgressPercentage;
            };
            backgroundWorker.RunWorkerAsync();
        }

        private void showRawDICOMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RAWDataForm rawDataForm = new RAWDataForm();

            rawDataForm.Focus();
            rawDataForm.Content = this.reader.ListAllTags();
            rawDataForm.ShowDialog();
        }

        private void exportToBMPToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Form1.tempDirPath);

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
    }
}
