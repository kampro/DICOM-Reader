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

            this.reader = new Reader(this.pictureBox1);
            this.reader.Read();
        }
    }
}
