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

            this.reader = new Reader();
            this.rawDataForm = new RAWDataForm();
        }

        private void ReadFromReader()
        {
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
                    TreeNode node;
                    FileInfo fileInfo;

                    foreach (object o in filesArray)
                    {
                        filePath = o.ToString();

                        fileInfo = new FileInfo(filePath);

                        node = new TreeNode(fileInfo.Name);
                        node.Tag = filePath;

                        this.treeView1.Nodes.Add(node);
                    }
                }
            }
            catch (Exception ex)
            {
                // DEBUG
                System.Diagnostics.Debug.WriteLine("DragDrop exception: " + ex.Message);
            }
        }

        private void showRawDICOMDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (this.rawDataForm == null)
            //    this.rawDataForm = new RAWDataForm();

            this.rawDataForm.Content = this.reader.ListAllTags();
            this.rawDataForm.ShowDialog();
        }
    }
}
