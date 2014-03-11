using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DICOMReader
{
    public partial class Form1 : Form
    {
        private Reader reader;

        public Form1()
        {
            InitializeComponent();

            this.reader = new Reader();

            if (this.reader.Read())
            {
                this.pictureBox1.Image = this.reader.BitmapImage;

                StringBuilder stringBuilder = new StringBuilder("Patient: ");
                stringBuilder.Append(this.reader.PatientName);
                stringBuilder.Append(" | Body part: ");
                stringBuilder.Append(this.reader.BodyPart);
                this.toolStripStatusLabel1.Text = stringBuilder.ToString();
            }
        }
    }
}
