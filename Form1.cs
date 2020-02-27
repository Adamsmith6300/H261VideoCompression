using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace assignment2
{
    public partial class Form1 : Form
    {

        String filename;
        IntraFrame IaFrame;
        InterFrame IeFrame;

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            this.pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
        }


        /// <summary>Handles the Click event of the openFileToolStripMenuItem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "Image (*bmp)|*.bmp|All Files|*.*";

            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                this.filename = fileDlg.FileName;
                Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(fileDlg.FileName);
                IaFrame = new IntraFrame(bmp);
                this.pictureBox1.Image = this.IaFrame.ogBmp;
                
            }
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            IaFrame.convertToYCbCr();
            IaFrame.setCbCrBmp();
            this.pictureBox1.Image = this.IaFrame.cbBmp;
        }

        private void YCrCbRGB_Click(object sender, EventArgs e)
        {
            IaFrame.subsampleChrominance();
            IaFrame.setCbCrBmp();
            this.pictureBox1.Image = this.IaFrame.cbBmp;
        }

        private void test_Click(object sender, EventArgs e)
        {
            List<List<double>> testVals = new List<List<double>> {
                new List<double> { 0, 1, 2, 3 },
                new List<double> { 0, 1, 2, 3 },
                new List<double> { 0, 1, 2, 3 },
                new List<double> { 0, 1, 2, 3 }
            };
            List<List<double>> newVals = IaFrame.dct(testVals);
            IaFrame.printDctValues(newVals);
            
        }
    }
}
