using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assignment2
{
    class InterFrame : Frame
    {
        public Frame iFrame { get; set; }
        public List<List<MV>> mvY { get; set; }
        public List<List<MV>> mvCb { get; set; }
        public List<List<MV>> mvCr { get; set; }
        public byte[] mvYBytes { get; set; }
        public byte[] mvCbBytes { get; set; }
        public byte[] mvCrBytes { get; set; }
        public List<List<double>> yDiffs { get; set; }
        public List<List<double>> cbDiffs { get; set; }
        public List<List<double>> crDiffs { get; set; }
        public List<List<List<double>>> yDiffBlocks { get; set; }
        public List<List<List<double>>> cbDiffBlocks { get; set; }
        public List<List<List<double>>> crDiffBlocks { get; set; }

        public InterFrame() { }
        public InterFrame(Bitmap bmp): base(bmp)
        {
        }
        public InterFrame(sbyte[] frameBytes, int w, int h): base(frameBytes, w, h)
        {
        }
        public InterFrame(Frame _iFrame, sbyte[] frameBytes, int w, int h, byte[] _mvYBytes, byte[] _mvCbBytes, byte[] _mvCrBytes)
        {
            iFrame = _iFrame;
            width = w;
            height = h;
            encoded = frameBytes;
            mvYBytes = _mvYBytes;
            mvCbBytes = _mvCbBytes;
            mvCrBytes = _mvCrBytes;
            decompressInterFrame();
        }



        public void prepInterFrame()
        {
            convertToYCbCr();
            subsampleChrominance();
        }
        public void decompressInterFrame()
        {
            convertBytesToMV();

            List<sbyte> decoded = runLengthDecode(encoded);
            
            int yCount = (decoded.Count * 2) / 3;
            int chromCount = yCount / 4;
            this.ogBmp = new Bitmap(width, height);

            yCoded = decoded.GetRange(0, yCount);
            cbCoded = decoded.GetRange(yCount, chromCount);
            crCoded = decoded.GetRange(yCount + chromCount, chromCount);

            yDiffBlocks = reverseZigZagAndBuildBlocks(yCoded);
            cbDiffBlocks = reverseZigZagAndBuildBlocks(cbCoded);
            crDiffBlocks = reverseZigZagAndBuildBlocks(crCoded);

            yDiffBlocks = reverseQuantizeAllBlocks(yDiffBlocks, diffQTable);
            cbDiffBlocks = reverseQuantizeAllBlocks(cbDiffBlocks, diffQTable);
            crDiffBlocks = reverseQuantizeAllBlocks(crDiffBlocks, diffQTable);

            yDiffBlocks = reverseDctAllBlocks(yDiffBlocks);
            cbDiffBlocks = reverseDctAllBlocks(cbDiffBlocks);
            crDiffBlocks = reverseDctAllBlocks(crDiffBlocks);

            yDiffs = getPixelsFromBlocks(yDiffBlocks, width, height);
            cbDiffs = getPixelsFromBlocks(cbDiffBlocks, width / 2, height / 2);
            crDiffs = getPixelsFromBlocks(crDiffBlocks, width / 2, height / 2);

            yDoubles = generateDoublesFromDiffs(mvY, yDiffs, iFrame.yDoubles);
            cbDoubles = generateDoublesFromDiffs(mvCb, cbDiffs, iFrame.cbDoubles);
            crDoubles = generateDoublesFromDiffs(mvCr, crDiffs, iFrame.crDoubles);
            upsampleChrominance();
            convertToRGB();
        }
        public void compressInterFrame()
        {
            yDiffs = generateDiffs(mvY, this.yDoubles, iFrame.yDoubles);
            cbDiffs = generateDiffs(mvCb, this.cbDoubles, iFrame.cbDoubles);
            crDiffs = generateDiffs(mvCr, this.crDoubles, iFrame.crDoubles);
            
            yDiffBlocks = formBlocks(yDiffs, 8);
            cbDiffBlocks = formBlocks(cbDiffs, 8);
            crDiffBlocks = formBlocks(crDiffs, 8);

            yDiffBlocks = dctAllBlocks(yDiffBlocks);
            cbDiffBlocks = dctAllBlocks(cbDiffBlocks);
            crDiffBlocks = dctAllBlocks(crDiffBlocks);

            yDiffBlocks = quantizeAllBlocks(yDiffBlocks, diffQTable);
            cbDiffBlocks = quantizeAllBlocks(cbDiffBlocks, diffQTable);
            crDiffBlocks = quantizeAllBlocks(crDiffBlocks, diffQTable);

            yCoded = zigZagAllBlocks(yDiffBlocks);
            cbCoded = zigZagAllBlocks(cbDiffBlocks);
            crCoded = zigZagAllBlocks(crDiffBlocks);

            yCoded = runLengthEncode(yCoded);
            cbCoded = runLengthEncode(cbCoded);
            crCoded = runLengthEncode(crCoded);

            List<sbyte> YCbCr = new List<sbyte>(); 
            YCbCr.AddRange(yCoded);
            YCbCr.AddRange(cbCoded);
            YCbCr.AddRange(crCoded);
            encoded = YCbCr.ToArray();
            convertMVToBytes();
        }

        public List<List<double>> generateDoublesFromDiffs(List<List<MV>> mv, List<List<double>> diffs, List<List<double>> reference)
        {
            List<List<double>> values = new List<List<double>>();
            for (int row = 0; row < diffs.Count; ++row)
            {
                values.Add(new List<double>());
                for (int col = 0; col < diffs[row].Count; ++col)
                {
                    int mvX = mv[(int)Math.Floor((double)row / 8)][(int)Math.Floor((double)col / 8)].x;
                    int mvY = mv[(int)Math.Floor((double)row / 8)][(int)Math.Floor((double)col / 8)].y;
                    double val = diffs[row][col] + reference[row - mvY][col - mvX];
                    values[row].Add(val);
                }
            }
            return values;
        }
        public List<List<double>> generateDiffs(List<List<MV>> mv, List<List<double>> target, List<List<double>> reference)
        {
            List<List<double>> diffs = new List<List<double>>();
            for(int row = 0; row < target.Count; ++row)
            {
                diffs.Add(new List<double>());
                for(int col = 0; col < target[row].Count; ++col)
                {
                    int mvX = mv[row/8][col/8].x;
                    int mvY = mv[row/8][col/8].y;
                    double diff = target[row][col] - reference[row+ mvX][col+ mvY];
                    diffs[row].Add(diff);
                }
            }

            return diffs;
        }

        public void convertBytesToMV()
        {
            int mvWidth = (int)Math.Ceiling((double)width / 8);
            int mvHeight = (int)Math.Ceiling((double)height / 8);
            mvY = bytesGetMV(mvYBytes, mvWidth, mvHeight);
            mvWidth /= 2;
            mvHeight /= 2;
            mvCb = bytesGetMV(mvCbBytes, mvWidth, mvHeight);
            mvCr = bytesGetMV(mvCrBytes, mvWidth, mvHeight);
        }
        public void convertMVToBytes()
        {
            mvYBytes = mvGetBytes(mvY);
            mvCbBytes = mvGetBytes(mvCb);
            mvCrBytes = mvGetBytes(mvCr);
        }
        public List<List<MV>> bytesGetMV(byte[] mvBytes, int mvWidth, int mvHeight)
        {
            List<List<MV>> mv = new List<List<MV>>();
            sbyte[] mvSBytes = Array.ConvertAll(mvBytes, b => (sbyte)b);
            for(int row = 0; row < mvHeight; ++row)
            {
                mv.Add(new List<MV>());
                for(int col = 0; col < mvWidth; ++col)
                {
                    int index = ((mvWidth * row) + col)*2;
                    sbyte x = mvSBytes[index];
                    sbyte y = mvSBytes[index+1];
                    MV vector = new MV(x, y);
                    mv[row].Add(vector);
                }
            }
            
            return mv;
        }
        public byte[] mvGetBytes(List<List<MV>> mv)
        {
            List<SByte> bytesList = new List<SByte>();
            for(int row = 0; row < mv.Count; ++row)
            {
                for(int col = 0; col < mv[row].Count; ++col)
                {
                    bytesList.Add(mv[row][col].x);
                    bytesList.Add(mv[row][col].y);
                }
            }
            sbyte[] mvBytesSigned = bytesList.ToArray();

            return (byte[])mvBytesSigned.Cast<byte>();
        }
        public void generateMv(Frame iFrame, int p)
        {
            this.iFrame = iFrame;
            mvY = mvSearch(p, 8, this.yDoubles, iFrame.yDoubles);
            mvCb = mvSearch(p, 8, this.cbDoubles, iFrame.cbDoubles);
            mvCr = mvSearch(p, 8, this.crDoubles, iFrame.crDoubles);
        }

        public List<List<MV>> mvSearch(int p, int n, List<List<double>> target, List<List<double>> reference)
        {
            List<List<MV>> motionVectors = new List<List<MV>>();
            //loop through each block in target frame
            for(int row = 0; row < target.Count; row+=n)
            {
                motionVectors.Add(new List<MV>());
                for(int col = 0; col < target[row].Count; col+=n)
                {
                    sbyte u = 0, v = 0;
                    double min_mad = Double.MaxValue;
                    for(int i = -p; i < p; i++)
                    {
                        for(int j = -p; j < p; j++)
                        {
                            double cur_mad = mad(i, j, row, col, n, target, reference);
                            if (cur_mad < min_mad)
                            {
                                min_mad = cur_mad;
                                u = (sbyte)i;
                                v = (sbyte)j;
                            }
                        }
                    }
                    motionVectors[row/n].Add(new MV(u, v));
                }
            }
            return motionVectors;

        }
        public double mad(int i, int j, int x, int y, int n, List<List<double>> target, List<List<double>> reference)
        {
            double diff = 0.0;
            for(int k = 0; k < n; k++)
            {
                for(int l = 0; l < n; l++)
                {
                    int targetX = x + k;
                    int targetY = y + l;
                    int refX = x + i + k;
                    int refY = y + j + l;
                    double targetValue = 0.0;
                    if((targetY >= 0 && targetY < target.Count) && (targetX >= 0 && targetX < target[0].Count))
                    {
                        targetValue = target[targetX][targetY];
                    }
                    double refValue = 0.0;
                    if ((refY >= 0 && refY < reference.Count) && (refX >= 0 && refX < reference[0].Count))
                    {
                        refValue = reference[refX][refY];
                    }
                    diff += Math.Abs(targetValue - refValue);
                }
            }
            return (1 / (Math.Pow(n, 2)) * diff);
        }

        public Bitmap drawMotionVectors()
        {
            Pen redPen = new Pen(Color.Red, 1);
            Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ffff"));
            for (int row = 0; row < yDoubles.Count; row += 8)
            {
                for (int col = 0; col < yDoubles[row].Count; col += 8)
                {
                    int startX = row + 4;
                    int startY = col + 4;
                    int endX = startX + mvY[row/8][col/8].x;
                    int endY = startY + mvY[row/8][col/8].y;
                    using (var graphics = Graphics.FromImage(ogBmp))
                    {
                        if(startX == endX && startY == endY)
                        {
                            graphics.FillEllipse(brsh, startX, startY, 2, 2);
                        } else
                        {
                            graphics.DrawLine(redPen, startX, startY, endX, endY);
                        }
                    }
                }
            }
            return ogBmp;
        }
    }
}
