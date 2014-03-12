using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DICOMReader
{
    public partial class Form1 : Form
    {
        private Reader reader;
        private FileInformation fileInformation;
        private Dictionary<string, string> dictionary;

        public Form1()
        {
            InitializeComponent();

            this.reader = new Reader();

            if (this.reader.Read())
            {
                this.pictureBox1.Image = this.reader.BitmapImage;
                this.fileInformation = this.reader.FileInf;
                this.dictionary = this.fileInformation.GetTagsDictionary();

                StringBuilder stringBuilder = new StringBuilder("Patient: ");
                stringBuilder.Append(this.fileInformation.PatientName);
                stringBuilder.Append(" | Body part: ");
                stringBuilder.Append(this.fileInformation.BodyPart);
                this.toolStripStatusLabel1.Text = stringBuilder.ToString();

                this.dataGridView1.Columns.Add("tagName", "Tag name");
                this.dataGridView1.Columns.Add("tagValue", "Tag value");

                foreach (KeyValuePair<string, string> kv in this.dictionary)
                    this.dataGridView1.Rows.Add(kv.Key, kv.Value);
            }
        }
    }
}
