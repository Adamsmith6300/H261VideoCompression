using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace assignment2
{
    public partial class Form1 : Form
    {
        InterFrame IaFrame;
        InterFrame IeFrame;
        int num_frames_ready = 0;
        int pValue = 8;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            numericUpDown1.Value = 8;
            numericUpDown1.Maximum = 15;
            numericUpDown1.Minimum = 1;
            numericUpDown1.DecimalPlaces = 0;
        }


        /// <summary>Handles the Click event of the openFileToolStripMenuItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save an Image File";
            saveFileDialog1.ShowDialog();
            pValue = (int)numericUpDown1.Value;
            if (saveFileDialog1.FileName != "" && num_frames_ready > 1)
            {
                IeFrame.generateMv(IaFrame, pValue);
                IaFrame.decompressFrame();
                IeFrame.compressInterFrame();
                IaFrame.compressFrame();
                ShrunkFileIO sf = new ShrunkFileIO(IaFrame, IeFrame);
                sf.writeFile(saveFileDialog1.FileName);
                Debug.WriteLine("Finished Compressing/Writing to file...");
                this.pictureBox2.Image = IeFrame.drawMotionVectors();
                this.Text = "Compression Ratio: " + sf.compressionRatio;
            }

        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "Shrunk (*shrunk)|*.shrunk";

            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                string filename = fileDlg.FileName;
                string ext = Path.GetExtension(filename);
                if (ext == ".shrunk")
                {
                    ShrunkFileIO sf = new ShrunkFileIO(filename);
                    this.pictureBox1.Image = sf.iaFrame.ogBmp;
                    this.pictureBox2.Image = sf.ieFrame.ogBmp;
                }
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "Image Files|*.bmp;*.BMP;*.shrunk;";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                string filename = fileDlg.FileName;
                string ext = Path.GetExtension(filename);
                if (ext == ".bmp" || ext == ".BMP")
                {
                    Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(filename);
                    IaFrame = new InterFrame(bmp);
                    this.pictureBox1.Image = this.IaFrame.ogBmp;
                    IaFrame.subSampleAndYCbCr();
                    IaFrame.compressFrame();
                    num_frames_ready++;
                }
                
            }
        }


        private void test_Click(object sender, EventArgs e)
        {

            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "Image Files|*.bmp;*.BMP;*.shrunk;";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                string filename = fileDlg.FileName;
                string ext = Path.GetExtension(filename);
                if (ext == ".bmp" || ext == ".BMP")
                {
                    Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(filename);
                    IeFrame = new InterFrame(bmp);
                    this.pictureBox2.Image = this.IeFrame.ogBmp;
                    IeFrame.prepInterFrame();
                    num_frames_ready++;
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Value = Decimal.Round(numericUpDown1.Value, 0);
        }
    }
}
