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

        String filename;
        InterFrame IaFrame;
        InterFrame IeFrame;
        int num_frames_ready = 0;

        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
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
                //string filename = fileDlg.FileName;
                //Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(filename);
                //IaFrame = new IntraFrame(bmp);
                //this.pictureBox1.Image = this.IaFrame.ogBmp;
                //ShrunkFileIO sf = new ShrunkFileIO();
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
                Debug.WriteLine(ext);
                if (ext == ".bmp" || ext == ".BMP")
                {
                    Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(filename);
                    IaFrame = new InterFrame(bmp);
                    this.pictureBox1.Image = this.IaFrame.ogBmp;
                    IaFrame.prepFrameForShrink();
                    num_frames_ready++;
                    ShrunkFileIO sf = new ShrunkFileIO(IaFrame);
                    bool success = sf.writeFile("intraF.shrunk");
                    Debug.WriteLine(success ? "File Written" : "Error");
                }
                if (ext == ".shrunk")
                {
                    num_frames_ready--;
                    ShrunkFileIO sf = new ShrunkFileIO(filename);
                    IaFrame = sf.ioFrame;
                    this.pictureBox1.Image = IaFrame.ogBmp;
                    //IaFrame.setCbCrBmp();
                    //this.pictureBox2.Image = IaFrame.cbBmp;
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
                Debug.WriteLine(ext);
                if (ext == ".bmp" || ext == ".BMP")
                {
                    Bitmap bmp = (System.Drawing.Bitmap)Image.FromFile(filename);
                    IeFrame = new InterFrame(bmp);
                    this.pictureBox2.Image = this.IeFrame.ogBmp;
                    IeFrame.prepFrameForShrink();
                    num_frames_ready++;
                    ShrunkFileIO sf = new ShrunkFileIO(IeFrame);
                    bool success = sf.writeFile("interF.shrunk");
                    Debug.WriteLine(success ? "File Written" : "Error");
                }
                if (ext == ".shrunk")
                {
                    num_frames_ready--;
                    ShrunkFileIO sf = new ShrunkFileIO(filename);
                    IeFrame = sf.ioFrame;
                    this.pictureBox2.Image = IeFrame.ogBmp;
                    //IaFrame.setCbCrBmp();
                    //this.pictureBox2.Image = IaFrame.cbBmp;
                }
            }

            //ShrunkFileIO sf = new ShrunkFileIO();

            //IaFrame.prepFrameForShrink();
            //bool success = sf.writeFile("dog");
            //Debug.WriteLine(success?"File Written":"Error");

            //List<List<List<double>>> testVals = new List<List<List<double>>> {
            //new List<List<double>> {
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 }
            //},
            //new List<List<double>>{
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 }
            //},
            //new List<List<double>>
            //{
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 }
            //},
            //new List<List<double>>
            //{
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 },
            //    new List<double> { 0, 1, 2, 3, 4, 5, 6, 7 }
            //}
            //};
            //Frame f = new Frame();
            //testVals = f.dctAllBlocks(testVals);
            //testVals = f.quantizeAllBlocks(testVals, Frame.lumQTable);
            ////IaFrame.printBlocks(testVals);
            //List<sbyte> vals = f.zigZagAllBlocks(testVals);

            //Debug.WriteLine("before:");
            //vals = f.runLengthEncode(vals);
            //for (int i = 0; i < vals.Count; i++)
            //{
            //    Debug.Write(vals[i] + ",");
            //}
            //Debug.WriteLine("after:");
            //vals = f.runLengthDecode(vals.ToArray());
            //for (int i = 0; i < vals.Count; i++)
            //{
            //    Debug.Write(vals[i] + ",");
            //}


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(num_frames_ready > 1)
            {
                IeFrame.mv(IaFrame, 10);
                this.pictureBox2.Image = IeFrame.drawMotionVectors();
                //foreach(var mVec in IeFrame.mvY)
                //{
                //    Debug.WriteLine("mv-" + mVec[0]+ ":" + mVec[1]);
                //}
            }
        }
    }
}
