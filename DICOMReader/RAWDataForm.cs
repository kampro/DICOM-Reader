using System;
using System.Windows.Forms;

namespace DICOMReader
{
    class RAWDataForm : Form
    {
        private Panel panel;
        private TextBox textBox;

        public string Content
        {
            get { return this.textBox.Text; }
            set { this.textBox.Text = value; } 
        }

        public RAWDataForm()
        {
            this.textBox = new TextBox();
            this.textBox.BackColor = System.Drawing.Color.White;
            this.textBox.Multiline = true;
            this.textBox.Dock = DockStyle.Fill;
            this.textBox.ScrollBars = ScrollBars.Both;
            this.textBox.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericMonospace, this.textBox.Font.Size);
            this.textBox.TabStop = false;

            this.panel = new Panel();
            this.panel.AutoScroll = true;
            this.panel.Dock = DockStyle.Fill;
            this.panel.BackColor = System.Drawing.Color.White;
            this.panel.Controls.Add(this.textBox);

            this.Text = "DICOM raw data";
            this.Width = 800;
            this.Height = 600;
            this.Controls.Add(this.panel);
        }
    }
}
