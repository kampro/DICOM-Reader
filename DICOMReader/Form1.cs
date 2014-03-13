using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DICOMReader
{
    public partial class Form1 : Form
    {
        private Reader reader;
        private FileInformation fileInformation;
        private Dictionary<string, string> dictionary;
        private RAWDataForm rawDataForm;

        public Form1()
        {
            InitializeComponent();

            this.label1.Visible = false;
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

                StringBuilder stringBuilder = new StringBuilder("Patient: ");
                stringBuilder.Append(this.fileInformation.PatientName);
                stringBuilder.Append(" | Body part: ");
                stringBuilder.Append(this.fileInformation.BodyPart);
                this.toolStripStatusLabel1.Text = stringBuilder.ToString();

                this.dataGridView1.Rows.Clear();

                foreach (KeyValuePair<string, string> kv in this.dictionary)
                    this.dataGridView1.Rows.Add(kv.Key, kv.Value);
            }

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
                    string filePath;
                    FileInfo fileInfo;

                    foreach (object o in filesArray)
                    {
                        filePath = o.ToString();

                        fileInfo = new FileInfo(filePath);

                        if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                        }
                        else if (fileInfo.Extension == ".dcm" || fileInfo.Extension == ".DCM")
                            this.AddFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // DEBUG
                System.Diagnostics.Debug.WriteLine("DragDrop exception: " + ex.Message);
            }
        }

        private void AddDir(string path)
        {
        }

        private void AddFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            TreeNode node = new TreeNode(fileInfo.Name);

            node.Tag = path;
            this.treeView1.Nodes.Add(node);
        }

        private void showRawDICOMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.rawDataForm == null)
                this.rawDataForm = new RAWDataForm();

            this.rawDataForm.Focus();
            this.rawDataForm.Content = this.reader.ListAllTags();
            this.rawDataForm.ShowDialog();
        }
    }
}
